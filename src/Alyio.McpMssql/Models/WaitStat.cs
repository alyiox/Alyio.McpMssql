// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// A per-query wait statistic from the execution plan's
/// <c>WaitStats</c> element (SQL Server 2016+). Indicates where
/// the query spent time waiting rather than executing.
/// </summary>
public sealed class WaitStat
{
    /// <summary>
    /// Wait type name (e.g. <c>SOS_SCHEDULER_YIELD</c>,
    /// <c>PAGEIOLATCH_SH</c>, <c>CXSYNC_PORT</c>).
    /// Maps directly to SQL Server wait types.
    /// </summary>
    public required string WaitType { get; init; }

    /// <summary>
    /// Total time spent on this wait type, in milliseconds.
    /// </summary>
    public int WaitTimeMs { get; init; }

    /// <summary>
    /// Number of times this wait occurred during query execution.
    /// </summary>
    public int WaitCount { get; init; }
}
