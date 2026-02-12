// MIT License

using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class CatalogServiceTests(SqlServerFixture fixture) : SqlServerFunctionalTest(fixture)
{
    private readonly ICatalogService _service = fixture.Services.GetRequiredService<ICatalogService>();

    [Fact]
    public async Task ListCatalogs_Returns_Test_Database()
    {
        var result = await _service.ListCatalogsAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Columns);
        Assert.NotEmpty(result.Rows);

        result.Columns.AssertHasColumns(
            "name",
            "state_desc",
            "is_read_only",
            "is_system_db");

        Assert.Contains(
            result.Rows,
            r => r[0]?.ToString() == TestDatabaseName);
    }

    // -----------------------------
    // Schemas
    // -----------------------------

    [Fact]
    public async Task ListSchemas_Returns_Dbo_Schema_For_Explicit_Catalog()
    {
        var result = await _service.ListSchemasAsync(TestDatabaseName);

        Assert.NotEmpty(result.Rows);
        result.Columns.AssertHasColumns("name");

        Assert.Contains(
            result.Rows,
            r => r[0]?.ToString() == "dbo");
    }

    [Fact]
    public async Task ListSchemas_Without_Catalog_Uses_Default_Database()
    {
        var result = await _service.ListSchemasAsync();

        Assert.NotEmpty(result.Rows);
        result.Columns.AssertHasColumns("name");

        Assert.Contains(
            result.Rows,
            r => r[0]?.ToString() == "dbo");
    }

    // -----------------------------
    // Relations (tables + views)
    // -----------------------------

    [Fact]
    public async Task ListRelations_Returns_Tables_And_Views_For_Explicit_Scope()
    {
        var result = await _service.ListRelationsAsync(
            catalog: TestDatabaseName,
            schema: "dbo");

        Assert.NotEmpty(result.Rows);

        result.Columns.AssertHasColumns(
            "name",
            "type",
            "schema");

        var names = result.Rows.Select(r => r[0]?.ToString()).ToList();

        // tables
        Assert.Contains("Users", names);
        Assert.Contains("Orders", names);

        // views
        Assert.Contains("ActiveUsers", names);
        Assert.Contains("OrderSummary", names);
    }

    [Fact]
    public async Task ListRelations_Without_Parameters_Uses_Default_Scope()
    {
        var result = await _service.ListRelationsAsync();

        Assert.NotEmpty(result.Rows);

        var names = result.Rows.Select(r => r[0]?.ToString()).ToList();

        Assert.Contains("Users", names);
        Assert.Contains("Orders", names);
        Assert.Contains("ActiveUsers", names);
        Assert.Contains("OrderSummary", names);
    }

    // -----------------------------
    // Routines (procedures + functions)
    // -----------------------------

    [Fact]
    public async Task ListRoutines_Returns_Procedures_And_Functions_For_Explicit_Scope()
    {
        var result = await _service.ListRoutinesAsync(
            catalog: TestDatabaseName,
            schema: "dbo");

        Assert.NotEmpty(result.Rows);

        result.Columns.AssertHasColumns(
            "name",
            "type",
            "schema");

        var names = result.Rows.Select(r => r[0]?.ToString()).ToList();

        // stored procedures
        Assert.Contains("GetUserCount", names);
        Assert.Contains("GetUserById", names);

        // scalar functions
        Assert.Contains("GetUserEmail", names);
        Assert.Contains("GetTotalOrderAmount", names);
    }

    [Fact]
    public async Task ListRoutines_Without_Parameters_Uses_Default_Scope()
    {
        var result = await _service.ListRoutinesAsync();

        Assert.NotEmpty(result.Rows);

        var names = result.Rows.Select(r => r[0]?.ToString()).ToList();

        Assert.Contains("GetUserCount", names);
        Assert.Contains("GetUserById", names);
        Assert.Contains("GetUserEmail", names);
        Assert.Contains("GetTotalOrderAmount", names);
    }

    // -----------------------------
    // Describe columns
    // -----------------------------

    [Fact]
    public async Task DescribeColumns_Returns_Users_Column_Metadata()
    {
        var result = await _service.DescribeColumnsAsync(
            name: "Users",
            catalog: TestDatabaseName,
            schema: "dbo");

        Assert.NotEmpty(result.Columns);
        Assert.NotEmpty(result.Rows);

        result.Columns.AssertHasColumns(
            "name",
            "type",
            "nullable",
            "position");

        var nameIndex =
            result.Columns
                  .Select((c, i) => (c, i))
                  .First(p => p.c.Equals("name", StringComparison.OrdinalIgnoreCase))
                  .i;

        var columnNames =
            result.Rows.Select(r => r[nameIndex]?.ToString()).ToList();

        Assert.Contains("UserId", columnNames);
        Assert.Contains("UserName", columnNames);
        Assert.Contains("Email", columnNames);
        Assert.Contains("CreatedDate", columnNames);
    }

    [Fact]
    public async Task DescribeColumns_Without_Catalog_Or_Schema_Uses_Default_Resolution()
    {
        var result = await _service.DescribeColumnsAsync("Users");

        Assert.NotEmpty(result.Rows);
    }

    // -----------------------------
    // Describe indexes
    // -----------------------------

    [Fact]
    public async Task DescribeIndexes_Returns_Orders_Index_Metadata()
    {
        var result = await _service.DescribeIndexesAsync(
            name: "Orders",
            catalog: TestDatabaseName,
            schema: "dbo");

        Assert.NotEmpty(result.Columns);
        Assert.NotEmpty(result.Rows);

        result.Columns.AssertHasColumns(
            "index_name",
            "index_type",
            "is_unique",
            "is_disabled",
            "has_filter",
            "filter_definition",
            "key_ordinal",
            "is_descending",
            "column_name",
            "is_included_column");

        var indexNameIndex = result.Columns
            .Select((c, i) => (c, i))
            .First(p => p.c.Equals("index_name", StringComparison.OrdinalIgnoreCase))
            .i;

        var indexNames = result.Rows.Select(r => r[indexNameIndex]?.ToString()).Distinct().ToList();

        // PK name is system-generated (e.g. PK__Orders__...); we assert on the named indexes we created
        Assert.Contains("IX_Orders_OrderDate", indexNames);
        Assert.Contains("IX_Orders_User_Date", indexNames);
        Assert.Contains("IX_Orders_OrderDate_Filtered", indexNames);
        Assert.True(indexNames.Count >= 4, "Orders should have at least 4 indexes (PK + 3 named).");
    }

    [Fact]
    public async Task DescribeIndexes_Without_Catalog_Or_Schema_Uses_Default_Resolution()
    {
        var result = await _service.DescribeIndexesAsync("Orders");

        Assert.NotEmpty(result.Rows);
    }
}
