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
            new SqlParameter("@schema", schema ?? (object)DBNull.Value)
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
        var sql = await Loader.ReadText("indexes.sql", cancellationToken).ConfigureAwait(false);

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

