// MIT License

using Alyio.McpMssql.Tests.Infrastructure.Fixtures;

namespace Alyio.McpMssql.Tests.Functional;

/// <summary>
/// Base class for functional tests that execute against a shared SQL Server
/// test database managed by <see cref="SqlServerFixture"/>.
/// </summary>
/// <remarks>
/// Tests inheriting from this type run in the <c>SqlServer</c> test scope and
/// are executed serially to ensure database consistency.
/// </remarks>
[Collection("SqlServer")]
public abstract class SqlServerFunctionalTest(SqlServerFixture fixture)
{
    protected const string TestDatabaseName = "McpMssqlTest";

    protected SqlServerFixture Fixture { get; } = fixture;
}
