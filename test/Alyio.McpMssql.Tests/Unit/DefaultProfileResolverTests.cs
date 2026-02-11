// MIT License

using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Services;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Tests.Unit;

public class DefaultProfileResolverTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_With_Null_Or_Empty_Or_Whitespace_Returns_Default_Profile(string? profileName)
    {
        var defaultProfile = new McpMssqlProfileOptions { ConnectionString = "Server=.;Database=DefaultDb;" };
        var options = new McpMssqlOptions
        {
            DefaultProfile = McpMssqlOptions.DefaultProfileName,
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = defaultProfile,
            },
        };
        var resolver = new DefaultProfileResolver(Options.Create(options));

        var result = resolver.Resolve(profileName);

        Assert.Same(defaultProfile, result);
    }

    [Fact]
    public void Resolve_With_Explicit_Name_Returns_That_Profile()
    {
        var otherProfile = new McpMssqlProfileOptions { ConnectionString = "Server=other;Database=OtherDb;" };
        var options = new McpMssqlOptions
        {
            DefaultProfile = McpMssqlOptions.DefaultProfileName,
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions { ConnectionString = "Server=.;" },
                ["other"] = otherProfile,
            },
        };
        var resolver = new DefaultProfileResolver(Options.Create(options));

        var result = resolver.Resolve("other");

        Assert.Same(otherProfile, result);
    }

    [Fact]
    public void Resolve_Is_Case_Insensitive()
    {
        var otherProfile = new McpMssqlProfileOptions { ConnectionString = "Server=other;" };
        var options = new McpMssqlOptions
        {
            DefaultProfile = McpMssqlOptions.DefaultProfileName,
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["Other"] = otherProfile,
            },
        };
        var resolver = new DefaultProfileResolver(Options.Create(options));

        var result = resolver.Resolve("other");

        Assert.Same(otherProfile, result);
    }

    [Fact]
    public void Resolve_With_Custom_DefaultProfile_Returns_That_Profile_When_Given_Null()
    {
        var customDefault = new McpMssqlProfileOptions { ConnectionString = "Server=custom;" };
        var options = new McpMssqlOptions
        {
            DefaultProfile = "custom",
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["custom"] = customDefault,
            },
        };
        var resolver = new DefaultProfileResolver(Options.Create(options));

        var result = resolver.Resolve(null);

        Assert.Same(customDefault, result);
    }

    [Fact]
    public void Resolve_Throws_When_Profile_Not_Found()
    {
        var options = new McpMssqlOptions
        {
            DefaultProfile = McpMssqlOptions.DefaultProfileName,
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions(),
            },
        };
        var resolver = new DefaultProfileResolver(Options.Create(options));

        var ex = Assert.Throws<InvalidOperationException>(() => resolver.Resolve("missing"));

        Assert.Contains("missing", ex.Message);
        Assert.Contains("default", ex.Message);
    }

    [Fact]
    public void Resolve_Throws_When_Default_Profile_Missing_And_Given_Null()
    {
        var options = new McpMssqlOptions
        {
            DefaultProfile = McpMssqlOptions.DefaultProfileName,
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase),
        };
        var resolver = new DefaultProfileResolver(Options.Create(options));

        var ex = Assert.Throws<InvalidOperationException>(() => resolver.Resolve(null));

        Assert.Contains("default", ex.Message);
        Assert.Contains("Available profiles:", ex.Message);
    }
}
