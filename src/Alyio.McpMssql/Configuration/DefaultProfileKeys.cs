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
    public const string ConnectionString
        = "MCP_MSSQL_CONNECTION_STRING";

    /// <summary>
    /// Overrides the maximum number of rows returned by SELECT
    /// operations on the default profile.
    /// </summary>
    public const string SelectMaxRows
        = "MCP_MSSQL_SELECT_MAX_ROWS";

    /// <summary>
    /// Overrides the SQL command timeout (in seconds) for SELECT
    /// operations on the default profile.
    /// </summary>
    public const string SelectCommandTimeoutSeconds
        = "MCP_MSSQL_SELECT_COMMAND_TIMEOUT_SECONDS";

    /// <summary>
    /// Overrides the default maximum number of rows used by SELECT
    /// operations when no explicit limit is configured on the
    /// default profile.
    /// </summary>
    public const string SelectDefaultMaxRows
        = "MCP_MSSQL_SELECT_DEFAULT_MAX_ROWS";
}
