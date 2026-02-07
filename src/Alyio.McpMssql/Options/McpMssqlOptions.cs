// MIT License

using System.ComponentModel.DataAnnotations;

namespace Alyio.McpMssql.Options;

/// <summary>
/// Configuration for the MCP SQL Server tool.
/// Values are bound from environment variables (via IConfiguration) and then clamped for safety.
/// </summary>
public sealed class McpMssqlOptions
{
    /// <summary>
    /// Connection string used by the tool.
    /// </summary>
    [Required]
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Default maximum number of rows returned when a query call doesn't specify maxRows.
    /// </summary>
    public int DefaultMaxRows { get; set; } = 200;

    /// <summary>
    /// Maximum number of rows the server will return for a single query call.
    /// </summary>
    public int HardMaxRows { get; set; } = 5000;

    /// <summary>
    /// SqlCommand timeout in seconds. 0 means infinite in SqlClient, but we never allow 0 here.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Non-configurable absolute safety ceiling for rows.
    /// </summary>
    public const int AbsoluteMaxRowsCeiling = 50_000;

    /// <summary>
    /// Non-configurable absolute safety ceiling for command timeout in seconds.
    /// </summary>
    public const int AbsoluteCommandTimeoutSecondsCeiling = 300;

    /// <summary>
    /// Configuration key for the connection string.
    /// </summary>
    public const string ConnectionStringKey = "MCP_MSSQL_CONNECTION_STRING";

    /// <summary>
    /// Configuration key for default max rows.
    /// </summary>
    public const string DefaultMaxRowsKey = "MCP_MSSQL_DEFAULT_MAX_ROWS";

    /// <summary>
    /// Configuration key for hard max rows.
    /// </summary>
    public const string HardMaxRowsKey = "MCP_MSSQL_HARD_MAX_ROWS";

    /// <summary>
    /// Configuration key for command timeout seconds.
    /// </summary>
    public const string CommandTimeoutSecondsKey = "MCP_MSSQL_COMMAND_TIMEOUT_SECONDS";
}

