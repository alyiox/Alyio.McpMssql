// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes read-only metadata about the connected SQL Server instance.
/// </summary>
[McpServerResourceType]
public static class ServerContext
{
    /// <summary>
    /// Returns core SQL Server properties such as engine edition, version,
    /// server name, and related environment metadata.
    /// </summary>
    [McpServerResource(
        Name = "Server Properties",
        UriTemplate = "mssql://{profile}/context/server/properties",
        MimeType = "application/json")]
    [Description(
        "SQL Server instance properties derived from SERVERPROPERTY and related metadata. " +
        "Use this resource to reason about engine edition, version compatibility, feature availability, " +
        "and server-level execution behavior.")]
    public static Task<string> GetPropertiesAsync(
        IServerContextService server,
        [Description("Profile name (e.g. default).")]
        string profile,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsTextAsync(
            ct => server.GetPropertiesAsync(profile, ct),
            cancellationToken);
    }
}
