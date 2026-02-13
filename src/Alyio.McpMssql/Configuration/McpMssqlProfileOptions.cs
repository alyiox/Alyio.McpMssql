// MIT License


// MIT License

namespace Alyio.McpMssql.Configuration;

/// <summary>
/// Configuration for a single named SQL Server profile.
///
/// A profile defines how MCP tools connect to and interact with a
/// specific SQL Server database, including connection information
/// and server-enforced execution limits.
/// </summary>
public sealed class McpMssqlProfileOptions
{
    /// <summary>
    /// Optional description of the profile for humans and AI agents.
    ///
    /// Intended for tooling discovery and AI reasoning.
    /// This value has no effect on execution behavior.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// SQL Server connection string for this profile.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Execution options for interactive read-only queries.
    /// </summary>
    public QueryOptions Query { get; set; } = new();

    /// <summary>
    /// The well-known name of the default MCP MSSQL profile.
    /// </summary>
    internal const string DefaultProfileName = "default";
}
