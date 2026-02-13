// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Exposes the list of configured Microsoft SQL Server / Azure SQL Database
/// profiles for this MCP server so hosts and agents can discover valid
/// profile names for this server's tools and resources.
/// </summary>
[McpServerToolType]
[McpServerResourceType]
public static class ProfileContext
{
    /// <summary>
    /// Resource that returns available profiles.
    /// No profile segment in the URI; this is server-level metadata.
    /// </summary>
    [McpServerResource(
        Name = "profile context",
        UriTemplate = "mssql://context/profiles",
        MimeType = "application/json")]
    [Description(
        "List configured Microsoft SQL Server / Azure SQL Database connection profiles for this MCP server only. " +
        "Valid profile names for this server's tools and resources (e.g. default, warehouse). " +
        "Use this server's list_profiles tool or this resource to obtain valid values.")]
    public static Task<string> GetProfilesAsTextAsync(
        IProfileService service,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsTextAsync(
            _ => Task.FromResult(service.GetContext()),
            cancellationToken);
    }

    /// <summary>
    /// Tool that returns available profiles.
    /// </summary>
    [McpServerTool]
    [Description(
        "List configured Microsoft SQL Server / Azure SQL Database connection profiles for this MCP server only. " +
        "Use to discover valid profile names for this server's tools and resources (e.g. default, warehouse). " +
        "Profiles are scoped to this server; valid values are returned by this tool or the profile context resource.")]
    public static Task<Models.ProfileContext> ListProfilesAsync(
        IProfileService service,
        CancellationToken cancellationToken = default)
    {
        return McpExecutor.RunAsync(
            _ => Task.FromResult(service.GetContext()),
            cancellationToken);
    }
}

