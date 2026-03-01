// MIT License

using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Alyio.McpMssql.Internal;

/// <summary>
/// Shared helpers for building <see cref="SqlParameter"/> instances
/// from MCP tool input values.
/// </summary>
internal static class SqlParameterHelper
{
    /// <summary>
    /// Builds a list of <see cref="SqlParameter"/> from a name-value dictionary.
    /// Returns <c>null</c> when the dictionary is null or empty.
    /// </summary>
    public static IReadOnlyList<SqlParameter>? Build(
        IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not { Count: > 0 })
        {
            return null;
        }

        var list = new List<SqlParameter>(parameters.Count);

        foreach (var (name, value) in parameters)
        {
            list.Add(new SqlParameter($"@{name}", Normalize(value)));
        }

        return list;
    }

    /// <summary>
    /// Normalizes JSON-native and CLR values into SQL-provider-compatible
    /// parameter values.
    /// </summary>
    public static object Normalize(object? value)
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
                    $"Unsupported JsonElement parameter kind: {je.ValueKind}"),
            };
        }

        return value;
    }
}
