// MIT License

namespace Alyio.McpMssql.Configuration;

/// <summary>
/// Server-enforced execution defaults and safety limits
/// for interactive read-only queries.
/// </summary>
public sealed class QueryOptions
{
    /// <summary>
    /// Default number of rows returned when no explicit limit is specified.
    /// </summary>
    public int DefaultMaxRows { get; set; } = 100;

    /// <summary>
    /// Maximum number of rows that may be returned for a single query.
    /// Clamped to <see cref="HardRowLimit"/>.
    /// </summary>
    public int MaxRows { get; set; } = 5_000;

    /// <summary>
    /// Maximum execution time for a query command, in seconds.
    /// Clamped to <see cref="HardCommandTimeoutSeconds"/>.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    // -------------------------
    // Hard safety invariants
    // -------------------------

    /// <summary>
    /// Absolute, non-configurable hard limit for query row counts.
    /// </summary>
    internal const int HardRowLimit = 50_000;

    /// <summary>
    /// Absolute, non-configurable hard limit for query execution time.
    /// </summary>
    internal const int HardCommandTimeoutSeconds = 300;
}
