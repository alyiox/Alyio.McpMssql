// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Server-enforced execution policies and defaults for SQL operations.
/// </summary>
public sealed class ExecutionLimits
{
    /// <summary>
    /// Execution constraints for bounded, read-only queries.
    /// </summary>
    public required QueryLimits Query { get; init; }

    // Future execution contexts:
    // public AnalyzeExecutionOptions? Analyze { get; init; }
    // public ExportExecutionOptions? Export { get; init; }
}
