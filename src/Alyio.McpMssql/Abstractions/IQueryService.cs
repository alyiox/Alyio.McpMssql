// MIT License

using Alyio.McpMssql.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Read-only query operations against SQL Server: executing bounded
/// SELECT queries and capturing execution plans for analysis.
/// </summary>
public interface IQueryService
{
    /// <summary>
    /// Executes a read-only SELECT query and returns a bounded tabular result.
    /// </summary>
    /// <param name="sql">The read-only SELECT statement to execute.</param>
    /// <param name="catalog">Optional catalog (database) name.</param>
    /// <param name="parameters">Optional parameter values keyed by name.</param>
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<QueryResult> RunQueryAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        string? profile = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes the execution plan for the given SQL and returns
    /// a compact summary.
    /// </summary>
    /// <param name="sql">A read-only SELECT statement to analyze.</param>
    /// <param name="catalog">Optional catalog (database) name.</param>
    /// <param name="parameters">Optional parameter values keyed by name.</param>
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="estimated">
    /// When <c>true</c>, returns the estimated plan without executing the query.
    /// When <c>false</c> (default), executes the query to capture the actual
    /// plan with runtime statistics.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<AnalyzeResult> AnalyzeQueryAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        string? profile = null,
        bool estimated = false,
        CancellationToken cancellationToken = default);
}
