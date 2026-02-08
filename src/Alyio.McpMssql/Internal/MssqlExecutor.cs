// MIT License

using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol;

namespace Alyio.McpMssql.Internal;

/// <summary>
/// Executes MCP operations with standardized error handling
/// and protocol-friendly text output.
/// </summary>
/// <remarks>
/// Cancellation is propagated without translation.
/// Non-MCP exceptions are normalized to <see cref="McpException"/>.
/// </remarks>
internal static class MssqlExecutor
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Executes an asynchronous operation and normalizes exceptions.
    /// </summary>
    /// <typeparam name="T">The operation result type.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The operation result.</returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested.
    /// </exception>
    /// <exception cref="McpException">
    /// Thrown when the operation fails and the exception is translated.
    /// </exception>
    public static async Task<T> RunAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        try
        {
            return await operation(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is not McpException)
        {
            throw new McpException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Executes an asynchronous operation and returns its result as text.
    /// </summary>
    /// <typeparam name="T">The operation result type.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>
    /// A string result. Non-string values are JSON-serialized;
    /// <c>null</c> results return an empty string.
    /// </returns>
    /// <remarks>
    /// JSON output uses <see cref="JsonNamingPolicy.SnakeCaseLower"/> naming
    /// and omits null values to align with MCP expectations.
    /// </remarks>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested.
    /// </exception>
    /// <exception cref="McpException">
    /// Thrown when the operation fails and the exception is translated.
    /// </exception>
    public static async Task<string> RunAsTextAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        var result = await RunAsync(operation, cancellationToken);

        return result switch
        {
            string s => s,
            null => string.Empty,
            _ => JsonSerializer.Serialize(result, s_jsonOptions)
        };
    }
}
