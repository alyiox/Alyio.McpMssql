// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes read-only information about the current SQL Server connection environment.
/// </summary>
[McpServerResourceType]
public static class ConnectionContext
{
    /// <summary>
    /// Provides the active SQL Server connection context, including server identity,
    /// effective user, current database, and engine version.
    /// </summary>
    [McpServerResource(
        Name = "connection context",
        UriTemplate = "mssql://context/connection",
        MimeType = "application/json")]
    [Description(
        "Current SQL Server connection context: server identity, effective user, database, and engine version. " +
        "Use this context to reason about permissions, execution scope, and SQL feature compatibility.")]
    public static Task<string> GetAsync(
        IConnectionContextService server,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsTextAsync(
            server.GetConnectionContextAsync,
            cancellationToken);
    }
}
