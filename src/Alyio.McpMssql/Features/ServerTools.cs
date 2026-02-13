// MIT License

using System.ComponentModel;
using Alyio.McpMssql;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Tools for server metadata: list profiles and get server properties with execution limits.
/// </summary>
[McpServerToolType]
public static class ServerTools
{
    /// <summary>
    /// List configured profiles.
    /// </summary>
    [McpServerTool(Name = "db.profiles")]
    [Description("List configured SQL Server or Azure SQL Database connection profiles.")]
    public static IReadOnlyList<Profile> GetProfiles(
        IProfileService profileService)
    {
        return profileService.GetProfiles();
    }

    /// <summary>
    /// Get server properties and execution limits.
    /// </summary>
    [McpServerTool(Name = "db.server.properties")]
    [Description("Get SQL Server or Azure SQL Database server properties and execution limits.")]
    public static async Task<ServerProperties> GetServerPropertiesAsync(
        IServerContextService serverContextService,
        [Description("Optional. Profile name. Src: profiles.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        return await McpExecutor.RunAsync(async ct =>
            await serverContextService.GetPropertiesAsync(profile, ct).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }
}
