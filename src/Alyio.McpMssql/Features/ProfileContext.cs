// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes the list of configured MCP MSSQL profiles so that hosts and
/// agents can discover which profile names are valid for tools and resources.
/// </summary>
[McpServerToolType]
[McpServerResourceType]
public static class ProfileContext
{
    /// <summary>
    /// Resource that returns available profiles and the default profile name.
    /// No profile segment in the URI; this is server-level metadata.
    /// </summary>
    [McpServerResource(
        Name = "profile context",
        UriTemplate = "mssql://context/profiles",
        MimeType = "application/json")]
    [Description("List configured connection profiles and the default profile name.")]
    public static Task<string> GetProfilesAsTextAsync(
        IProfileService service,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsTextAsync(
            _ => Task.FromResult(service.GetContext()),
            cancellationToken);
    }

    /// <summary>
    /// Tool that returns available profiles and the default profile name.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description(
        "List configured MCP MSSQL connection profiles. " +
        "Use this to discover valid profile names for other tools and resources (e.g. default, warehouse).")]
    public static Task<Models.ProfileContext> ListProfilesAsync(
        IProfileService service,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            _ => Task.FromResult(service.GetContext()),
            cancellationToken);
    }
}

