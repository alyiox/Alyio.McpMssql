// MIT License

using System.Text.Json.Serialization;

namespace Alyio.McpMssql.Models;

/// <summary>
/// What to include when db.object is used for a single relation or routine: columns, indexes, constraints, or T-SQL definition.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ObjectInclude
{
    /// <summary>Column metadata for the relation.</summary>
    Columns,

    /// <summary>Index metadata for the relation.</summary>
    Indexes,

    /// <summary>Constraints (PK, UQ, FK, CHECK, DEFAULT) for the table. Tables only.</summary>
    Constraints,

    /// <summary>T-SQL routine body (procedure or function). Routine only.</summary>
    Definition,
}
