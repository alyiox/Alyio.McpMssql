// MIT License

using Alyio.McpMssql;
using Alyio.McpMssql.Options;
using Alyio.McpMssql.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// DI registration helpers for the MCP SQL Server tool.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQL Server services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration" /> containing the connection string and other options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddMcpMssql(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMcpMssqlOptions(configuration);
        services.AddSingleton<ISqlServerService, SqlServerService>();
        return services;
    }

    /// <summary>
    /// Registers <see cref="McpMssqlOptions"/> and binds it from environment-driven configuration.
    /// </summary>
    public static IServiceCollection AddMcpMssqlOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<McpMssqlOptions>()
            .Configure(options =>
            {
                options.ConnectionString = GetString(configuration, McpMssqlOptions.ConnectionStringKey) ?? options.ConnectionString;
                options.DefaultMaxRows = GetInt(configuration, McpMssqlOptions.DefaultMaxRowsKey) ?? options.DefaultMaxRows;
                options.HardMaxRows = GetInt(configuration, McpMssqlOptions.HardMaxRowsKey) ?? options.HardMaxRows;
                options.CommandTimeoutSeconds = GetInt(configuration, McpMssqlOptions.CommandTimeoutSecondsKey) ?? options.CommandTimeoutSeconds;
            })
            .PostConfigure(options =>
            {
                options.HardMaxRows = Math.Clamp(options.HardMaxRows, 1, McpMssqlOptions.AbsoluteMaxRowsCeiling);
                options.CommandTimeoutSeconds = Math.Clamp(options.CommandTimeoutSeconds, 1, McpMssqlOptions.AbsoluteCommandTimeoutSecondsCeiling);
                options.DefaultMaxRows = Math.Clamp(options.DefaultMaxRows, 1, options.HardMaxRows);
                options.ConnectionString = new SqlConnectionStringBuilder(options.ConnectionString) { CommandTimeout = options.CommandTimeoutSeconds }.ConnectionString;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static string? GetString(IConfiguration configuration, string key)
    {
        var raw = configuration[key];
        return string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
    }

    private static int? GetInt(IConfiguration configuration, string key)
    {
        var raw = configuration[key];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return int.TryParse(raw.Trim(), out var value) ? value : null;
    }
}

