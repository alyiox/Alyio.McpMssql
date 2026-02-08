// MIT License

using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using Microsoft.Extensions.DependencyInjection;

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
public abstract class SqlServerFunctionalTest
{
    protected const string TestDatabaseName = "McpMssqlTest";

    protected SqlServerFixture Fixture { get; }

    protected SqlServerFunctionalTest(SqlServerFixture fixture)
    {
        Fixture = fixture;
    }

    /// <summary>
    /// Resolves a required application service from the
    /// test DI container.
    /// </summary>
    protected T GetRequiredService<T>() where T : notnull
        => Fixture.Services.GetRequiredService<T>();
}
