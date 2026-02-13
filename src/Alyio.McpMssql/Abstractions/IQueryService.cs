// MIT License

using Alyio.McpMssql.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Executes bounded, read-only SELECT operations against SQL Server.
/// Intended for interactive, ad-hoc data exploration and inspection.
/// </summary>
public interface IQueryService
{
    /// <summary>
    /// Executes a read-only SELECT query and returns a bounded tabular result.
    /// </summary>
    /// <param name="sql">The read-only SELECT statement to execute.</param>
    /// <param name="catalog">Optional catalog (database) name.</param>
    /// <param name="parameters">Optional parameter values keyed by name.</param>
    /// <param name="maxRows">Optional maximum rows; clamped to server limits.</param>
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<QueryResult> ExecuteAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        int? maxRows = null,
        string? profile = null,
        CancellationToken cancellationToken = default);
}
