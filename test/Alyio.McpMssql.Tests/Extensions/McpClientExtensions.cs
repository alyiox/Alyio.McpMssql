// MIT License

using System.Text.Json;
using ModelContextProtocol.Client;

namespace ModelContextProtocol.Protocol;

internal static class McpClientExtensions
{
    /// <summary>
    /// Checks if all provided tool names are discovered in the server's manifest.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="toolNames">The names of the tools to check for registration.</param>
    /// <returns>True if all specified tools are registered; otherwise, false.</returns>
    public static async Task<bool> IsToolRegisteredAsync(this McpClient client, params string[] toolNames)
    {
        ArgumentNullException.ThrowIfNull(client);

        var tools = await client.ListToolsAsync();
        var registered = tools.Select(t => t.Name).ToHashSet();
        return toolNames.All(registered.Contains);
    }

    /// <summary>
    /// Checks if all provided URI templates are available for dynamic discovery.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="templates">The URI templates to check for registration.</param>
    /// <returns>True if all specified URI templates are registered; otherwise, false.</returns>
    public static async Task<bool> IsResourceTemplateRegisteredAsync(this McpClient client, params string[] templates)
    {
        ArgumentNullException.ThrowIfNull(client);

        var resources = await client.ListResourceTemplatesAsync();
        var registered = resources.Select(r => r.UriTemplate).ToHashSet();
        return templates.All(registered.Contains);
    }

    /// <summary>
    /// Checks if all provided static URIs are available in the resource list.
    /// </summary>
    /// <param name="client">The MCP client instance.</param>
    /// <param name="uris">The static URIs to check for registration.</param>
    /// <returns>True if all specified URIs are registered; otherwise, false.</returns>
    public static async Task<bool> IsResourceRegisteredAsync(this McpClient client, params string[] uris)
    {
        var resources = await client.ListResourcesAsync();
        var registered = resources.Select(r => r.Uri).ToHashSet();
        return uris.All(registered.Contains);
    }

    /// <summary>
    /// Invokes a tool by name on the specified client and deserializes the result as JSON to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the tool's JSON result will be deserialized.</typeparam>
    /// <param name="client">The client instance used to call the tool. Cannot be null.</param>
    /// <param name="toolName">The name of the tool to invoke. Cannot be null.</param>
    /// <param name="arguments">An optional dictionary containing arguments to pass to the tool. May be null if no arguments are required.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the deserialized value of type T, or
    /// null if the tool did not return a result.</returns>
    public static async Task<T?> CallToolAsJsonAsync<T>(this McpClient client, string toolName, IReadOnlyDictionary<string, object?>? arguments = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(toolName);

        var result = await client.CallToolAsync(toolName, arguments, cancellationToken: cancellationToken);

        return result.ReadAsJson<T>();
    }

    /// <summary>
    /// Deserializes the text content of the specified result as a JSON object of the given type.
    /// </summary>
    /// <remarks>If the result does not contain any text content, the method returns the default value for T.
    /// The deserialization uses case-insensitive property names by default unless custom options are
    /// provided.</remarks>
    /// <typeparam name="T">The type to deserialize the JSON content into.</typeparam>
    /// <param name="result">The result containing the text content to be deserialized. Cannot be null.</param>
    /// <param name="options">The options to use for JSON deserialization. If null, default options with case-insensitive property names are
    /// used.</param>
    /// <returns>An instance of type T deserialized from the JSON content, or the default value for T if the content is null or
    /// whitespace.</returns>
    /// <exception cref="NotSupportedException">Thrown if the type of the result is not supported for JSON deserialization.</exception>
    public static T? ReadAsJson<T>(this Result? result, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        string? jsonText = result switch
        {
            CallToolResult toolResult => toolResult.Content
                .OfType<TextContentBlock>()
                .FirstOrDefault()?.Text,

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
