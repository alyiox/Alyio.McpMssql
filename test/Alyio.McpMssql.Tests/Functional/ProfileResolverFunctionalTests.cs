// MIT License

using Alyio.McpMssql.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class DefaultProfileResolverFunctionalTests
{
    private static IProfileResolver BuildResolver((string Key, string Value)[] envVars)
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
        services.AddSingleton<IProfileResolver, DefaultProfileResolver>();
        return services.BuildServiceProvider().GetRequiredService<IProfileResolver>();
    }

    [Fact]
    public void Resolve_Default_From_Only_Mcp_Flat_Keys()
    {
        var envVars = new[]
        {
            ("MCPMSSQL_CONNECTION_STRING", "Server=.;Database=FlatOnly;TrustServerCertificate=True;"),
        };
        var resolver = BuildResolver(envVars);
        var profile = resolver.Resolve(null);

        Assert.NotNull(profile);
        Assert.Contains("FlatOnly", profile.ConnectionString);
    }

    [Fact]
    public void Resolve_Default_From_Only_Mcp_Flat_Keys_Includes_Select_Options()
    {
        var envVars = new[]
        {
            ("MCPMSSQL_CONNECTION_STRING", "Server=.;TrustServerCertificate=True;"),
            ("MCPMSSQL_SELECT_MAX_ROWS", "2000"),
        };
        var resolver = BuildResolver(envVars);

        var profile = resolver.Resolve(null);

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

        var resolver = BuildResolver(envVars);

        var defaultProfile = resolver.Resolve(null);
        var otherProfile = resolver.Resolve("other");

        Assert.Contains("DefaultDb", defaultProfile.ConnectionString);
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

        var resolver = BuildResolver(envVars);

        var profile = resolver.Resolve(null);

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

        var resolver = BuildResolver(envVars);

        var profile = resolver.Resolve(null);
        Assert.Contains("FromFlat", profile.ConnectionString);
    }

    [Fact]
    public void Resolve_Default_And_Named_From_Only_Section_Keys()
    {
        var envVars = new[]
        {
            ("MCPMSSQL__DEFAULTPROFILE", "default"),
            ("MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING", "Server=.;Database=DefaultFromSection;TrustServerCertificate=True;"),
            ("MCPMSSQL__PROFILES__OTHER__CONNECTIONSTRING", "Server=other;Database=OtherFromSection;TrustServerCertificate=True;"),
        };

        var resolver = BuildResolver(envVars);

        var defaultProfile = resolver.Resolve(null);
        var otherProfile = resolver.Resolve("other");

        Assert.Contains("DefaultFromSection", defaultProfile.ConnectionString);
        Assert.Contains("OtherFromSection", otherProfile.ConnectionString);
    }

    [Fact]
    public void Resolve_Respects_DefaultProfile_From_Environment()
    {
        var envVars = new[]
        {
            ("MCPMSSQL__DEFAULTPROFILE", "other"),
            ("MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING", "Server=.;Database=Default;TrustServerCertificate=True;"),
            ("MCPMSSQL__PROFILES__OTHER__CONNECTIONSTRING", "Server=.;Database=OtherAsDefault;TrustServerCertificate=True;"),
        };

        var resolver = BuildResolver(envVars);

        var profile = resolver.Resolve(null);
        Assert.Contains("OtherAsDefault", profile.ConnectionString);
    }

    [Fact]
    public void Resolve_Default_From_Uppercase_Section_Keys()
    {
        var envVars = new[]
        {
            ("MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING", "Server=.;Database=UppercaseSection;TrustServerCertificate=True;"),
        };

        var resolver = BuildResolver(envVars);

        var profile = resolver.Resolve(null);
        Assert.Contains("UppercaseSection", profile.ConnectionString);
    }

    [Fact]
    public void Resolve_Default_Throws_When_No_Mcp_Or_Convention_Key_For_Default()
    {
        var envVars = new[]
        {
            ("MCPMSSQL__PROFILES__OTHER__CONNECTIONSTRING", "Server=other;Database=OtherDb;TrustServerCertificate=True;"),
        };

        var ex = Assert.Throws<InvalidOperationException>(() => BuildResolver(envVars));

        Assert.Contains("default", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
