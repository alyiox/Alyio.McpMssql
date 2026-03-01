// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Read-only T-SQL query tools: execute SELECT statements and
/// analyze execution plans.
/// </summary>
[McpServerToolType]
public static class QueryTools
{
    /// <summary>
    /// Execute read-only T-SQL SELECT.
    /// </summary>
    [McpServerTool]
    [Description(
        "[MSSQL] Execute Read-only T-SQL SELECT and return tabular results. " +
        "Results are bounded by server-enforced limits. Only SELECT is allowed. " +
        "Use TOP or OFFSET-FETCH in the query for pagination.")]
    public static Task<QueryResult> RunQueryAsync(
        IQueryService queryService,
        [Description("Read-only T-SQL SELECT statement. Use @paramName syntax for parameters. For IN/NOT IN, use numbered params (e.g. @id_0, @id_1).")]
        string sql,
        [Description("Optional. Profile name. Src: profiles.")]
        string? profile = null,
        [Description("Optional. Catalog (database) name. Src: catalogs.")]
        string? catalog = null,
        [Description("Optional parameter values keyed by name (without '@').")]
        IReadOnlyDictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            ct => queryService.RunQueryAsync(sql, catalog, parameters, profile, ct),
            cancellationToken);
    }

    /// <summary>
    /// Analyze a read-only T-SQL SELECT execution plan.
    /// </summary>
    [McpServerTool]
    [Description(
        "[MSSQL] Analyze execution plan for a read-only SELECT. " +
        "Returns a compact JSON summary of cost, top operators, cardinality issues, " +
        "warnings, missing indexes, wait stats, and statistics. " +
        "Full XML plan available via the returned `plan_uri`.")]
    public static Task<AnalyzeResult> AnalyzeQueryAsync(
        IQueryService queryService,
        [Description("Read-only T-SQL SELECT statement to analyze. Only SELECT is allowed.")]
        string sql,
        [Description("Optional. Profile name. Src: profiles.")]
        string? profile = null,
        [Description("Optional. Catalog (database) name. Src: catalogs.")]
        string? catalog = null,
        [Description("Optional parameter values keyed by name (without '@').")]
        IReadOnlyDictionary<string, object>? parameters = null,
        [Description("Optional. When true, returns the estimated plan without executing the query. Default: false (actual plan with runtime statistics).")]
        bool estimated = false,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            ct => queryService.AnalyzeQueryAsync(sql, catalog, parameters, profile, estimated, ct),
            cancellationToken);
    }
}
