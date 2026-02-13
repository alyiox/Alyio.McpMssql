// MIT License

using System.Text.Json;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;

namespace Alyio.McpMssql.Tests.E2E;

public sealed class ProfilesE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private readonly McpClient _client = fixture.Client;

    private const string ToolName = "list_profiles";

    // ── Tool discovery ──

    [Fact]
    public async Task Profiles_Tool_Is_Discoverable()
    {
        Assert.True(await _client.IsToolRegisteredAsync(ToolName));
    }

    // ── Resource discovery ──

    [Fact]
    public async Task Profiles_Resource_Is_Discoverable()
    {
        Assert.True(
            await _client.IsResourceRegisteredAsync("mssql://profiles"),
            "Profiles resource should be discoverable.");
    }

    // ── Tool ──

    [Fact]
    public async Task Profiles_Tool_Returns_At_Least_Default_Profile()
    {
        var result = await _client.CallToolAsync(ToolName, new Dictionary<string, object?>());
        var root = result.ReadJsonRoot();

        // Result is an array of Profile objects
        Assert.True(root.ValueKind is JsonValueKind.Array);
        Assert.True(root.GetArrayLength() >= 1, "At least the default profile should be configured.");

        var first = root[0];
        Assert.True(first.TryGetPropertyIgnoreCase("name", out _));
    }

    // ── Resource ──

    [Fact]
    public async Task Profiles_Resource_Returns_At_Least_Default_Profile()
    {
        var result = await _client.ReadResourceAsync("mssql://profiles");
        var root = result.ReadJsonRoot();

        Assert.True(root.ValueKind is JsonValueKind.Array);
        Assert.True(root.GetArrayLength() >= 1);
    }
}
