// MIT License

using System.Text.Json;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;

namespace Alyio.McpMssql.Tests.E2E;

public class ServerPropertiesContextE2ETests(McpServerFixture fixture)
    : IClassFixture<McpServerFixture>
{
    private static readonly JsonSerializerOptions s_jsonOptions = McpJsonDefaults.Options;

    private readonly McpClient _client = fixture.Client;

    [Fact]
    public async Task Server_Properties_Resource_Is_Discoverable()
    {
        Assert.True(
            await _client.IsResourceTemplateRegisteredAsync("mssql://{profile}/context/server/properties"));
    }

    [Fact]
    public async Task Reading_Server_Properties_Returns_Server_Metadata()
    {
        var result = await _client.ReadResourceAsync("mssql://default/context/server/properties");

        var json = result.ReadAsText();

        var context = JsonSerializer.Deserialize<ServerPropertiesContext>(
            json, s_jsonOptions);

        Assert.NotNull(context);

        // Versioning & product metadata
        Assert.False(string.IsNullOrWhiteSpace(context.ProductVersion));
        Assert.False(string.IsNullOrWhiteSpace(context.ProductLevel));

        // Edition metadata
        Assert.False(string.IsNullOrWhiteSpace(context.Edition));
        Assert.True(context.EngineEdition > 0);
        Assert.False(string.IsNullOrWhiteSpace(context.EngineEditionName));
    }
}
