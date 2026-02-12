// MIT License

using Alyio.McpMssql.Models;
using Microsoft.Data.SqlClient;

namespace Alyio.McpMssql.Services;

internal sealed class CatalogService(IProfileService profileService) : ICatalogService
{
    public async Task<TabularResult> ListCatalogsAsync(
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
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

        using var conn = new SqlConnection(resolved.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        return await conn.ExecuteAsTabularResultAsync(sql, null, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TabularResult> ListSchemasAsync(
        string? catalog = null,
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
        const string sql =
            """
            SELECT [SCHEMA_NAME] AS [name]
            FROM INFORMATION_SCHEMA.SCHEMATA
            WHERE [SCHEMA_NAME] NOT IN ('sys', 'INFORMATION_SCHEMA')
            ORDER BY [SCHEMA_NAME]
            """;

        using var conn = new SqlConnection(resolved.ConnectionString);
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
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
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

        using var conn = new SqlConnection(resolved.ConnectionString);
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
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
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

        using var conn = new SqlConnection(resolved.ConnectionString);
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

    public async Task<TabularResult> DescribeColumnsAsync(
        string name,
        string? catalog = null,
        string? schema = null,
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
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

        using var conn = new SqlConnection(resolved.ConnectionString);
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

    public async Task<TabularResult> DescribeIndexesAsync(
        string name,
        string? catalog = null,
        string? schema = null,
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
        const string sql =
            """
            SELECT
                i.name                    AS index_name,
                i.type_desc               AS index_type,        -- CLUSTERED / NONCLUSTERED
                i.is_unique               AS is_unique,
                i.is_disabled             AS is_disabled,
                i.has_filter              AS has_filter,
                i.filter_definition,
                ic.key_ordinal            AS key_ordinal,       -- 0 = included column, 1+ = key
                ic.is_descending_key      AS is_descending,
                c.name                    AS column_name,
                CASE WHEN ic.key_ordinal = 0 THEN 1 ELSE 0 END AS is_included_column
            FROM sys.indexes i
            INNER JOIN sys.index_columns ic
                ON i.object_id = ic.object_id AND i.index_id = ic.index_id
            INNER JOIN sys.columns c
                ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            INNER JOIN sys.objects o
                ON i.object_id = o.object_id AND o.type IN ('U', 'V')
            INNER JOIN sys.schemas s
                ON o.schema_id = s.schema_id
            WHERE o.name = @table
              AND (@schema IS NULL OR s.name = @schema)
              AND i.type > 0                              -- exclude heap (index_id = 0)
            ORDER BY
                i.name,
                CASE WHEN ic.key_ordinal = 0 THEN 1 ELSE 0 END,  -- key columns (1..n) first, then included (0)
                ic.key_ordinal,
                ic.index_column_id;
            """;

        using var conn = new SqlConnection(resolved.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        var parameters = new[]
        {
            new SqlParameter("@table", name),
            new SqlParameter("@schema", schema ?? (object)DBNull.Value)
        };

        return await conn.ExecuteAsTabularResultAsync(sql, parameters, cancellationToken)
            .ConfigureAwait(false);
    }
}

