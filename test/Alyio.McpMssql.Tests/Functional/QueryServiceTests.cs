// MIT License

using Alyio.McpMssql.Models;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class QueryServiceTests(SqlServerFixture fixture) : SqlServerFunctionalTest(fixture)
{
    private readonly IQueryService _query = fixture.Services.GetRequiredService<IQueryService>();
    private readonly IPlanStore _planStore = fixture.Services.GetRequiredService<IPlanStore>();

    // ── RunQueryAsync ──────────────────────────────────────────────

    [Fact]
    public async Task RunQuery_From_Users_Returns_Seeded_Data()
    {
        QueryResult result = await _query.RunQueryAsync(
            "select UserId, UserName from dbo.Users order by UserId",
            catalog: TestDatabaseName);

        result.Columns.AssertHasColumns(
            "UserId",
            "UserName");

        Assert.Equal(5, result.Rows.Count);
        Assert.Equal("Alice", result.Rows[0][1]);
        Assert.Equal("Eve", result.Rows[^1][1]);
    }

    [Fact]
    public async Task RunQuery_With_Parameter_Filters_Seeded_Data()
    {
        var result = await _query.RunQueryAsync(
            "select UserName from dbo.Users where UserId = @id",
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object>
            {
                ["id"] = 3,
            });

        Assert.Single(result.Rows);
        Assert.Equal("Charlie", result.Rows[0][0]);
    }

    [Fact]
    public async Task RunQuery_Join_Users_And_Orders()
    {
        var result = await _query.RunQueryAsync(
            """
            select u.UserName, count(o.OrderId) as OrderCount
            from dbo.Users u
            join dbo.Orders o on o.UserId = u.UserId
            where u.UserName = @name
            group by u.UserName
            """,
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object>
            {
                ["name"] = "Charlie",
            });

        Assert.Single(result.Rows);
        Assert.Equal("Charlie", result.Rows[0][0]);
        Assert.Equal(3, result.Rows[0][1]);
    }

    [Fact]
    public async Task RunQuery_In_With_Integer_Parameters()
    {
        var result = await _query.RunQueryAsync(
            "select UserName from dbo.Users where UserId in (@id_0, @id_1, @id_2) order by UserId",
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object>
            {
                ["id_0"] = 1,
                ["id_1"] = 3,
                ["id_2"] = 5,
            });

        Assert.Equal(3, result.Rows.Count);
        Assert.Equal("Alice", result.Rows[0][0]);
        Assert.Equal("Charlie", result.Rows[1][0]);
        Assert.Equal("Eve", result.Rows[2][0]);
    }

    [Fact]
    public async Task RunQuery_In_With_String_Parameters()
    {
        var result = await _query.RunQueryAsync(
            "select UserId, UserName from dbo.Users where UserName in (@name_0, @name_1) order by UserId",
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object>
            {
                ["name_0"] = "Bob",
                ["name_1"] = "Diana",
            });

        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("Bob", result.Rows[0][1]);
        Assert.Equal("Diana", result.Rows[1][1]);
    }

    [Fact]
    public async Task RunQuery_In_With_Single_Element()
    {
        var result = await _query.RunQueryAsync(
            "select UserName from dbo.Users where UserId in (@id_0)",
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object>
            {
                ["id_0"] = 2,
            });

        Assert.Single(result.Rows);
        Assert.Equal("Bob", result.Rows[0][0]);
    }

    [Fact]
    public async Task RunQuery_Non_Select_Is_Rejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _query.RunQueryAsync("delete from dbo.Users"));
    }

    // ── AnalyzeQueryAsync ──────────────────────────────────────────

    [Fact]
    public async Task AnalyzeQuery_Simple_Select_Returns_Statement_Summary()
    {
        var result = await _query.AnalyzeQueryAsync(
            "SELECT UserId, UserName FROM dbo.Users",
            catalog: TestDatabaseName);

        Assert.True(result.Statement.EstimatedCost > 0);
        Assert.NotNull(result.Statement.OptimizationLevel);
        Assert.True(result.Statement.CeVersion > 0);
        Assert.NotNull(result.Statement.QueryHash);
        Assert.NotNull(result.Statement.PlanHash);
    }

    [Fact]
    public async Task AnalyzeQuery_Returns_Valid_PlanUri_And_Stored_Xml()
    {
        var result = await _query.AnalyzeQueryAsync(
            "SELECT UserId FROM dbo.Users",
            catalog: TestDatabaseName);

        Assert.StartsWith("mssql://plans/", result.PlanUri);

        var id = result.PlanUri["mssql://plans/".Length..];
        var xml = _planStore.TryGet(id);
        Assert.NotNull(xml);
        Assert.Contains("ShowPlanXML", xml);
    }

    [Fact]
    public async Task AnalyzeQuery_Returns_TopOperators()
    {
        var result = await _query.AnalyzeQueryAsync(
            "SELECT UserId, UserName FROM dbo.Users ORDER BY UserId",
            catalog: TestDatabaseName);

        Assert.NotEmpty(result.TopOperators);
        var top = result.TopOperators[0];
        Assert.NotEmpty(top.PhysicalOp);
        Assert.NotEmpty(top.LogicalOp);
        Assert.True(top.EstimatedCostPct > 0);
    }

    [Fact]
    public async Task AnalyzeQuery_Join_Returns_Operators()
    {
        var result = await _query.AnalyzeQueryAsync(
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
    public async Task AnalyzeQuery_With_Parameters()
    {
        var result = await _query.AnalyzeQueryAsync(
            "SELECT UserName FROM dbo.Users WHERE UserId = @id",
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object> { ["id"] = 1 });

        Assert.NotEmpty(result.TopOperators);
        Assert.True(result.Statement.EstimatedCost > 0);
    }

    [Fact]
    public async Task AnalyzeQuery_Estimated_Returns_Plan_Without_Actuals()
    {
        var result = await _query.AnalyzeQueryAsync(
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
    public async Task AnalyzeQuery_Estimated_Stores_Xml()
    {
        var result = await _query.AnalyzeQueryAsync(
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
    public async Task AnalyzeQuery_Non_Select_Is_Rejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _query.AnalyzeQueryAsync(
                "DELETE FROM dbo.Users",
                catalog: TestDatabaseName));
    }
}
