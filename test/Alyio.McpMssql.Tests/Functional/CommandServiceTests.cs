// MIT License

using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Models;
using Alyio.McpMssql.Services;
using Alyio.McpMssql.Tests.Infrastructure.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Tests.Functional;

public sealed class CommandServiceTests(SqlServerFixture fixture) : SqlServerFunctionalTest(fixture)
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    /// <summary>
    /// Builds a write-enabled <see cref="CommandService"/> that reuses the
    /// test database connection string. The shared fixture profile is
    /// read-only, so writes must go through a dedicated write-enabled profile.
    /// </summary>
    private CommandService CreateWriteEnabledService()
    {
        string connectionString = Fixture.Services
            .GetRequiredService<IProfileService>()
            .Resolve()
            .ConnectionString;

        var options = new McpMssqlOptions
        {
            Profiles = new Dictionary<string, McpMssqlProfileOptions>(StringComparer.OrdinalIgnoreCase)
            {
                [McpMssqlOptions.DefaultProfileName] = new McpMssqlProfileOptions
                {
                    ConnectionString = connectionString,
                    AllowWrite = true,
                },
            },
        };

        return new CommandService(
            new ProfileService(Options.Create(options)),
            NullLogger<CommandService>.Instance);
    }

    [Fact]
    public async Task Execute_Ddl_And_Dml_On_Write_Enabled_Profile()
    {
        CommandService service = CreateWriteEnabledService();
        string table = $"dbo.WriteTest_{Guid.NewGuid():N}";

        try
        {
            CommandResult create = await service.ExecuteAsync(
                $"create table {table} (Id int not null)",
                catalog: TestDatabaseName,
                cancellationToken: CancellationToken);

            // DDL reports no affected rows.
            Assert.Equal(-1, create.RowsAffected);

            CommandResult insert = await service.ExecuteAsync(
                $"insert into {table} (Id) values (1), (2), (3)",
                catalog: TestDatabaseName,
                cancellationToken: CancellationToken);

            Assert.Equal(3, insert.RowsAffected);

            CommandResult update = await service.ExecuteAsync(
                $"update {table} set Id = Id + 10 where Id >= @min",
                catalog: TestDatabaseName,
                parameters: new Dictionary<string, object> { ["min"] = 2 },
                cancellationToken: CancellationToken);

            Assert.Equal(2, update.RowsAffected);

            CommandResult delete = await service.ExecuteAsync(
                $"delete from {table} where Id = @id",
                catalog: TestDatabaseName,
                parameters: new Dictionary<string, object> { ["id"] = 1 },
                cancellationToken: CancellationToken);

            Assert.Equal(1, delete.RowsAffected);
        }
        finally
        {
            await service.ExecuteAsync(
                $"drop table if exists {table}",
                catalog: TestDatabaseName,
                cancellationToken: CancellationToken);
        }
    }

    [Fact]
    public async Task Execute_MultiStatement_Batch_On_Write_Enabled_Profile()
    {
        // Multi-statement batches are rejected by the read-only query path
        // (which forbids ';') but must succeed here.
        CommandService service = CreateWriteEnabledService();
        string table = $"dbo.WriteBatch_{Guid.NewGuid():N}";

        try
        {
            CommandResult batch = await service.ExecuteAsync(
                $"""
                create table {table} (Id int not null);
                insert into {table} (Id) values (1), (2);
                insert into {table} (Id) values (3), (4), (5);
                """,
                catalog: TestDatabaseName,
                cancellationToken: CancellationToken);

            // ExecuteNonQuery aggregates affected rows across the batch;
            // the DDL statement contributes none.
            Assert.Equal(5, batch.RowsAffected);
        }
        finally
        {
            await service.ExecuteAsync(
                $"drop table if exists {table}",
                catalog: TestDatabaseName,
                cancellationToken: CancellationToken);
        }
    }

    [Fact]
    public async Task Execute_Captures_Server_Print_Messages()
    {
        CommandService service = CreateWriteEnabledService();

        CommandResult result = await service.ExecuteAsync(
            "print 'hello from server'",
            catalog: TestDatabaseName,
            cancellationToken: CancellationToken);

        Assert.Contains(result.Messages, m => m.Contains("hello from server", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Execute_Is_Rejected_On_Default_ReadOnly_Profile()
    {
        // The DI-wired default profile does not set AllowWrite, so it is locked.
        ICommandService service = Fixture.Services.GetRequiredService<ICommandService>();

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(
                "create table dbo.ShouldNotBeCreated (Id int)",
                catalog: TestDatabaseName,
                cancellationToken: CancellationToken));

        Assert.Contains("does not permit write", ex.Message);
    }
}
