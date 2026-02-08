// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Read-only query execution tools.
/// Provides a safe SELECT-only query engine with enforced row limits.
/// </summary>
[McpServerToolType]
public static class QueryEngine
{
    /// <summary>
    /// Executes a read-only SELECT query and returns a tabular result.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description("Executes a read-only SELECT query and returns tabular results.")]
    public static Task<QueryResult> SelectAsync(
        IQueryService queryService,

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
            "Clamped to the server-enforced row limit.")]
        int? maxRows = null,

        CancellationToken cancellationToken = default)
    {
        return MssqlExecutor.RunAsync(
            ct => queryService.ExecuteSelectAsync(sql, catalog, parameters, maxRows, ct), cancellationToken);
    }
}

