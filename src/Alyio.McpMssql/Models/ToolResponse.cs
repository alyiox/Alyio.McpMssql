// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Standard response format for all MCP tools.
/// </summary>
public class ToolResponse
{
    /// <summary>
    /// Column metadata for the result set.
    /// </summary>
    public required Column[] Columns { get; init; }

    /// <summary>
    /// The result set rows. Each row is an array of values corresponding to the columns.
    /// </summary>
    public required object?[][] Rows { get; init; }

    /// <summary>
    /// Optional metadata about the response (pagination, timing, etc.).
    /// Only included when relevant.
    /// </summary>
    public ResponseMeta? Meta { get; init; }

    /// <summary>
    /// Creates a simple response with columns and rows (no metadata).
    /// </summary>
    public static ToolResponse Create(Column[] columns, object?[][] rows)
    {
        return new ToolResponse
        {
            Columns = columns,
            Rows = rows
        };
    }

    /// <summary>
    /// Creates a response with columns, rows, and metadata.
    /// </summary>
    public static ToolResponse Create(Column[] columns, object?[][] rows, ResponseMeta meta)
    {
        return new ToolResponse
        {
            Columns = columns,
            Rows = rows,
            Meta = meta
        };
    }
}
