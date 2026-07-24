// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Represents the outcome of a write command (DDL/DML) executed
/// via the <c>run_command</c> tool.
/// </summary>
public sealed class CommandResult
{
    /// <summary>
    /// Number of rows affected by the command, as reported by the provider.
    /// DDL statements and batches that report no row count return <c>-1</c>.
    /// </summary>
    public required int RowsAffected { get; init; }

    /// <summary>
    /// Informational and warning messages emitted by the server during
    /// execution (e.g. <c>PRINT</c> output). Empty when none were raised.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } = [];
}
