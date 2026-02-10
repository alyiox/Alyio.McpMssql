// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Summary information for a single MCP MSSQL profile.
/// </summary>
public sealed class ProfileInfo
{
    /// <summary>
    /// Profile name (e.g. default, warehouse).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional human- or agent-facing description.
    /// </summary>
    public string? Description { get; init; }
}
