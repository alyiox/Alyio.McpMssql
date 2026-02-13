// MIT License

using Alyio.McpMssql.Features;
using Alyio.McpMssql.Models;
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
            "type");

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
            "type");

        var names = result.Rows.Select(r => r[0]?.ToString()).ToList();

        // stored procedures
        Assert.Contains("GetUserCount", names);
        Assert.Contains("GetUserById", names);

        // scalar functions
        Assert.Contains("GetUserEmail", names);
        Assert.Contains("GetTotalOrderAmount", names);
    }

    [Fact]
    public async Task GetRoutineDefinition_Returns_NonEmpty_Definition_For_Existing_Routine()
    {
        var result = await _service.GetRoutineDefinitionAsync(
            name: "GetUserById",
            catalog: TestDatabaseName,
            schema: "dbo");

        result.Columns.AssertHasColumns("definition");
        Assert.Single(result.Rows);
        var definition = result.Rows[0][0]?.ToString();
        Assert.NotNull(definition);
        Assert.Contains("SELECT", definition, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetRoutineDefinition_Returns_Empty_Rows_For_Nonexistent_Routine()
    {
        var result = await _service.GetRoutineDefinitionAsync(
            name: "NonExistentRoutine_XYZ",
            catalog: TestDatabaseName,
            schema: "dbo");

        result.Columns.AssertHasColumns("definition");
        Assert.Empty(result.Rows);
    }

    [Fact]
    public async Task ListRoutines_IncludeSystem_Returns_Superset()
    {
        var userOnly = await _service.ListRoutinesAsync(
            catalog: TestDatabaseName, schema: "dbo");

        var withSystem = await _service.ListRoutinesAsync(
            catalog: TestDatabaseName, schema: "dbo", includeSystem: true);

        Assert.True(withSystem.Rows.Count >= userOnly.Rows.Count);
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
    // GetObject – combined includes
    // -----------------------------

    [Fact]
    public async Task GetObject_WithColumnsAndIndexes_Returns_Both_Sections()
    {
        var result = await ObjectTools.GetObjectAsync(
            _service,
            ObjectKind.Relation,
            "Orders",
            catalog: TestDatabaseName,
            schema: "dbo",
            includes: [ObjectInclude.Columns, ObjectInclude.Indexes]);

        Assert.NotNull(result.Columns);
        Assert.NotEmpty(result.Columns.Rows);
        result.Columns.Columns.AssertHasColumns("name", "type", "is_nullable", "column_id");

        Assert.NotNull(result.Indexes);
        Assert.NotEmpty(result.Indexes.Rows);
        result.Indexes.Columns.AssertHasColumns(
            "index_name", "index_type", "is_unique", "is_disabled", "has_filter",
            "filter_definition", "key_ordinal", "is_descending", "column_name", "is_included_column");

        Assert.Null(result.Constraints);
        Assert.Null(result.Definition);
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
            "is_nullable",
            "column_id");

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

    // -----------------------------
    // Constraints
    // -----------------------------

    [Fact]
    public async Task DescribeConstraints_Returns_Orders_Constraint_Metadata()
    {
        var result = await _service.DescribeConstraintsAsync(
            name: "Orders",
            catalog: TestDatabaseName,
            schema: "dbo");

        Assert.NotNull(result.PrimaryKeys);
        Assert.NotNull(result.UniqueConstraints);
        Assert.NotNull(result.ForeignKeys);
        Assert.NotNull(result.CheckConstraints);
        Assert.NotNull(result.DefaultConstraints);

        result.PrimaryKeys.Columns.AssertHasColumns("constraint_name", "column_name", "column_ordinal");
        result.UniqueConstraints.Columns.AssertHasColumns("constraint_name", "column_name", "column_ordinal");
        result.ForeignKeys.Columns.AssertHasColumns(
            "constraint_name", "column_name", "column_ordinal",
            "referenced_schema", "referenced_table", "referenced_column",
            "on_delete", "on_update", "is_disabled", "is_not_trusted");
        result.CheckConstraints.Columns.AssertHasColumns(
            "constraint_name", "definition", "is_disabled", "is_not_trusted");
        result.DefaultConstraints.Columns.AssertHasColumns("constraint_name", "column_name", "definition");

        Assert.NotEmpty(result.PrimaryKeys.Rows);
        Assert.NotEmpty(result.ForeignKeys.Rows);
        Assert.NotEmpty(result.CheckConstraints.Rows);
        Assert.NotEmpty(result.DefaultConstraints.Rows);
    }
}
