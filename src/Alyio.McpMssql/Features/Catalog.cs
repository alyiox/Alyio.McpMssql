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
        UriTemplate = "mssql://catalogs",
        MimeType = "application/json")]
    [Description("List catalogs (databases).")]
    public static Task<string> CatalogsAsync(
        ICatalogService catalogService,
        CancellationToken cancellationToken)
        => MssqlExecutor.RunAsTextAsync(catalogService.ListCatalogsAsync, cancellationToken);

    /// <summary>
    /// Resource that returns schemas within a catalog.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://catalogs/{catalog}/schemas",
        MimeType = "application/json")]
    [Description("List schemas in a catalog.")]
    public static Task<string> SchemasAsync(
        ICatalogService catalogService,
        [Description("Catalog (database) name.")]
        string catalog,
        CancellationToken cancellationToken)
        => MssqlExecutor.RunAsTextAsync(ct => catalogService.ListSchemasAsync(catalog, ct), cancellationToken);

    /// <summary>
    /// Resource that returns relations within a schema.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://catalogs/{catalog}/schemas/{schema}/relations",
        MimeType = "application/json")]
    [Description("List relations (tables and views) in a schema.")]
    public static Task<string> RelationsAsync(
        ICatalogService catalogService,
        [Description("Catalog (database) name.")]
        string catalog,
        [Description("Schema name.")]
        string schema,
        CancellationToken cancellationToken)
        => MssqlExecutor.RunAsTextAsync(ct => catalogService.ListRelationsAsync(catalog, schema, ct), cancellationToken);

    /// <summary>
    /// Resource that describes a relation (table or view).
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://catalogs/{catalog}/schemas/{schema}/relations/{name}",
        MimeType = "application/json")]
    [Description("Describe a relation (table or view).")]
    public static Task<string> RelationAsync(
        ICatalogService catalogService,
        [Description("Catalog (database) name.")]
        string catalog,
        [Description("Schema name.")]
        string schema,
        [Description("Relation name.")]
        string name,
        CancellationToken cancellationToken)
        => MssqlExecutor.RunAsTextAsync(ct => catalogService.DescribeRelationAsync(name, catalog, schema, ct), cancellationToken);

    /// <summary>
    /// Resource that returns routines within a schema.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://catalogs/{catalog}/schemas/{schema}/routines",
        MimeType = "application/json")]
    [Description("List routines (procedures and functions) in a schema.")]
    public static Task<string> RoutinesAsync(
        ICatalogService catalogService,
        [Description("Catalog (database) name.")]
        string catalog,
        [Description("Schema name.")]
        string schema,
        CancellationToken cancellationToken)
        => MssqlExecutor.RunAsTextAsync(ct => catalogService.ListRoutinesAsync(catalog, schema, ct), cancellationToken);

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
        CancellationToken cancellationToken = default)
        => MssqlExecutor.RunAsync(catalogService.ListCatalogsAsync, cancellationToken);

    /// <summary>
    /// Tool that lists schemas.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description("List schemas.")]
    public static Task<TabularResult> ListSchemasAsync(
        ICatalogService catalogService,
        [Description("Optional catalog (database) name.")]
        string? catalog = null,
        CancellationToken cancellationToken = default)
        => MssqlExecutor.RunAsync(ct => catalogService.ListSchemasAsync(catalog, ct), cancellationToken);

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
        CancellationToken cancellationToken = default)
        => MssqlExecutor.RunAsync(ct => catalogService.ListRelationsAsync(catalog, schema, ct), cancellationToken);

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
        CancellationToken cancellationToken = default)
        => MssqlExecutor.RunAsync(ct => catalogService.ListRoutinesAsync(catalog, schema, ct), cancellationToken);

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
        CancellationToken cancellationToken = default)
        => MssqlExecutor.RunAsync(ct => catalogService.DescribeRelationAsync(name, catalog, schema, ct), cancellationToken);
}
