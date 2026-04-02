// MIT License

using System.ComponentModel;
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
    [McpServerTool]
    [Description(
        "[MSSQL] List configured connection profiles. " +
        "Call before other tools when you must pick a non-default profile.")]
    public static IReadOnlyList<Profile> ListProfiles(
        IProfileService profileService)
    {
        return profileService.GetProfiles();
    }

    /// <summary>
    /// Get server properties and execution limits.
    /// </summary>
    [McpServerTool]
    [Description(
        "[MSSQL] Get server properties and execution limits for a profile " +
        "(timeouts, row caps, and other guardrails).")]
    public static async Task<ServerProperties> GetServerPropertiesAsync(
        IServerContextService serverContextService,
        [Description("If omitted or empty, uses the default profile. Src: profiles.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        return await McpExecutor.RunAsync(async ct =>
            await serverContextService.GetPropertiesAsync(profile, ct).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }
}
