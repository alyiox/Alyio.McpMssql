// MIT License

using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace Alyio.McpMssql.Tests.Infrastructure.Database;

/// <summary>
/// Helper class for initializing test databases by executing SQL scripts.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Executes an embedded SQL script resource with support for GO batch separators.
    /// </summary>
    /// <param name="connectionString">SQL Server connection string.</param>
    /// <param name="resourceName">Fully qualified embedded resource name (e.g., "McpMssql.Tool.Tests.Scripts.schema.sql").</param>
    public static async Task ExecuteEmbeddedScriptAsync(string connectionString, string resourceName)
    {
        var script = LoadEmbeddedResource(resourceName);
        await ExecuteScriptAsync(connectionString, script);
    }

    /// <summary>
    /// Executes a SQL script with support for GO batch separators.
    /// </summary>
    /// <param name="connectionString">SQL Server connection string.</param>
    /// <param name="script">SQL script content.</param>
    public static async Task ExecuteScriptAsync(string connectionString, string script)
    {
        var batches = SplitSqlBatches(script);

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        foreach (var batch in batches)
        {
            if (string.IsNullOrWhiteSpace(batch))
                continue;

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = batch;
            cmd.CommandTimeout = 60; // Longer timeout for DDL operations
            await cmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// Loads an embedded resource as a string.
    /// </summary>
    private static string LoadEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource not found: {resourceName}. Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Splits SQL script into batches separated by GO statements.
    /// </summary>
    private static IEnumerable<string> SplitSqlBatches(string script)
    {
        // Split on GO statements (case-insensitive, must be on its own line)
        var batches = Regex.Split(
            script,
            @"^\s*GO\s*$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase
        );

        return batches.Where(b => !string.IsNullOrWhiteSpace(b));
    }
}
