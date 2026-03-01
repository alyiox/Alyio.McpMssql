// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// A single operator from the execution plan, representing one
/// physical step the engine performs (scan, seek, join, sort, etc.).
/// Operators are ranked by cost percentage; only the top N are returned.
/// </summary>
public sealed class PlanOperator
{
    /// <summary>
    /// Node identifier within the plan tree.
    /// Correlates with other sections (warnings, cardinality issues).
    /// </summary>
    public int NodeId { get; init; }

    /// <summary>
    /// Physical operation name (e.g. <c>Clustered Index Scan</c>,
    /// <c>Hash Match</c>, <c>Sort</c>, <c>Key Lookup</c>).
    /// </summary>
    public required string PhysicalOp { get; init; }

    /// <summary>
    /// Logical operation name. May differ from <see cref="PhysicalOp"/>
    /// (e.g. physical <c>Hash Match</c> with logical <c>Inner Join</c>).
    /// </summary>
    public required string LogicalOp { get; init; }

    /// <summary>
    /// This operator's own estimated cost as a percentage of the
    /// total plan cost. Computed as <c>(EstimateCPU + EstimateIO)
    /// / RootSubtreeCost * 100</c>.
    /// </summary>
    public double EstimatedCostPct { get; init; }

    /// <summary>
    /// Estimated output rows. Compare with <see cref="ActualRows"/>
    /// to detect cardinality estimation errors.
    /// </summary>
    public double EstimatedRows { get; init; }

    /// <summary>
    /// Actual output rows at runtime. <c>null</c> for estimated-only
    /// plans. Summed across threads for parallel operators.
    /// </summary>
    public long? ActualRows { get; init; }

    /// <summary>
    /// Estimated rows physically read before filtering (scan/seek only).
    /// A large gap vs. <see cref="EstimatedRows"/> signals a missing
    /// index or non-selective predicate.
    /// </summary>
    public double? EstimatedRowsRead { get; init; }

    /// <summary>
    /// Actual rows physically read at runtime (scan/seek only).
    /// <c>null</c> for non-scan operators or estimated-only plans.
    /// </summary>
    public long? ActualRowsRead { get; init; }

    /// <summary>
    /// Number of times this operator was executed.
    /// <c>&gt;1</c> typically indicates the inner side of a nested loop.
    /// </summary>
    public long? ActualExecutions { get; init; }

    /// <summary>
    /// Wall-clock time spent in this operator, in milliseconds.
    /// </summary>
    public long? ActualElapsedMs { get; init; }

    /// <summary>
    /// Execution mode: <c>Row</c> (traditional row-at-a-time) or
    /// <c>Batch</c> (columnar, significantly faster for analytics).
    /// </summary>
    public string? ExecutionMode { get; init; }

    /// <summary>
    /// Whether this operator ran in parallel across multiple threads.
    /// </summary>
    public bool IsParallel { get; init; }

    /// <summary>
    /// Fully qualified object name for scan/seek/lookup operators
    /// (e.g. <c>[dbo].[Orders].[PK_Orders]</c>). <c>null</c> for
    /// non-data-access operators like joins and sorts.
    /// </summary>
    public string? ObjectName { get; init; }
}
