// MIT License

using System.Text.Json;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;

namespace Alyio.McpMssql.Tests.E2E;

public sealed class ObjectE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private readonly McpClient _client = fixture.Client;

    private const string ObjectsToolName = "db.objects";
    private const string ObjectToolName = "db.object";
    private static readonly string[] IncludeColumns = ["columns"];
    private static readonly string[] IncludeIndexes = ["indexes"];
    private static readonly string[] IncludeConstraints = ["constraints"];
    private static readonly string[] IncludeDefinition = ["definition"];

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
                "mssql://objects?{kind,profile,catalog,schema}",
                "mssql://object?{kind,name,profile,catalog,schema,includes}"),
            "Object resource templates should be discoverable.");
    }

    // ── db.objects (list) ──

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

    // ── db.object (detail) ──

    [Fact]
    public async Task DescribeColumns_Tool_Returns_Expected_Columns()
    {
        var result = await _client.CallToolAsync(ObjectToolName, new Dictionary<string, object?>
        {
            ["kind"] = "relation",
            ["catalog"] = "master",
            ["schema"] = "dbo",
            ["name"] = "sysobjects",
            ["includes"] = IncludeColumns
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
            ["catalog"] = "McpMssqlTest",
            ["schema"] = "dbo",
            ["name"] = "Orders",
            ["includes"] = IncludeIndexes
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
            ["catalog"] = "McpMssqlTest",
            ["schema"] = "dbo",
            ["name"] = "Orders",
            ["includes"] = IncludeConstraints
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
            ["includes"] = IncludeDefinition
        });
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRowsFrom("definition");
        columns.AssertHasColumns("definition");
    }

    // ── mssql://objects (resource list) ──

    [Fact(Skip = "URI template matching needs investigation – SDK may require {?var} (RFC 6570 query expansion) instead of ?{var}.")]
    public async Task Catalogs_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync("mssql://objects?kind=catalog");
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name", "state_desc", "is_read_only", "is_system_db");
    }

    [Fact(Skip = "URI template matching needs investigation – SDK may require {?var} (RFC 6570 query expansion) instead of ?{var}.")]
    public async Task Schemas_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync("mssql://objects?kind=schema&catalog=master");
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name");
    }

    [Fact(Skip = "URI template matching needs investigation – SDK may require {?var} (RFC 6570 query expansion) instead of ?{var}.")]
    public async Task Relations_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync("mssql://objects?kind=relation&catalog=master&schema=dbo");
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name", "type");
    }

    [Fact(Skip = "URI template matching needs investigation – SDK may require {?var} (RFC 6570 query expansion) instead of ?{var}.")]
    public async Task Routines_Resource_Returns_Expected_Columns()
    {
        var result = await _client.ReadResourceAsync("mssql://objects?kind=routine&catalog=master&schema=dbo");
        var root = result.ReadJsonRoot();
        var (columns, _) = root.ReadColumnRows();
        columns.AssertHasColumns("name", "type");
    }
}
