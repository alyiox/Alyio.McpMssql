// MIT License

using Alyio.McpMssql;
using Alyio.McpMssql.Options;
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
    public static IServiceCollection AddMcpMssql(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMcpMssqlOptions(configuration);

        services.AddSingleton<IConnectionContextService, ConnectionContextService>();
        services.AddSingleton<ICatalogService, CatalogService>();
        services.AddSingleton<ISelectService, SelectService>();
        services.AddSingleton<IExecutionContextService, ExecutionContextService>();

        return services;
    }

    /// <summary>
    /// Registers and validates MCP MSSQL execution options.
    /// Default configuration binding is applied first,
    /// then overridden by MCP-specific flat environment variables.
    /// </summary>
    public static IServiceCollection AddMcpMssqlOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<McpMssqlOptions>()
            .Bind(configuration.GetSection("McpMssql"))
            .Configure(options => OverrideFromMcpEnvironment(options, configuration))
            .PostConfigure(ClampAndNormalize)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static void OverrideFromMcpEnvironment(McpMssqlOptions options, IConfiguration configuration)
    {
        var connectionString = GetString(configuration, McpMssqlOptions.ConnectionStringKey);

        if (connectionString is not null)
        {
            options.ConnectionString = connectionString;
        }

        var maxRows = GetInt(configuration, SelectExecutionOptions.MaxRowsKey);

        if (maxRows is not null)
        {
            options.Select.MaxRows = maxRows.Value;
        }

        var timeoutSeconds = GetInt(configuration, SelectExecutionOptions.CommandTimeoutSecondsKey);

        if (timeoutSeconds is not null)
        {
            options.Select.CommandTimeoutSeconds = timeoutSeconds.Value;
        }
    }

    private static void ClampAndNormalize(McpMssqlOptions options)
    {
        // Clamp select execution limits
        options.Select.MaxRows = Math.Clamp(
            options.Select.MaxRows,
            min: 1,
            max: SelectExecutionOptions.HardRowLimit);

        options.Select.CommandTimeoutSeconds = Math.Clamp(
            options.Select.CommandTimeoutSeconds,
            min: 1,
            max: SelectExecutionOptions.HardCommandTimeoutSeconds);

        // Normalize command timeout into the connection string
        var builder = new SqlConnectionStringBuilder(options.ConnectionString)
        {
            CommandTimeout = options.Select.CommandTimeoutSeconds
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
