// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;
using static System.Net.Mime.MediaTypeNames;

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
    /// <param name="server">
    /// The SQL Server service bound to the active connection.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A JSON representation of the current SQL Server connection context.
    /// </returns>
    [McpServerResource(
        UriTemplate = "mssql://connection/context",
        Name = "Connection Context",
        MimeType = Application.Json)]
    [Description(
        "Represents the current SQL Server connection context, including server identity, " +
        "effective user, current database, and engine version. " +
        "Use this resource as stable context for reasoning about permissions and SQL feature compatibility.")]
    public static Task<string> GetSqlConnectionContextAsync(ISqlServerService server, CancellationToken cancellationToken = default)
    {
        return MssqlExecutor.ExecuteAsync(
            async () => await server.GetConnectionContextAsync(cancellationToken));
    }
}

