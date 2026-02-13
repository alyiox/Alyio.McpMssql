// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes Microsoft SQL Server / Azure SQL Database catalog metadata as
/// MCP tools and resources for this server. Discovery of catalogs, schemas,
/// relations, and routines; scoped to this MCP server's profiles only.
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
    [Description("List catalogs (databases) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server's profile only.")]
    public static Task<string> CatalogsAsync(
        ICatalogService catalogService,
        [Description("Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource.")]
        string profile,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.ListCatalogsAsync(profile, ct), cancellationToken);

    /// <summary>
    /// Resource that returns schemas within a catalog.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas",
        MimeType = "application/json")]
    [Description("List schemas in a catalog for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server's profile only.")]
    public static Task<string> SchemasAsync(
        ICatalogService catalogService,
        [Description("Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource.")]
        string profile,
        [Description("Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string catalog,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.ListSchemasAsync(catalog, profile, ct), cancellationToken);

    /// <summary>
    /// Resource that returns relations within a schema.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations",
        MimeType = "application/json")]
    [Description("List relations (tables and views) in a schema for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<string> RelationsAsync(
        ICatalogService catalogService,
        [Description("Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource.")]
        string profile,
        [Description("Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string catalog,
        [Description("Schema name. Valid values from this server's list_schemas or the schemas resource.")]
        string schema,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.ListRelationsAsync(catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Resource that describes columns of a relation (table or view).
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/columns",
        MimeType = "application/json")]
    [Description("Describe columns of a relation (table or view) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<string> ColumnsAsync(
        ICatalogService catalogService,
        [Description("Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource.")]
        string profile,
        [Description("Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string catalog,
        [Description("Schema name. Valid values from this server's list_schemas or the schemas resource.")]
        string schema,
        [Description("Relation (table or view) name. Valid values from this server's list_relations tool or the relations resource.")]
        string name,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.DescribeColumnsAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Resource that describes indexes of a relation (table or view).
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/indexes",
        MimeType = "application/json")]
    [Description("Describe indexes of a relation (table or view) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<string> IndexesAsync(
        ICatalogService catalogService,
        [Description("Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource.")]
        string profile,
        [Description("Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string catalog,
        [Description("Schema name. Valid values from this server's list_schemas or the schemas resource.")]
        string schema,
        [Description("Relation (table or view) name. Valid values from this server's list_relations tool or the relations resource.")]
        string name,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.DescribeIndexesAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Resource that describes constraints of a table (PK, UQ, FK, CHECK, DEFAULT). Tables only.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/constraints",
        MimeType = "application/json")]
    [Description("Describe constraints of a table (PK, unique, foreign key, check, default) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<string> ConstraintsAsync(
        ICatalogService catalogService,
        [Description("Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource.")]
        string profile,
        [Description("Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string catalog,
        [Description("Schema name. Valid values from this server's list_schemas or the schemas resource.")]
        string schema,
        [Description("Table name. Valid values from this server's list_relations tool or the relations resource.")]
        string name,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.DescribeConstraintsAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Resource that returns routines within a schema.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/routines",
        MimeType = "application/json")]
    [Description("List routines (procedures and functions) in a schema for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<string> RoutinesAsync(
        ICatalogService catalogService,
        [Description("Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource.")]
        string profile,
        [Description("Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string catalog,
        [Description("Schema name. Valid values from this server's list_schemas or the schemas resource.")]
        string schema,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.ListRoutinesAsync(catalog, schema, profile, null, ct), cancellationToken);

    /// <summary>
    /// Resource that returns the T-SQL definition of a routine (procedure or function). Tabular: one column "definition", one row when found.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/routines/{name}/definition",
        MimeType = "application/json")]
    [Description("Get T-SQL routine definition (procedure or function body) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<string> RoutineDefinitionAsync(
        ICatalogService catalogService,
        [Description("Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource.")]
        string profile,
        [Description("Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string catalog,
        [Description("Schema name. Valid values from this server's list_schemas or the schemas resource.")]
        string schema,
        [Description("Routine (procedure or function) name. Valid values from this server's list_routines tool or the routines resource.")]
        string name,
        CancellationToken cancellationToken)
        => McpExecutor.RunAsTextAsync(ct => catalogService.GetRoutineDefinitionAsync(name, catalog, schema, profile, ct), cancellationToken);

    // =========================================================
    // Tools (imperative, parameter-driven)
    // =========================================================

    /// <summary>
    /// Tool that lists all catalogs.
    /// </summary>
    [McpServerTool]
    [Description("List catalogs (databases) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server's profile only.")]
    public static Task<TabularResult> ListCatalogsAsync(
        ICatalogService catalogService,
        [Description("Optional. Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource. If omitted, default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.ListCatalogsAsync(profile, ct), cancellationToken);

    /// <summary>
    /// Tool that lists schemas.
    /// </summary>
    [McpServerTool]
    [Description("List schemas for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<TabularResult> ListSchemasAsync(
        ICatalogService catalogService,
        [Description("Optional. Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string? catalog = null,
        [Description("Optional. Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource. If omitted, default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.ListSchemasAsync(catalog, profile, ct), cancellationToken);

    /// <summary>
    /// Tool that lists relations.
    /// </summary>
    [McpServerTool]
    [Description("List relations (tables and views) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<TabularResult> ListRelationsAsync(
        ICatalogService catalogService,
        [Description("Optional. Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string? catalog = null,
        [Description("Optional. Schema name. Valid values from this server's list_schemas or the schemas resource.")]
        string? schema = null,
        [Description("Optional. Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource. If omitted, default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.ListRelationsAsync(catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Tool that lists routines.
    /// </summary>
    [McpServerTool]
    [Description("List routines (procedures and functions) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<TabularResult> ListRoutinesAsync(
        ICatalogService catalogService,
        [Description("Optional. Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string? catalog = null,
        [Description("Optional. Schema name. Valid values from this server's list_schemas or the schemas resource. If omitted, uses the default schema of the caller.")]
        string? schema = null,
        [Description("Optional. Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource. If omitted, default profile is used.")]
        string? profile = null,
        [Description("Optional. When true, include system routines; when false or omitted, exclude them.")]
        bool? includeSystem = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.ListRoutinesAsync(catalog, schema, profile, includeSystem, ct), cancellationToken);

    /// <summary>
    /// Tool that gets the T-SQL definition of a routine (procedure or function). Tabular: one column "definition", one row when found.
    /// </summary>
    [McpServerTool]
    [Description("Get T-SQL routine definition (procedure or function body) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<TabularResult> GetRoutineDefinitionAsync(
        ICatalogService catalogService,
        [Description("Routine (procedure or function) name. Valid values from this server's list_routines tool or the routines resource.")]
        string name,
        [Description("Optional. Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string? catalog = null,
        [Description("Optional. Schema name. Valid values from this server's list_schemas or the schemas resource.")]
        string? schema = null,
        [Description("Optional. Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource. If omitted, default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.GetRoutineDefinitionAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Tool that describes columns of a relation (table or view).
    /// </summary>
    [McpServerTool]
    [Description("Describe columns of a relation (table or view) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<TabularResult> DescribeColumnsAsync(
        ICatalogService catalogService,
        [Description("Relation (table or view) name. Valid values from this server's list_relations tool or the relations resource.")]
        string name,
        [Description("Optional. Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string? catalog = null,
        [Description("Optional. Schema name. Valid values from this server's list_schemas or the schemas resource.")]
        string? schema = null,
        [Description("Optional. Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource. If omitted, default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.DescribeColumnsAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Tool that describes indexes of a relation (table or view).
    /// </summary>
    [McpServerTool]
    [Description("Describe indexes of a relation (table or view) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<TabularResult> DescribeIndexesAsync(
        ICatalogService catalogService,
        [Description("Relation (table or view) name. Valid values from this server's list_relations tool or the relations resource.")]
        string name,
        [Description("Optional. Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string? catalog = null,
        [Description("Optional. Schema name. Valid values from this server's list_schemas or the schemas resource.")]
        string? schema = null,
        [Description("Optional. Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource. If omitted, default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.DescribeIndexesAsync(name, catalog, schema, profile, ct), cancellationToken);

    /// <summary>
    /// Tool that describes constraints of a table (PK, UQ, FK, CHECK, DEFAULT). Tables only.
    /// </summary>
    [McpServerTool]
    [Description("Describe constraints of a table (primary key, unique, foreign key, check, default) for Microsoft SQL Server / Azure SQL Database. Scoped to this MCP server only.")]
    public static Task<TableConstraints> DescribeConstraintsAsync(
        ICatalogService catalogService,
        [Description("Table name. Valid values from this server's list_relations tool or the relations resource.")]
        string name,
        [Description("Optional. Catalog (database) name. Valid values from this server's list_catalogs or the catalogs resource.")]
        string? catalog = null,
        [Description("Optional. Schema name. Valid values from this server's list_schemas or the schemas resource.")]
        string? schema = null,
        [Description("Optional. Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource. If omitted, default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
        => McpExecutor.RunAsync(ct => catalogService.DescribeConstraintsAsync(name, catalog, schema, profile, ct), cancellationToken);
}
