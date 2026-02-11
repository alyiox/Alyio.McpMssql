// MIT License

using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Services;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Tests.Unit;

public class ProfileServiceTests
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
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = defaultProfile,
            },
        };
        var profileService = new ProfileService(Options.Create(options));

        var result = profileService.Resolve(profileName);

        Assert.Same(defaultProfile, result);
    }

    [Fact]
    public void Resolve_With_Explicit_Name_Returns_That_Profile()
    {
        var otherProfile = new McpMssqlProfileOptions { ConnectionString = "Server=other;Database=OtherDb;" };
        var options = new McpMssqlOptions
        {
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions { ConnectionString = "Server=.;" },
                ["other"] = otherProfile,
            },
        };
        var profileService = new ProfileService(Options.Create(options));

        var result = profileService.Resolve("other");

        Assert.Same(otherProfile, result);
    }

    [Fact]
    public void Resolve_Is_Case_Insensitive()
    {
        var otherProfile = new McpMssqlProfileOptions { ConnectionString = "Server=other;" };
        var options = new McpMssqlOptions
        {
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions { ConnectionString = "Server=.;" },
                ["Other"] = otherProfile,
            },
        };
        var profileService = new ProfileService(Options.Create(options));

        var result = profileService.Resolve("other");

        Assert.Same(otherProfile, result);
    }

    [Fact]
    public void Resolve_Throws_When_Profile_Not_Found()
    {
        var options = new McpMssqlOptions
        {
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions(),
            },
        };
        var profileService = new ProfileService(Options.Create(options));

        var ex = Assert.Throws<InvalidOperationException>(() => profileService.Resolve("missing"));

        Assert.Contains("missing", ex.Message);
        Assert.Contains("default", ex.Message);
    }

    [Fact]
    public void Resolve_Throws_When_Default_Profile_Missing_And_Given_Null()
    {
        var options = new McpMssqlOptions
        {
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase),
        };
        var profileService = new ProfileService(Options.Create(options));

        var ex = Assert.Throws<InvalidOperationException>(() => profileService.Resolve(null));

        Assert.Contains("default", ex.Message);
        Assert.Contains("Available profiles:", ex.Message);
    }

    [Fact]
    public void GetContext_Returns_Profiles_And_DefaultProfile_Name()
    {
        var options = new McpMssqlOptions
        {
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = new McpMssqlProfileOptions { Description = "Default instance" },
                ["warehouse"] = new McpMssqlProfileOptions { Description = "Warehouse DB" },
            },
        };
        var profileService = new ProfileService(Options.Create(options));

        var context = profileService.GetContext();

        Assert.NotNull(context);
        Assert.Equal(2, context.Profiles.Count);
        Assert.Equal(McpMssqlOptions.DefaultProfileName, context.DefaultProfile);
        var names = context.Profiles.Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal).ToList();
        Assert.Equal(["default", "warehouse"], names);
        Assert.Equal("Default instance", context.Profiles.Single(p => p.Name == "default").Description);
        Assert.Equal("Warehouse DB", context.Profiles.Single(p => p.Name == "warehouse").Description);
    }

    [Fact]
    public void GetContext_Returns_Null_Description_When_Profile_Description_Is_Null_Or_Whitespace()
    {
        var options = new McpMssqlOptions
        {
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions { Description = null },
                ["other"] = new McpMssqlProfileOptions { Description = "   " },
                ["empty"] = new McpMssqlProfileOptions { Description = "" },
            },
        };
        var profileService = new ProfileService(Options.Create(options));

        var context = profileService.GetContext();

        Assert.Null(context.Profiles.Single(p => p.Name == McpMssqlOptions.DefaultProfileName).Description);
        Assert.Null(context.Profiles.Single(p => p.Name == "other").Description);
        Assert.Null(context.Profiles.Single(p => p.Name == "empty").Description);
    }

    [Fact]
    public void GetContext_Trims_Profile_Description()
    {
        var options = new McpMssqlOptions
        {
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions { Description = "  dev server  " },
            },
        };
        var profileService = new ProfileService(Options.Create(options));

        var context = profileService.GetContext();

        Assert.Equal("dev server", context.Profiles.Single().Description);
    }

    [Fact]
    public void GetContext_Returns_Empty_Profiles_When_No_Profiles_Configured()
    {
        var options = new McpMssqlOptions
        {
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase),
        };
        var profileService = new ProfileService(Options.Create(options));

        var context = profileService.GetContext();

        Assert.NotNull(context);
        Assert.Empty(context.Profiles);
        Assert.Equal(McpMssqlOptions.DefaultProfileName, context.DefaultProfile);
    }

    [Fact]
    public void Resolve_Throws_When_Profile_Not_Found_Message_Includes_Available_Profile_Names()
    {
        var options = new McpMssqlOptions
        {
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions(),
                ["warehouse"] = new McpMssqlProfileOptions(),
            },
        };
        var profileService = new ProfileService(Options.Create(options));

        var ex = Assert.Throws<InvalidOperationException>(() => profileService.Resolve("missing"));

        Assert.Contains("missing", ex.Message);
        Assert.Contains("default", ex.Message);
        Assert.Contains("warehouse", ex.Message);
        Assert.Contains("Available profiles:", ex.Message);
    }
}
