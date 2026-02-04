// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Represents column metadata in a query result.
/// </summary>
public class Column
{
    /// <summary>
    /// The column name.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The zero-based ordinal position.
    /// </summary>
    public required int Ordinal { get; init; }
    
    /// <summary>
    /// The data type name (e.g., "int", "varchar", "datetime").
    /// Optional - only included when relevant.
    /// </summary>
    public string? DataTypeName { get; init; }
    
    /// <summary>
    /// Whether the column allows NULL values.
    /// Optional - only included when relevant.
    /// </summary>
    public bool? AllowDbNull { get; init; }
}
