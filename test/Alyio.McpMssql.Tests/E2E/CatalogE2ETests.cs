// MIT License

using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;

namespace Alyio.McpMssql.Tests.E2E;

public sealed class CatalogE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private readonly McpClient _client = fixture.Client;

    private const string ListCatalogsTool = "list_catalogs";
    private const string ListSchemasTool = "list_schemas";
    private const string ListRelationsTool = "list_relations";
    private const string ListRoutinesTool = "list_routines";
    private const string DescribeColumnsTool = "describe_columns";
    private const string DescribeIndexesTool = "describe_indexes";
    private const string DescribeConstraintsTool = "describe_constraints";

    // ------------------------------------------------------------------
    // Tool discovery
    // ------------------------------------------------------------------

    [Fact]
    public async Task Catalog_Tools_Are_Discoverable()
    {
        Assert.True(await _client.IsToolRegisteredAsync(
            ListCatalogsTool,
            ListSchemasTool,
            ListRelationsTool,
            ListRoutinesTool,
            DescribeColumnsTool,
            DescribeIndexesTool,
            DescribeConstraintsTool));
    }

    // ------------------------------------------------------------------
    // Resource discovery
    // ------------------------------------------------------------------

    [Fact]
    public async Task All_Catalog_Resource_Templates_Are_Discoverable()
    {
        Assert.True(
            await _client.IsResourceTemplateRegisteredAsync(
                "mssql://{profile}/catalogs",
                "mssql://{profile}/catalogs/{catalog}/schemas",
                "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations",
                "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/columns",
                "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/indexes",
                "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/constraints",
                "mssql://{profile}/catalogs/{catalog}/schemas/{schema}/routines"),
            "All catalog resource templates should be discoverable.");
    }

    // ------------------------------------------------------------------
    // Tools
    // ------------------------------------------------------------------

    [Fact]
    public async Task ListCatalogs_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(ListCatalogsTool);

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "name",
            "state_desc",
            "is_read_only",
            "is_system_db");
    }

    [Fact]
    public async Task ListSchemas_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(
            ListSchemasTool,
            new Dictionary<string, object?>
            {
                ["catalog"] = "master"
            });

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns("name");
    }

    [Fact]
    public async Task ListRelations_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(
            ListRelationsTool,
            new Dictionary<string, object?>
            {
                ["catalog"] = "master",
                ["schema"] = "dbo"
            });

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "name",
            "schema");
    }

    [Fact]
    public async Task ListRoutines_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(
            ListRoutinesTool,
            new Dictionary<string, object?>
            {
                ["catalog"] = "master",
                ["schema"] = "dbo"
            });

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "schema",
            "name",
            "type");
    }

    [Fact]
    public async Task DescribeColumns_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(
            DescribeColumnsTool,
            new Dictionary<string, object?>
            {
                ["name"] = "sysobjects",
                ["catalog"] = "master",
                ["schema"] = "dbo"
            });

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "name",
            "type",
            "nullable",
            "position");
    }

    [Fact]
    public async Task DescribeIndexes_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(
            DescribeIndexesTool,
            new Dictionary<string, object?>
            {
                ["name"] = "Orders",
                ["catalog"] = "McpMssqlTest",
                ["schema"] = "dbo"
            });

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "index_name",
            "index_type",
            "is_unique",
            "is_disabled",
            "has_filter",
            "filter_definition",
            "key_ordinal",
            "is_descending",
            "column_name",
            "is_included_column");
    }

    // ------------------------------------------------------------------
    // Resources
    // ------------------------------------------------------------------

    [Fact]
    public async Task Catalogs_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync("mssql://default/catalogs");

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "name",
            "state_desc",
            "is_read_only",
            "is_system_db");
    }

    [Fact]
    public async Task Schemas_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync(
            "mssql://default/catalogs/master/schemas");

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns("name");
    }

    [Fact]
    public async Task Relations_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync(
            "mssql://default/catalogs/master/schemas/dbo/relations");

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "name",
            "schema");
    }

    [Fact]
    public async Task Routines_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync(
            "mssql://default/catalogs/master/schemas/dbo/routines");

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "schema",
            "name",
            "type");
    }

    [Fact]
    public async Task DescribeColumns_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync(
            "mssql://default/catalogs/master/schemas/dbo/relations/sysobjects/columns");

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "name",
            "type",
            "nullable",
            "position");
    }

    [Fact]
    public async Task DescribeIndexes_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync(
            "mssql://default/catalogs/McpMssqlTest/schemas/dbo/relations/Orders/indexes");

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "index_name",
            "index_type",
            "is_unique",
            "is_disabled",
            "has_filter",
            "filter_definition",
            "key_ordinal",
            "is_descending",
            "column_name",
            "is_included_column");
    }

    [Fact]
    public async Task DescribeConstraints_Tool_Returns_Expected_Structure()
    {
        var result = await _client.CallToolAsync(
            DescribeConstraintsTool,
            new Dictionary<string, object?>
            {
                ["name"] = "Orders",
                ["catalog"] = "McpMssqlTest",
                ["schema"] = "dbo"
            });

        var root = result.ReadJsonRoot();
        Assert.True(root.TryGetPropertyIgnoreCase("primary_keys", out var pk));
        Assert.True(pk.TryGetProperty("columns", out _));
        Assert.True(pk.TryGetProperty("rows", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("unique_constraints", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("foreign_keys", out var fk));
        Assert.True(fk.TryGetProperty("columns", out _));
        Assert.True(fk.TryGetProperty("rows", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("check_constraints", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("default_constraints", out _));
    }

    [Fact]
    public async Task DescribeConstraints_Resource_Returns_Expected_Structure()
    {
        var result = await _client.ReadResourceAsync(
            "mssql://default/catalogs/McpMssqlTest/schemas/dbo/relations/Orders/constraints");

        var root = result.ReadJsonRoot();
        Assert.True(root.TryGetPropertyIgnoreCase("primary_keys", out var pk));
        Assert.True(pk.TryGetProperty("columns", out _));
        Assert.True(pk.TryGetProperty("rows", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("unique_constraints", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("foreign_keys", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("check_constraints", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("default_constraints", out _));
    }
}
