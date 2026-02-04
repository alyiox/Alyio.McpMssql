// MIT License

using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Alyio.McpMssql.Tools;

/// <summary>
/// Executor for MCP tools with standardized error handling.
/// Wraps tool execution and translates exceptions into MCP-compatible errors.
/// </summary>
internal static class ToolExecutor
{
    /// <summary>
    /// Executes a tool function with standardized error handling.
    /// </summary>
    /// <param name="toolFunc">The tool function to execute.</param>
    /// <returns>The tool result as a JSON string.</returns>
    public static async Task<string> ExecuteAsync(Func<Task<string>> toolFunc)
    {
        try
        {
            return await toolFunc();
        }
        catch (ArgumentException ex)
        {
            throw new McpToolException("INVALID_INPUT", ex.Message, ex);
        }
        catch (SqlException ex)
        {
            throw new McpToolException("SQL_ERROR", FormatSqlException(ex), ex);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase))
        {
            throw new McpToolException("CONNECTION_FAILED", ex.Message, ex);
        }
        catch (TimeoutException ex)
        {
            throw new McpToolException("TIMEOUT", "Query execution timed out.", ex);
        }
        catch (JsonException ex)
        {
            throw new McpToolException("INVALID_JSON", $"Invalid JSON in parameters: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not McpToolException)
        {
            throw new McpToolException("INTERNAL_ERROR", $"Unexpected error: {ex.Message}", ex);
        }
    }

    private static string FormatSqlException(SqlException ex)
    {
        return $"SQL Error {ex.Number}: {ex.Message}";
    }
}

/// <summary>
/// Custom exception for MCP tool errors with error codes.
/// </summary>
public class McpToolException : Exception
{
    /// <summary>
    /// Gets the error code associated with this exception.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpToolException"/> class with a specified error code and message.
    /// </summary>
    /// <param name="errorCode">The error code for this exception.</param>
    /// <param name="message">The error message.</param>
    public McpToolException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpToolException"/> class with a specified error code, message, and inner exception.
    /// </summary>
    /// <param name="errorCode">The error code for this exception.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public McpToolException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Returns a string representation of the exception including the error code.
    /// </summary>
    /// <returns>A string in the format "[ErrorCode] Message".</returns>
    public override string ToString()
    {
        return $"[{ErrorCode}] {Message}";
    }
}
