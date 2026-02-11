// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// List of configured MCP MSSQL profiles and the default profile name.
/// </summary>
public sealed class ProfileContext
{
    /// <summary>
    /// Available profile names and optional descriptions.
    /// </summary>
    public required IReadOnlyList<Profile> Profiles { get; init; }

    /// <summary>
    /// Name of the default profile used when no profile is specified.
    /// </summary>
    public required string DefaultProfile { get; init; }
}
