// MIT License

using Microsoft.Extensions.Configuration;

namespace Alyio.McpMssql.Configuration;

/// <summary>
/// Applies environment variable overrides to the default MCP MSSQL
/// profile to preserve backward compatibility with legacy single-
/// connection configurations.
/// </summary>
internal static class DefaultProfileOverrideApplier
{
    public static void Apply(IConfiguration configuration, McpMssqlOptions options)
    {
        if (!options.Profiles.TryGetValue(McpMssqlOptions.DefaultProfileName, out var profile))
        {
            return;
        }

        var connectionString = GetString(configuration, DefaultProfileKeys.ConnectionString);

        if (connectionString is not null)
        {
            profile.ConnectionString = connectionString;
        }

        var description = GetString(configuration, DefaultProfileKeys.Description);
        if (description is not null)
        {
            profile.Description = description;
        }

        var query = profile.Query;

        ApplyInt(
            configuration,
            DefaultProfileKeys.QueryMaxRows,
            v => query.MaxRows = v);

        ApplyInt(
            configuration,
            DefaultProfileKeys.QueryCommandTimeoutSeconds,
            v => query.CommandTimeoutSeconds = v);

        ApplyInt(
            configuration,
            DefaultProfileKeys.QuerySnapshotMaxRows,
            v => query.SnapshotMaxRows = v);

        ApplyInt(
            configuration,
            DefaultProfileKeys.QuerySnapshotCommandTimeoutSeconds,
            v => query.SnapshotCommandTimeoutSeconds = v);

        var analyze = profile.Analyze;

        ApplyInt(
            configuration,
            DefaultProfileKeys.AnalyzeCommandTimeoutSeconds,
            v => analyze.CommandTimeoutSeconds = v);

        var allowWrite = GetBool(configuration, DefaultProfileKeys.AllowWrite);
        if (allowWrite is not null)
        {
            profile.AllowWrite = allowWrite.Value;
        }

        var write = profile.Write;

        ApplyInt(
            configuration,
            DefaultProfileKeys.WriteCommandTimeoutSeconds,
            v => write.CommandTimeoutSeconds = v);
    }

    private static void ApplyInt(IConfiguration configuration, string key, Action<int> apply)
    {
        int? value = GetInt(configuration, key);
        if (value is not null)
        {
            apply(value.Value);
        }
    }

    private static bool? GetBool(IConfiguration configuration, string key)
    {
        string? value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return bool.TryParse(value.Trim(), out var parsed)
            ? parsed
            : null;
    }

    private static string? GetString(IConfiguration configuration, string key)
    {
        string? value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int? GetInt(IConfiguration configuration, string key)
    {
        string? value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value.Trim(), out var parsed)
            ? parsed
            : null;
    }
}

