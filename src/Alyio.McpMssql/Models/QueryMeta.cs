// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Metadata describing execution constraints and result completeness
/// for an interactive query.
/// </summary>
public sealed class QueryMeta
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

    /// <summary>
    /// The total number of rows produced by the query,
    /// when known and greater than the number returned.
    /// </summary>
    public int? TotalRowCount { get; init; }
}

