// MIT License

using Alyio.McpMssql.Models;
using Alyio.McpMssql.Services.Scripts;
using Microsoft.Data.SqlClient;

namespace Alyio.McpMssql.Services;

internal sealed class CatalogService(IProfileService profileService) : ICatalogService
{
    public async Task<TabularResult> ListCatalogsAsync(
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
        var sql = await Loader.ReadText("databases.sql", cancellationToken).ConfigureAwait(false);

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
        var sql = await Loader.ReadText("schemas.sql", cancellationToken).ConfigureAwait(false);

        using var conn = new SqlConnection(resolved.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        var parameters = new[] { new SqlParameter("@is_ms_shipped", DBNull.Value) };
        return await conn.ExecuteAsTabularResultAsync(sql, parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TabularResult> ListRelationsAsync(
        string? catalog = null,
        string? schema = null,
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
        var sql = await Loader.ReadText("relations.sql", cancellationToken).ConfigureAwait(false);

        using var conn = new SqlConnection(resolved.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        var parameters = new[]
        {
            new SqlParameter("@schema", schema ?? (object)DBNull.Value),
            new SqlParameter("@is_ms_shipped", DBNull.Value)
        };

        return await conn.ExecuteAsTabularResultAsync(sql, parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<TabularResult> ListRoutinesAsync(
        string? catalog = null,
        string? schema = null,
        string? profile = null,
        bool? includeSystem = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
        var sql = await Loader.ReadText("routines.sql", cancellationToken).ConfigureAwait(false);

        using var conn = new SqlConnection(resolved.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        var parameters = new[]
        {
            new SqlParameter("@schema", schema ?? (object)DBNull.Value),
            new SqlParameter("@is_ms_shipped", includeSystem == true ? (object)DBNull.Value : 0),
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
        var sql = await Loader.ReadText("columns.sql", cancellationToken).ConfigureAwait(false);

        using var conn = new SqlConnection(resolved.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        var parameters = new[]
        {
            new SqlParameter("@name", name),
            new SqlParameter("@schema", schema ?? (object)DBNull.Value),
            new SqlParameter("@is_ms_shipped", DBNull.Value)
        };

        return await conn.ExecuteAsTabularResultAsync(sql, parameters, cancellationToken)
            .ConfigureAwait(false);
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

    public async Task<TableConstraints> DescribeConstraintsAsync(
        string name,
        string? catalog = null,
        string? schema = null,
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
        var sql = await Loader.ReadText("constraints.sql", cancellationToken).ConfigureAwait(false);

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

        var results = await conn.ExecuteMultipleTabularResultsAsync(sql, parameters, cancellationToken)
            .ConfigureAwait(false);

        if (results.Count != 5)
        {
            throw new InvalidOperationException(
                $"Describe constraints script must return exactly 5 result sets; got {results.Count}.");
        }

        return new TableConstraints
        {
            PrimaryKeys = results[0],
            UniqueConstraints = results[1],
            ForeignKeys = results[2],
            CheckConstraints = results[3],
            DefaultConstraints = results[4]
        };
    }

    public async Task<TabularResult> GetRoutineDefinitionAsync(
        string name,
        string? catalog = null,
        string? schema = null,
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
        var sql = await Loader.ReadText("routine_definition.sql", cancellationToken).ConfigureAwait(false);

        using var conn = new SqlConnection(resolved.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        var parameters = new[]
        {
            new SqlParameter("@schema", schema ?? (object)DBNull.Value),
            new SqlParameter("@name", name)
        };

        var result = await conn.ExecuteAsTabularResultAsync(sql, parameters, cancellationToken)
            .ConfigureAwait(false);

        if (result.Rows.Count == 0 || result.Columns.Count == 0)
        {
            return new TabularResult
            {
                Columns = ["definition"],
                Rows = []
            };
        }

        var definition = result.Rows[0][0];
        return new TabularResult
        {
            Columns = ["definition"],
            Rows = [[definition]],
        };
    }
}

