// MIT License

using Alyio.McpMssql.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Write operations against SQL Server: executing arbitrary T-SQL
/// (DDL/DML) commands for profiles that opt in to writes.
/// </summary>
public interface ICommandService
{
    /// <summary>
    /// Executes an arbitrary T-SQL command against a write-enabled profile.
    /// </summary>
    /// <param name="sql">The T-SQL command (or batch) to execute.</param>
    /// <param name="catalog">Optional catalog (database) name.</param>
    /// <param name="parameters">Optional parameter values keyed by name.</param>
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the resolved profile does not permit write operations.
    /// </exception>
    Task<CommandResult> ExecuteAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        string? profile = null,
        CancellationToken cancellationToken = default);
}
