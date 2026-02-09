// MIT License

using Alyio.McpMssql.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Executes bounded, read-only SELECT operations against SQL Server.
/// Intended for interactive data exploration and inspection.
/// </summary>
public interface ISelectService
{
    /// <summary>
    /// Executes a read-only SELECT query and returns a bounded tabular result.
    /// </summary>
    Task<QueryResult> ExecuteAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        int? maxRows = null,
        CancellationToken cancellationToken = default);
}

