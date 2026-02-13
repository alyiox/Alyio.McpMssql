// MIT License

namespace Alyio.McpMssql.Configuration;

/// <summary>
/// Well-known environment variable keys used to override settings
/// on the default MCP MSSQL profile.
///
/// These keys exist primarily for backward compatibility and
/// operational convenience. They apply only to the default profile
/// and are evaluated before profile-level configuration.
/// </summary>
internal static class DefaultProfileKeys
{
    /// <summary>
    /// Overrides the connection string of the default profile.
    /// </summary>
    public const string ConnectionString = "MCPMSSQL_CONNECTION_STRING";

    /// <summary>
    /// Overrides the optional description of the default profile
    /// (for tooling discovery and AI agents).
    /// </summary>
    public const string Description = "MCPMSSQL_DESCRIPTION";

    /// <summary>
    /// Overrides the maximum number of rows returned by query
    /// operations on the default profile.
    /// </summary>
    public const string QueryMaxRows = "MCPMSSQL_QUERY_MAX_ROWS";

    /// <summary>
    /// Overrides the SQL command timeout (in seconds) for query
    /// operations on the default profile.
    /// </summary>
    public const string QueryCommandTimeoutSeconds = "MCPMSSQL_QUERY_COMMAND_TIMEOUT_SECONDS";

    /// <summary>
    /// Overrides the default maximum number of rows used by query
    /// operations when no explicit limit is configured on the
    /// default profile.
    /// </summary>
    public const string QueryDefaultMaxRows = "MCPMSSQL_QUERY_DEFAULT_MAX_ROWS";
}
