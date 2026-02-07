// MIT License

using System.Text.Json;
using System.Text.Json.Serialization;
using Alyio.McpMssql.Models;
using ModelContextProtocol;

namespace Alyio.McpMssql.Internal;

/// <summary>
/// Executes tools with standardized error handling.
/// </summary>
internal static class ToolExecutor
{
    /// <summary>
    /// Executes a tool function, translating exceptions into MCP-compatible errors.
    /// </summary>
    /// <param name="tool">The tool function to execute.</param>
    /// <returns>The tool result as a JSON string.</returns>
    public static async Task<string> ExecuteAsync(Func<Task<ToolResponse>> tool)
    {
        try
        {
            ToolResponse response = await tool();
            return JsonSerializer.Serialize(response, s_jsonOptions);
        }
        catch (Exception ex) when (ex is not McpException)
        {
            throw new McpException(ex.Message, ex);
        }
    }

    /// <summary>
    /// JSON serialization options for tool responses.
    /// </summary>
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
