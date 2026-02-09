// MIT License

using Alyio.McpMssql.Options;
using Alyio.McpMssql.Tests.Infrastructure.Database;
using Alyio.McpMssql.Tests.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Tests.Infrastructure.Fixtures;

/// <summary>
/// xUnit class fixture that manages the lifecycle of a shared SQL Server
/// test database and exposes the application's DI container for functional tests.
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
    /// schema and seed scripts.
    /// </summary>
    public async Task InitializeAsync()
    {
        var connectionString =
            Services
                .GetRequiredService<IOptions<McpMssqlOptions>>()
                .Value
                .ConnectionString;

        await DatabaseInitializer.ExecuteEmbeddedScriptAsync(
            connectionString,
            "Alyio.McpMssql.Tests.Infrastructure.Database.Scripts.schema.sql");

        await DatabaseInitializer.ExecuteEmbeddedScriptAsync(
            connectionString,
            "Alyio.McpMssql.Tests.Infrastructure.Database.Scripts.seed.sql");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

