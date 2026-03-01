// MIT License

using System.Text.Json;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Alyio.McpMssql.Tests.E2E;

public class QueryToolsE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private const string RunQueryTool = "run_query";
    private const string AnalyzeQueryTool = "analyze_query";
    private const string AnalyzableSql = "SELECT TOP 1 name, object_id FROM sys.objects WHERE type = 'U'";
    private readonly McpClient _client = fixture.Client;

    // ── run_query ─────────────────────────────────────────────────

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

    // ── analyze_query ─────────────────────────────────────────────

    [Fact]
    public async Task AnalyzeQuery_Tool_Is_Discoverable()
    {
        Assert.True(await _client.IsToolRegisteredAsync(AnalyzeQueryTool));
    }

    [Fact]
    public async Task AnalyzeQuery_Returns_Summary_With_PlanUri()
    {
        var result = await CallAnalyzeQueryAsync(AnalyzableSql);

        Assert.True(result.IsError is not true);

        var root = result.ReadJsonRoot();
        Assert.True(root.TryGetProperty("plan_uri", out var planUri));
        Assert.StartsWith("mssql://plans/", planUri.GetString());

        Assert.True(root.TryGetProperty("statement", out var stmt));
        Assert.True(stmt.GetProperty("estimated_cost").GetDouble() > 0);
    }

    [Fact]
    public async Task AnalyzeQuery_Estimated_Omits_Actuals()
    {
        var result = await CallAnalyzeQueryAsync(AnalyzableSql, estimated: true);

        Assert.True(result.IsError is not true);

        var root = result.ReadJsonRoot();
        Assert.True(root.TryGetProperty("plan_uri", out _));

        var stmt = root.GetProperty("statement");
        Assert.False(stmt.TryGetProperty("cpu_time_ms", out _));
        Assert.False(stmt.TryGetProperty("elapsed_time_ms", out _));
    }

    [Fact]
    public async Task AnalyzeQuery_Rejects_NonSelect_Statements()
    {
        var result = await CallAnalyzeQueryAsync("DELETE FROM sys.objects");

        Assert.True(result.IsError);

        var message = result.Content
            .OfType<TextContentBlock>()
            .FirstOrDefault()
            ?.Text;

        Assert.Contains("read-only", message, StringComparison.OrdinalIgnoreCase);
    }

    // ── helpers ───────────────────────────────────────────────────

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

    private ValueTask<CallToolResult> CallAnalyzeQueryAsync(
        string sql,
        bool estimated = false,
        string? catalog = null)
    {
        var args = new Dictionary<string, object?>
        {
            ["sql"] = sql,
        };

        if (estimated)
            args["estimated"] = true;

        if (catalog is not null)
            args["catalog"] = catalog;

        return _client.CallToolAsync(AnalyzeQueryTool, args);
    }
}
