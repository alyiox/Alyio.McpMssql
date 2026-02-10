// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes SQL Server catalog metadata as MCP tools and resources.
/// Supports discovery of catalogs, schemas, relations, and routines.
/// </summary>
[McpServerToolType]
[McpServerResourceType]
public static class Catalogs
{
    // =========================================================
    // Resources (hierarchical, addressable)
    // =========================================================

    /// <summary>
    /// Resource that returns all catalogs (databases).
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs",
        MimeType = "application/json")]
    [Description("List catalogs (databases).")]
    public static Task<string> CatalogsAsync(
        ICatalogService catalogService,
        [Description("Profile name (e.g. default).")]
        string profile,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.ListCatalogsAsync(profile, ct), cancellationToken);

    /// <summary>
    /// Resource that returns schemas within a catalog.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas",
        MimeType = "application/json")]
    [Description("List schemas in a catalog.")]
    public static Task<string> SchemasAsync(
        ICatalogService catalogService,
        [Description("Profile name (e.g. default).")]
        string profile,
        [Description("Catalog (database) name.")]
        string catalog,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.ListSchemasAsync(catalog, profile, ct), cancellationToken);

    /// <summary>
    /// Resource that returns relations within a schema.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations",
        MimeType = "application/json")]
    [Description("List relations (tables and views) in a schema.")]
    public static Task<string> RelationsAsync(
        ICatalogService catalogService,
        [Description("Profile name (e.g. default).")]
        string profile,
        [Description("Catalog (database) name.")]
        string catalog,
        [Description("Schema name.")]
        string schema,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.ListRelationsAsync(catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Resource that describes a relation (table or view).
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}",
        MimeType = "application/json")]
    [Description("Describe a relation (table or view).")]
    public static Task<string> RelationAsync(
        ICatalogService catalogService,
        [Description("Profile name (e.g. default).")]
        string profile,
        [Description("Catalog (database) name.")]
        string catalog,
        [Description("Schema name.")]
        string schema,
        [Description("Relation name.")]
        string name,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.DescribeRelationAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Resource that returns routines within a schema.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/routines",
        MimeType = "application/json")]
    [Description("List routines (procedures and functions) in a schema.")]
    public static Task<string> RoutinesAsync(
        ICatalogService catalogService,
        [Description("Profile name (e.g. default).")]
        string profile,
        [Description("Catalog (database) name.")]
        string catalog,
        [Description("Schema name.")]
        string schema,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.ListRoutinesAsync(catalog, schema, profile, ct), cancellationToken);

    // =========================================================
    // Tools (imperative, parameter-driven)
    // =========================================================

    /// <summary>
    /// Tool that lists all catalogs.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description("List catalogs (databases).")]
    public static Task<TabularResult> ListCatalogsAsync(
        ICatalogService catalogService,
        [Description("Optional profile name. If omitted, the default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.ListCatalogsAsync(profile, ct), cancellationToken);

    /// <summary>
    /// Tool that lists schemas.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description("List schemas.")]
    public static Task<TabularResult> ListSchemasAsync(
        ICatalogService catalogService,
        [Description("Optional catalog (database) name.")]
        string? catalog = null,
        [Description("Optional profile name. If omitted, the default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.ListSchemasAsync(catalog, profile, ct), cancellationToken);

    /// <summary>
    /// Tool that lists relations.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description("List relations (tables and views).")]
    public static Task<TabularResult> ListRelationsAsync(
        ICatalogService catalogService,
        [Description("Optional catalog (database) name.")]
        string? catalog = null,
        [Description("Optional schema name.")]
        string? schema = null,
        [Description("Optional profile name. If omitted, the default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.ListRelationsAsync(catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Tool that lists routines.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description("List routines (procedures and functions).")]
    public static Task<TabularResult> ListRoutinesAsync(
        ICatalogService catalogService,
        [Description("Optional catalog (database) name.")]
        string? catalog = null,
        [Description("Optional schema name.")]
        string? schema = null,
        [Description("Optional profile name. If omitted, the default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.ListRoutinesAsync(catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Tool that describes a relation.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description("Describe a relation (table or view).")]
    public static Task<TabularResult> DescribeRelationAsync(
        ICatalogService catalogService,
        [Description("Relation name.")]
        string name,
        [Description("Optional catalog (database) name.")]
        string? catalog = null,
        [Description("Optional schema name.")]
        string? schema = null,
        [Description("Optional profile name. If omitted, the default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.DescribeRelationAsync(name, catalog, schema, profile, ct), cancellationToken);
}
