// MIT License

using Alyio.McpMssql.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Executes read-only SQL queries against SQL Server.
///
/// This service is responsible for validating and executing SELECT queries
/// and returning bounded, tabular results for analysis and inspection.
/// </summary>
public interface IQueryService
{
    /// <summary>
    /// Executes a read-only SELECT query and returns tabular results.
    /// </summary>
    Task<QueryResult> ExecuteSelectAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        int? maxRows = null,
        CancellationToken cancellationToken = default);
}

