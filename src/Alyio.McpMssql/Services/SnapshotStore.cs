// MIT License

using Microsoft.Extensions.Logging;

namespace Alyio.McpMssql.Services;

/// <summary>
/// Query result snapshot store backed by <see cref="ContentStore"/>.
/// Snapshots are cached under <c>~/.cache/mcp-mssql/snapshots/</c> with a 7-day TTL.
/// </summary>
internal sealed class SnapshotStore : ContentStore, ISnapshotStore
{
    private const string CacheRelativePath = ".cache/mcp-mssql/snapshots";
    private const string FileExtension = ".snapshot.csv";
    internal static readonly TimeSpan Ttl = TimeSpan.FromDays(7);

    public SnapshotStore(ILogger<SnapshotStore> logger)
        : base(CacheRelativePath, FileExtension, Ttl, logger)
    {
    }

    /// <summary>
    /// Internal constructor for testing with an explicit directory path.
    /// </summary>
    internal SnapshotStore(string snapshotsDirectory, ILogger<SnapshotStore> logger)
        : base(snapshotsDirectory, FileExtension, Ttl, logger, default)
    {
    }
}
