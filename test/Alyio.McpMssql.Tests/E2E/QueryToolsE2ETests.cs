// MIT License

using System.Text.Json;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Alyio.McpMssql.Tests.E2E;

public class QueryToolsE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private const string RunQueryTool = "run_query";
    private readonly McpClient _client = fixture.Client;

    [Fact]
    public async Task RunQuery_Tool_Is_Discoverable()
    {
        Assert.True(await _client.IsToolRegisteredAsync(RunQueryTool));
    }

    [Fact]
    public async Task RunQuery_Returns_Tabular_Result()
    {
        var result = await CallRunQueryAsync("SELECT 1 AS Value");

        var root = result.ReadJsonRoot();
        var (columns, rows) = root.ReadColumnRows();

        Assert.Equal(1, columns.GetArrayLength());
        Assert.Equal("Value", columns[0].GetString());

        Assert.Equal(1, rows.GetArrayLength());
        Assert.True(rows[0][0].ValueKind is JsonValueKind.Number);
    }

    [Fact]
    public async Task RunQuery_Supports_Parameters()
    {
        var result = await CallRunQueryAsync(
            "SELECT @value AS Value",
            parameters: new Dictionary<string, object?>
            {
                ["value"] = 42,
            });

        var root = result.ReadJsonRoot();
        var (_, rows) = root.ReadColumnRows();

        Assert.Equal(1, rows.GetArrayLength());
        Assert.Equal(42, rows[0][0].GetInt32());
    }

    [Fact]
    public async Task RunQuery_Can_Use_Explicit_Catalog()
    {
        var result = await CallRunQueryAsync(
            "SELECT DB_NAME() AS DbName",
            catalog: "master");

        var root = result.ReadJsonRoot();
        var (_, rows) = root.ReadColumnRows();

        Assert.Equal(1, rows.GetArrayLength());
        Assert.False(string.IsNullOrWhiteSpace(rows[0][0].GetString()));
    }

    [Fact]
    public async Task RunQuery_Rejects_NonSelect_Statements()
    {
        var result = await CallRunQueryAsync("DELETE FROM sys.objects");

        Assert.True(result.IsError);

        var message = result.Content
            .OfType<TextContentBlock>()
            .FirstOrDefault()
            ?.Text;

        Assert.Contains("read-only", message, StringComparison.OrdinalIgnoreCase);
    }

    private ValueTask<CallToolResult> CallRunQueryAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object?>? parameters = null)
    {
        var args = new Dictionary<string, object?>
        {
            ["sql"] = sql,
        };

        if (catalog is not null)
            args["catalog"] = catalog;

        if (parameters is not null)
            args["parameters"] = parameters;

        return _client.CallToolAsync(RunQueryTool, args);
    }
}
