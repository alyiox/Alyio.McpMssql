// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Response shape for get_object (single object detail).
/// Identity plus optional detail parts (columns, indexes, constraints, definition) per include request.
/// </summary>
public sealed class ObjectResult
{
    /// <summary>Column metadata. Present when include requested columns.</summary>
    public TabularResult? Columns { get; init; }

    /// <summary>Index metadata. Present when include requested indexes.</summary>
    public TabularResult? Indexes { get; init; }

    /// <summary>Table constraints (PK, UQ, FK, CHECK, DEFAULT). Present when include requested constraints; relation only.</summary>
    public TableConstraints? Constraints { get; init; }

    /// <summary>T-SQL routine body. Present when include requested definition; routine only.</summary>
    public TabularResult? Definition { get; init; }
}
