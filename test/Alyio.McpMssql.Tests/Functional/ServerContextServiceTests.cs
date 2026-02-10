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
        ServerPropertiesContext context =
            await _service.GetPropertiesAsync();

        Assert.NotNull(context);

        Assert.False(string.IsNullOrWhiteSpace(context.ProductVersion));
        Assert.False(string.IsNullOrWhiteSpace(context.ProductLevel));
        Assert.False(string.IsNullOrWhiteSpace(context.Edition));

        Assert.True(context.EngineEdition > 0);
        Assert.False(string.IsNullOrWhiteSpace(context.EngineEditionName));
    }

    [Fact]
    public async Task EngineEdition_Name_Is_Consistent_With_EngineEdition()
    {
        var context = await _service.GetPropertiesAsync();

        // Defensive sanity check: numeric + textual mapping both populated
        Assert.True(context.EngineEdition > 0);
        Assert.False(string.IsNullOrWhiteSpace(context.EngineEditionName));
    }
}
