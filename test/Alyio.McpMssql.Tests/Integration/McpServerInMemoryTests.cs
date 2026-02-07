// MIT License

using System.Text.Json;
using Alyio.McpMssql.Models;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Alyio.McpMssql.Integration.Tests;

/// <summary>
/// Tests for MCP server using shared fixture for performance.
/// All tests use the same server instance (read-only operations).
/// </summary>
public sealed class McpServerInMemoryTests(McpServerFixture fixture) : IClassFixture<SqlServerFixture>, IClassFixture<McpServerFixture>
{
    private const string TestDatabaseName = "McpMssqlTest";
    private readonly McpClient _client = fixture.Client;

    // ---------- Discoveries ----------
    [Fact]
    public async Task ListTools_IncludesExpectedTools()
    {
        var tools = await _client.ListToolsAsync();
        var toolNames = tools.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("select", toolNames);
        Assert.Contains("list_databases", toolNames);
        Assert.Contains("list_schemas", toolNames);
        Assert.Contains("list_tables", toolNames);
        Assert.Contains("list_views", toolNames);
        Assert.Contains("list_procedures", toolNames);
        Assert.Contains("list_functions", toolNames);
        Assert.Contains("describe_table", toolNames);
    }

    [Fact]
    public async Task ListResources_IncludesExpectedResources()
    {
        var resources = await _client.ListResourcesAsync();
        var resourceUris = resources.Select(r => r.Uri).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("mssql://databases", resourceUris);
    }

    [Fact]
    public async Task ListResourceTemplates_IncludesExpectedTemplates()
    {
        var resources = await _client.ListResourceTemplatesAsync();
        var resourceUris = resources.Select(r => r.UriTemplate).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("mssql://schemas{?database}", resourceUris);
        Assert.Contains("mssql://tables{?database,schema}", resourceUris);
        Assert.Contains("mssql://views{?database,schema}", resourceUris);
        Assert.Contains("mssql://procedures{?database,schema}", resourceUris);
        Assert.Contains("mssql://functions{?database,schema}", resourceUris);
        Assert.Contains("mssql://tables/{table}/columns{?database,schema}", resourceUris);
    }

    // ---------- Tools ----------
    [Fact]
    public async Task ListDatabases_ReturnsExpectedTestDatabase()
    {
        var result = await _client.CallToolAsync("list_databases", new Dictionary<string, object?>());

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains(TestDatabaseName, json);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("McpMssqlTest", null)]
    [InlineData("McpMssqlTest", "dbo")]
    public async Task ListSchemas_ReturnsDboSchema(string? database, string? schema)
    {
        var parameters = new Dictionary<string, object?>();
        if (database is not null) parameters["database"] = database;
        if (schema is not null) parameters["schema"] = schema;

        var result = await _client.CallToolAsync("list_schemas", parameters);
        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("dbo", json);
    }

    [Theory]
    //[InlineData(null, null)]
    [InlineData("McpMssqlTest", null)]
    [InlineData("McpMssqlTest", "dbo")]
    public async Task ListTables_ReturnsValidJsonAndExpectedTables(string? database, string? schema)
    {
        var parameters = new Dictionary<string, object?>();
        if (database is not null) parameters["database"] = database;
        if (schema is not null) parameters["schema"] = schema;

        var result = await _client.CallToolAsync("list_tables", parameters);

        var json = result.Content.OfType<TextContentBlock>().First().Text;
        Assert.Contains("Users", json);
        Assert.Contains("Orders", json);
    }

    [Theory]
    //[InlineData(null, null)]
    [InlineData("McpMssqlTest", null)]
    [InlineData("McpMssqlTest", "dbo")]
    public async Task ListViews_ReturnsValidJsonAndExpectedViews(string? database, string? schema)
    {
        var parameters = new Dictionary<string, object?>();
        if (database is not null) parameters["database"] = database;
        if (schema is not null) parameters["schema"] = schema;

        var result = await _client.CallToolAsync("list_views", parameters);

        var json = result.Content.OfType<TextContentBlock>().First().Text;
        Assert.Contains("ActiveUsers", json);
        Assert.Contains("OrderSummary", json);
    }

    [Theory]
    //[InlineData(null, null)]
    [InlineData("McpMssqlTest", null)]
    [InlineData("McpMssqlTest", "dbo")]
    public async Task ListProcedures_ReturnsValidJsonAndExpectedProcedures(string? database, string? schema)
    {
        var parameters = new Dictionary<string, object?>();
        if (database is not null) parameters["database"] = database;
        if (schema is not null) parameters["schema"] = schema;

        var result = await _client.CallToolAsync("list_procedures", parameters);

        var json = result.Content.OfType<TextContentBlock>().First().Text;
        Assert.Contains("GetUserCount", json);
        Assert.Contains("GetUserById", json);
    }

