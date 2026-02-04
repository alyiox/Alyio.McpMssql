// MIT License

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql;

/// <summary>
/// DI registration helpers for the MCP SQL Server tool.
/// </summary>
public static class ServiceCollectionExtensions
{
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

