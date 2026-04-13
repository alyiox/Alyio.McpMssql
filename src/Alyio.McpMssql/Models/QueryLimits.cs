// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Constraints and defaults applied to interactive ad-hoc queries.
/// These limits are enforced by the server to ensure safe, bounded execution.
/// </summary>
public sealed class QueryLimits
{
    /// <summary>
    /// Maximum number of rows that may be returned by any query.
    /// </summary>
    public required OptionDescriptor<int> MaxRows { get; init; }

    /// <summary>
    /// Absolute, non-configurable hard limit for query row counts.
    /// </summary>
    public required OptionDescriptor<int> HardRowLimit { get; init; }

    /// <summary>
    /// Maximum execution time allowed for an inspection query,
    /// in seconds.
    /// </summary>
    public required OptionDescriptor<int> CommandTimeoutSeconds { get; init; }

    /// <summary>
    /// Maximum number of rows captured for a snapshot query.
    /// </summary>
    public required OptionDescriptor<int> SnapshotMaxRows { get; init; }

    /// <summary>
    /// Absolute, non-configurable hard limit for snapshot query row counts.
    /// </summary>
    public required OptionDescriptor<int> HardSnapshotRowLimit { get; init; }

    /// <summary>
    /// Maximum execution time for a snapshot query, in seconds.
    /// </summary>
    public required OptionDescriptor<int> SnapshotCommandTimeoutSeconds { get; init; }
}
