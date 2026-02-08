// MIT License

using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class CatalogTests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private readonly McpClient _client = fixture.Client;

    private const string ListCatalogsTool = "list_catalogs";
    private const string ListSchemasTool = "list_schemas";
    private const string ListRelationsTool = "list_relations";
    private const string ListRoutinesTool = "list_routines";
    private const string DescribeRelationTool = "describe_relation";

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
            DescribeRelationTool));
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
    public async Task DescribeRelation_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(
            DescribeRelationTool,
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
            "ordinal");
    }

    // ------------------------------------------------------------------
    // Resources
    // ------------------------------------------------------------------

    [Fact]
    public async Task Catalogs_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync("mssql://catalogs");

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
            "mssql://catalogs/master/schemas");

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns("name");
    }

    [Fact]
    public async Task Relations_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync(
            "mssql://catalogs/master/schemas/dbo/relations");

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
            "mssql://catalogs/master/schemas/dbo/routines");

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "schema",
            "name",
            "type");
    }

    [Fact]
    public async Task DescribeRelation_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync(
            "mssql://catalogs/master/schemas/dbo/relations/sysobjects");

        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();

        columns.AssertHasColumns(
            "name",
            "type",
            "nullable",
            "ordinal");
    }
}
