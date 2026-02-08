// MIT License

using System.ComponentModel.DataAnnotations;

namespace Alyio.McpMssql.DependencyInjection;

/// <summary>
/// Configuration options for the MCP SQL Server integration.
/// Values are bound from configuration and clamped for safety.
/// </summary>
public sealed class McpMssqlOptions
{
    /// <summary>
    /// SQL Server connection string.
    /// </summary>
    [Required]
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Default number of rows returned when a query does not
    /// explicitly specify a row limit.
    /// </summary>
    public int DefaultMaxRows { get; init; } = 10;

    /// <summary>
    /// Maximum number of rows that may be returned for a single query.
    /// This value is clamped to <see cref="HardRowLimit"/>.
    /// </summary>
    public int RowLimit { get; set; } = 5_000;

    /// <summary>
    /// Maximum execution time for a SQL command, in seconds.
    /// This value is clamped to <see cref="HardCommandTimeout"/>.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    // -------------------------
    // Configuration keys
    // -------------------------

    /// <summary>
    /// Configuration key for the SQL Server connection string.
    /// </summary>
    public const string ConnectionStringKey = "MCP_MSSQL_CONNECTION_STRING";

    /// <summary>
    /// Configuration key for the default maximum number of rows.
    /// </summary>
    public const string DefaultMaxRowsKey = "MCP_MSSQL_DEFAULT_MAX_ROWS";

    /// <summary>
    /// Configuration key for the maximum number of rows.
    /// </summary>
    public const string RowLimitKey = "MCP_MSSQL_ROW_LIMIT";

    /// <summary>
    /// Configuration key for the SQL command timeout.
    /// </summary>
    public const string CommandTimeoutSecondsKey = "MCP_MSSQL_COMMAND_TIMEOUT_SECONDS";

    // -------------------------
    // Hard safety invariants
    // -------------------------

    /// <summary>
    /// Absolute, non-configurable hard limit for query row counts.
    /// </summary>
    internal const int HardRowLimit = 50_000;

    /// <summary>
    /// Absolute, non-configurable hard limit for command execution time, in seconds.
    /// </summary>
    internal const int HardCommandTimeout = 300;
}

