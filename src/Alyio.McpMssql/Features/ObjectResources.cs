// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Resources for catalog metadata: list objects (mssql://objects) or one object detail (mssql://object). Mirror list_objects and get_object tools.
/// </summary>
[McpServerResourceType]
public static class ObjectResources
{
    /// <summary>
    /// List catalog metadata.
    /// </summary>
    [McpServerResource(
        Name = "objects",
        UriTemplate = "mssql://objects/{kind}{?profile,catalog,schema}",
        MimeType = "application/json")]
    [Description("[MSSQL] List catalog metadata. Path {kind}: catalog | schema | relation | routine. Query params: profile (Src: profiles), catalog (Src: catalogs), schema (Src: schemas) — all optional.")]
    public static async Task<string> ListAsync(
        ICatalogService catalogService,
        [Description("Kind: catalog | schema | relation | routine.")]
        string kind,
        [Description("Optional. Profile name. Src: profiles.")]
        string? profile = null,
        [Description("Optional. Catalog (database) name. Src: catalogs.")]
        string? catalog = null,
        [Description("Optional. Schema name. Src: schemas.")]
        string? schema = null,
        CancellationToken cancellationToken = default)
    {
        var target = kind.ToLowerInvariant() switch
        {
            "catalog" => ObjectKind.Catalog,
            "schema" => ObjectKind.Schema,
            "relation" => ObjectKind.Relation,
            "routine" => ObjectKind.Routine,
            _ => throw new ArgumentException($"Invalid kind: {kind}. Must be one of: catalog, schema, relation, routine.", nameof(kind))
        };

        return await McpExecutor.RunAsTextAsync(async ct =>
            await ObjectTools.ListObjectsAsync(
                catalogService,
                target,
                profile,
                catalog,
                schema,
                ct).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get metadata for one relation or routine.
    /// </summary>
    [McpServerResource(
        Name = "object",
        UriTemplate = "mssql://objects/{kind}/{name}{?profile,catalog,schema,includes}",
        MimeType = "application/json")]
    [Description("[MSSQL] Get metadata for one relation or routine. Path {kind}: relation | routine. Path {name}: object name. Query params: profile (Src: profiles), catalog (Src: catalogs), schema (Src: schemas) — all optional. includes (required, comma-separated): columns, indexes, constraints (relation) or definition (routine).")]
    public static async Task<string> GetAsync(
        ICatalogService catalogService,
        [Description("Kind: relation | routine.")]
        string kind,
        [Description("Relation or routine name. Src: relations or routines.")]
        string name,
        [Description("Optional. Profile name. Src: profiles.")]
        string? profile = null,
        [Description("Optional. Catalog (database) name. Src: catalogs.")]
        string? catalog = null,
        [Description("Optional. Schema name. Src: schemas.")]
        string? schema = null,
        [Description("Required. Comma-separated includes: columns, indexes, constraints (relation), definition (routine).")]
        string? includes = null,
        CancellationToken cancellationToken = default)
    {
        var target = kind.ToLowerInvariant() switch
        {
            "catalog" => ObjectKind.Catalog,
            "schema" => ObjectKind.Schema,
            "relation" => ObjectKind.Relation,
            "routine" => ObjectKind.Routine,
            _ => throw new ArgumentException($"Invalid kind: {kind}. Must be one of: catalog, schema, relation, routine.", nameof(kind))
        };

        var includeList = new List<ObjectInclude>();
        foreach (var include in (includes ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var includeEnum = include.ToLowerInvariant() switch
            {
                "columns" => ObjectInclude.Columns,
                "indexes" => ObjectInclude.Indexes,
                "constraints" => ObjectInclude.Constraints,
                "definition" => ObjectInclude.Definition,
                _ => throw new ArgumentException($"Invalid include: {include}. Must be one of: columns, indexes, constraints, definition.", nameof(includes))
            };
            includeList.Add(includeEnum);
        }

        if (includeList.Count == 0)
        {
            throw new ArgumentException($"At least one include must be specified in includes: {includes}. Must be one of: columns, indexes, constraints, definition.", nameof(includes));
        }

        return await McpExecutor.RunAsTextAsync(async ct =>
            await ObjectTools.GetObjectAsync(
                catalogService,
                target,
                name,
                profile,
                catalog,
                schema,
                includeList,
                ct).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
    }
}
