// MIT License

using System.Text.Json.Serialization;

namespace Alyio.McpMssql.Models;

/// <summary>
/// Scope for db.objects and db.object: catalog (databases), schema (namespaces), relation (tables/views), or routine (procedures/functions).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ObjectKind
{
    /// <summary>List catalogs (databases).</summary>
    Catalog,

    /// <summary>List schemas within a catalog.</summary>
    Schema,

    /// <summary>List relations (tables and views) within a schema.</summary>
    Relation,

    /// <summary>List routines (procedures and functions) within a schema.</summary>
    Routine,
}
