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
    [McpServerTool(UseStructuredContent = true)]
    [Description(
        "[MSSQL] Execute Read-only T-SQL SELECT and return tabular results. " +
        "Results are bounded by server-enforced limits; only SELECT is allowed (no DML/DDL). " +
        "Use TOP or OFFSET-FETCH for pagination. " +
        "Use snapshot=true for large/reporting queries — returns a resource URI instead of inline rows. " +
        "Prefer analyze_query when tuning plans.")]
    public static Task<QueryResult> RunQueryAsync(
        IQueryService queryService,
        [Description("Read-only T-SQL SELECT. Bind @paramName placeholders; for IN lists use numbered names (e.g. @id_0, @id_1).")]
        string sql,
        [Description("If omitted or empty, uses the default profile. Src: profiles.")]
        string? profile = null,
        [Description("If omitted, uses the active catalog on the connection. Src: catalogs.")]
        string? catalog = null,
        [Description("Values for SQL parameters; keys are names without '@' (e.g. id → @id).")]
        IReadOnlyDictionary<string, object>? parameters = null,
        [Description(
            "When true, persists the full result as a CSV snapshot resource and returns its URI " +
            "instead of inline data. Use for large or reporting queries to avoid flooding the " +
            "context window. Default false.")]
        bool snapshot = false,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            ct => queryService.RunQueryAsync(sql, catalog, parameters, profile, snapshot, ct),
            cancellationToken);
    }

    /// <summary>
    /// Analyze a read-only T-SQL SELECT execution plan.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description(
        "[MSSQL] Analyze execution plan for a read-only SELECT. " +
        "Returns a compact JSON summary (cost, operators, cardinality, warnings, indexes, waits, stats). " +
        "Fetch full XML from plan_uri; does not return raw result rows.")]
    public static Task<AnalyzeResult> AnalyzeQueryAsync(
        IQueryService queryService,
        [Description("Read-only T-SQL SELECT to analyze; only SELECT is allowed.")]
        string sql,
        [Description("If omitted or empty, uses the default profile. Src: profiles.")]
        string? profile = null,
        [Description("If omitted, uses the active catalog on the connection. Src: catalogs.")]
        string? catalog = null,
        [Description("Values for SQL parameters; keys are names without '@'.")]
        IReadOnlyDictionary<string, object>? parameters = null,
        [Description("When true, returns estimated plan without executing. Default false: actual plan with runtime stats.")]
        bool estimated = false,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            ct => queryService.AnalyzeQueryAsync(sql, catalog, parameters, profile, estimated, ct),
            cancellationToken);
    }
}
