// MIT License

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ModelContextProtocol.Client;
#pragma warning restore IDE0130 // Namespace does not match folder structure

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
}
