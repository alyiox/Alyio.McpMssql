// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Describes server-enforced execution policies and defaults
/// for SQL operations.
/// </summary>
public sealed class ExecutionContext
{
    /// <summary>
    /// Execution constraints for bounded, read-only SELECT operations.
    /// </summary>
    public required SelectExecutionContext Select { get; init; }

    // Future execution contexts:
    // public AnalyzeExecutionOptions? Analyze { get; init; }
    // public ExportExecutionOptions? Export { get; init; }
}

