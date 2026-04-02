// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Resources for server metadata: list profiles (mssql://profiles) and server properties (mssql://server-properties). Mirror list_profiles and get_server_properties tools.
/// </summary>
[McpServerResourceType]
public static class ServerResources
{
    /// <summary>
    /// List configured profiles.
    /// </summary>
    [McpServerResource(
        Name = "profiles",
        UriTemplate = "mssql://profiles",
        MimeType = "application/json")]
    [Description(
        "[MSSQL] List configured connection profiles. " +
        "Same data as list_profiles; use when the client prefers resources over tools.")]
    public static async Task<string> ListProfilesAsync(
        IProfileService profileService,
        CancellationToken cancellationToken = default)
    {
        return await McpExecutor.RunAsTextAsync(
            _ => Task.FromResult(profileService.GetProfiles()),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get server properties and execution limits.
    /// </summary>
    [McpServerResource(
        Name = "server-properties",
        UriTemplate = "mssql://server-properties{?profile}",
        MimeType = "application/json")]
    [Description(
        "[MSSQL] Get server properties and execution limits. " +
        "Same data as get_server_properties.")]
    public static async Task<string> GetServerPropertiesAsync(
        IServerContextService serverContextService,
        [Description("If omitted or empty, uses the default profile. Src: profiles.")]
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        return await McpExecutor.RunAsTextAsync(async ct =>
            await serverContextService.GetPropertiesAsync(profile, ct).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }
}
