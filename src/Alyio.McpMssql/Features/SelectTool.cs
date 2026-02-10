// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Interactive read-only data access tools.
/// 
/// Provides safe, bounded SELECT execution intended for
/// exploration, inspection, and reasoning over SQL Server data.
/// </summary>
[McpServerToolType]
public static class SelectTool
{
    /// <summary>
    /// Executes a read-only SELECT statement and returns a bounded tabular result.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description(
        "Executes a read-only SQL SELECT statement and returns tabular results. " +
        "Results are bounded by server-enforced limits to ensure safe, interactive use.")]
    public static Task<QueryResult> SelectAsync(
        ISelectService selectService,

        [Description(
            "SQL SELECT statement. Must be read-only. " +
            "Use @paramName syntax for parameters.")]
        string sql,

        [Description(
            "Optional catalog (database) name. " +
            "If omitted, the current catalog is used.")]
        string? catalog = null,

        [Description(
            "Optional parameter values keyed by parameter name (without '@').")]
        IReadOnlyDictionary<string, object>? parameters = null,

        [Description(
            "Optional maximum number of rows to return. " +
            "The value is clamped to server-enforced limits.")]
        int? maxRows = null,

        [Description(
            "Optional profile name. If omitted, the default profile is used.")]
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

