// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Optional metadata about the tool response.
/// </summary>
public class ResponseMeta
{
    /// <summary>
    /// Whether the result set was truncated due to row limits.
    /// </summary>
    public bool? Truncated { get; init; }

    /// <summary>
    /// The maximum number of rows that were returned.
    /// </summary>
    public int? MaxRows { get; init; }

    /// <summary>
    /// The actual number of rows in the result.
    /// Only included when different from rows.length (e.g., when pagination info is needed).
    /// </summary>
    public int? RowCount { get; init; }
}
