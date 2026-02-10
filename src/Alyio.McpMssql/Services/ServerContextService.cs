// MIT License

using Alyio.McpMssql.Models;
using Alyio.McpMssql.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Services;

internal sealed class ServerContextService(IOptions<McpMssqlOptions> options) : IServerContextService
{
    public async Task<ServerPropertiesContext> GetPropertiesAsync(
        CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(options.Value.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
SELECT
    CAST(SERVERPROPERTY('ProductVersion')     AS nvarchar(128)) AS ProductVersion,
    CAST(SERVERPROPERTY('ProductLevel')       AS nvarchar(128)) AS ProductLevel,
    CAST(SERVERPROPERTY('ProductUpdateLevel') AS nvarchar(128)) AS ProductUpdateLevel,
    CAST(SERVERPROPERTY('Edition')            AS nvarchar(128)) AS Edition,
    CAST(SERVERPROPERTY('EngineEdition')      AS int)           AS EngineEdition,
    CASE CAST(SERVERPROPERTY('EngineEdition') AS int)
        WHEN 1  THEN 'Personal / Desktop Engine'
        WHEN 2  THEN 'Standard'
        WHEN 3  THEN 'On-premises SQL Server'
        WHEN 4  THEN 'Express'
        WHEN 5  THEN 'Azure SQL Database'
        WHEN 6  THEN 'Azure Synapse Analytics'
        WHEN 8  THEN 'Azure SQL Managed Instance'
        WHEN 9  THEN 'Azure SQL Edge'
        WHEN 11 THEN 'Azure Synapse serverless SQL pool / Microsoft Fabric'
        WHEN 12 THEN 'Microsoft Fabric SQL database'
        ELSE 'Unknown'
    END AS EngineEditionName;
";

        using var cmd = new SqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);

        return new ServerPropertiesContext
        {
            ProductVersion = reader.GetString(0),
            ProductLevel = reader.GetString(1),
            ProductUpdateLevel = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            Edition = reader.GetString(3),
            EngineEdition = reader.GetInt32(4),
            EngineEditionName = reader.GetString(5)
        };
    }
}
