// MIT License

using System.ComponentModel;
using Alyio.McpMssql;
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
    [McpServerTool]
    [Description("[MSSQL] List catalog metadata (catalogs, schemas, relations, routines).")]
    public static async Task<TabularResult> ListObjectsAsync(
        ICatalogService catalogService,
        [Description("Kind: catalog | schema | relation | routine.")]
        ObjectKind kind,
        [Description("Optional. Profile name. Src: profiles.")]
        string? profile = null,
        [Description("Optional. Catalog (database) name. Src: catalogs.")]
        string? catalog = null,
        [Description("Optional. Schema name. Src: schemas.")]
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
    [McpServerTool]
    [Description("[MSSQL] Get metadata for one relation or routine (columns, indexes, constraints, definition).")]
    public static async Task<ObjectResult> GetObjectAsync(
        ICatalogService catalogService,
        [Description("Kind: relation | routine.")]
        ObjectKind kind,
        [Description("Relation or routine name. Src: relations or routines.")]
        string name,
        [Description("Optional. Profile name. Src: profiles.")]
        string? profile = null,
        [Description("Optional. Catalog (database) name. Src: catalogs.")]
        string? catalog = null,
        [Description("Optional. Schema name. Src: schemas.")]
        string? schema = null,
        [Description("Optional. Includes: columns, indexes, constraints (relation), definition (routine).")]
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
