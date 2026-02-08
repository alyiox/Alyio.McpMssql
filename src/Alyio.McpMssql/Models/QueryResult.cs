// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Represents the result of an interactive SQL query.
/// </summary>
public sealed class QueryResult : TabularResult
{
    /// <summary>
    /// Indicates whether the result set was truncated due to
    /// the enforced row limit.
    /// </summary>
    public bool Truncated { get; init; }

    /// <summary>
    /// The enforced maximum number of rows that this server
    /// will return for a single query.
    /// </summary>
    public int RowLimit { get; init; }
}

