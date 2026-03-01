// MIT License

using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class AnalyzeServiceTests(SqlServerFixture fixture) : SqlServerFunctionalTest(fixture)
{
    private readonly IAnalyzeService _analyze = fixture.Services.GetRequiredService<IAnalyzeService>();
    private readonly IPlanStore _planStore = fixture.Services.GetRequiredService<IPlanStore>();

    [Fact]
    public async Task Analyze_Simple_Select_Returns_Statement_Summary()
    {
        var result = await _analyze.AnalyzeAsync(
            "SELECT UserId, UserName FROM dbo.Users",
            catalog: TestDatabaseName);

        Assert.True(result.Statement.EstimatedCost > 0);
        Assert.NotNull(result.Statement.OptimizationLevel);
        Assert.True(result.Statement.CeVersion > 0);
        Assert.NotNull(result.Statement.QueryHash);
        Assert.NotNull(result.Statement.PlanHash);
    }

    [Fact]
    public async Task Analyze_Returns_Valid_PlanUri_And_Stored_Xml()
    {
        var result = await _analyze.AnalyzeAsync(
            "SELECT UserId FROM dbo.Users",
            catalog: TestDatabaseName);

        Assert.StartsWith("mssql://plans/", result.PlanUri);

        var id = result.PlanUri["mssql://plans/".Length..];
        var xml = _planStore.TryGet(id);
        Assert.NotNull(xml);
        Assert.Contains("ShowPlanXML", xml);
    }

    [Fact]
    public async Task Analyze_Returns_TopOperators()
    {
        var result = await _analyze.AnalyzeAsync(
            "SELECT UserId, UserName FROM dbo.Users ORDER BY UserId",
            catalog: TestDatabaseName);

        Assert.NotEmpty(result.TopOperators);
        var top = result.TopOperators[0];
        Assert.NotEmpty(top.PhysicalOp);
        Assert.NotEmpty(top.LogicalOp);
        Assert.True(top.EstimatedCostPct > 0);
    }

    [Fact]
    public async Task Analyze_Join_Returns_Operators()
    {
        var result = await _analyze.AnalyzeAsync(
            """
            SELECT u.UserName, o.TotalAmount
            FROM dbo.Users u
            JOIN dbo.Orders o ON o.UserId = u.UserId
            """,
            catalog: TestDatabaseName);

        Assert.NotEmpty(result.TopOperators);
        Assert.True(result.Statement.EstimatedCost > 0);
    }

    [Fact]
    public async Task Analyze_With_Parameters()
    {
        var result = await _analyze.AnalyzeAsync(
            "SELECT UserName FROM dbo.Users WHERE UserId = @id",
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object> { ["id"] = 1 });

        Assert.NotEmpty(result.TopOperators);
        Assert.True(result.Statement.EstimatedCost > 0);
    }

    [Fact]
    public async Task Analyze_Estimated_Returns_Plan_Without_Actuals()
    {
        var result = await _analyze.AnalyzeAsync(
            "SELECT UserId, UserName FROM dbo.Users ORDER BY UserId",
            catalog: TestDatabaseName,
            estimated: true);

        Assert.True(result.Statement.EstimatedCost > 0);
        Assert.NotNull(result.Statement.OptimizationLevel);
        Assert.NotEmpty(result.TopOperators);

        // Estimated plans have no runtime information
        var top = result.TopOperators[0];
        Assert.Null(top.ActualRows);
        Assert.Null(top.ActualElapsedMs);

        // Timing comes from QueryTimeStats which is absent in estimated plans
        Assert.Null(result.Statement.CpuTimeMs);
        Assert.Null(result.Statement.ElapsedTimeMs);
    }

    [Fact]
    public async Task Analyze_Estimated_Stores_Xml()
    {
        var result = await _analyze.AnalyzeAsync(
            "SELECT UserId FROM dbo.Users",
            catalog: TestDatabaseName,
            estimated: true);

        Assert.StartsWith("mssql://plans/", result.PlanUri);

        var id = result.PlanUri["mssql://plans/".Length..];
        var xml = _planStore.TryGet(id);
        Assert.NotNull(xml);
        Assert.Contains("ShowPlanXML", xml);
    }

    [Fact]
    public async Task Analyze_Non_Select_Is_Rejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _analyze.AnalyzeAsync(
                "DELETE FROM dbo.Users",
                catalog: TestDatabaseName));
    }
}
