// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Write T-SQL tools: execute DDL/DML commands against profiles that
/// explicitly opt in to writes.
/// </summary>
[McpServerToolType]
public static class WriteTools
{
    /// <summary>
    /// Execute an arbitrary write T-SQL command (DDL/DML).
    /// </summary>
    [McpServerTool(UseStructuredContent = true, Destructive = true, ReadOnly = false)]
    [Description(
        "[MSSQL] Execute write T-SQL (DDL/DML). " +
        "Requires a write-enabled profile; rejected on read-only profiles (the default). " +
        "Runs arbitrary statements/batches; intended for human-supervised use. " +
        "Caller manages transactions (BEGIN/COMMIT/ROLLBACK). " +
        "Returns rows affected (-1 for DDL) and any server messages. " +
        "For reads use run_query.")]
    public static Task<CommandResult> RunCommandAsync(
        ICommandService commandService,
        [Description("Write T-SQL to execute. Bind @paramName placeholders for values.")]
        string sql,
        [Description("If omitted or empty, uses the default profile. Must be write-enabled. Src: profiles.")]
        string? profile = null,
        [Description("If omitted, uses the active catalog on the connection. Src: catalogs.")]
        string? catalog = null,
        [Description("Values for SQL parameters; keys are names without '@' (e.g. id → @id).")]
        IReadOnlyDictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            ct => commandService.ExecuteAsync(sql, catalog, parameters, profile, ct),
            cancellationToken);
    }
}
