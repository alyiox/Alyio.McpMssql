// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Read-only T-SQL data access for Microsoft SQL Server / Azure SQL Database.
/// Safe, bounded SELECT execution for exploration and reasoning over
/// this server's connected instances.
/// </summary>
[McpServerToolType]
public static class SelectTool
{
    /// <summary>
    /// Executes a read-only SELECT statement and returns a bounded tabular result.
    /// </summary>
    [McpServerTool]
    [Description(
        "Executes a read-only T-SQL SELECT statement against Microsoft SQL Server or Azure SQL Database and returns tabular results. " +
        "Results are bounded by this server's enforced limits. Use only for this MCP server's profiles.")]
    public static Task<QueryResult> SelectAsync(
        ISelectService selectService,

        [Description(
            "Read-only T-SQL SELECT statement. Use @paramName syntax for parameters. " +
            "For IN/NOT IN clauses, expand each value as a separate numbered parameter " +
            "(e.g., WHERE id IN (@id_0, @id_1, @id_2)). Only SELECT is allowed.")]
        string sql,

        [Description(
            "Optional. Catalog (database) name for Microsoft SQL Server / Azure SQL Database. " +
            "Valid values from this server's list_catalogs tool or the catalogs resource. If omitted, current catalog is used.")]
        string? catalog = null,

        [Description(
            "Optional parameter values keyed by parameter name (without '@'). " +
            "Each value must be a scalar (string, number, boolean, or null). " +
            "For IN/NOT IN clauses, provide individually numbered entries " +
            "(e.g., {\"id_0\": 1, \"id_1\": 2, \"id_2\": 3}).")]
        IReadOnlyDictionary<string, object>? parameters = null,

        [Description(
            "Optional maximum number of rows to return. " +
            "The value is clamped to server-enforced limits.")]
        int? maxRows = null,

        [Description(
            "Optional. Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource. If omitted, default profile is used.")]
        string? profile = null,

        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            ct => selectService.ExecuteAsync(
                sql,
                catalog,
                parameters,
                maxRows,
                profile,
                ct),
            cancellationToken);
    }
}

