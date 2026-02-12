// MIT License

using System.Text.Json;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;

namespace Alyio.McpMssql.Tests.E2E;

public sealed class ProfileContextE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private readonly McpClient _client = fixture.Client;

    private const string ListProfilesTool = "list_profiles";

    [Fact]
    public async Task ListProfiles_Tool_Is_Discoverable()
    {
        Assert.True(await _client.IsToolRegisteredAsync(ListProfilesTool));
    }

    [Fact]
    public async Task Profiles_Resource_Is_Discoverable()
    {
        Assert.True(
            await _client.IsResourceRegisteredAsync("mssql://context/profiles"),
            "Profile context resource should be discoverable.");
    }

    [Fact]
    public async Task ListProfiles_Tool_Returns_Profiles()
    {
        var result = await _client.CallToolAsync(ListProfilesTool);
        var text = result.ReadAsText();
        var root = UnwrapToolResult(text);

        Assert.True(
            root.TryGetPropertyIgnoreCase("profiles", out var profiles),
            "Root must have 'profiles' or 'Profiles'.");
        Assert.Equal(JsonValueKind.Array, profiles.ValueKind);
        Assert.True(profiles.GetArrayLength() >= 1, "At least the default profile should be configured.");

        var first = profiles[0];
        Assert.True(first.TryGetPropertyIgnoreCase("name", out var name));
        Assert.Equal(JsonValueKind.String, name.ValueKind);
    }

    private static JsonElement UnwrapToolResult(string text)
    {
        using var doc = JsonDocument.Parse(text);
        var root = doc.RootElement;
        // SDK may wrap in { "text": "<json>" }
        if (root.TryGetProperty("text", out var textProp))
        {
            using var inner = JsonDocument.Parse(textProp.GetString() ?? "{}");
            return inner.RootElement.Clone();
        }
        return root.Clone();
    }

    [Fact]
    public async Task Profiles_Resource_Returns_Profiles()
    {
        var result = await _client.ReadResourceAsync("mssql://context/profiles");
        var root = result.ReadJsonRoot();

        Assert.True(root.TryGetProperty("profiles", out var profiles));
        Assert.Equal(JsonValueKind.Array, profiles.ValueKind);
        Assert.True(profiles.GetArrayLength() >= 1);
    }
}
