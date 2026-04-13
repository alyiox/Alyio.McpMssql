// MIT License

using Alyio.McpMssql.Models;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class QueryServiceTests(SqlServerFixture fixture) : SqlServerFunctionalTest(fixture)
{
    private readonly IQueryService _query = fixture.Services.GetRequiredService<IQueryService>();
    private readonly IPlanStore _planStore = fixture.Services.GetRequiredService<IPlanStore>();
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    // ── RunQueryAsync ──────────────────────────────────────────────

    [Fact]
    public async Task RunQuery_From_Users_Returns_Seeded_Data()
    {
        QueryResult result = await _query.RunQueryAsync(
            "select UserId, UserName from dbo.Users order by UserId",
            catalog: TestDatabaseName,
            cancellationToken: CancellationToken);

        Assert.NotNull(result.Data);
        TabularAssertions.ParseCsvHeaders(result.Data)
            .AssertHasColumns("UserId", "UserName");

        Assert.Equal(5, result.RowCount);
        var rows = TabularAssertions.ParseCsvDataRows(result.Data);
        Assert.Equal("Alice", rows[0][1]);
        Assert.Equal("Eve", rows[^1][1]);
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
            },
            cancellationToken: CancellationToken);

        Assert.Equal(1, result.RowCount);
        Assert.NotNull(result.Data);
        var rows = TabularAssertions.ParseCsvDataRows(result.Data);
        Assert.Equal("Charlie", rows[0][0]);
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
            },
            cancellationToken: CancellationToken);

        Assert.Equal(1, result.RowCount);
        Assert.NotNull(result.Data);
        var rows = TabularAssertions.ParseCsvDataRows(result.Data);
        Assert.Equal("Charlie", rows[0][0]);
        Assert.Equal("3", rows[0][1]);
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
            },
            cancellationToken: CancellationToken);

        Assert.Equal(3, result.RowCount);
        Assert.NotNull(result.Data);
        var rows = TabularAssertions.ParseCsvDataRows(result.Data);
        Assert.Equal("Alice", rows[0][0]);
        Assert.Equal("Charlie", rows[1][0]);
        Assert.Equal("Eve", rows[2][0]);
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
            },
            cancellationToken: CancellationToken);

        Assert.Equal(2, result.RowCount);
        Assert.NotNull(result.Data);
        var rows = TabularAssertions.ParseCsvDataRows(result.Data);
        Assert.Equal("Bob", rows[0][1]);
        Assert.Equal("Diana", rows[1][1]);
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
            },
            cancellationToken: CancellationToken);

        Assert.Equal(1, result.RowCount);
        Assert.NotNull(result.Data);
        var rows = TabularAssertions.ParseCsvDataRows(result.Data);
        Assert.Equal("Bob", rows[0][0]);
    }

    [Fact]
    public async Task RunQuery_Non_Select_Is_Rejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _query.RunQueryAsync("delete from dbo.Users", cancellationToken: CancellationToken));
    }

    // ── AnalyzeQueryAsync ──────────────────────────────────────────

    [Fact]
    public async Task AnalyzeQuery_Simple_Select_Returns_Statement_Summary()
    {
        var result = await _query.AnalyzeQueryAsync(
            "SELECT UserId, UserName FROM dbo.Users",
            catalog: TestDatabaseName,
            cancellationToken: CancellationToken);

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
            catalog: TestDatabaseName,
            cancellationToken: CancellationToken);

        Assert.StartsWith("mssql://plans/", result.PlanUri);

        var id = result.PlanUri["mssql://plans/".Length..];
        var xml = await _planStore.TryGetAsync(id, CancellationToken);
        Assert.NotNull(xml);
        Assert.Contains("ShowPlanXML", xml);
    }

    [Fact]
    public async Task AnalyzeQuery_Returns_TopOperators()
    {
        var result = await _query.AnalyzeQueryAsync(
            "SELECT UserId, UserName FROM dbo.Users ORDER BY UserId",
            catalog: TestDatabaseName,
            cancellationToken: CancellationToken);

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
            catalog: TestDatabaseName,
            cancellationToken: CancellationToken);

        Assert.NotEmpty(result.TopOperators);
        Assert.True(result.Statement.EstimatedCost > 0);
    }

    [Fact]
    public async Task AnalyzeQuery_With_Parameters()
    {
        var result = await _query.AnalyzeQueryAsync(
            "SELECT UserName FROM dbo.Users WHERE UserId = @id",
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object> { ["id"] = 1 },
            cancellationToken: CancellationToken);

        Assert.NotEmpty(result.TopOperators);
        Assert.True(result.Statement.EstimatedCost > 0);
    }

    [Fact]
    public async Task AnalyzeQuery_Estimated_Returns_Plan_Without_Actuals()
    {
        var result = await _query.AnalyzeQueryAsync(
            "SELECT UserId, UserName FROM dbo.Users ORDER BY UserId",
            catalog: TestDatabaseName,
            estimated: true,
            cancellationToken: CancellationToken);

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
            estimated: true,
            cancellationToken: CancellationToken);

        Assert.StartsWith("mssql://plans/", result.PlanUri);

        var id = result.PlanUri["mssql://plans/".Length..];
        var xml = await _planStore.TryGetAsync(id, CancellationToken);
        Assert.NotNull(xml);
        Assert.Contains("ShowPlanXML", xml);
    }

    [Fact]
    public async Task AnalyzeQuery_Non_Select_Is_Rejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _query.AnalyzeQueryAsync(
                "DELETE FROM dbo.Users",
                catalog: TestDatabaseName,
                cancellationToken: CancellationToken));
    }
}
