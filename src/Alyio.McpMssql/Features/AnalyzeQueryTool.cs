// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Analyze a read-only T-SQL SELECT to capture the execution plan
/// and return a compact tuning summary.
/// </summary>
[McpServerToolType]
public static class AnalyzeQueryTool
{
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
        IAnalyzeService analyzeService,
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
            ct => analyzeService.AnalyzeAsync(sql, catalog, parameters, profile, estimated, ct),
            cancellationToken);
    }
}
