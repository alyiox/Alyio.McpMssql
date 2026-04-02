// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// List or describe catalog metadata (catalogs, schemas, relations, routines).
/// </summary>
[McpServerToolType]
public static class ObjectTools
{
    /// <summary>
    /// List catalog metadata.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description(
        "[MSSQL] List catalog metadata. kind=catalog: databases; schema: schemas in a catalog; " +
        "relation: tables/views; routine: procedures/functions (system routines excluded by default). " +
        "catalog omitted → active catalog (ignored for kind=catalog). " +
        "schema: for relation omit → all schemas; for routine omit → caller default schema; ignored for catalog or schema kinds.")]
    public static async Task<TabularResult> ListObjectsAsync(
        ICatalogService catalogService,
        [Description("catalog | schema | relation | routine.")]
        ObjectKind kind,
        [Description("If omitted or empty, uses the default profile. Src: profiles.")]
        string? profile = null,
        [Description("If omitted, uses the active catalog (not used when kind=catalog). Src: catalogs.")]
        string? catalog = null,
        [Description("Omission depends on kind; see tool description. Src: schemas.")]
        string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return await McpExecutor.RunAsync(async ct =>
        {
            return kind switch
            {
                ObjectKind.Catalog => await catalogService.ListCatalogsAsync(profile, ct).ConfigureAwait(false),
                ObjectKind.Schema => await catalogService.ListSchemasAsync(catalog, profile, ct).ConfigureAwait(false),
                ObjectKind.Relation => await catalogService.ListRelationsAsync(catalog, schema, profile, ct).ConfigureAwait(false),
                ObjectKind.Routine => await catalogService.ListRoutinesAsync(catalog, schema, profile, null, ct).ConfigureAwait(false),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Invalid object kind."),
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get metadata for one relation or routine.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description(
        "[MSSQL] Get metadata for one relation or routine. " +
        "Use list_objects to resolve names. If includes is null or empty, returns empty detail payloads.")]
    public static async Task<ObjectResult> GetObjectAsync(
        ICatalogService catalogService,
        [Description("relation | routine.")]
        ObjectKind kind,
        [Description("Unqualified or schema-qualified name; use schema param when needed. Src: relations or routines.")]
        string name,
        [Description("If omitted or empty, uses the default profile. Src: profiles.")]
        string? profile = null,
        [Description("If omitted, uses the active catalog. Src: catalogs.")]
        string? catalog = null,
        [Description("If omitted, uses default schema resolution for the object. Src: schemas.")]
        string? schema = null,
        [Description("columns, indexes, constraints (relations only), definition (routines only).")]
        IReadOnlyList<ObjectInclude>? includes = null,
        CancellationToken cancellationToken = default)
    {
        if (kind is not ObjectKind.Relation and not ObjectKind.Routine)
        {
            throw new McpException("kind must be relation or routine for get_object.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("name is required.");
        }

        if (includes is not { Count: > 0 })
        {
            return new ObjectResult();
        }

        return await McpExecutor.RunAsync(async ct =>
        {
            var includeSet = includes.ToHashSet();
            TabularResult? columns = null;
            TabularResult? indexes = null;
            TableConstraints? constraints = null;
            TabularResult? definition = null;

            if (includeSet.Contains(ObjectInclude.Columns))
            {
                columns = await catalogService.DescribeColumnsAsync(name, catalog, schema, profile, ct).ConfigureAwait(false);
            }

            if (includeSet.Contains(ObjectInclude.Indexes))
            {
                indexes = await catalogService.DescribeIndexesAsync(name, catalog, schema, profile, ct).ConfigureAwait(false);
            }

            if (includeSet.Contains(ObjectInclude.Constraints) && kind == ObjectKind.Relation)
            {
                constraints = await catalogService.DescribeConstraintsAsync(name, catalog, schema, profile, ct).ConfigureAwait(false);
            }

            if (includeSet.Contains(ObjectInclude.Definition) && kind == ObjectKind.Routine)
            {
                definition = await catalogService.GetRoutineDefinitionAsync(name, catalog, schema, profile, ct).ConfigureAwait(false);
            }

            return new ObjectResult
            {
                Columns = columns,
                Indexes = indexes,
                Constraints = constraints,
                Definition = definition,
            };
        }, cancellationToken).ConfigureAwait(false);
    }
}