    [Theory]
    //[InlineData(null, null)]
    [InlineData("McpMssqlTest", null)]
    [InlineData("McpMssqlTest", "dbo")]
    public async Task ListFunctions_ReturnsValidJsonAndExpectedFunctions(string? database, string? schema)
    {
        var parameters = new Dictionary<string, object?>();
        if (database is not null) parameters["database"] = database;
        if (schema is not null) parameters["schema"] = schema;

        var result = await _client.CallToolAsync("list_functions", parameters);

        var json = result.Content.OfType<TextContentBlock>().First().Text;
        Assert.Contains("GetUserEmail", json);
        Assert.Contains("GetTotalOrderAmount", json);
    }

    [Fact]
    public async Task DescribeTable_MissingTableParameter_ReturnsError()
    {
        var result = await _client.CallToolAsync(
            "describe_table",
            new Dictionary<string, object?>
            {
                ["table"] = "", // or null
            },
            null);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task DescribeTable_NonExistentTable_ReturnsEmpty()
    {
        var result = await _client.CallToolAsync(
            "describe_table",
            new Dictionary<string, object?>
            {
                ["table"] = "NonExistentTable_12345",
                ["database"] = TestDatabaseName
            },
            null);

        var response = result.ReadAsJson<ToolResponse>();

        Assert.NotNull(response);
        Assert.Empty(response.Rows);
    }

    [Fact]
    public async Task DescribeTable_NonExistentDatabase_ReturnsError()
    {
        var result = await _client.CallToolAsync(
            "describe_table",
            new Dictionary<string, object?>
            {
                ["table"] = "Users",
                ["database"] = "NonExistentDB_12345"
            },
            null);

        Assert.True(result.IsError);
    }

    [Theory]
    [InlineData("Users", "McpMssqlTest", null)]
    [InlineData("Users", "McpMssqlTest", "dbo")]
    public async Task DescribeTable_ReturnsCorrectColumns(string table, string? database, string? schema)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["table"] = table,
            ["database"] = database,
        };
        if (schema is not null) parameters["schema"] = schema;

