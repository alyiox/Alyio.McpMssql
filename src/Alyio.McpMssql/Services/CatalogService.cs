// MIT License

using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Models;
using Microsoft.Data.SqlClient;

namespace Alyio.McpMssql.Services;

internal sealed class CatalogService(IProfileResolver profileResolver) : ICatalogService
{
    public async Task<TabularResult> ListCatalogsAsync(
        CancellationToken cancellationToken = default)
    {
        var profile = profileResolver.Resolve();
        const string sql =
            """
            SELECT 
                [name], 
                state_desc, 
                is_read_only,
                CAST(CASE 
                    WHEN [name] IN ('master', 'tempdb', 'model', 'msdb') OR is_distributor = 1 THEN 1 
                    ELSE 0 
                END AS BIT) AS is_system_db
            FROM sys.databases
            ORDER BY database_id
            """;

        using var conn = new SqlConnection(profile.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        return await conn.ExecuteAsTabularResultAsync(sql, null, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TabularResult> ListSchemasAsync(
        string? catalog = null,
        CancellationToken cancellationToken = default)
    {
        var profile = profileResolver.Resolve();
        const string sql =
            """
            SELECT [SCHEMA_NAME] AS [name]
            FROM INFORMATION_SCHEMA.SCHEMATA
            WHERE [SCHEMA_NAME] NOT IN ('sys', 'INFORMATION_SCHEMA')
            ORDER BY [SCHEMA_NAME]
            """;

        using var conn = new SqlConnection(profile.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        return await conn.ExecuteAsTabularResultAsync(sql, null, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TabularResult> ListRelationsAsync(
        string? catalog = null,
        string? schema = null,
        CancellationToken cancellationToken = default)
    {
        var profile = profileResolver.Resolve();
        const string sql =
            """
            SELECT
                TABLE_NAME AS [name],
                TABLE_TYPE AS [type],
                TABLE_SCHEMA AS [schema]
            FROM INFORMATION_SCHEMA.TABLES
            WHERE @schema IS NULL OR TABLE_SCHEMA = @schema
            ORDER BY TABLE_SCHEMA, TABLE_NAME
            """;

        using var conn = new SqlConnection(profile.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        var parameters = new[]
        {
            new SqlParameter("@schema", schema ?? (object)DBNull.Value)
        };

        return await conn.ExecuteAsTabularResultAsync(sql, parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TabularResult> ListRoutinesAsync(
        string? catalog = null,
        string? schema = null,
        CancellationToken cancellationToken = default)
    {
        var profile = profileResolver.Resolve();
        const string sql =
            """
            SELECT
                ROUTINE_NAME   AS [name],
                ROUTINE_TYPE   AS [type],
                ROUTINE_SCHEMA AS [schema]
            FROM INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_TYPE IN ('PROCEDURE', 'FUNCTION')
              AND (@schema IS NULL OR ROUTINE_SCHEMA = @schema)
            ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME;
            """;

        using var conn = new SqlConnection(profile.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        var parameters = new[]
        {
            new SqlParameter("@schema", schema ?? (object)DBNull.Value)
        };

        return await conn.ExecuteAsTabularResultAsync(sql, parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TabularResult> DescribeRelationAsync(
        string name,
        string? catalog = null,
        string? schema = null,
        CancellationToken cancellationToken = default)
    {
        var profile = profileResolver.Resolve();
        const string sql =
            """
            SELECT
                c.COLUMN_NAME      AS [name],
                c.DATA_TYPE        AS [type],
                CASE c.IS_NULLABLE
                    WHEN 'YES' THEN CAST(1 AS bit)
                    ELSE CAST(0 AS bit)
                END                AS [nullable],
                c.ORDINAL_POSITION AS [position]
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE c.TABLE_NAME = @name
              AND (@schema IS NULL OR c.TABLE_SCHEMA = @schema)
            ORDER BY c.ORDINAL_POSITION;
            """;

        using var conn = new SqlConnection(profile.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        var parameters = new[]
        {
            new SqlParameter("@name", name),
            new SqlParameter("@schema", schema ?? (object)DBNull.Value)
        };

        var result = await conn.ExecuteAsTabularResultAsync(
            sql,
            parameters,
            cancellationToken).ConfigureAwait(false);

        return result;
    }
}

