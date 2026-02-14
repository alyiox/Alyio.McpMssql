// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Execute read-only T-SQL SELECT for interactive, ad-hoc queries.
/// </summary>
[McpServerToolType]
public static class QueryTool
{
    /// <summary>
    /// Execute read-only T-SQL SELECT.
    /// </summary>
    [McpServerTool]
    [Description(
        "[MSSQL] Execute Read-only T-SQL SELECT and return tabular results. " +
        "Results are bounded by server-enforced limits. Only SELECT is allowed.")]
    public static Task<QueryResult> QueryAsync(
        IQueryService queryService,
        [Description("Read-only T-SQL SELECT statement. Use @paramName syntax for parameters. For IN/NOT IN, use numbered params (e.g. @id_0, @id_1).")]
        string sql,
        [Description("Optional. Profile name. Src: profiles.")]
        string? profile = null,
        [Description("Optional. Catalog (database) name. Src: catalogs.")]
        string? catalog = null,
        [Description("Optional parameter values keyed by name (without '@').")]
        IReadOnlyDictionary<string, object>? parameters = null,
        [Description("Optional max rows (clamped to server limits).")]
        int? maxRows = null,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            ct => queryService.ExecuteAsync(sql, catalog, parameters, maxRows, profile, ct),
            cancellationToken);
    }
}
