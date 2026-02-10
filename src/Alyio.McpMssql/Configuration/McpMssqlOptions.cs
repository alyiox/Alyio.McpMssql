// MIT License

namespace Alyio.McpMssql.Configuration;

/// <summary>
/// Root configuration options for the MCP SQL Server integration.
///
/// Defines named connection profiles and global defaults used by
/// MCP-exposed SQL tools. These options are typically bound from
/// application configuration at startup and remain static for the
/// lifetime of the server.
/// </summary>
public sealed class McpMssqlOptions
{
    /// <summary>
    /// The well-known name of the default MCP MSSQL profile.
    /// </summary>
    public const string DefaultProfileName = "default";

    /// <summary>
    /// The name of the default profile used when no profile is explicitly
    /// selected by the MCP client.
    /// </summary>
    /// <remarks>
    /// If not specified, the profile named <c>"default"</c> is used.
    /// </remarks>
    public string DefaultProfile { get; set; } = DefaultProfileName;

    /// <summary>
    /// Named SQL Server connection profiles.
    ///
    /// Each profile represents an isolated configuration consisting of
    /// a connection string and feature-specific execution options.
    /// </summary>
    public Dictionary<string, McpMssqlProfileOptions> Profiles { get; set; }
        = new(StringComparer.OrdinalIgnoreCase);
}
