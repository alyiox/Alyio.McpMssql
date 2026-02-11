// MIT License

using Alyio.McpMssql.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

#pragma warning disable IDE0130 // Intentional: extension methods for IServiceCollection
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Dependency injection helpers for the MCP SQL Server integration.
/// </summary>
public static class McpMssqlOptionsExtensions
{
    /// <summary>
    /// Registers and validates MCP MSSQL configuration options.
    ///
    /// Configuration is bound using standard .NET configuration behavior.
    /// MCP-specific flat environment variables are applied to the default
    /// profile only, preserving backward compatibility for single-profile
    /// configurations.
    /// </summary>
    public static IServiceCollection AddMcpMssqlOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<McpMssqlOptions>()
            .Bind(configuration.GetSection("McpMssql"))
            .PostConfigure(options =>
            {
                EnsureProfilesExist(options);
                DefaultProfileOverrideApplier.Apply(configuration, options);
                ValidateAndNormalize(options);
            })
            .ValidateOnStart();

        return services;
    }

    // -------------------------
    // Configuration processing
    // -------------------------

    private static void EnsureProfilesExist(McpMssqlOptions options)
    {
        if (options.Profiles.Count == 0)
        {
            options.Profiles[McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions();
            return;
        }

        if (!options.Profiles.TryGetValue(McpMssqlOptions.DefaultProfileName, out _))
        {
            options.Profiles[McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions();
        }
    }

    private static void ValidateAndNormalize(McpMssqlOptions options)
    {
        if (!options.Profiles.TryGetValue(McpMssqlOptions.DefaultProfileName, out _))
        {
            throw new InvalidOperationException(
                $"Default MCP MSSQL profile '{McpMssqlOptions.DefaultProfileName}' was not found. " +
                $"Available profiles: {string.Join(", ", options.Profiles.Keys)}");
        }

        foreach ((string? name, McpMssqlProfileOptions profile) in options.Profiles)
        {
            if (string.IsNullOrWhiteSpace(profile.ConnectionString))
            {
                throw new InvalidOperationException(
                    $"ConnectionString is required for MCP MSSQL profile '{name ?? "<unnamed>"}'.");

            }

            ClampSelectOptions(profile.Select);
            NormalizeConnectionString(profile);
        }
    }

    // -------------------------
    // Normalization helpers
    // -------------------------

    private static void ClampSelectOptions(SelectExecutionOptions select)
    {
        select.MaxRows = Math.Clamp(
            select.MaxRows,
            min: 1,
            max: SelectExecutionOptions.HardRowLimit);

        select.DefaultMaxRows = Math.Clamp(
            select.DefaultMaxRows,
            min: 1,
            max: select.MaxRows);

        select.CommandTimeoutSeconds = Math.Clamp(
            select.CommandTimeoutSeconds,
            min: 1,
            max: SelectExecutionOptions.HardCommandTimeoutSeconds);
    }

    private static void NormalizeConnectionString(
        McpMssqlProfileOptions profile)
    {
        var builder = new SqlConnectionStringBuilder(profile.ConnectionString!)
        {
            CommandTimeout = profile.Select.CommandTimeoutSeconds
        };

        profile.ConnectionString = builder.ConnectionString;
    }
}

