// MIT License

using System.Text.Json;
using Alyio.McpMssql.Models;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Alyio.McpMssql.Tests.Functional;

/// <summary>
/// Functional tests for the SQL Server connection context resource,
/// verifying discovery and correctness of returned metadata.
/// </summary>
public class ServerContextTests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly McpClient _client = fixture.Client;

    [Fact]
    public async Task Server_Exposes_Connection_Context_Resource()
    {
        // Assert - The connection context resource is discoverable
        Assert.True(
            await _client.IsResourceRegisteredAsync("mssql://connection/context"),
            "Connection context resource should be registered and discoverable.");
    }

    [Fact]
    public async Task Reading_Connection_Context_Returns_Valid_Metadata()
    {
        var uri = new Uri("mssql://connection/context");
        var result = await _client.ReadResourceAsync(uri);

        var text = result.ReadAsText();

        var context = JsonSerializer.Deserialize<ServerConnectionContext>(text, s_jsonOptions);

        Assert.NotNull(context);

        ValidateConnectionContext(context);
    }

    private static void ValidateConnectionContext(ServerConnectionContext context)
    {
        Assert.False(
            string.IsNullOrWhiteSpace(context.Server),
            "Server name should be present.");

        Assert.False(
            string.IsNullOrWhiteSpace(context.Database),
            "Current database should be present.");

        Assert.False(
            string.IsNullOrWhiteSpace(context.User),
            "Effective user identity should be present.");

        Assert.False(
            string.IsNullOrWhiteSpace(context.Version),
            "SQL Server version information should be present.");

        Assert.Contains(
            "Microsoft SQL Server",
            context.Version);
    }
}