        var result = await _client.CallToolAsync("describe_table", parameters);
        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("UserId", json);
        Assert.Contains("UserName", json);
        Assert.Contains("Email", json);
        Assert.Contains("CreatedDate", json);
        Assert.Contains("int", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("nvarchar", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("datetime", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DescribeView_ReturnsColumns()
    {
        var result = await _client.CallToolAsync(
            "describe_table", // Assuming describe_table works for views
            new Dictionary<string, object?>
            {
                ["table"] = "ActiveUsers",
                ["database"] = TestDatabaseName,
                ["schema"] = "dbo"
            },
            null);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("UserId", json);
        Assert.Contains("UserName", json);
        Assert.Contains("Email", json);
    }

    [Theory]
    [InlineData("update dbo.Users set Name = 'x'")]
    [InlineData("INSERT INTO dbo.t (id) VALUES (1)")]
    [InlineData("SELECT 1; DELETE FROM dbo.t")]
    [InlineData("DELETE FROM dbo.Users")]
    [InlineData("")]
    [InlineData("EXEC dbo.GetUserCount")]
    public async Task Query_RejectedSql_ReturnsError(string sql)
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = sql,
            },
            null);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Query_SelectAllUsers_ReturnsSeededData()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT UserId, UserName, Email FROM dbo.Users ORDER BY UserId",
                ["database"] = TestDatabaseName,
                ["maxRows"] = 10
            },
            null);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("Alice", json);
        Assert.Contains("alice@test.com", json);
        Assert.Contains("Bob", json);
        Assert.Contains("bob@test.com", json);
        Assert.Contains("Charlie", json);
        Assert.Contains("charlie@test.com", json);
        Assert.Contains("Diana", json);
        Assert.Contains("Eve", json);
    }

    [Fact]
    public async Task Query_WithParameterFilter_ReturnsFilteredUser()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT UserId, UserName, Email FROM dbo.Users WHERE UserId = @userId",
                ["database"] = TestDatabaseName,
                ["parametersJson"] = "{\"userId\": 1}",
                ["maxRows"] = 10
            },
            null);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("Alice", json);
        Assert.DoesNotContain("Bob", json);
        Assert.DoesNotContain("Charlie", json);
    }

    [Fact]
    public async Task Query_SelectFromView_ReturnsActiveUsers()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT UserId, UserName FROM dbo.ActiveUsers ORDER BY UserId",
                ["database"] = TestDatabaseName,
                ["maxRows"] = 10
            },
            null);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("Alice", json);
        Assert.Contains("Bob", json);
        Assert.Contains("Charlie", json);
    }

    [Fact]
    public async Task Query_JoinTablesWithAggregation_ReturnsCorrectResults()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = @"
                    SELECT 
                        u.UserName,
                        COUNT(o.OrderId) AS OrderCount
                    FROM dbo.Users u
                    LEFT JOIN dbo.Orders o ON u.UserId = o.UserId
                    GROUP BY u.UserName
                    ORDER BY u.UserName",
                ["database"] = TestDatabaseName,
            },
            null);

        var json = result.Content.OfType<TextContentBlock>().First().Text;
        var doc = JsonDocument.Parse(json);
        var rows = doc.RootElement.GetProperty("rows").EnumerateArray().ToDictionary(
            row => row[0].GetString()!,
            row => row[1].GetInt32());

        Assert.Equal(2, rows["Alice"]);
        Assert.Equal(1, rows["Bob"]);
        Assert.Equal(3, rows["Charlie"]);
        Assert.Equal(0, rows["Eve"]);
    }

    [Fact]
    public async Task Query_WithMaxRows_TruncatesResults()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT UserId, UserName FROM dbo.Users ORDER BY UserId",
                ["database"] = TestDatabaseName,
                ["maxRows"] = 2
            },
            null);

        var json = result.Content.OfType<TextContentBlock>().First().Text;
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.GetProperty("meta").GetProperty("truncated").GetBoolean());
        Assert.Equal(2, doc.RootElement.GetProperty("rows").GetArrayLength());
        Assert.Contains("Alice", json);
        Assert.Contains("Bob", json);
        Assert.DoesNotContain("Charlie", json);
    }

    //[Fact]
    //public async Task Query_DataTypes_HandlesVariousTypes()
    //{
    //    var result = await _client.CallToolAsync(
    //        "select",
    //        new Dictionary<string, object?>
    //        {
    //            ["sql"] = "SELECT TOP 1 * FROM dbo.DataTypes",
    //            ["database"] = TestDatabaseName,
    //        },
    //        null);

    //    var json = result.Content.OfType<TextContentBlock>().First().Text;

    //    var doc = JsonDocument.Parse(json);
    //    var columns = doc.RootElement.GetProperty("columns").EnumerateArray().ToList();
    //    Assert.Contains(columns, c => c.GetProperty("name").GetString() == "c_int" && c.GetProperty("dataTypeName").GetString() == "int");
    //    Assert.Contains(columns, c => c.GetProperty("name").GetString() == "c_varchar" && c.GetProperty("dataTypeName").GetString() == "varchar");
    //    Assert.Contains(columns, c => c.GetProperty("name").GetString() == "c_datetime" && c.GetProperty("dataTypeName").GetString() == "datetime");
    //}

    [Fact]
    public async Task Query_InvalidParametersJson_ReturnsError()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT * FROM dbo.Users WHERE UserId = @id",
                ["database"] = TestDatabaseName,
                ["parametersJson"] = "{\"id\": 1" // Invalid JSON
            },
            null);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Query_EmptyResultSet_ReturnsValidJson()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT * FROM dbo.Users WHERE 1 = 0",
                ["database"] = TestDatabaseName,
            },
            null);

        var json = result.Content.OfType<TextContentBlock>().First().Text;
        var doc = JsonDocument.Parse(json);
        Assert.Equal(0, doc.RootElement.GetProperty("rows").GetArrayLength());
        Assert.True(doc.RootElement.TryGetProperty("columns", out _));
    }

    [Fact]
    public async Task Query_InvalidDatabase_ReturnsError()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT 1",
                ["database"] = "NonExistentDB_12345"
            },
            null);

        Assert.True(result.IsError);
    }

    // ---------- Resources ----------
    [Fact]
    public async Task Resource_Databases_ReturnsValidContent()
    {
        var result = await _client.ReadResourceAsync(new Uri("mssql://databases"));

        Assert.NotNull(result);
        Assert.NotNull(result.Contents);

        if (result.Contents[0] is TextResourceContents textContent)
        {
            Assert.Contains(TestDatabaseName, textContent.Text);
        }
        else
        {
            Assert.Fail("Resource did not return text content.");
        }
    }

    //[Theory]
    ////[InlineData(null, null)]
    //[InlineData("McpMssqlTest", null)]
    //[InlineData("McpMssqlTest", "dbo")]
    //public async Task Resource_Schemas_ReturnsValidContent(string? database, string? schema)
    //{
    //    var uri = "mssql://schemas" + BuildQueryString(new { database, schema });
    //    var result = await _client.ReadResourceAsync(new Uri(uri));

    //    Assert.NotNull(result);
    //    Assert.NotNull(result.Contents);

    //    if (result.Contents[0] is TextResourceContents textContent)
    //    {
    //        Assert.Contains("dbo", textContent.Text);
    //    }
    //    else
    //    {
    //        Assert.Fail("Resource did not return text content.");
    //    }
    //}

    //[Theory]
    //[InlineData(null, null)]
    //[InlineData("McpMssqlTest", null)]
    //[InlineData("McpMssqlTest", "dbo")]
    //public async Task Resource_Tables_ReturnsValidContent(string? database, string? schema)
    //{
    //    var uri = "mssql://tables" + BuildQueryString(new { database, schema });
    //    var result = await _client.ReadResourceAsync(new Uri(uri));

    //    Assert.NotNull(result);
    //    Assert.NotNull(result.Contents);

    //    if (result.Contents[0] is TextResourceContents textContent)
    //    {
    //        Assert.Contains("Users", textContent.Text);
    //        Assert.Contains("Orders", textContent.Text);
    //    }
    //    else
    //    {
    //        Assert.Fail("Resource did not return text content.");
    //    }
    //}

    //[Theory]
    ////[InlineData(null, null)]
    //[InlineData("McpMssqlTest", null)]
    //[InlineData("McpMssqlTest", "dbo")]
    //public async Task Resource_Views_ReturnsValidContent(string? database, string? schema)
    //{
    //    var uri = "mssql://views" + BuildQueryString(new { database, schema });
    //    var result = await _client.ReadResourceAsync(new Uri(uri));

    //    Assert.NotNull(result);
    //    Assert.NotNull(result.Contents);

    //    if (result.Contents[0] is TextResourceContents textContent)
    //    {
    //        Assert.Contains("ActiveUsers", textContent.Text);
    //        Assert.Contains("OrderSummary", textContent.Text);
    //    }
    //    else
    //    {
    //        Assert.Fail("Resource did not return text content.");
    //    }
    //}

    //[Theory]
    ////[InlineData(null, null)]
    //[InlineData("McpMssqlTest", null)]
    //[InlineData("McpMssqlTest", "dbo")]
    //public async Task Resource_Procedures_ReturnsValidContent(string? database, string? schema)
    //{
    //    var uri = "mssql://procedures" + BuildQueryString(new { database, schema });
    //    var result = await _client.ReadResourceAsync(new Uri(uri));

    //    Assert.NotNull(result);
    //    Assert.NotNull(result.Contents);

    //    if (result.Contents[0] is TextResourceContents textContent)
    //    {
    //        Assert.Contains("GetUserCount", textContent.Text);
    //        Assert.Contains("GetUserById", textContent.Text);
    //    }
    //    else
    //    {
    //        Assert.Fail("Resource did not return text content.");
    //    }
    //}

    //[Theory]
    ////[InlineData(null, null)]
    //[InlineData("McpMssqlTest", null)]
    //[InlineData("McpMssqlTest", "dbo")]
    //public async Task Resource_Functions_ReturnsValidContent(string? database, string? schema)
    //{
    //    var uri = "mssql://functions" + BuildQueryString(new { database, schema });
    //    var result = await _client.ReadResourceAsync(new Uri(uri));

    //    Assert.NotNull(result);
    //    Assert.NotNull(result.Contents);

    //    if (result.Contents[0] is TextResourceContents textContent)
    //    {
    //        Assert.Contains("GetUserEmail", textContent.Text);
    //        Assert.Contains("GetTotalOrderAmount", textContent.Text);
    //    }
    //    else
    //    {
    //        Assert.Fail("Resource did not return text content.");
    //    }
    //}

    private static string BuildQueryString(object? obj)
    {
        if (obj is null)
        {
            return string.Empty;
        }

        var props = obj.GetType().GetProperties()
            .Where(p => p.GetValue(obj) is not null)
            .Select(p => $"{p.Name.ToLowerInvariant()}={Uri.EscapeDataString(p.GetValue(obj)!.ToString()!)}");

        return props.Any() ? $"?{string.Join("&", props)}" : "";
    }
}
