// MIT License

using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Tests.Unit;

public class CommandServiceTests
{
    private static CommandService CreateService(bool allowWrite)
    {
        var options = new McpMssqlOptions
        {
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions
                {
                    ConnectionString = "Server=.;Database=Db;",
                    AllowWrite = allowWrite,
                },
            },
        };

        return new CommandService(
            new ProfileService(Options.Create(options)),
            NullLogger<CommandService>.Instance);
    }

    [Fact]
    public async Task Execute_Rejects_When_Profile_Does_Not_Allow_Write()
    {
        CommandService service = CreateService(allowWrite: false);

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(
                "create table dbo.Widgets (Id int)",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains(McpMssqlOptions.DefaultProfileName, ex.Message);
        Assert.Contains("does not permit write", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Execute_Rejects_Empty_Sql(string? sql)
    {
        // A write-enabled profile still rejects empty SQL before connecting.
        CommandService service = CreateService(allowWrite: true);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync(sql!, cancellationToken: TestContext.Current.CancellationToken));
    }
}
