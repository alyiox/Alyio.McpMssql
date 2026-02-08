// MIT License

using Alyio.McpMssql.DependencyInjection;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Services;

/// <summary>
/// Executes read-only SQL queries against SQL Server and returns
/// tabular results suitable for MCP tool responses.
/// </summary>
internal sealed class QueryService(IOptions<McpMssqlOptions> options) : IQueryService
{
    public async Task<QueryResult> ExecuteSelectAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        int? maxRows = null,
        CancellationToken cancellationToken = default)
    {
        SqlReadOnlyValidator.Validate(sql);

        using var conn = new SqlConnection(options.Value.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        IReadOnlyList<SqlParameter>? sqlParameters = null;

        if (parameters is { Count: > 0 })
        {
            var list = new List<SqlParameter>(parameters.Count);
            foreach (var (name, value) in parameters)
            {
                list.Add(new SqlParameter(
                    $"@{name}",
                    value ?? DBNull.Value));
            }

            sqlParameters = list;
        }

        var rowLimit = maxRows ?? options.Value.DefaultMaxRows;
        rowLimit = Math.Clamp(rowLimit, 1, options.Value.RowLimit);

        var result = await conn.ExecuteAsQueryResultAsync(
            sql,
            sqlParameters,
            rowLimit,
            cancellationToken).ConfigureAwait(false);

        return result;
    }
}
