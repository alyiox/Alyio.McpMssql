// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// A missing index suggestion embedded in the execution plan by
/// the SQL Server optimizer. Directly translatable to a
/// <c>CREATE NONCLUSTERED INDEX</c> statement.
/// </summary>
public sealed class MissingIndex
{
    /// <summary>
    /// Target table in <c>[schema].[table]</c> format.
    /// </summary>
    public required string Table { get; init; }

    /// <summary>
    /// Columns used in equality predicates (<c>=</c>).
    /// These become the leading key columns of the suggested index.
    /// </summary>
    public required IReadOnlyList<string> EqualityColumns { get; init; }

    /// <summary>
    /// Columns used in inequality predicates (<c>&lt;</c>, <c>&gt;</c>,
    /// <c>BETWEEN</c>, etc.). These follow the equality columns
    /// in the index key.
    /// </summary>
    public required IReadOnlyList<string> InequalityColumns { get; init; }

    /// <summary>
    /// Columns needed for covering (selected but not filtered on).
    /// These become <c>INCLUDE</c> columns in the index definition.
    /// </summary>
    public required IReadOnlyList<string> IncludeColumns { get; init; }

    /// <summary>
    /// Estimated performance improvement as a percentage (0–100).
    /// Higher values indicate greater expected benefit.
    /// </summary>
    public double Impact { get; init; }
}
