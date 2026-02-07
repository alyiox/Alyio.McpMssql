// MIT License

using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol;

namespace Alyio.McpMssql.Internal;

/// <summary>
/// Provides a unified execution wrapper for MCP capabilities (Tools, Resources, and Prompts),
/// ensuring standardized JSON serialization and protocol-compliant error translation.
/// </summary>
internal static class MssqlExecutor
{
    /// <summary>
    /// Executes the specified operation, capturing exceptions and translating them 
    /// into <see cref="McpException"/> to prevent generic protocol errors.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <returns>A JSON-serialized string representation of the result, or the raw string if already serialized.</returns>
    /// <exception cref="McpException">Thrown when the underlying operation fails, containing the original error message.</exception>
    public static async Task<string> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            T result = await operation();

            if (result is string s)
            {
                return s;
            }

            return JsonSerializer.Serialize(result, s_jsonOptions);
        }
        catch (Exception ex) when (ex is not McpException)
        {
            throw new McpException(ex.Message, ex);
        }
    }

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}

