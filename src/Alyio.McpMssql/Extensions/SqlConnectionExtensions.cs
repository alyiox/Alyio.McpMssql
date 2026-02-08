// MIT License

using System.Data;
using Alyio.McpMssql.Models;

#pragma warning disable IDE0130 // Intentional: extension methods for SqlConnection
namespace Microsoft.Data.SqlClient;
#pragma warning restore IDE0130

/// <summary>
/// Lightweight execution helpers for <see cref="SqlConnection"/>.
/// These extensions assume the connection is already opened and
/// optionally scoped to the correct database.
/// </summary>
internal static class SqlConnectionExtensions
{
    /// <summary>
    /// Executes a metadata query that returns column definitions only.
    /// Used for describing relations such as tables or views.
    /// </summary>
    public static async Task<TabularResult> ExecuteAsTabularResultAsync(
        this SqlConnection connection,
        string sql,
        IReadOnlyList<SqlParameter>? parameters,
        CancellationToken cancellationToken)
    {
        using var cmd = connection.CreateCommand(sql, parameters);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var columns = reader.ReadColumns();
        var (rows, _) = await reader.ReadRowsAsync(int.MaxValue, cancellationToken).ConfigureAwait(false);

        return new TabularResult
        {
            Columns = columns,
            Rows = rows
        };
    }

    /// <summary>
    /// Executes a query and returns a tabular result consisting of
    /// column metadata and row values.
    /// Used by the SELECT query engine and similar read-only operations.
    /// </summary>
    public static async Task<QueryResult> ExecuteAsQueryResultAsync(
        this SqlConnection connection,
        string sql,
        IReadOnlyList<SqlParameter>? parameters,
        int rowLimit,
        CancellationToken cancellationToken)
    {
        using var cmd = connection.CreateCommand(sql, parameters);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken)
            .ConfigureAwait(false);

        var columns = reader.ReadColumns();
        var (rows, truncated) = await reader
            .ReadRowsAsync(rowLimit, cancellationToken)
            .ConfigureAwait(false);

        return new QueryResult
        {
            Columns = columns,
            Rows = rows,
            Truncated = truncated,
            RowLimit = rowLimit
        };
    }

    /// <summary>
    /// Reads column metadata describing the shape of the current result set.
    /// </summary>
    public static IReadOnlyList<string> ReadColumns(this SqlDataReader reader)
    {
        return reader.GetColumnSchema().Select(c => c.ColumnName).ToList().AsReadOnly();
    }

    /// <summary>
    /// Reads result rows up to a maximum row limit.
    /// Each row is returned as an array aligned with the column ordinals.
    /// </summary>
    private static async Task<(IReadOnlyList<object?[]> Rows, bool Truncated)> ReadRowsAsync(
        this SqlDataReader reader,
        int rowLimit,
        CancellationToken cancellationToken)
    {
        var rows = new List<object?[]>();
        var fieldCount = reader.FieldCount;
        var truncated = false;

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (rows.Count == rowLimit)
            {
                truncated = true;
                break;
            }

            var values = new object?[fieldCount];
            reader.GetValues(values);

            // Normalize DBNull to null for JSON / LLM safety
            for (int i = 0; i < fieldCount; i++)
            {
                if (values[i] is DBNull)
                {
                    values[i] = null;
                }
            }

            rows.Add(values);
        }

        return (rows.AsReadOnly(), truncated);
    }

    /// <summary>
    /// Creates a <see cref="SqlCommand"/> with optional parameters.
    /// </summary>
    private static SqlCommand CreateCommand(this SqlConnection connection, string sql, IReadOnlyList<SqlParameter>? parameters)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = sql;

        if (parameters is { Count: > 0 })
        {
            foreach (var p in parameters)
            {
                cmd.Parameters.Add(p);
            }
        }

        return cmd;
    }
}

