// MIT License

using Alyio.McpMssql.Tests.Helpers;

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
        var baseConnectionString = GetConnectionString();

        // Execute schema.sql (creates database, tables, views, procedures, functions)
        await DatabaseInitializer.ExecuteEmbeddedScriptAsync(
            baseConnectionString,
            "Alyio.McpMssql.Tests.Scripts.schema.sql");

        // Execute seed.sql (inserts test data)
        await DatabaseInitializer.ExecuteEmbeddedScriptAsync(
            baseConnectionString,
            "Alyio.McpMssql.Tests.Scripts.seed.sql");
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

    /// <summary>
    /// Loads the connection string from configuration.
    /// </summary>
    /// <returns>The connection string, or null if missing.</returns>
    private static string GetConnectionString()
    {
        var config = ConfigurationLoader.Load();
        var connectionString = config[ConfigurationLoader.ConnectionStringKey];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Missing connection string configuration. Set '{ConfigurationLoader.ConnectionStringKey}' in user secrets or environment variables.");
        }

        return connectionString;
    }
}
