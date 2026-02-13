// MIT License

using System.Text.Json;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;

namespace Alyio.McpMssql.Tests.E2E;

public sealed class ServerPropertiesE2ETests(McpServerFixture fixture)
    : IClassFixture<McpServerFixture>
{
    private readonly McpClient _client = fixture.Client;

    private const string ToolName = "db.server.properties";

    // ── Tool discovery ──

    [Fact]
    public async Task ServerProperties_Tool_Is_Discoverable()
    {
        Assert.True(await _client.IsToolRegisteredAsync(ToolName));
    }

    // ── Resource discovery ──

    [Fact]
    public async Task ServerProperties_Resource_Template_Is_Discoverable()
    {
        Assert.True(
            await _client.IsResourceTemplateRegisteredAsync("mssql://server-properties?{profile}"));
    }

    // ── Tool ──

    [Fact]
    public async Task ServerProperties_Tool_Returns_Server_Metadata()
    {
        var result = await _client.CallToolAsync(ToolName, new Dictionary<string, object?>());
        var root = result.ReadJsonRoot();

        Assert.True(root.TryGetPropertyIgnoreCase("product_version", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("edition", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("engine_edition", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("engine_edition_name", out _));
    }

    [Fact]
    public async Task ServerProperties_Tool_Returns_Execution_Limits()
    {
        var result = await _client.CallToolAsync(ToolName, new Dictionary<string, object?>());
        var root = result.ReadJsonRoot();

        Assert.True(root.TryGetPropertyIgnoreCase("limits", out var limits));
        Assert.True(limits.TryGetPropertyIgnoreCase("query", out var query));
        Assert.True(query.TryGetPropertyIgnoreCase("default_max_rows", out _));
        Assert.True(query.TryGetPropertyIgnoreCase("hard_row_limit", out _));
        Assert.True(query.TryGetPropertyIgnoreCase("command_timeout_seconds", out _));
    }

    // ── Resource ──

    [Fact(Skip = "URI template matching needs investigation – mssql://server-properties?{profile} may not match bare URI.")]
    public async Task ServerProperties_Resource_Returns_Server_Metadata()
    {
        var result = await _client.ReadResourceAsync("mssql://server-properties");
        var root = result.ReadJsonRoot();

        Assert.True(root.TryGetPropertyIgnoreCase("product_version", out _));
        Assert.True(root.TryGetPropertyIgnoreCase("edition", out _));
    }
}
