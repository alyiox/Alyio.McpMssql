// MIT License

using Alyio.McpMssql.Tests.Fixtures;
using ModelContextProtocol.Protocol;

namespace Alyio.McpMssql.Tests;

/// <summary>
/// Tests for MCP server using shared fixture for performance.
/// All tests use the same server instance (read-only operations).
/// </summary>
public sealed class McpServerInMemoryTests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    [Fact]
    public async Task ListTools_IncludesExpectedTools()
    {
        var tools = await fixture.Harness.Client.ListToolsAsync();
        var toolNames = tools.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("ping", toolNames);
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
    public async Task Ping_ReturnsVersionInfo()
    {
        var result = await fixture.Harness.Client.CallToolAsync("ping", new Dictionary<string, object?>(), cancellationToken: CancellationToken.None);
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        // Should return JSON with version column and SQL Server version info
        Assert.Contains("\"columns\"", text);
        Assert.Contains("\"rows\"", text);
        Assert.Contains("version", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Query_NonSelect_IsRejected()
    {
        var result = await fixture.Harness.Client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "update dbo.Users set Name = 'x'",
                ["maxRows"] = 1,
            },
            cancellationToken: CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Query_InsertKeyword_IsRejected()
    {
        var result = await fixture.Harness.Client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "INSERT INTO dbo.t (id) VALUES (1)",
                ["maxRows"] = 1,
            },
            cancellationToken: CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Query_MultipleStatements_IsRejected()
    {
        var result = await fixture.Harness.Client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT 1; DELETE FROM dbo.t",
                ["maxRows"] = 1,
            },
            cancellationToken: CancellationToken.None);

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task ListViews_ReturnsJsonFormat()
    {
        var result = await fixture.Harness.Client.CallToolAsync("list_views", new Dictionary<string, object?>(), cancellationToken: CancellationToken.None);
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        // Should return JSON with columns and rows
        Assert.Contains("\"columns\"", text);
        Assert.Contains("\"rows\"", text);
    }

    [Fact]
    public async Task ListProcedures_ReturnsJsonFormat()
    {
        var result = await fixture.Harness.Client.CallToolAsync("list_procedures", new Dictionary<string, object?>(), cancellationToken: CancellationToken.None);
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        // Should return JSON with columns and rows
        Assert.Contains("\"columns\"", text);
        Assert.Contains("\"rows\"", text);
    }

    [Fact]
    public async Task ListFunctions_ReturnsJsonFormat()
    {
        var result = await fixture.Harness.Client.CallToolAsync("list_functions", new Dictionary<string, object?>(), cancellationToken: CancellationToken.None);
        var text = result.Content.OfType<TextContentBlock>().First().Text;

        // Should return JSON with columns and rows
        Assert.Contains("\"columns\"", text);
        Assert.Contains("\"rows\"", text);
    }

    [Fact]
    public async Task DescribeTable_MissingTableParameter_ReturnsError()
    {
        var result = await fixture.Harness.Client.CallToolAsync(
            "describe_table",
            new Dictionary<string, object?>
            {
                ["table"] = "",
            },
            cancellationToken: CancellationToken.None);

        Assert.True(result.IsError);
    }
}
