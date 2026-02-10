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
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    ValueTask<ExecutionContext> GetContextAsync(
        string? profile = null,
        CancellationToken cancellationToken = default);
}

