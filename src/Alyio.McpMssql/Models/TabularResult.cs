// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Represents a tabular result with a fixed column set
/// and row values aligned by column index.
/// </summary>
public class TabularResult
{
    /// <summary>
    /// Gets the ordered list of column names.
    /// </summary>
    public required IReadOnlyList<string> Columns { get; init; }

    /// <summary>
    /// Gets the row values.
    /// Each row aligns with <see cref="Columns"/> by index.
    /// </summary>
    public required IReadOnlyList<object?[]> Rows { get; init; }
}
