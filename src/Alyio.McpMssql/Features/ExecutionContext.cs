// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;
using ExecutionContextModel = Alyio.McpMssql.Models.ExecutionContext;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes execution rules and defaults for this MCP server's
/// Microsoft SQL Server / Azure SQL Database operations.
/// </summary>
[McpServerToolType]
[McpServerResourceType]
public static class ExecutionContext
{
    /// <summary>
    /// Provides the current execution context, including
    /// defaults and hard limits applied to query execution.
    /// </summary>
    [McpServerResource(
        Name = "execution context",
        UriTemplate = "mssql://{profile}/context/execution",
        MimeType = "application/json")]
    [Description(
        "Microsoft SQL Server / Azure SQL Database execution rules and defaults for this MCP server (row limits, timeouts). " +
        "Scoped by profile and operation type (e.g. SELECT). Profiles are scoped to this server only.")]
    public static Task<string> GetAsync(
        IExecutionContextService options,
        [Description("Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource.")]
        string profile,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsTextAsync(
            async ct => await options.GetContextAsync(profile, ct).ConfigureAwait(false),
            cancellationToken);
    }

    /// <summary>
    /// Tool that returns the execution context (row limits, timeouts, etc.)
    /// for the given profile.
    /// </summary>
    [McpServerTool]
    [Description(
        "Returns execution rules and defaults for this MCP server's Microsoft SQL Server / Azure SQL Database operations. " +
        "Includes SELECT row limits, hard caps, and command timeouts. Use to reason about limits before running T-SQL SELECTs.")]
    public static Task<ExecutionContextModel> GetExecutionContextAsync(
        IExecutionContextService options,
        [Description("Optional. Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource. If omitted, default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            async ct => await options.GetContextAsync(profile, ct).ConfigureAwait(false),
            cancellationToken);
    }
}

