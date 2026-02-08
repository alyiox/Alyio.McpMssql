// MIT License

using System.Text.Json;
using Alyio.McpMssql.Models;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;

namespace Alyio.McpMssql.Tests.E2E;

public class ServerContextE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly McpClient _client = fixture.Client;

    [Fact]
    public async Task Connection_Context_Resource_Is_Discoverable()
    {
        Assert.True(
            await _client.IsResourceRegisteredAsync("mssql://connection/context"));
    }

    [Fact]
    public async Task Reading_Connection_Context_Returns_Metadata()
    {
        var result = await _client.ReadResourceAsync(
            new Uri("mssql://connection/context"));

        var text = result.ReadAsText();
        var context = JsonSerializer.Deserialize<ServerConnectionContext>(
            text, s_jsonOptions);

        Assert.NotNull(context);

        Assert.False(string.IsNullOrWhiteSpace(context.Server));
        Assert.False(string.IsNullOrWhiteSpace(context.Database));
        Assert.False(string.IsNullOrWhiteSpace(context.User));
        Assert.False(string.IsNullOrWhiteSpace(context.Version));
    }
}
