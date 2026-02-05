// MIT License

using Alyio.McpMssql.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Tests.Fixtures;

/// <summary>
/// xUnit class fixture for managing SQL Server test database lifecycle.
/// Creates test database with schema and seed data before tests run.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    /// <summary>
    /// Called once before any test in the class runs.
    /// Creates test database and executes schema/seed scripts.
    /// </summary>
    public async Task InitializeAsync()
    {
        var connectionString = ServiceScopeFactory.Create().ServiceProvider.GetRequiredService<IOptions<McpMssqlOptions>>().Value.ConnectionString;

        // Execute schema.sql (creates database, tables, views, procedures, functions)
        await DatabaseInitializer.ExecuteEmbeddedScriptAsync(connectionString, "Alyio.McpMssql.Tests.Scripts.schema.sql");

        // Execute seed.sql (inserts test data)
        await DatabaseInitializer.ExecuteEmbeddedScriptAsync(connectionString, "Alyio.McpMssql.Tests.Scripts.seed.sql");
    }

    /// <summary>
    /// Called once after all tests in the class have completed.
    /// Cleanup is handled by schema.sql on next test run.
    /// </summary>
    public Task DisposeAsync()
    {
        // No-op: cleanup is handled by schema.sql which drops and recreates the database
        return Task.CompletedTask;
    }
}
