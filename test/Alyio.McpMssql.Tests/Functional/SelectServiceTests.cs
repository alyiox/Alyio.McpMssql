// MIT License

using Alyio.McpMssql.Models;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class SelectServiceTests(SqlServerFixture fixture) : SqlServerFunctionalTest(fixture)
{
    private readonly ISelectService _select = fixture.Services.GetRequiredService<ISelectService>();

    [Fact]
    public async Task Select_From_Users_Returns_Seeded_Data()
    {
        QueryResult result = await _select.ExecuteAsync(
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
    public async Task Select_With_Parameter_Filters_Seeded_Data()
    {
        var result = await _select.ExecuteAsync(
            "select UserName from dbo.Users where UserId = @id",
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object>
            {
                ["id"] = 3
            });

        Assert.Single(result.Rows);
        Assert.Equal("Charlie", result.Rows[0][0]);
    }

    [Fact]
    public async Task Select_Join_Users_And_Orders()
    {
        var result = await _select.ExecuteAsync(
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
                ["name"] = "Charlie"
            });

        Assert.Single(result.Rows);
        Assert.Equal("Charlie", result.Rows[0][0]);
        Assert.Equal(3, result.Rows[0][1]);
    }

    [Fact]
    public async Task Select_Respects_MaxRows_On_Seeded_Data()
    {
        var result = await _select.ExecuteAsync(
            "select OrderId from dbo.Orders order by OrderId",
            catalog: TestDatabaseName,
            maxRows: 2);

        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(101, result.Rows[0][0]);
        Assert.Equal(102, result.Rows[1][0]);
    }

    [Fact]
    public async Task Select_In_With_Integer_Parameters()
    {
        var result = await _select.ExecuteAsync(
            "select UserName from dbo.Users where UserId in (@id_0, @id_1, @id_2) order by UserId",
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object>
            {
                ["id_0"] = 1,
                ["id_1"] = 3,
                ["id_2"] = 5
            });

        Assert.Equal(3, result.Rows.Count);
        Assert.Equal("Alice", result.Rows[0][0]);
        Assert.Equal("Charlie", result.Rows[1][0]);
        Assert.Equal("Eve", result.Rows[2][0]);
    }

    [Fact]
    public async Task Select_In_With_String_Parameters()
    {
        var result = await _select.ExecuteAsync(
            "select UserId, UserName from dbo.Users where UserName in (@name_0, @name_1) order by UserId",
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object>
            {
                ["name_0"] = "Bob",
                ["name_1"] = "Diana"
            });

        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("Bob", result.Rows[0][1]);
        Assert.Equal("Diana", result.Rows[1][1]);
    }

    [Fact]
    public async Task Select_In_With_Single_Element()
    {
        var result = await _select.ExecuteAsync(
            "select UserName from dbo.Users where UserId in (@id_0)",
            catalog: TestDatabaseName,
            parameters: new Dictionary<string, object>
            {
                ["id_0"] = 2
            });

        Assert.Single(result.Rows);
        Assert.Equal("Bob", result.Rows[0][0]);
    }

    [Fact]
    public async Task Non_Select_SQL_Is_Rejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _select.ExecuteAsync(
                "delete from dbo.Users"));
    }
}

