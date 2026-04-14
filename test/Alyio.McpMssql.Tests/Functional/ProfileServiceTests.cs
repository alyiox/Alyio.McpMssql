// MIT License

using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class ProfileServiceTests
{
    private static IProfileService BuildProfileService(params (string Key, string Value)[][] configurationSources)
    {
        var configurationBuilder = new ConfigurationBuilder();

        foreach (var source in configurationSources)
        {
            var keyValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in source)
            {
                keyValues.Add(key.Replace("__", ConfigurationPath.KeyDelimiter), value);
            }

            configurationBuilder.AddInMemoryCollection(keyValues);
        }

        IConfiguration configuration = configurationBuilder.Build();

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

        var profiles = profileService.GetProfiles();

        Assert.NotNull(profiles);
        Assert.NotEmpty(profiles);

        var defaultProfile = profiles.FirstOrDefault(p => p.Name.Equals(McpMssqlProfileOptions.DefaultProfileName, StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(defaultProfile);
        Assert.Contains(envVars[1].Item2, defaultProfile.Description);

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

        var profiles = profileService.GetProfiles();

        Assert.NotNull(profiles);
        Assert.Equal(2, profiles.Count);
        var defaultProfile = profiles.FirstOrDefault(p => p.Name.Equals(McpMssqlOptions.DefaultProfileName, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(defaultProfile);
        Assert.Equal("Default database", defaultProfile.Description);
        var warehouse = profiles.FirstOrDefault(p => p.Name.Equals("warehouse", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(warehouse);
        Assert.Equal("Warehouse read-only", warehouse.Description);
    }

    [Fact]
    public void Resolve_Default_From_Only_Mcp_Flat_Keys_Includes_Select_Options()
    {
        var envVars = new[]
        {
            ("MCPMSSQL_CONNECTION_STRING", "Server=.;TrustServerCertificate=True;"),
            ("MCPMSSQL_QUERY_MAX_ROWS", "750"),
        };
        var profileService = BuildProfileService(envVars);

        var profile = profileService.Resolve(null);

        Assert.Equal(750, profile.Query.MaxRows);
    }

    [Fact]
    public void Resolve_Default_From_Flat_Keys_Includes_Snapshot_Options()
    {
        var envVars = new[]
        {
            ("MCPMSSQL_CONNECTION_STRING", "Server=.;TrustServerCertificate=True;"),
            ("MCPMSSQL_QUERY_SNAPSHOT_MAX_ROWS", "5000"),
            ("MCPMSSQL_QUERY_SNAPSHOT_COMMAND_TIMEOUT_SECONDS", "60"),
        };
        var profileService = BuildProfileService(envVars);

        var profile = profileService.Resolve(null);

        Assert.Equal(5000, profile.Query.SnapshotMaxRows);
        Assert.Equal(60, profile.Query.SnapshotCommandTimeoutSeconds);
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
    public void Resolve_Default_And_Named_From_Config_Section()
    {
        var userConfig = new[]
        {
            ("McpMssql:Profiles:Default:ConnectionString", "Server=.;Database=UserDefault;TrustServerCertificate=True;"),
            ("McpMssql:Profiles:Default:Description", "Default profile from user config"),
            ("McpMssql:Profiles:Warehouse:ConnectionString", "Server=warehouse;Database=WarehouseUser;TrustServerCertificate=True;"),
            ("McpMssql:Profiles:Warehouse:Description", "Warehouse profile from user config"),
        };

        var profileService = BuildProfileService(userConfig);

        var defaultProfile = profileService.Resolve(null);
        var warehouseProfile = profileService.Resolve("warehouse");
        var profiles = profileService.GetProfiles();

        Assert.Contains("UserDefault", defaultProfile.ConnectionString);
        Assert.Contains("WarehouseUser", warehouseProfile.ConnectionString);
        Assert.Contains(
            profiles,
            p => p.Name.Equals("default", StringComparison.OrdinalIgnoreCase)
                && p.Description == "Default profile from user config");
        Assert.Contains(
            profiles,
            p => p.Name.Equals("warehouse", StringComparison.OrdinalIgnoreCase)
                && p.Description == "Warehouse profile from user config");
    }

    [Fact]
    public void Resolve_Default_Hierarchical_Env_Overrides_User_Config()
    {
        var userConfig = new[]
        {
            ("McpMssql:Profiles:Default:ConnectionString", "Server=.;Database=FromUserConfig;TrustServerCertificate=True;"),
        };
        var envVars = new[]
        {
            ("MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING", "Server=.;Database=FromEnv;TrustServerCertificate=True;"),
        };

        var profileService = BuildProfileService(userConfig, envVars);

        var profile = profileService.Resolve(null);

        Assert.Contains("FromEnv", profile.ConnectionString);
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
