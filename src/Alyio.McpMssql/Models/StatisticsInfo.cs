// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Information about a statistics object used by the optimizer
/// when compiling the execution plan. Surfaced from the plan's
/// <c>OptimizerStatsUsage</c> element. Only problematic statistics
/// are included (low sampling rate or stale due to modifications).
/// </summary>
public sealed class StatisticsInfo
{
    /// <summary>
    /// Table that the statistics belong to, in <c>[table]</c> format.
    /// </summary>
    public required string Table { get; init; }

    /// <summary>
    /// Statistics object name. Names starting with <c>_WA_Sys_</c>
    /// are auto-created by SQL Server and may have low sampling rates.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Whether this statistics object was auto-created by SQL Server
    /// (inferred from the <c>_WA_Sys_</c> naming convention).
    /// Auto-created statistics often use low sampling percentages
    /// on large tables.
    /// </summary>
    public bool AutoCreated { get; init; }

    /// <summary>
    /// Percentage of rows sampled when the statistics were last
    /// computed. Low values (e.g. 3%) on large tables can produce
    /// inaccurate cardinality estimates. Consider
    /// <c>UPDATE STATISTICS ... WITH FULLSCAN</c>.
    /// </summary>
    public double SamplingPct { get; init; }

    /// <summary>
    /// Number of row modifications since the statistics were last
    /// updated. High values indicate stale statistics that may
    /// mislead the optimizer.
    /// </summary>
    public long ModificationCount { get; init; }

    /// <summary>
    /// When the statistics were last recomputed (UTC).
    /// </summary>
    public string? LastUpdate { get; init; }
}
