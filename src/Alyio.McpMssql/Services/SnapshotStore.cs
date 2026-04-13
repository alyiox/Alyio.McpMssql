// MIT License

using Microsoft.Extensions.Logging;

namespace Alyio.McpMssql.Services;

/// <summary>
/// Query result snapshot store backed by <see cref="ContentStore"/>.
/// Snapshots are cached under <c>~/.cache/mcpmssql/snapshots/</c> with a 1-day TTL.
/// </summary>
internal sealed class SnapshotStore : ContentStore, ISnapshotStore
{
    private const string CacheRelativePath = ".cache/mcpmssql/snapshots";
    private const string FileExtension = ".snapshot.csv";
    private static readonly TimeSpan s_ttl = TimeSpan.FromDays(1);

    public SnapshotStore(ILogger<SnapshotStore> logger)
        : base(CacheRelativePath, FileExtension, s_ttl, logger)
    {
    }

    /// <summary>
    /// Internal constructor for testing with an explicit directory path.
    /// </summary>
    internal SnapshotStore(string snapshotsDirectory, ILogger<SnapshotStore> logger)
        : base(snapshotsDirectory, FileExtension, s_ttl, logger, default)
    {
    }
}
