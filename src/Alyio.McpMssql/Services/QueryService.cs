// MIT License

using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using Microsoft.Data.SqlClient;

namespace Alyio.McpMssql.Services;

internal sealed class QueryService(IProfileService profileService) : IQueryService
{
    public async Task<QueryResult> ExecuteAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        SqlReadOnlyValidator.Validate(sql);
        var resolved = profileService.Resolve(profile);

        using var conn = new SqlConnection(resolved.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        var sqlParameters = SqlParameterHelper.Build(parameters);
        var rowLimit = resolved.Query.MaxRows;

        var result = await conn.ExecuteAsQueryResultAsync(
            sql,
            sqlParameters,
            rowLimit,
            cancellationToken).ConfigureAwait(false);

        return result;
    }
}
