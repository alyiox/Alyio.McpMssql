// MIT License

using System.Text.Json;
using ModelContextProtocol.Client;

namespace ModelContextProtocol.Protocol;

internal static class McpClientExtensions
{
    public static async Task<T?> CallToolAsJsonAsync<T>(this McpClient client, string toolName, IReadOnlyDictionary<string, object?>? arguments = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(toolName);

        var result = await client.CallToolAsync(toolName, arguments, cancellationToken: cancellationToken);

        return result.ReadAsJson<T>();
    }

    public static T? ReadAsJson<T>(this Result? result, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        string? jsonText = result switch
        {
            // Tools return a list of Content objects (e.g., TextContentBlock)
            CallToolResult toolResult => toolResult.Content
                .OfType<TextContentBlock>()
                .FirstOrDefault()?.Text,

            // Resources return a list of ResourceContents (e.g., TextResourceContents)
            ReadResourceResult resourceResult => resourceResult.Contents
                .OfType<TextResourceContents>()
                .FirstOrDefault()?.Text,

            _ => throw new NotSupportedException($"Result type '{result.GetType().FullName}' is not supported.")
        };

        if (string.IsNullOrWhiteSpace(jsonText))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(jsonText, options ?? new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
