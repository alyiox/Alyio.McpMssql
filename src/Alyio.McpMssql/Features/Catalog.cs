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
    /// Resource that describes columns of a relation (table or view).
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/columns",
        MimeType = "application/json")]
    [Description("Describe columns of a relation (table or view).")]
    public static Task<string> ColumnsAsync(
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
        => McpExecutor.RunAsTextAsync(ct => catalogService.DescribeColumnsAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Resource that describes indexes of a relation (table or view).
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/indexes",
        MimeType = "application/json")]
    [Description("Describe indexes of a relation (table or view).")]
    public static Task<string> IndexesAsync(
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
        => McpExecutor.RunAsTextAsync(ct => catalogService.DescribeIndexesAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Resource that describes constraints of a table (PK, UQ, FK, CHECK, DEFAULT). Tables only.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/constraints",
        MimeType = "application/json")]
    [Description("Describe constraints of a table (primary key, unique, foreign key, check, default).")]
    public static Task<string> ConstraintsAsync(
        ICatalogService catalogService,
        [Description("Profile name (e.g. default).")]
        string profile,
        [Description("Catalog (database) name.")]
        string catalog,
        [Description("Schema name.")]
        string schema,
        [Description("Table name.")]
        string name,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.DescribeConstraintsAsync(name, catalog, schema, profile, ct), cancellationToken);

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
        => McpExecutor.RunAsTextAsync(ct => catalogService.ListRoutinesAsync(catalog, schema, profile, null, ct), cancellationToken);

    /// <summary>
    /// Resource that returns the T-SQL definition of a routine (procedure or function). Tabular: one column "definition", one row when found.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/routines/{name}/definition",
        MimeType = "application/json")]
    [Description("Get routine definition (T-SQL body).")]
    public static Task<string> RoutineDefinitionAsync(
        ICatalogService catalogService,
        [Description("Profile name (e.g. default).")]
        string profile,
        [Description("Catalog (database) name.")]
        string catalog,
        [Description("Schema name.")]
        string schema,
        [Description("Routine name.")]
        string name,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.GetRoutineDefinitionAsync(name, catalog, schema, profile, ct), cancellationToken);

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
        [Description("Optional schema name. If omitted, uses the default schema of the caller.")]
        string? schema = null,
        [Description("Optional profile name. If omitted, the default profile is used.")]
        string? profile = null,
        [Description("Optional. When true, include system routines; when false or omitted, exclude them.")]
        bool? includeSystem = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.ListRoutinesAsync(catalog, schema, profile, includeSystem, ct), cancellationToken);

    /// <summary>
    /// Tool that gets the T-SQL definition of a routine (procedure or function). Tabular: one column "definition", one row when found.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description("Get routine definition (T-SQL body).")]
    public static Task<TabularResult> GetRoutineDefinitionAsync(
        ICatalogService catalogService,
        [Description("Routine name.")]
        string name,
        [Description("Optional catalog (database) name.")]
        string? catalog = null,
        [Description("Optional schema name.")]
        string? schema = null,
        [Description("Optional profile name. If omitted, the default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.GetRoutineDefinitionAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Tool that describes columns of a relation (table or view).
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description("Describe columns of a relation (table or view).")]
    public static Task<TabularResult> DescribeColumnsAsync(
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
        => McpExecutor.RunAsync(ct => catalogService.DescribeColumnsAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Tool that describes indexes of a relation (table or view).
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description("Describe indexes of a relation (table or view).")]
    public static Task<TabularResult> DescribeIndexesAsync(
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
        => McpExecutor.RunAsync(ct => catalogService.DescribeIndexesAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Tool that describes constraints of a table (PK, UQ, FK, CHECK, DEFAULT). Tables only.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description("Describe constraints of a table (primary key, unique, foreign key, check, default).")]
    public static Task<TableConstraints> DescribeConstraintsAsync(
        ICatalogService catalogService,
        [Description("Table name.")]
        string name,
        [Description("Optional catalog (database) name.")]
        string? catalog = null,
        [Description("Optional schema name.")]
        string? schema = null,
        [Description("Optional profile name. If omitted, the default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.DescribeConstraintsAsync(name, catalog, schema, profile, ct), cancellationToken);
}
