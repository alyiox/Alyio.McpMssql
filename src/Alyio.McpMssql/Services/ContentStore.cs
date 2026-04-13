// MIT License

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Alyio.McpMssql.Services;

/// <summary>
/// Ephemeral file-backed content store with in-memory caching and
/// time-based eviction. Subclasses provide content-type-specific
/// configuration (cache path, file extension, TTL).
/// </summary>
internal abstract partial class ContentStore : IContentStore
{
    private readonly string _fileExtension;
    private readonly TimeSpan _ttl;
    private readonly ILogger _logger;
    private readonly string _directory;
    private readonly Lazy<ConcurrentDictionary<string, string>> _memoryStore;

    protected ContentStore(
        string cacheRelativePath,
        string fileExtension,
        TimeSpan ttl,
        ILogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheRelativePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);

        _fileExtension = fileExtension;
        _ttl = ttl;
        _logger = logger;
        _directory = GetDefaultDirectory(cacheRelativePath);
        _memoryStore = new Lazy<ConcurrentDictionary<string, string>>(
            LoadExistingIntoMemory,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Internal constructor for testing with an explicit directory path.
    /// </summary>
    internal ContentStore(
        string directory,
        string fileExtension,
        TimeSpan ttl,
        ILogger logger,
        bool _)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);

        _fileExtension = fileExtension;
        _ttl = ttl;
        _logger = logger;
        _directory = directory;
        _memoryStore = new Lazy<ConcurrentDictionary<string, string>>(
            LoadExistingIntoMemory,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task<string> SaveAsync(string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        cancellationToken.ThrowIfCancellationRequested();

        var memoryStore = _memoryStore.Value;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = Guid.NewGuid().ToString("N")[..8];
            var finalPath = GetFilePath(id);
            var tempPath = Path.Combine(_directory, $".{id}.{Guid.NewGuid():N}.tmp");

            try
            {
                await File.WriteAllTextAsync(tempPath, content, cancellationToken).ConfigureAwait(false);
                File.Move(tempPath, finalPath);

                memoryStore[id] = content;
                return id;
            }
            catch (IOException) when (File.Exists(finalPath))
            {
                // Rare ID collision; retry with a new ID.
            }
            catch (IOException ex)
            {
                LogSaveContentFailed(_logger, ex, finalPath);
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogSaveContentUnauthorized(_logger, ex, finalPath);
                throw;
            }
            finally
            {
                TryDeleteFile(tempPath);
            }
        }
    }

    public async Task<string?> TryGetAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var memoryStore = _memoryStore.Value;

        if (memoryStore.TryGetValue(id, out var inMemory))
        {
            return inMemory;
        }

        var filePath = GetFilePath(id);
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var content = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            memoryStore[id] = content;
            return content;
        }
        catch (IOException ex)
        {
            LogReadContentFailed(_logger, ex, id, filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogReadContentUnauthorized(_logger, ex, id, filePath);
        }

        return null;
    }

    private ConcurrentDictionary<string, string> LoadExistingIntoMemory()
    {
        var memoryStore = new ConcurrentDictionary<string, string>();
        Directory.CreateDirectory(_directory);

        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(_directory, $"*{_fileExtension}");
        }
        catch (IOException ex)
        {
            LogEnumerateFilesFailed(_logger, ex, _directory);
            return memoryStore;
        }
        catch (UnauthorizedAccessException ex)
        {
            LogEnumerateFilesUnauthorized(_logger, ex, _directory);
            return memoryStore;
        }

        var now = DateTime.UtcNow;
        foreach (var file in files)
        {
            if (!TryGetExpiryUtc(file, out _, now))
            {
                TryDeleteFile(file);
                continue;
            }

            var id = TryGetId(file);
            if (string.IsNullOrEmpty(id))
            {
                TryDeleteFile(file);
                continue;
            }

            try
            {
                var content = File.ReadAllText(file);
                memoryStore[id] = content;
            }
            catch (IOException ex)
            {
                LogReadContentFailed(_logger, ex, id, file);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogReadContentUnauthorized(_logger, ex, id, file);
            }
        }

        return memoryStore;
    }

    private bool TryGetExpiryUtc(string path, out DateTime expiryUtc, DateTime now)
    {
        expiryUtc = default;
        try
        {
            var lastWriteUtc = File.GetLastWriteTimeUtc(path);
            if (lastWriteUtc == DateTime.MinValue)
            {
                return false;
            }

            expiryUtc = lastWriteUtc + _ttl;
            return expiryUtc > now;
        }
        catch (IOException ex)
        {
            LogReadMetadataFailed(_logger, ex, path);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            LogReadMetadataUnauthorized(_logger, ex, path);
            return false;
        }
    }

    private string GetFilePath(string id)
        => Path.Combine(_directory, $"{id}{_fileExtension}");

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException ex)
        {
            LogDeleteFileFailed(_logger, ex, path);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogDeleteFileUnauthorized(_logger, ex, path);
        }
    }

    private string? TryGetId(string filePath)
    {
        var name = Path.GetFileName(filePath);
        if (!name.EndsWith(_fileExtension, StringComparison.Ordinal))
        {
            return null;
        }

        return name[..^_fileExtension.Length];
    }

    private static string GetDefaultDirectory(string cacheRelativePath)
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(userProfile))
        {
            userProfile = Path.GetTempPath();
        }

        return Path.Combine(userProfile, cacheRelativePath);
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Failed to save content file at path '{path}'.")]
    private static partial void LogSaveContentFailed(ILogger logger, Exception ex, string path);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning, Message = "Unauthorized while saving content file at path '{path}'.")]
    private static partial void LogSaveContentUnauthorized(ILogger logger, Exception ex, string path);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning, Message = "Failed to read content file for id '{id}' from path '{path}'.")]
    private static partial void LogReadContentFailed(ILogger logger, Exception ex, string id, string path);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Warning, Message = "Unauthorized while reading content file for id '{id}' from path '{path}'.")]
    private static partial void LogReadContentUnauthorized(ILogger logger, Exception ex, string id, string path);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Warning, Message = "Failed to enumerate content files in '{directory}'.")]
    private static partial void LogEnumerateFilesFailed(ILogger logger, Exception ex, string directory);

    [LoggerMessage(EventId = 2006, Level = LogLevel.Warning, Message = "Unauthorized while enumerating content files in '{directory}'.")]
    private static partial void LogEnumerateFilesUnauthorized(ILogger logger, Exception ex, string directory);

    [LoggerMessage(EventId = 2007, Level = LogLevel.Warning, Message = "Failed to read content file metadata for '{path}'. Treating as expired.")]
    private static partial void LogReadMetadataFailed(ILogger logger, Exception ex, string path);

    [LoggerMessage(EventId = 2008, Level = LogLevel.Warning, Message = "Unauthorized reading content metadata for '{path}'. Treating as expired.")]
    private static partial void LogReadMetadataUnauthorized(ILogger logger, Exception ex, string path);

    [LoggerMessage(EventId = 2009, Level = LogLevel.Warning, Message = "Failed to delete content file '{path}' during best-effort cleanup.")]
    private static partial void LogDeleteFileFailed(ILogger logger, Exception ex, string path);

    [LoggerMessage(EventId = 2010, Level = LogLevel.Warning, Message = "Unauthorized deleting content file '{path}' during best-effort cleanup.")]
    private static partial void LogDeleteFileUnauthorized(ILogger logger, Exception ex, string path);
}
