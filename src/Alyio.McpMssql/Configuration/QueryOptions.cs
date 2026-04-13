// MIT License

namespace Alyio.McpMssql.Configuration;

/// <summary>
/// Server-enforced execution defaults and safety limits
/// for interactive read-only queries.
/// </summary>
public sealed class QueryOptions
{
    /// <summary>
    /// Maximum number of rows that may be returned for a single interactive query.
    /// Clamped to <see cref="HardRowLimit"/>.
    /// </summary>
    public int MaxRows { get; set; } = 500;

    /// <summary>
    /// Maximum execution time for an interactive query command, in seconds.
    /// Clamped to <see cref="HardCommandTimeoutSeconds"/>.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of rows captured for a snapshot query.
    /// Clamped to <see cref="HardSnapshotRowLimit"/>.
    /// </summary>
    public int SnapshotMaxRows { get; set; } = 10_000;

    /// <summary>
    /// Maximum execution time for a snapshot query command, in seconds.
    /// Clamped to <see cref="HardSnapshotCommandTimeoutSeconds"/>.
    /// </summary>
    public int SnapshotCommandTimeoutSeconds { get; set; } = 120;

    // -------------------------
    // Hard safety invariants
    // -------------------------

    /// <summary>
    /// Absolute, non-configurable hard limit for interactive query row counts.
    /// </summary>
    internal const int HardRowLimit = 1_000;

    /// <summary>
    /// Absolute, non-configurable hard limit for interactive query execution time.
    /// </summary>
    internal const int HardCommandTimeoutSeconds = 300;

    /// <summary>
    /// Absolute, non-configurable hard limit for snapshot query row counts.
    /// </summary>
    internal const int HardSnapshotRowLimit = 50_000;

    /// <summary>
    /// Absolute, non-configurable hard limit for snapshot query execution time.
    /// </summary>
    internal const int HardSnapshotCommandTimeoutSeconds = 300;
}
