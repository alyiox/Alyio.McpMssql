// MIT License

using System.ComponentModel;
using Alyio.McpMssql;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Resources for catalog metadata: list objects (mssql://objects) or one object detail (mssql://object). Mirror db.objects and db.object tools.
/// </summary>
[McpServerResourceType]
public static class ObjectResources
{
    /// <summary>
    /// List catalog metadata.
    /// </summary>
    [McpServerResource(
        Name = "objects",
        UriTemplate = "mssql://objects?{kind,profile,catalog,schema}",
        MimeType = "application/json")]
    [Description("[MSSQL] List catalog metadata (catalogs, schemas, relations, routines).")]
    public static async Task<string> ListAsync(
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
        return await McpExecutor.RunAsTextAsync(async ct =>
            await ObjectTools.ListObjectsAsync(
                catalogService,
                kind,
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
        UriTemplate = "mssql://object?{kind,name,profile,catalog,schema,includes}",
        MimeType = "application/json")]
    [Description("[MSSQL] Get metadata for one relation or routine (columns, indexes, constraints, definition).")]
    public static async Task<string> GetAsync(
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
        return await McpExecutor.RunAsTextAsync(async ct =>
            await ObjectTools.GetObjectAsync(
                catalogService,
                kind,
                name,
                profile,
                catalog,
                schema,
                includes,
                ct).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
    }
}
