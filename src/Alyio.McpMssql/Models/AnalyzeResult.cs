// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Compact JSON summary of a SQL Server actual execution plan.
/// Extracted server-side from the showplan XML to minimize
/// token cost while preserving all actionable tuning signals.
/// The full raw XML is available at <see cref="PlanUri"/>.
/// </summary>
public sealed class AnalyzeResult
{
    /// <summary>
    /// Resource URI for the full XML execution plan
    /// (e.g. <c>mssql://plans/{id}</c>). Fetch this resource
    /// when the summary alone is insufficient for deep analysis.
    /// </summary>
    public required string PlanUri { get; init; }

    /// <summary>
    /// Statement-level metrics: cost, optimization level,
    /// cardinality estimator version, timing, and memory grant.
    /// </summary>
    public required StatementSummary Statement { get; init; }

    /// <summary>
    /// Top operators ranked by estimated cost percentage.
    /// Identifies where the plan spends the most resources.
    /// </summary>
    public required IReadOnlyList<PlanOperator> TopOperators { get; init; }

    /// <summary>
    /// Operators where estimated vs. actual row counts diverged
    /// by more than 10x. Empty when no significant mismatches exist.
    /// </summary>
    public required IReadOnlyList<CardinalityIssue> CardinalityIssues { get; init; }

    /// <summary>
    /// Optimizer and runtime warnings (spills, implicit conversions,
    /// missing statistics, cartesian products, memory grant issues).
    /// </summary>
    public required IReadOnlyList<PlanWarning> Warnings { get; init; }

    /// <summary>
    /// Missing index suggestions from the optimizer, directly
    /// translatable to <c>CREATE INDEX</c> statements.
    /// </summary>
    public required IReadOnlyList<MissingIndex> MissingIndexes { get; init; }

    /// <summary>
    /// Per-query wait statistics (SQL Server 2016+), filtered
    /// to the most significant wait types. Shows where the query
    /// spent time waiting rather than executing.
    /// </summary>
    public required IReadOnlyList<WaitStat> WaitStats { get; init; }

    /// <summary>
    /// Statistics objects the optimizer relied on, filtered to
    /// those that may be problematic (low sampling or stale).
    /// </summary>
    public required IReadOnlyList<StatisticsInfo> Statistics { get; init; }
}
