// MIT License

using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Models;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Services;

internal sealed class ProfileService(IOptions<McpMssqlOptions> options) : IProfileService
{
    private readonly McpMssqlOptions _options = options.Value;

    public McpMssqlProfileOptions Resolve(string? profileName = null)
    {
        var name = string.IsNullOrWhiteSpace(profileName) ? McpMssqlOptions.DefaultProfileName : profileName;

        if (_options.Profiles.TryGetValue(name, out var profile))
        {
            return profile;
        }

        throw new InvalidOperationException(
            $"MCP MSSQL profile '{name}' was not found. "
            + $"Available profiles: {string.Join(", ", _options.Profiles.Keys)}");
    }

    /// <inheritdoc />
    public IReadOnlyList<Profile> GetProfiles()
    {
        return options.Value.Profiles
            .Select(p => new Profile
            {
                Name = p.Key,
                Description = string.IsNullOrWhiteSpace(p.Value.Description) ? null : p.Value.Description.Trim(),
            })
            .ToList();
    }
}

