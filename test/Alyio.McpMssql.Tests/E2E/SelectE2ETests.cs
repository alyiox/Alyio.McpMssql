// MIT License

using System.Text.Json;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Alyio.McpMssql.Tests.E2E;

public class SelectE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private const string ToolName = "select";
    private readonly McpClient _client = fixture.Client;

    [Fact]
    public async Task Select_Tool_Is_Discoverable()
    {
        Assert.True(await _client.IsToolRegisteredAsync(ToolName));
    }

    [Fact]
    public async Task Select_Returns_Tabular_Result()
    {
        var result = await CallSelectAsync("SELECT 1 AS Value");

        var root = result.ReadJsonRoot();
        var (columns, rows) = root.ReadColumnRows();

        Assert.Equal(1, columns.GetArrayLength());
        Assert.Equal("Value", columns[0].GetString());

        Assert.Equal(1, rows.GetArrayLength());
        Assert.True(rows[0][0].ValueKind is JsonValueKind.Number);
    }

    [Fact]
    public async Task Select_Supports_Parameters()
    {
        var result = await CallSelectAsync(
            "SELECT @value AS Value",
            parameters: new Dictionary<string, object?>
            {
                ["value"] = 42
            });

        var root = result.ReadJsonRoot();
        var (_, rows) = root.ReadColumnRows();

        Assert.Equal(1, rows.GetArrayLength());
        Assert.Equal(42, rows[0][0].GetInt32());
    }

    [Fact]
    public async Task Select_Respects_MaxRows()
    {
        var result = await CallSelectAsync(
            "SELECT name FROM sys.objects",
            maxRows: 5);

        var root = result.ReadJsonRoot();
        var (_, rows) = root.ReadColumnRows();

        Assert.True(rows.GetArrayLength() <= 5);
    }

    [Fact]
    public async Task Select_Can_Use_Explicit_Catalog()
    {
        var result = await CallSelectAsync(
            "SELECT DB_NAME() AS DbName",
            catalog: "master");

        var root = result.ReadJsonRoot();
        var (_, rows) = root.ReadColumnRows();

        Assert.Equal(1, rows.GetArrayLength());
        Assert.False(string.IsNullOrWhiteSpace(rows[0][0].GetString()));
    }

    [Fact]
    public async Task Select_Rejects_NonSelect_Statements()
    {
        var result = await CallSelectAsync("DELETE FROM sys.objects");

        Assert.True(result.IsError);

        var message = result.Content
            .OfType<TextContentBlock>()
            .FirstOrDefault()
            ?.Text;

        Assert.Contains("read-only", message, StringComparison.OrdinalIgnoreCase);
    }

    private ValueTask<CallToolResult> CallSelectAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object?>? parameters = null,
        int? maxRows = null)
    {
        var args = new Dictionary<string, object?>
        {
            ["sql"] = sql
        };

        if (catalog is not null)
            args["catalog"] = catalog;

        if (parameters is not null)
            args["parameters"] = parameters;

        if (maxRows is not null)
            args["maxRows"] = maxRows;

        return _client.CallToolAsync(ToolName, args);
    }
}

