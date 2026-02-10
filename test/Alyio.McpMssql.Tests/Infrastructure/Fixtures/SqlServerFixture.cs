// MIT License

// IProfileResolver and profile-based config (default profile for tests)
using Alyio.McpMssql;
using Alyio.McpMssql.Tests.Infrastructure.Database;
using Alyio.McpMssql.Tests.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql.Tests.Infrastructure.Fixtures;

/// <summary>
/// xUnit class fixture that manages the lifecycle of a shared SQL Server
/// test database and exposes the application's DI container for functional tests.
/// Uses the default MCP MSSQL profile (compatible with original single-connection behavior).
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    /// <summary>
    /// Root service provider for resolving application services
    /// against the initialized test database.
    /// </summary>
    public IServiceProvider Services { get; }

    public SqlServerFixture()
    {
        Services = ServiceBuilder.Build().BuildServiceProvider();
    }

    /// <summary>
    /// Creates and initializes the test database by executing
    /// schema and seed scripts using the default profile's connection string.
    /// </summary>
    public async Task InitializeAsync()
    {
        var profileResolver = Services.GetRequiredService<IProfileResolver>();
        var profile = profileResolver.Resolve();
        var connectionString = profile.ConnectionString;

        await DatabaseInitializer.ExecuteEmbeddedScriptAsync(
            connectionString,
            "Alyio.McpMssql.Tests.Infrastructure.Database.Scripts.schema.sql");

        await DatabaseInitializer.ExecuteEmbeddedScriptAsync(
            connectionString,
            "Alyio.McpMssql.Tests.Infrastructure.Database.Scripts.seed.sql");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

