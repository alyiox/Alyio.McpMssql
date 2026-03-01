// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// A warning surfaced by the query optimizer or runtime engine.
/// Warnings indicate conditions that typically degrade performance
/// and are actionable (e.g. spills, implicit conversions, missing
/// statistics, cartesian products).
/// </summary>
public sealed class PlanWarning
{
    /// <summary>
    /// Warning category. Stable enum-like values:
    /// <c>SpillToTempDb</c>, <c>NoJoinPredicate</c>,
    /// <c>ColumnsWithNoStatistics</c>, <c>ImplicitConversion</c>,
    /// <c>MemoryGrantExcessive</c>, <c>MemoryGrantIncrease</c>,
    /// <c>UnmatchedIndexes</c>.
    /// </summary>
    public required string Kind { get; init; }

    /// <summary>
    /// Node identifier of the operator that produced the warning.
    /// <c>null</c> for plan-level warnings (e.g. implicit conversions).
    /// </summary>
    public int? NodeId { get; init; }

    /// <summary>
    /// Physical operation name of the affected operator.
    /// <c>null</c> for plan-level warnings.
    /// </summary>
    public string? Operator { get; init; }

    /// <summary>
    /// Human-readable detail describing the specific issue
    /// (e.g. spill level, conversion expression, affected columns).
    /// </summary>
    public required string Detail { get; init; }
}
