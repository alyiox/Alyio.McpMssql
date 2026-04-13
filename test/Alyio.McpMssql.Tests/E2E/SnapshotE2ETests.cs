// MIT License

using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using ModelContextProtocol.Client;

namespace Alyio.McpMssql.Tests.E2E;

public class SnapshotE2ETests(McpServerFixture fixture) : IClassFixture<McpServerFixture>
{
    private const string RunQueryTool = "run_query";
    private readonly McpClient _client = fixture.Client;
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Snapshot_Resource_Template_Is_Discoverable()
    {
        Assert.True(
            await _client.IsResourceTemplateRegisteredAsync("mssql://snapshots/{id}"));
    }

    [Fact]
    public async Task RunQuery_Snapshot_Returns_Uri_Without_Data_Field()
    {
        var result = await _client.CallToolAsync(
            RunQueryTool,
            new Dictionary<string, object?> { ["sql"] = "SELECT 1 AS Value", ["snapshot"] = true },
            cancellationToken: CancellationToken);

        Assert.True(result.IsError is not true);

        var root = result.ReadJsonRoot();
        Assert.True(root.TryGetProperty("snapshot_uri", out var uri));
        Assert.StartsWith("mssql://snapshots/", uri.GetString());

        Assert.False(root.TryGetProperty("data", out _));

        Assert.True(root.TryGetProperty("row_count", out var rowCount));
        Assert.Equal(1, rowCount.GetInt32());
    }

    [Fact]
    public async Task RunQuery_Snapshot_Round_Trip_Returns_Csv()
    {
        var result = await _client.CallToolAsync(
            RunQueryTool,
            new Dictionary<string, object?> { ["sql"] = "SELECT 1 AS Value", ["snapshot"] = true },
            cancellationToken: CancellationToken);

        Assert.True(result.IsError is not true);

        var root = result.ReadJsonRoot();
        var snapshotUri = root.GetProperty("snapshot_uri").GetString()!;

        var resource = await _client.ReadResourceAsync(snapshotUri, cancellationToken: CancellationToken);
        var csv = resource.ReadAsText();

        Assert.NotEmpty(csv);
        var headers = TabularAssertions.ParseCsvHeaders(csv);
        var rows = TabularAssertions.ParseCsvDataRows(csv);

        Assert.Equal("Value", headers[0]);
        var row = Assert.Single(rows);
        Assert.Equal("1", row[0]);
    }

    [Fact]
    public async Task Snapshot_Unknown_Id_Throws()
    {
        var ex = await Assert.ThrowsAnyAsync<Exception>(async () =>
            await _client.ReadResourceAsync("mssql://snapshots/nonexistent", cancellationToken: CancellationToken));

        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
