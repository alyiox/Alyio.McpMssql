// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes server-enforced execution rules and defaults
/// that govern SQL operations.
/// </summary>
[McpServerResourceType]
public static class ExecutionContext
{
    /// <summary>
    /// Provides the current execution context, including
    /// defaults and hard limits applied to query execution.
    /// </summary>
    [McpServerResource(
        Name = "execution context",
        UriTemplate = "mssql://context/execution",
        MimeType = "application/json")]
    [Description(
        "Server-enforced execution rules and defaults that govern SQL operations. " +
        "Includes row limits, hard caps, and execution timeouts. " +
        "Values are scoped by operation type (e.g. SELECT).")]
    public static Task<string> GetAsync(
        IExecutionContextService options,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsTextAsync(
            async ct => await options.GetContextAsync(ct),
            cancellationToken);
    }
}

