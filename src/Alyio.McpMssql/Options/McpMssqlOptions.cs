// MIT License

using System.ComponentModel.DataAnnotations;

namespace Alyio.McpMssql.Options;

/// <summary>
/// Root configuration options for the MCP SQL Server integration.
/// </summary>
public sealed class McpMssqlOptions
{
    /// <summary>
    /// SQL Server connection string.
    /// </summary>
    [Required]
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Execution options for interactive SELECT operations.
    /// </summary>
    public SelectExecutionOptions Select { get; set; } = new();

    // Reserved for future expansion:
    // public AnalyzeExecutionOptions Analyze { get; init; } = new();

    /// <summary>
    /// Configuration key for the SQL Server connection string.
    /// </summary>
    internal const string ConnectionStringKey = "MCP_MSSQL_CONNECTION_STRING";
}
