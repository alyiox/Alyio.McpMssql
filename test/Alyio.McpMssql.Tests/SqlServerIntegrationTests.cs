// MIT License

using Alyio.McpMssql.Tests.Fixtures;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Alyio.McpMssql.Tests;

/// <summary>
/// Integration tests that verify actual SQL Server behavior with real database.
/// Requires SQL Server connection (configured via user secrets or environment variables).
/// Uses both SqlServerFixture (database) and McpServerFixture (MCP server).
/// </summary>
public sealed class SqlServerIntegrationTests(McpServerFixture fixture) : IClassFixture<SqlServerFixture>, IClassFixture<McpServerFixture>
{
    private const string TestDatabaseName = "McpMssqlTest";

    private readonly McpClient _client = fixture.Harness.Client;

    [Fact]
    public async Task ListTables_ReturnsUsersAndOrdersTables()
    {
        var result = await _client.CallToolAsync(
            "list_tables",
            new Dictionary<string, object?>
            {
                ["database"] = TestDatabaseName
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("Users", json);
        Assert.Contains("Orders", json);
    }

    [Fact]
    public async Task ListViews_ReturnsActiveUsersAndOrderSummaryViews()
    {
        var result = await _client.CallToolAsync(
            "list_views",
            new Dictionary<string, object?>
            {
                ["database"] = TestDatabaseName
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("ActiveUsers", json);
        Assert.Contains("OrderSummary", json);
    }

    [Fact]
    public async Task ListProcedures_ReturnsGetUserCountAndGetUserById()
    {
        var result = await _client.CallToolAsync(
            "list_procedures",
            new Dictionary<string, object?>
            {
                ["database"] = TestDatabaseName
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("GetUserCount", json);
        Assert.Contains("GetUserById", json);
    }

    [Fact]
    public async Task ListFunctions_ReturnsGetUserEmailAndGetTotalOrderAmount()
    {
        var result = await _client.CallToolAsync(
            "list_functions",
            new Dictionary<string, object?>
            {
                ["database"] = TestDatabaseName
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("GetUserEmail", json);
        Assert.Contains("GetTotalOrderAmount", json);
    }

    [Fact]
    public async Task DescribeTable_UsersTable_ReturnsCorrectColumns()
    {
        var result = await _client.CallToolAsync(
            "describe_table",
            new Dictionary<string, object?>
            {
                ["table"] = "Users",
                ["database"] = TestDatabaseName
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        // Verify column names
        Assert.Contains("UserId", json);
        Assert.Contains("UserName", json);
        Assert.Contains("Email", json);
        Assert.Contains("CreatedDate", json);

        // Verify data types
        Assert.Contains("int", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("nvarchar", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("datetime", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Query_SelectAllUsers_ReturnsSeededData()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT UserId, UserName, Email FROM dbo.Users ORDER BY UserId",
                ["database"] = TestDatabaseName,
                ["maxRows"] = 10
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        // Verify seeded user data
        Assert.Contains("Alice", json);
        Assert.Contains("alice@test.com", json);
        Assert.Contains("Bob", json);
        Assert.Contains("bob@test.com", json);
        Assert.Contains("Charlie", json);
        Assert.Contains("charlie@test.com", json);
        Assert.Contains("Diana", json);
        Assert.Contains("Eve", json);
    }

    [Fact]
    public async Task Query_WithParameterFilter_ReturnsFilteredUser()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT UserId, UserName, Email FROM dbo.Users WHERE UserId = @userId",
                ["database"] = TestDatabaseName,
                ["parametersJson"] = "{\"userId\": 1}",
                ["maxRows"] = 10
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        // Should only return Alice
        Assert.Contains("Alice", json);
        Assert.DoesNotContain("Bob", json);
        Assert.DoesNotContain("Charlie", json);
    }

    [Fact]
    public async Task Query_SelectFromView_ReturnsActiveUsers()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT UserId, UserName FROM dbo.ActiveUsers ORDER BY UserId",
                ["database"] = TestDatabaseName,
                ["maxRows"] = 10
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        // View should return all users (UserId > 0)
        Assert.Contains("Alice", json);
        Assert.Contains("Bob", json);
        Assert.Contains("Charlie", json);
    }

    [Fact]
    public async Task Query_JoinTablesWithAggregation_ReturnsCorrectResults()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = @"
                    SELECT 
                        u.UserId,
                        u.UserName,
                        COUNT(o.OrderId) AS OrderCount,
                        ISNULL(SUM(o.TotalAmount), 0) AS TotalSpent
                    FROM dbo.Users u
                    LEFT JOIN dbo.Orders o ON u.UserId = o.UserId
                    GROUP BY u.UserId, u.UserName
                    ORDER BY u.UserId",
                ["database"] = TestDatabaseName,
                ["maxRows"] = 10
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        // Alice has 2 orders
        Assert.Contains("Alice", json);

        // Bob has 1 order
        Assert.Contains("Bob", json);

        // Charlie has 3 orders
        Assert.Contains("Charlie", json);

        // Eve has 0 orders (but should still appear due to LEFT JOIN)
        Assert.Contains("Eve", json);
    }

    [Fact]
    public async Task Query_WithMaxRows_TruncatesResults()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT UserId, UserName FROM dbo.Users ORDER BY UserId",
                ["database"] = TestDatabaseName,
                ["maxRows"] = 2  // Only return 2 rows
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        // Should indicate truncation
        Assert.Contains("\"truncated\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("true", json);

        // Should return Alice and Bob (first 2)
        Assert.Contains("Alice", json);
        Assert.Contains("Bob", json);
    }

    [Fact]
    public async Task Query_DataTypes_HandlesVariousTypes()
    {
        var result = await _client.CallToolAsync(
            "select",
            new Dictionary<string, object?>
            {
                ["sql"] = "SELECT TOP 1 UserId, UserName, Email, CreatedDate FROM dbo.Users",
                ["database"] = TestDatabaseName,
                ["maxRows"] = 1
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        // Verify response includes column metadata with data types
        Assert.Contains("\"columns\"", json);
        Assert.Contains("\"rows\"", json);
        Assert.Contains("\"dataTypeName\"", json);
    }

    [Fact]
    public async Task ListSchemas_McpMssqlTest_ReturnsDboSchema()
    {
        var result = await _client.CallToolAsync(
            "list_schemas",
            new Dictionary<string, object?>
            {
                ["database"] = TestDatabaseName
            },
            cancellationToken: CancellationToken.None);

        var json = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("dbo", json);
        Assert.Contains("McpMssqlTest", json);
    }
}
