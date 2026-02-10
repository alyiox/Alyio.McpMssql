// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes the list of configured MCP MSSQL profiles so that hosts and
/// agents can discover which profile names are valid for tools and resources.
/// </summary>
[McpServerToolType]
[McpServerResourceType]
public static class Profiles
{
    /// <summary>
    /// Resource that returns available profiles and the default profile name.
    /// No profile segment in the URI; this is server-level metadata.
    /// </summary>
    [McpServerResource(
        Name = "Profiles",
        UriTemplate = "mssql://profiles",
        MimeType = "application/json")]
    [Description("List configured connection profiles and the default profile name.")]
    public static Task<string> GetProfilesAsTextAsync(
        IOptions<Configuration.McpMssqlOptions> options,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsTextAsync(
            _ => Task.FromResult(BuildResult(options.Value)),
            cancellationToken);
    }

    /// <summary>
    /// Tool that returns available profiles and the default profile name.
    /// </summary>
    [McpServerTool(UseStructuredContent = true)]
    [Description(
        "List configured MCP MSSQL connection profiles. " +
        "Use this to discover valid profile names for other tools and resources (e.g. default, warehouse).")]
    public static Task<ProfileList> ListProfilesAsync(
        IOptions<Configuration.McpMssqlOptions> options,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            _ => Task.FromResult(BuildResult(options.Value)),
            cancellationToken);
    }

    private static ProfileList BuildResult(Configuration.McpMssqlOptions options)
    {
        var list = options.Profiles
            .Select(p => new ProfileInfo
            {
                Name = p.Key,
                Description = string.IsNullOrWhiteSpace(p.Value.Description) ? null : p.Value.Description.Trim(),
            })
            .ToList();

        return new ProfileList
        {
            Profiles = list,
            DefaultProfile = options.DefaultProfile,
        };
    }
}
