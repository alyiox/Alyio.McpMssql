// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes read-only resources that describe the current SQL Server connection environment.
/// </summary>
[McpServerResourceType]
public static class ServerContext
{
    /// <summary>
    /// Represents the current SQL Server connection context, including server identity,
    /// effective user, current database, and engine version.
    /// </summary>
    [McpServerResource(
        UriTemplate = "mssql://connection/context",
        MimeType = "application/json")]
    [Description(
        "Current SQL Server connection context: server identity, effective user, database, and engine version. " +
        "Use for reasoning about permissions and SQL feature compatibility.")]
    public static Task<string> ContextAsync(IServerContextService server, CancellationToken cancellationToken = default)
    {
        return MssqlExecutor.RunAsTextAsync(server.GetConnectionContextAsync, cancellationToken);
    }
}

