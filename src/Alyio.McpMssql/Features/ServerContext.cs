// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes read-only metadata about the connected Microsoft SQL Server or
/// Azure SQL Database instance for this MCP server.
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
        "Microsoft SQL Server / Azure SQL Database instance properties for this MCP server (SERVERPROPERTY and related metadata). " +
        "Use to reason about engine edition, version, feature availability. Scoped to this server's profile only.")]
    public static Task<string> GetPropertiesAsync(
        IServerContextService server,
        [Description("Profile name for this MCP server. Valid values from this server's list_profiles tool or the profile context resource.")]
        string profile,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsTextAsync(
            ct => server.GetPropertiesAsync(profile, ct),
            cancellationToken);
    }
}
