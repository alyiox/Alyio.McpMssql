// MIT License

using ExecutionContext = Alyio.McpMssql.Models.ExecutionContext;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides server-enforced execution rules and defaults
/// that govern how SQL operations are executed.
/// </summary>
public interface IExecutionContextService
{
    /// <summary>
    /// Returns the current execution context, including
    /// defaults and hard limits applied to SQL execution.
    /// </summary>
    ValueTask<ExecutionContext> GetContextAsync(CancellationToken cancellationToken = default);
}

