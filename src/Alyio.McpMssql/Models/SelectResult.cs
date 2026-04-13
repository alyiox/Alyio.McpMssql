// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Raw output of a SELECT query execution: column names, row values,
/// truncation flag, and the row limit that was applied.
/// Used internally to decouple query execution from result formatting.
/// </summary>
internal sealed record SelectResult(
    IReadOnlyList<string> Columns,
    IReadOnlyList<object?[]> Rows,
    bool Truncated,
    int RowLimit);
