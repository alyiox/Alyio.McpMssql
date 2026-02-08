// MIT License

using Alyio.McpMssql.DependencyInjection;
using Alyio.McpMssql.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Services;

internal sealed class ServerContextService(IOptions<McpMssqlOptions> options) : IServerContextService
{
    public async Task<SqlConnectionContext> GetConnectionContextAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = options.Value.ConnectionString;
        var builder = new SqlConnectionStringBuilder(connectionString);
        var dataSourceParts = builder.DataSource.Split(',');

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand("SELECT SUSER_SNAME(), @@VERSION", connection);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);

        var result = new SqlConnectionContext
        {
            Server = dataSourceParts[0].Trim(),
            Port = dataSourceParts.Length > 1 ? dataSourceParts[1].Trim() : "1433",
            Database = connection.Database,
            User = reader.GetString(0),
            Version = reader.GetString(1)
        };

        return result;
    }
}
