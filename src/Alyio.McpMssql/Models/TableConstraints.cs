// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Contains the five constraint result sets for a table (PK, UQ, FK, CHECK, DEFAULT).
/// </summary>
public sealed class TableConstraints
{
    /// <summary>Primary key columns.</summary>
    public required TabularResult PrimaryKeys { get; init; }

    /// <summary>Unique constraint columns.</summary>
    public required TabularResult UniqueConstraints { get; init; }

    /// <summary>Foreign key columns and referenced table/column.</summary>
    public required TabularResult ForeignKeys { get; init; }

    /// <summary>Check constraint definitions.</summary>
    public required TabularResult CheckConstraints { get; init; }

    /// <summary>Default constraint definitions.</summary>
    public required TabularResult DefaultConstraints { get; init; }
}
