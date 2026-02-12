// MIT License

using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class ProfileServiceTests
{
    private static IProfileService BuildProfileService((string Key, string Value)[] envVars)
    {
        var keyValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in envVars)
        {
            keyValues.Add(key.Replace("__", ConfigurationPath.KeyDelimiter), value);
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(keyValues)
            .Build();

        var services = new ServiceCollection();
        services.AddMcpMssqlOptions(configuration);
        services.AddSingleton<IProfileService, ProfileService>();
        return services.BuildServiceProvider().GetRequiredService<IProfileService>();
    }

    [Fact]
    public void Resolve_Default_From_Only_Mcp_Flat_Keys()
    {
        var envVars = new[]
        {
            ("MCPMSSQL_CONNECTION_STRING", "Server=.;Database=FlatOnly;TrustServerCertificate=True;"),
            ("MCPMSSQL_DESCRIPTION", "Local development SQL Server instance for MCP-MSSQL testing.")

        };
        var profileService = BuildProfileService(envVars);
        var profile = profileService.Resolve(null);

        Assert.NotNull(profile);
        Assert.Contains("FlatOnly", profile.ConnectionString);

        var context = profileService.GetContext();

        Assert.NotNull(context);
        Assert.NotEmpty(context.Profiles);

        var defaultContext = context.Profiles.FirstOrDefault(p => p.Name.Equals(McpMssqlProfileOptions.DefaultProfileName, StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(defaultContext);
        Assert.Contains(envVars[1].Item2, defaultContext.Description);

    }

    [Fact]
    public void GetContext_From_Section_Returns_All_Profiles()
    {
        var envVars = new[]
        {
            ("MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING", "Server=.;Database=DefaultDb;TrustServerCertificate=True;"),
            ("MCPMSSQL__PROFILES__DEFAULT__DESCRIPTION", "Default database"),
            ("MCPMSSQL__PROFILES__WAREHOUSE__CONNECTIONSTRING", "Server=.;Database=WarehouseDb;TrustServerCertificate=True;"),
            ("MCPMSSQL__PROFILES__WAREHOUSE__DESCRIPTION", "Warehouse read-only"),
        };
        var profileService = BuildProfileService(envVars);

        var context = profileService.GetContext();

        Assert.NotNull(context);
        Assert.Equal(2, context.Profiles.Count);
        var defaultProfile = context.Profiles.FirstOrDefault(p => p.Name.Equals(McpMssqlOptions.DefaultProfileName, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(defaultProfile);
        Assert.Equal("Default database", defaultProfile.Description);
        var warehouse = context.Profiles.FirstOrDefault(p => p.Name.Equals("warehouse", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(warehouse);
        Assert.Equal("Warehouse read-only", warehouse.Description);
    }

    [Fact]
    public void Resolve_Default_From_Only_Mcp_Flat_Keys_Includes_Select_Options()
    {
        var envVars = new[]
        {
            ("MCPMSSQL_CONNECTION_STRING", "Server=.;TrustServerCertificate=True;"),
            ("MCPMSSQL_SELECT_MAX_ROWS", "2000"),
        };
        var profileService = BuildProfileService(envVars);

        var profile = profileService.Resolve(null);

        Assert.Equal(2000, profile.Select.MaxRows);
    }

    [Fact]
    public void Resolve_Default_From_Flat_And_Named_Profile_From_Section()
    {
        var envVars = new[]
        {
            ("MCPMSSQL_CONNECTION_STRING", "Server=.;Database=DefaultDb;TrustServerCertificate=True;"),
            ("MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING", "Server=.;Database=Ignored;TrustServerCertificate=True;"),
            ("MCPMSSQL__PROFILES__OTHER__CONNECTIONSTRING", "Server=other;Database=OtherDb;TrustServerCertificate=True;"),
        };

        var profileService = BuildProfileService(envVars);

        var defaultProfile = profileService.Resolve(null);
        var otherProfile = profileService.Resolve("other");

        Assert.Contains("DefaultDb", defaultProfile.ConnectionString);
        Assert.Contains("OtherDb", otherProfile.ConnectionString);
    }

    [Fact]
    public void Resolve_Default_From_Flat_Key_And_Other_Profile_From_Section_Without_Named_Default()
    {
        var envVars = new[]
        {
            ("MCPMSSQL_CONNECTION_STRING", "Server=.;Database=DefaultFromFlat;TrustServerCertificate=True;"),
            ("MCPMSSQL__PROFILES__OTHER__CONNECTIONSTRING", "Server=other;Database=OtherDb;TrustServerCertificate=True;"),
        };

        var profileService = BuildProfileService(envVars);

        var defaultProfile = profileService.Resolve(null);
        var otherProfile = profileService.Resolve("other");

        Assert.Contains("DefaultFromFlat", defaultProfile.ConnectionString);
        Assert.Contains("OtherDb", otherProfile.ConnectionString);
    }

    [Fact]
    public void Resolve_Default_From_Section_Only()
    {
        var envVars = new[]
        {
            ("MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING", "Server=.;Database=SectionOnly;TrustServerCertificate=True;"),
            ("MCPMSSQL__PROFILES__OTHER__CONNECTIONSTRING", "Server=other;Database=OtherFromSection;TrustServerCertificate=True;"),
        };

        var profileService = BuildProfileService(envVars);

        var profile = profileService.Resolve(null);

        Assert.Contains("SectionOnly", profile.ConnectionString);
    }

    [Fact]
    public void Resolve_Default_Flat_Overrides_Section()
    {
        var envVars = new[]
        {
            ("MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING", "Server=.;Database=FromSection;TrustServerCertificate=True;"),
            ("MCPMSSQL_CONNECTION_STRING", "Server=.;Database=FromFlat;TrustServerCertificate=True;"),
        };

        var profileService = BuildProfileService(envVars);

        var profile = profileService.Resolve(null);
        Assert.Contains("FromFlat", profile.ConnectionString);
    }

    [Fact]
    public void Resolve_Default_And_Named_From_Only_Section_Keys()
    {
        var envVars = new[]
        {
            ("MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING", "Server=.;Database=DefaultFromSection;TrustServerCertificate=True;"),
            ("MCPMSSQL__PROFILES__OTHER__CONNECTIONSTRING", "Server=other;Database=OtherFromSection;TrustServerCertificate=True;"),
        };

        var profileService = BuildProfileService(envVars);

        var defaultProfile = profileService.Resolve(null);
        var otherProfile = profileService.Resolve("other");

        Assert.Contains("DefaultFromSection", defaultProfile.ConnectionString);
        Assert.Contains("OtherFromSection", otherProfile.ConnectionString);
    }

    [Fact]
    public void Resolve_Default_From_Uppercase_Section_Keys()
    {
        var envVars = new[]
        {
            ("MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING", "Server=.;Database=UppercaseSection;TrustServerCertificate=True;"),
        };

        var profileService = BuildProfileService(envVars);

        var profile = profileService.Resolve(null);
        Assert.Contains("UppercaseSection", profile.ConnectionString);
    }

    [Fact]
    public void Resolve_Default_Throws_When_No_Mcp_Or_Convention_Key_For_Default()
    {
        var envVars = new[]
        {
            ("MCPMSSQL__PROFILES__OTHER__CONNECTIONSTRING", "Server=other;Database=OtherDb;TrustServerCertificate=True;"),
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            var svc = BuildProfileService(envVars);
            svc.Resolve(null);
        });

        Assert.Contains("default", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
