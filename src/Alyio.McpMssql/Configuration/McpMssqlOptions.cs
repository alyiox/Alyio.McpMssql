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
    /// When no profile is specified by the client, this profile is used.
    /// </summary>
    public const string DefaultProfileName = "default";

    /// <summary>
    /// Named SQL Server connection profiles.
    ///
    /// Each profile represents an isolated configuration consisting of
    /// a connection string and feature-specific execution options.
    /// </summary>
    public Dictionary<string, McpMssqlProfileOptions> Profiles { get; set; }
        = new(StringComparer.OrdinalIgnoreCase);
}
