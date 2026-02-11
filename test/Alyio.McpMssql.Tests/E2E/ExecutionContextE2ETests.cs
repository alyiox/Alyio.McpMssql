// MIT License

using System.Text.Json;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;

namespace Alyio.McpMssql.Tests.E2E;

[Collection("SqlServer")]
public sealed class ExecutionContextE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private readonly McpClient _client = fixture.Client;

    private const string GetExecutionContextTool = "get_execution_context";

    [Fact]
    public async Task GetExecutionContext_Tool_Is_Discoverable()
    {
        Assert.True(await _client.IsToolRegisteredAsync(GetExecutionContextTool));
    }

    [Fact]
    public async Task GetExecutionContext_Tool_Returns_Select_Execution_Metadata()
    {
        var result = await _client.CallToolAsync(GetExecutionContextTool);
        var root = result.ReadJsonRoot();

        Assert.True(
            root.TryGetProperty("select", out var select),
            "Execution context must expose a 'select' section.");
        Assert.Equal(JsonValueKind.Object, select.ValueKind);

        Assert.True(select.TryGetProperty("default_max_rows", out var defaultMaxRows));
        Assert.True(select.TryGetProperty("hard_row_limit", out var hardRowLimit));
        Assert.True(select.TryGetProperty("command_timeout_seconds", out var timeoutSeconds));

        AssertOptionDescriptor(defaultMaxRows);
        AssertOptionDescriptor(hardRowLimit);
        AssertOptionDescriptor(timeoutSeconds);
    }

    [Fact]
    public async Task Execution_Context_Resource_Is_Discoverable()
    {
        Assert.True(
            await _client.IsResourceTemplateRegisteredAsync("mssql://{profile}/context/execution"),
            "Execution context resource should be discoverable.");
    }

    [Fact]
    public async Task Reading_Execution_Context_Returns_Select_Execution_Metadata()
    {
        var result = await _client.ReadResourceAsync("mssql://default/context/execution");
        var root = result.ReadJsonRoot();

        // top-level select execution context
        Assert.True(
            root.TryGetProperty("select", out var select),
            "Execution context must expose a 'select' section.");

        Assert.Equal(JsonValueKind.Object, select.ValueKind);

        // expected select execution options
        Assert.True(select.TryGetProperty("default_max_rows", out var defaultMaxRows));
        Assert.True(select.TryGetProperty("hard_row_limit", out var hardRowLimit));
        Assert.True(select.TryGetProperty("command_timeout_seconds", out var timeoutSeconds));

        // validate descriptor shape (spot-check)
        AssertOptionDescriptor(defaultMaxRows);
        AssertOptionDescriptor(hardRowLimit);
        AssertOptionDescriptor(timeoutSeconds);
    }

    private static void AssertOptionDescriptor(JsonElement option)
    {
        Assert.Equal(JsonValueKind.Object, option.ValueKind);

        Assert.True(option.TryGetProperty("value", out _));
        Assert.True(option.TryGetProperty("description", out _));
    }
}
