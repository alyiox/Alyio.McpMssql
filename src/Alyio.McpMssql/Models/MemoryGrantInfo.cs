// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Memory grant details from the execution plan's
/// <c>MemoryGrantInfo</c> element. Compares granted vs. actually
/// used memory to detect over-grants (wasted memory from bad
/// cardinality estimates) and under-grants (spill risk).
/// </summary>
public sealed class MemoryGrantInfo
{
    /// <summary>
    /// Memory actually granted to the query, in KB.
    /// </summary>
    public int GrantedKb { get; init; }

    /// <summary>
    /// Memory the optimizer desired for optimal performance, in KB.
    /// If <see cref="GrantedKb"/> is less than this, the query may
    /// spill to tempdb.
    /// </summary>
    public int DesiredKb { get; init; }

    /// <summary>
    /// Peak memory actually consumed during execution, in KB.
    /// A large gap between <see cref="GrantedKb"/> and this value
    /// indicates an over-grant caused by inflated row estimates.
    /// </summary>
    public int MaxUsedKb { get; init; }

    /// <summary>
    /// Time the query waited for a memory grant, in milliseconds.
    /// <c>&gt;0</c> indicates memory pressure (RESOURCE_SEMAPHORE waits).
    /// </summary>
    public int GrantWaitTimeMs { get; init; }

    /// <summary>
    /// Whether the memory grant feedback mechanism (SQL Server 2019+)
    /// has adjusted this grant. Values like <c>"No: First Execution"</c>,
    /// <c>"Yes: Adjusting"</c>, or <c>"Yes: Stable"</c>.
    /// </summary>
    public string? FeedbackAdjusted { get; init; }
}
