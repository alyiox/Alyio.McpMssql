// MIT License

using System.Text.Json;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;

namespace Alyio.McpMssql.Tests.E2E;

public sealed class ObjectE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private readonly McpClient _client = fixture.Client;

    private const string ObjectsToolName = "list_objects";
    private const string ObjectToolName = "get_object";
    private static readonly string[] s_includeColumns = ["columns"];
    private static readonly string[] s_includeIndexes = ["indexes"];
    private static readonly string[] s_includeConstraints = ["constraints"];
    private static readonly string[] s_includeDefinition = ["definition"];

    // ── Tool discovery ──

    [Fact]
    public async Task Object_Tools_Are_Discoverable()
    {
        Assert.True(await _client.IsToolRegisteredAsync(ObjectsToolName, ObjectToolName));
    }

    // ── Resource discovery ──

    [Fact]
    public async Task Object_Resource_Templates_Are_Discoverable()
    {
        Assert.True(
            await _client.IsResourceTemplateRegisteredAsync(
                "mssql://objects/{kind}{?profile,catalog,schema}",
                "mssql://objects/{kind}/{name}{?profile,catalog,schema,includes}"),
            "Object resource templates should be discoverable.");
    }

    // ── list_objects ──

    [Fact]
    public async Task ListCatalogs_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(ObjectsToolName, new Dictionary<string, object?> { ["kind"] = "catalog" });
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name", "state_desc", "is_read_only", "is_system_db");
    }

    [Fact]
    public async Task ListSchemas_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(ObjectsToolName, new Dictionary<string, object?>
        {
            ["kind"] = "schema",
            ["catalog"] = "master"
        });
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name");
    }

    [Fact]
    public async Task ListRelations_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(ObjectsToolName, new Dictionary<string, object?>
        {
            ["kind"] = "relation",
            ["catalog"] = "master",
            ["schema"] = "dbo"
        });
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name", "type");
    }

    [Fact]
    public async Task ListRoutines_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(ObjectsToolName, new Dictionary<string, object?>
        {
            ["kind"] = "routine",
            ["catalog"] = "master",
            ["schema"] = "dbo"
        });
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name", "type");
    }

    // ── get_object ──

    [Fact]
    public async Task DescribeColumns_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(ObjectToolName, new Dictionary<string, object?>
        {
            ["kind"] = "relation",
            ["catalog"] = "master",
            ["schema"] = "dbo",
            ["name"] = "sysobjects",
            ["includes"] = s_includeColumns
        });
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRowsFrom("columns");
        columns.AssertHasColumns("name", "type", "is_nullable", "column_id");
    }

    [Fact]
    public async Task DescribeIndexes_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(ObjectToolName, new Dictionary<string, object?>
        {
            ["kind"] = "relation",
            ["catalog"] = "master",
            ["schema"] = "dbo",
            ["name"] = "sysobjects",
            ["includes"] = s_includeIndexes
        });
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRowsFrom("indexes");
        columns.AssertHasColumns(
            "index_name", "index_type", "is_unique", "is_disabled", "has_filter",
            "filter_definition", "key_ordinal", "is_descending", "column_name", "is_included_column");
    }

    [Fact]
    public async Task DescribeConstraints_Tool_Returns_Expected_Structure()
    {
        var result = await _client.CallToolAsync(ObjectToolName, new Dictionary<string, object?>
        {
            ["kind"] = "relation",
            ["catalog"] = "master",
            ["schema"] = "dbo",
            ["name"] = "sysobjects",
            ["includes"] = s_includeConstraints
        });
        var root = result.ReadJsonRoot();
        Assert.True(root.TryGetProperty("constraints", out var constraints));
        Assert.True(constraints.TryGetProperty("primary_keys", out var pk));
        Assert.True(pk.TryGetProperty("columns", out _));
        Assert.True(pk.TryGetProperty("rows", out _));
        Assert.True(constraints.TryGetProperty("unique_constraints", out _));
        Assert.True(constraints.TryGetProperty("foreign_keys", out _));
        Assert.True(constraints.TryGetProperty("check_constraints", out _));
        Assert.True(constraints.TryGetProperty("default_constraints", out _));
    }

    [Fact]
    public async Task GetRoutineDefinition_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(ObjectToolName, new Dictionary<string, object?>
        {
            ["kind"] = "routine",
            ["catalog"] = "master",
            ["schema"] = "dbo",
            ["name"] = "sp_who",
            ["includes"] = s_includeDefinition
        });
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRowsFrom("definition");
        columns.AssertHasColumns("definition");
    }

    // ── mssql://objects (resource list) ──

    [Theory]
    [InlineData("mssql://objects/catalog")]
    [InlineData("mssql://objects/catalog?profile=default")]
    public async Task Catalogs_Resource_Returns_Expected_Columns(string uri)
    {
        var result = await _client.ReadResourceAsync(uri);
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name", "state_desc", "is_read_only", "is_system_db");
    }

    [Theory]
    [InlineData("mssql://objects/schema")]
    [InlineData("mssql://objects/schema?catalog=master")]
    [InlineData("mssql://objects/schema?profile=default&catalog=master")]
    public async Task Schemas_Resource_Returns_Expected_Columns(string uri)
    {
        var result = await _client.ReadResourceAsync(uri);
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name");
    }

    [Theory]
    [InlineData("mssql://objects/relation")]
    [InlineData("mssql://objects/relation?catalog=master")]
    [InlineData("mssql://objects/relation?catalog=master&schema=dbo")]
    [InlineData("mssql://objects/relation?profile=default&catalog=master&schema=dbo")]
    public async Task Relations_Resource_Returns_Expected_Columns(string uri)
    {
        var result = await _client.ReadResourceAsync(uri);
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name", "type");
    }

    [Theory]
    [InlineData("mssql://objects/routine")]
    [InlineData("mssql://objects/routine?catalog=master")]
    [InlineData("mssql://objects/routine?catalog=master&schema=dbo")]
    [InlineData("mssql://objects/routine?profile=default&catalog=master&schema=dbo")]
    public async Task Routines_Resource_Returns_Expected_Columns(string uri)
    {
        var result = await _client.ReadResourceAsync(uri);
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name", "type");
    }

    // ── mssql://objects/{kind}/{name} (resource get) ──

    [Theory]
    [InlineData("mssql://objects/relation/objects?schema=sys&includes=columns")]
    [InlineData("mssql://objects/relation/objects?profile=default&schema=sys&includes=columns")]
    [InlineData("mssql://objects/relation/objects?catalog=master&schema=sys&includes=columns")]
    public async Task Object_Resource_Columns_Returns_Expected_Structure(string uri)
    {
        var result = await _client.ReadResourceAsync(uri);
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRowsFrom("columns");
        columns.AssertHasColumns("name", "type", "is_nullable", "column_id");
    }

    [Theory]
    [InlineData("mssql://objects/relation/objects?catalog=master&schema=sys&includes=columns,indexes")]
    public async Task Object_Resource_Multiple_Includes_Returns_Expected_Structure(string uri)
    {
        var result = await _client.ReadResourceAsync(uri);
        var root = result.ReadJsonRoot();

        var (columns, _) = root.ReadColumnRowsFrom("columns");
        columns.AssertHasColumns("name", "type", "is_nullable", "column_id");

        var (indexCols, _) = root.ReadColumnRowsFrom("indexes");
        indexCols.AssertHasColumns(
            "index_name", "index_type", "is_unique", "is_disabled", "has_filter",
            "filter_definition", "key_ordinal", "is_descending", "column_name", "is_included_column");

        Assert.False(root.TryGetProperty("constraints", out _));
        Assert.False(root.TryGetProperty("definition", out _));
    }

    [Theory]
    [InlineData("mssql://objects/routine/sp_who?catalog=master&schema=dbo&includes=definition")]
    public async Task Object_Resource_Routine_Definition_Returns_Expected_Structure(string uri)
    {
        var result = await _client.ReadResourceAsync(uri);
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRowsFrom("definition");
        columns.AssertHasColumns("definition");
    }

    [Fact]
    public async Task Object_Resource_Without_Includes_Throws()
    {
        await Assert.ThrowsAnyAsync<Exception>(
            async () => await _client.ReadResourceAsync("mssql://objects/relation/objects?schema=sys"));
    }
}
