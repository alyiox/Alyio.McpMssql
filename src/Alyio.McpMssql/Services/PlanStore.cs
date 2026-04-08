// MIT License

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Alyio.McpMssql.Services;

internal sealed partial class PlanStore : IPlanStore
{
    private static readonly TimeSpan s_ttl = TimeSpan.FromDays(7);
    private static readonly string s_cacheRelativePath = Path.Combine(".cache", "mcpmssql", "plans");
    private const string PlanFileExtension = ".sqlplan.xml";
    private readonly ILogger<PlanStore> _logger;
    private readonly string _plansDirectory;
    private readonly Lazy<ConcurrentDictionary<string, string>> _memoryStore;

    public PlanStore(ILogger<PlanStore> logger)
        : this(GetDefaultPlansDirectory(), logger)
    {
    }

    internal PlanStore(string plansDirectory, ILogger<PlanStore> logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plansDirectory);
        _logger = logger;
        _plansDirectory = plansDirectory;
        _memoryStore = new Lazy<ConcurrentDictionary<string, string>>(
            LoadExistingPlansIntoMemory,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task<string> SaveAsync(string xml, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(xml);
        cancellationToken.ThrowIfCancellationRequested();

        var memoryStore = _memoryStore.Value;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = Guid.NewGuid().ToString("N")[..8];
            var finalPath = GetPlanFilePath(id);
            var tempPath = Path.Combine(_plansDirectory, $".{id}.{Guid.NewGuid():N}.tmp");

            try
            {
                await File.WriteAllTextAsync(tempPath, xml, cancellationToken).ConfigureAwait(false);
                File.Move(tempPath, finalPath);

                memoryStore[id] = xml;
                return id;
            }
            catch (IOException) when (File.Exists(finalPath))
            {
                // Rare ID collision; retry with a new ID.
            }
            catch (IOException ex)
            {
                LogSavePlanFileFailed(_logger, ex, finalPath);
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                LogSavePlanFileUnauthorized(_logger, ex, finalPath);
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

        var planPath = GetPlanFilePath(id);
        if (!File.Exists(planPath))
        {
            return null;
        }

        try
        {
            var xml = await File.ReadAllTextAsync(planPath, cancellationToken).ConfigureAwait(false);
            memoryStore[id] = xml;
            return xml;
        }
        catch (IOException ex)
        {
            LogReadPlanFileFailed(_logger, ex, id, planPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogReadPlanFileUnauthorized(_logger, ex, id, planPath);
        }

        return null;
    }

    private ConcurrentDictionary<string, string> LoadExistingPlansIntoMemory()
    {
        var memoryStore = new ConcurrentDictionary<string, string>();
        Directory.CreateDirectory(_plansDirectory);

        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(_plansDirectory, $"*{PlanFileExtension}");
        }
        catch (IOException ex)
        {
            LogEnumeratePlanFilesFailed(_logger, ex, _plansDirectory);
            return memoryStore;
        }
        catch (UnauthorizedAccessException ex)
        {
            LogEnumeratePlanFilesUnauthorized(_logger, ex, _plansDirectory);
            return memoryStore;
        }

        var now = DateTime.UtcNow;
        foreach (var file in files)
        {
            if (!TryGetPlanExpiryUtc(file, out var expiryUtc, now))
            {
                TryDeleteFile(file);
                continue;
            }

            var id = TryGetPlanId(file);
            if (string.IsNullOrEmpty(id))
            {
                TryDeleteFile(file);
                continue;
            }

            try
            {
                var xml = File.ReadAllText(file);
                memoryStore[id] = xml;
            }
            catch (IOException ex)
            {
                LogReadPlanFileFailed(_logger, ex, id, file);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogReadPlanFileUnauthorized(_logger, ex, id, file);
            }
        }

        return memoryStore;
    }

    private bool TryGetPlanExpiryUtc(string path, out DateTime expiryUtc, DateTime now)
    {
        expiryUtc = default;
        try
        {
            var lastWriteUtc = File.GetLastWriteTimeUtc(path);
            if (lastWriteUtc == DateTime.MinValue)
            {
                return false;
            }

            expiryUtc = lastWriteUtc + s_ttl;
            return expiryUtc > now;
        }
        catch (IOException ex)
        {
            LogReadPlanMetadataFailed(_logger, ex, path);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            LogReadPlanMetadataUnauthorized(_logger, ex, path);
            return false;
        }
    }

    private string GetPlanFilePath(string id)
        => Path.Combine(_plansDirectory, $"{id}{PlanFileExtension}");

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
            LogDeletePlanFileFailed(_logger, ex, path);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogDeletePlanFileUnauthorized(_logger, ex, path);
        }
    }

    private static string GetDefaultPlansDirectory()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(userProfile))
        {
            userProfile = Path.GetTempPath();
        }

        return Path.Combine(userProfile, s_cacheRelativePath);
    }

    private static string? TryGetPlanId(string filePath)
    {
        var name = Path.GetFileName(filePath);
        if (!name.EndsWith(PlanFileExtension, StringComparison.Ordinal))
        {
            return null;
        }

        return name[..^PlanFileExtension.Length];
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Warning, Message = "Failed to save plan file at path '{path}'.")]
    private static partial void LogSavePlanFileFailed(ILogger logger, Exception ex, string path);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "Unauthorized while saving plan file at path '{path}'.")]
    private static partial void LogSavePlanFileUnauthorized(ILogger logger, Exception ex, string path);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Warning, Message = "Failed to read plan file for id '{id}' from path '{path}'.")]
    private static partial void LogReadPlanFileFailed(ILogger logger, Exception ex, string id, string path);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Warning, Message = "Unauthorized while reading plan file for id '{id}' from path '{path}'.")]
    private static partial void LogReadPlanFileUnauthorized(ILogger logger, Exception ex, string id, string path);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Warning, Message = "Failed to enumerate plan cache files in '{directory}'.")]
    private static partial void LogEnumeratePlanFilesFailed(ILogger logger, Exception ex, string directory);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Warning, Message = "Unauthorized while enumerating plan cache files in '{directory}'.")]
    private static partial void LogEnumeratePlanFilesUnauthorized(ILogger logger, Exception ex, string directory);

    [LoggerMessage(EventId = 1007, Level = LogLevel.Warning, Message = "Failed to read plan file metadata for '{path}'. Treating as expired.")]
    private static partial void LogReadPlanMetadataFailed(ILogger logger, Exception ex, string path);

    [LoggerMessage(EventId = 1008, Level = LogLevel.Warning, Message = "Unauthorized reading plan metadata for '{path}'. Treating as expired.")]
    private static partial void LogReadPlanMetadataUnauthorized(ILogger logger, Exception ex, string path);

    [LoggerMessage(EventId = 1009, Level = LogLevel.Warning, Message = "Failed to delete plan file '{path}' during best-effort cleanup.")]
    private static partial void LogDeletePlanFileFailed(ILogger logger, Exception ex, string path);

    [LoggerMessage(EventId = 1010, Level = LogLevel.Warning, Message = "Unauthorized deleting plan file '{path}' during best-effort cleanup.")]
    private static partial void LogDeletePlanFileUnauthorized(ILogger logger, Exception ex, string path);

}
