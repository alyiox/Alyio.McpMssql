// MIT License

using System.Text.Json;
using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using Microsoft.Data.SqlClient;

namespace Alyio.McpMssql.Services;

internal sealed class SelectService(IProfileResolver profileResolver) : ISelectService
{
    public async Task<QueryResult> ExecuteAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        int? maxRows = null,
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        SqlReadOnlyValidator.Validate(sql);
        var resolved = profileResolver.Resolve(profile);

        using var conn = new SqlConnection(resolved.ConnectionString);
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
                    NormalizeParameter(value)));
            }

            sqlParameters = list;
        }

        var rowLimit = maxRows ?? resolved.Select.DefaultMaxRows;
        rowLimit = Math.Clamp(rowLimit, 1, resolved.Select.MaxRows);

        var result = await conn.ExecuteAsQueryResultAsync(
            sql,
            sqlParameters,
            rowLimit,
            cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Normalizes JSON-native and CLR values into SQL-provider-compatible
    /// parameter values.
    /// </summary>
    private static object NormalizeParameter(object? value)
    {
        if (value is null)
        {
            return DBNull.Value;
        }

        if (value is JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.Number when je.TryGetInt32(out var i) => i,
                JsonValueKind.Number when je.TryGetInt64(out var l) => l,
                JsonValueKind.Number when je.TryGetDecimal(out var d) => d,

                JsonValueKind.String => je.GetString()!,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => DBNull.Value,

                _ => throw new NotSupportedException(
                    $"Unsupported JsonElement parameter kind: {je.ValueKind}")
            };
        }

        return value;
    }
}
