// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Statement-level metrics extracted from the execution plan's
/// <c>StmtSimple</c>, <c>QueryPlan</c>, and <c>QueryTimeStats</c> elements.
/// </summary>
public sealed class StatementSummary
{
    /// <summary>
    /// Optimizer's estimated total subtree cost for the statement.
    /// Unitless relative measure; higher means more expensive.
    /// </summary>
    public double EstimatedCost { get; init; }

    /// <summary>
    /// Estimated number of output rows for the statement.
    /// Compare against actual rows to gauge estimation accuracy.
    /// </summary>
    public double EstimatedRows { get; init; }

    /// <summary>
    /// Optimizer level used to compile this plan.
    /// <c>TRIVIAL</c> means the optimizer applied a trivial plan
    /// without full cost-based optimization. <c>FULL</c> means
    /// the optimizer performed full exploration.
    /// </summary>
    public required string OptimizationLevel { get; init; }

    /// <summary>
    /// Cardinality estimator model version.
    /// <c>70</c> = legacy CE, <c>120</c>/<c>150</c>/<c>160</c>/<c>170</c> = new CE.
    /// Different versions can produce different row estimates for the same query.
    /// </summary>
    public int CeVersion { get; init; }

    /// <summary>
    /// Hash identifying the query shape, independent of literal values.
    /// Use to correlate with <c>sys.dm_exec_query_stats</c> in the plan cache.
    /// </summary>
    public string? QueryHash { get; init; }

    /// <summary>
    /// Hash identifying the execution plan shape.
    /// A different plan hash for the same query hash indicates a plan regression.
    /// </summary>
    public string? PlanHash { get; init; }

    /// <summary>
    /// Whether the engine used batch mode on a row store (SQL Server 2019+).
    /// Batch mode can significantly improve analytical query performance.
    /// </summary>
    public bool? BatchModeOnRowStore { get; init; }

    /// <summary>
    /// Actual degree of parallelism used.
    /// <c>1</c> = serial plan; <c>&gt;1</c> = parallel plan.
    /// </summary>
    public int? DegreeOfParallelism { get; init; }

    /// <summary>
    /// Reason the optimizer chose a serial plan when parallelism
    /// was otherwise possible. <c>null</c> if the plan is parallel
    /// or if no reason was recorded.
    /// </summary>
    public string? NonParallelReason { get; init; }

    /// <summary>
    /// Total CPU time across all threads, in milliseconds.
    /// When greater than <see cref="ElapsedTimeMs"/>, the query
    /// ran in parallel (CPU is summed across threads).
    /// </summary>
    public int? CpuTimeMs { get; init; }

    /// <summary>
    /// Wall-clock elapsed time in milliseconds.
    /// </summary>
    public int? ElapsedTimeMs { get; init; }

    /// <summary>
    /// Memory grant details for sorts, hashes, and other
    /// memory-consuming operators. <c>null</c> if no memory
    /// grant was required.
    /// </summary>
    public MemoryGrantInfo? MemoryGrant { get; init; }
}
