// MIT License

using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Alyio.McpMssql.Tests.E2E;

public class PlanE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private const string AnalyzeQueryTool = "analyze_query";
    private const string AnalyzableSql = "SELECT TOP 1 name, object_id FROM sys.objects WHERE type = 'U'";
    private readonly McpClient _client = fixture.Client;
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task PlanResource_Template_Is_Discoverable()
    {
        Assert.True(
            await _client.IsResourceTemplateRegisteredAsync("mssql://plans/{id}"));
    }

    [Fact]
    public async Task PlanResource_Returns_Xml_For_Valid_Id()
    {
        var planUri = await AnalyzeAndGetPlanUriAsync();

        var resource = await _client.ReadResourceAsync(planUri, cancellationToken: CancellationToken);
        var xml = resource.ReadAsText();

        Assert.Contains("ShowPlanXML", xml);
    }

    [Fact]
    public async Task PlanResource_Unknown_Id_Throws()
    {
        var ex = await Assert.ThrowsAnyAsync<Exception>(async () =>
            await _client.ReadResourceAsync("mssql://plans/nonexistent", cancellationToken: CancellationToken));

        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> AnalyzeAndGetPlanUriAsync()
    {
        var args = new Dictionary<string, object?> { ["sql"] = AnalyzableSql };
        var result = await _client.CallToolAsync(AnalyzeQueryTool, args, cancellationToken: CancellationToken);

        Assert.True(result.IsError is not true);

        var root = result.ReadJsonRoot();
        return root.GetProperty("plan_uri").GetString()!;
    }
}
