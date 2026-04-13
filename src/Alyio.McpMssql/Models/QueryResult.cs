// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Represents the result of an interactive or snapshot SQL query.
/// </summary>
public sealed class QueryResult
{
    /// <summary>
    /// CSV-formatted result data (header row + data rows) for inline delivery.
    /// Null in snapshot mode — use <see cref="SnapshotUri"/> to fetch the data.
    /// </summary>
    public string? Data { get; init; }

    /// <summary>
    /// Total number of rows in the result, excluding the header row.
    /// </summary>
    public required int RowCount { get; init; }

    /// <summary>
    /// Indicates whether the result set was truncated due to the enforced row limit.
    /// </summary>
    public bool Truncated { get; init; }

    /// <summary>
    /// The enforced maximum number of rows for this query.
    /// </summary>
    public int RowLimit { get; init; }

    /// <summary>
    /// Resource URI for the full CSV snapshot.
    /// Set only in snapshot mode; omitted for inline results.
    /// </summary>
    public string? SnapshotUri { get; init; }
}
