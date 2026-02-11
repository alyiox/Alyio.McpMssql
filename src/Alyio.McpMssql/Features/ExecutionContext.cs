// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;
using ExecutionContextModel = Alyio.McpMssql.Models.ExecutionContext;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes server-enforced execution rules and defaults
/// that govern SQL operations.
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
        "Server-enforced execution rules and defaults that govern SQL operations. " +
        "Includes row limits, hard caps, and execution timeouts. " +
        "Values are scoped by operation type (e.g. SELECT).")]
    public static Task<string> GetAsync(
        IExecutionContextService options,
        [Description("Profile name (e.g. default).")]
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
    [McpServerTool(UseStructuredContent = true)]
    [Description(
        "Returns server-enforced execution rules and defaults for SQL operations. " +
        "Includes SELECT row limits, hard caps, and command timeouts. " +
        "Use to reason about query limits before running SELECTs.")]
    public static Task<ExecutionContextModel> GetExecutionContextAsync(
        IExecutionContextService options,
        [Description("Optional profile name. If omitted, the default profile is used.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            async ct => await options.GetContextAsync(profile, ct).ConfigureAwait(false),
            cancellationToken);
    }
}

