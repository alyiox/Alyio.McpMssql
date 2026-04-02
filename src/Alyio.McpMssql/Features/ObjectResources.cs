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
    [Description(
        "[MSSQL] List catalog metadata. Path {kind}: catalog | schema | relation | routine. " +
        "Query: profile (Src: profiles) defaults when omitted; catalog (Src: catalogs) → active catalog when omitted, ignored for kind=catalog; " +
        "schema (Src: schemas) omit → all schemas for relation, caller default for routine, ignored for catalog/schema.")]
    public static async Task<string> ListAsync(
        ICatalogService catalogService,
        [Description("catalog | schema | relation | routine.")]
        string kind,
        [Description("If omitted or empty, uses the default profile. Src: profiles.")]
        string? profile = null,
        [Description("If omitted, uses the active catalog (not used when kind=catalog). Src: catalogs.")]
        string? catalog = null,
        [Description("Omit → all schemas for relation, caller default for routine; ignored for catalog/schema. Src: schemas.")]
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
    [Description(
        "[MSSQL] Get metadata for one relation or routine. Path {kind}: relation | routine; {name}: object name. " +
        "Query includes (required): comma-separated columns, indexes, constraints (relations), definition (routines). " +
        "Profile/catalog/schema omission matches get_object tool.")]
    public static async Task<string> GetAsync(
        ICatalogService catalogService,
        [Description("relation | routine.")]
        string kind,
        [Description("Object name; use schema query param when needed. Src: relations or routines.")]
        string name,
        [Description("If omitted or empty, uses the default profile. Src: profiles.")]
        string? profile = null,
        [Description("If omitted, uses the active catalog. Src: catalogs.")]
        string? catalog = null,
        [Description("If omitted, uses default schema resolution. Src: schemas.")]
        string? schema = null,
        [Description("Required. Comma-separated: columns, indexes, constraints (relations), definition (routines).")]
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
