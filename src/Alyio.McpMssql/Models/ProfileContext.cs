// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// List of configured MCP MSSQL profiles.
/// </summary>
public sealed class ProfileContext
{
    /// <summary>
    /// Available profile names and optional descriptions.
    /// </summary>
    public required IReadOnlyList<Profile> Profiles { get; init; }
}
