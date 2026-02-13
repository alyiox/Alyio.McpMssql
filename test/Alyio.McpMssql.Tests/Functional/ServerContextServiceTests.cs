// MIT License

using Alyio.McpMssql.Models;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class ServerContextServiceTests(SqlServerFixture fixture) : SqlServerFunctionalTest(fixture)
{
    private readonly IServerContextService _service = fixture.Services.GetRequiredService<IServerContextService>();

    [Fact]
    public async Task GetServerProperties_Returns_Server_Metadata()
    {
        ServerProperties props = await _service.GetPropertiesAsync();

        Assert.NotNull(props);

        Assert.False(string.IsNullOrWhiteSpace(props.ProductVersion));
        Assert.False(string.IsNullOrWhiteSpace(props.ProductLevel));
        Assert.False(string.IsNullOrWhiteSpace(props.Edition));

        Assert.True(props.EngineEdition > 0);
        Assert.False(string.IsNullOrWhiteSpace(props.EngineEditionName));
    }

    [Fact]
    public async Task GetServerProperties_Returns_Execution_Limits()
    {
        var props = await _service.GetPropertiesAsync();

        Assert.NotNull(props.Limits);
        Assert.NotNull(props.Limits.Query);
        Assert.True(props.Limits.Query.DefaultMaxRows.Value > 0);
        Assert.True(props.Limits.Query.HardRowLimit.Value > 0);
        Assert.True(props.Limits.Query.CommandTimeoutSeconds.Value > 0);
    }

    [Fact]
    public async Task EngineEdition_Name_Is_Consistent_With_EngineEdition()
    {
        var props = await _service.GetPropertiesAsync();

        Assert.True(props.EngineEdition > 0);
        Assert.False(string.IsNullOrWhiteSpace(props.EngineEditionName));
    }
}
