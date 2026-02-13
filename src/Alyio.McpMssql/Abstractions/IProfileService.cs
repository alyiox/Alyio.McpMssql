// MIT License

using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Resolves MCP MSSQL connection profiles from configured options and provides
/// profile context (available profiles) for discovery.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Resolves a profile by name.
    /// </summary>
    /// <param name="profileName">
    /// Optional profile name. If null or empty, the default profile
    /// is resolved.
    /// </param>
    /// <returns>The resolved profile options.</returns>
    McpMssqlProfileOptions Resolve(string? profileName = null);

    /// <summary>
    /// Gets the available profiles: names and descriptions.
    /// </summary>
    /// <returns>The list of configured profiles.</returns>
    IReadOnlyList<Profile> GetProfiles();
}
