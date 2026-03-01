// MIT License

namespace Alyio.McpMssql.Configuration;

/// <summary>
/// Server-enforced execution defaults for query plan analysis.
/// Analysis runs the full query with <c>SET STATISTICS XML ON</c>
/// to capture the actual execution plan, which can be significantly
/// slower than a bounded interactive query.
/// </summary>
public sealed class AnalyzeOptions
{
    /// <summary>
    /// Maximum execution time for an analysis command, in seconds.
    /// Defaults to 300 (5 minutes) to accommodate complex queries
    /// that need to run to completion for accurate runtime statistics.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 300;
}
