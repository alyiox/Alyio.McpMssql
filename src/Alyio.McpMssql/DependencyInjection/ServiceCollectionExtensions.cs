// MIT License

using Alyio.McpMssql;
using Alyio.McpMssql.DependencyInjection;
using Alyio.McpMssql.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

#pragma warning disable IDE0130 // Intentional: extension methods for IServiceCollection
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Dependency injection helpers for the MCP SQL Server integration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all MCP SQL Server services and configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration source.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddMcpMssql(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMcpMssqlOptions(configuration);

        services.AddSingleton<IServerContextService, ServerContextService>();
        services.AddSingleton<ICatalogService, CatalogService>();
        services.AddSingleton<IQueryService, QueryService>();

        return services;
    }

    /// <summary>
    /// Registers and validates <see cref="McpMssqlOptions"/>.
    /// Values are sourced from environment-driven configuration
    /// and clamped to safe limits.
    /// </summary>
    public static IServiceCollection AddMcpMssqlOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<McpMssqlOptions>()
            .Configure(options =>
            {
                options.ConnectionString =
                    GetString(configuration, McpMssqlOptions.ConnectionStringKey)
                    ?? options.ConnectionString;

                options.RowLimit =
                    GetInt(configuration, McpMssqlOptions.RowLimitKey)
                    ?? options.RowLimit;

                options.CommandTimeoutSeconds =
                    GetInt(configuration, McpMssqlOptions.CommandTimeoutSecondsKey)
                    ?? options.CommandTimeoutSeconds;
            })
            .PostConfigure(ClampAndNormalize)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static void ClampAndNormalize(McpMssqlOptions options)
    {
        options.RowLimit = Math.Clamp(
            options.RowLimit,
            min: 1,
            max: McpMssqlOptions.HardRowLimit);

        options.CommandTimeoutSeconds = Math.Clamp(
            options.CommandTimeoutSeconds,
            min: 1,
            max: McpMssqlOptions.HardCommandTimeout);

        // Normalize command timeout into the connection string
        var builder = new SqlConnectionStringBuilder(options.ConnectionString)
        {
            CommandTimeout = options.CommandTimeoutSeconds
        };

        options.ConnectionString = builder.ConnectionString;
    }

    private static string? GetString(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int? GetInt(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value.Trim(), out var parsed)
            ? parsed
            : null;
    }
}
