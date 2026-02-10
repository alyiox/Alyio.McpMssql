// MIT License

using Alyio.McpMssql.Configuration;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Services;

/// <summary>
/// Default implementation of <see cref="IProfileResolver"/> that
/// resolves profiles from configured MCP MSSQL options.
/// </summary>
internal sealed class DefaultProfileResolver(IOptions<McpMssqlOptions> options) : IProfileResolver
{
    private readonly McpMssqlOptions _options = options.Value;

    public McpMssqlProfileOptions Resolve(string? profileName = null)
    {
        var name = string.IsNullOrWhiteSpace(profileName) ? _options.DefaultProfile : profileName;

        if (_options.Profiles.TryGetValue(name, out var profile))
        {
            return profile;
        }

        throw new InvalidOperationException(
            $"MCP MSSQL profile '{name}' was not found. "
            + $"Available profiles: {string.Join(", ", _options.Profiles.Keys)}");
    }
}

