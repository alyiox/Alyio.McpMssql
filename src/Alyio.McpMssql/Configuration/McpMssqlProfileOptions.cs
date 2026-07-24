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
    /// When <c>false</c> (default), write commands (DDL/DML) are rejected
    /// for this profile; only read-only tools are usable. When <c>true</c>,
    /// the <c>run_command</c> tool may execute arbitrary T-SQL.
    /// </summary>
    /// <remarks>
    /// This is a soft, application-level guard — not a security boundary.
    /// For a genuine read-only guarantee, use a login restricted to
    /// <c>db_datareader</c>. The read-only query tools are unaffected by
    /// this setting.
    /// </remarks>
    public bool AllowWrite { get; set; }

    /// <summary>
    /// Execution options for interactive read-only queries.
    /// </summary>
    public QueryOptions Query { get; set; } = new();

    /// <summary>
    /// Execution options for query plan analysis.
    /// </summary>
    public AnalyzeOptions Analyze { get; set; } = new();

    /// <summary>
    /// Execution options for write commands (DDL/DML).
    /// Only applies when <see cref="AllowWrite"/> is <c>true</c>.
    /// </summary>
    public WriteOptions Write { get; set; } = new();

    /// <summary>
    /// The well-known name of the default MCP MSSQL profile.
    /// </summary>
    internal const string DefaultProfileName = "default";
}
