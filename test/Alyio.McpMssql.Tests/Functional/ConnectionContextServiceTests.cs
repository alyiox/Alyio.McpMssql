// MIT License

using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class ConnectionContextServiceTests(SqlServerFixture fixture) : SqlServerFunctionalTest(fixture)
{
    private readonly IConnectionContextService _service =
        fixture.Services.GetRequiredService<IConnectionContextService>();

    [Fact]
    public async Task GetConnectionContext_Returns_Connection_Metadata()
    {
        var context = await _service.GetConnectionContextAsync();

        Assert.NotNull(context);

        // Structural assertions (not value-specific)
        Assert.False(string.IsNullOrWhiteSpace(context.Server));
        Assert.False(string.IsNullOrWhiteSpace(context.Database));
        Assert.False(string.IsNullOrWhiteSpace(context.User));
        Assert.False(string.IsNullOrWhiteSpace(context.Version));

        // Port is typically 1433 for SQL Server
        Assert.Equal(1433, context.Port);
    }

    [Fact]
    public async Task GetConnectionContext_Uses_Real_Server_Engine()
    {
        var context = await _service.GetConnectionContextAsync();

        Assert.Contains(
            "SQL Server",
            context.Version,
            StringComparison.OrdinalIgnoreCase);
    }
}
