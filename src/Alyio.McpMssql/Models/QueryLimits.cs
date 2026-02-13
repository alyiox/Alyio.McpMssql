// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Constraints and defaults applied to interactive ad-hoc queries.
/// These limits are enforced by the server to ensure safe, bounded execution.
/// </summary>
public sealed class QueryLimits
{
    /// <summary>
    /// Default number of rows returned when an inspection query
    /// does not explicitly specify a limit.
    /// </summary>
    public required OptionDescriptor<int> DefaultMaxRows { get; init; }

    /// <summary>
    /// Absolute maximum number of rows that may be returned
    /// by any inspection query.
    /// </summary>
    public required OptionDescriptor<int> HardRowLimit { get; init; }

    /// <summary>
    /// Maximum execution time allowed for an inspection query,
    /// in seconds.
    /// </summary>
    public required OptionDescriptor<int> CommandTimeoutSeconds { get; init; }
}
