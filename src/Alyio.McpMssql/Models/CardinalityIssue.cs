// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Flags a plan operator where the optimizer's row estimate
/// diverged significantly from the actual row count at runtime.
/// Cardinality mismatches are the most common root cause of
/// suboptimal plan choices (wrong join type, insufficient
/// memory grant, unnecessary parallelism).
/// </summary>
public sealed class CardinalityIssue
{
    /// <summary>
    /// Node identifier of the affected operator.
    /// </summary>
    public int NodeId { get; init; }

    /// <summary>
    /// Human-readable description of the operator
    /// (e.g. <c>Clustered Index Scan on [dbo].[Orders].[PK_Orders]</c>).
    /// </summary>
    public required string Operator { get; init; }

    /// <summary>
    /// Row count the optimizer estimated.
    /// </summary>
    public double EstimatedRows { get; init; }

    /// <summary>
    /// Actual row count observed at runtime.
    /// </summary>
    public long ActualRows { get; init; }

    /// <summary>
    /// <c>ActualRows / EstimatedRows</c>.
    /// <c>&gt;1</c> = underestimate (more rows than expected);
    /// <c>&lt;1</c> = overestimate. Values beyond 10x or below
    /// 0.1x indicate a significant estimation problem.
    /// </summary>
    public double Ratio { get; init; }
}
