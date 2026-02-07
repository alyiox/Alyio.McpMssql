// MIT License

using System.Text.Json;
using Alyio.McpMssql.DependencyInjection;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Alyio.McpMssql.Services;

internal sealed class SqlServerService(IOptions<McpMssqlOptions> options) : ISqlServerService
{
    public async Task<SqlConnectionContext> GetConnectionContextAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = options.Value.ConnectionString;
        var builder = new SqlConnectionStringBuilder(connectionString);
        var dataSourceParts = builder.DataSource.Split(',');

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var cmd = new SqlCommand("SELECT SUSER_SNAME(), @@VERSION", connection);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);

        var result = new SqlConnectionContext
        {
            Server = dataSourceParts[0].Trim(),
            Port = dataSourceParts.Length > 1 ? dataSourceParts[1].Trim() : "1433",
            Database = connection.Database,
            User = reader.GetString(0),
            Version = reader.GetString(1)
        };

        return result;
    }

    public async Task<string> ListDatabasesAsync(CancellationToken cancellationToken = default)
    {
        return await MssqlExecutor.ExecuteAsync(async () =>
        {
            const string sql = "SELECT name, database_id, create_date FROM sys.databases ORDER BY name";
            return await RunMetadataQueryAsync(options.Value, sql, cancellationToken);
        });
    }

    public async Task<string> ListSchemasAsync(string? database = null, CancellationToken cancellationToken = default)
    {
        return await MssqlExecutor.ExecuteAsync(async () =>
        {
            var sql = "SELECT CATALOG_NAME, SCHEMA_NAME, SCHEMA_OWNER FROM INFORMATION_SCHEMA.SCHEMATA;";
            return await RunMetadataQueryAsync(options.Value, sql, database, cancellationToken);
        });
    }

    public async Task<string> ListTablesAsync(string? database = null, string? schema = null, CancellationToken cancellationToken = default)
    {
        return await MssqlExecutor.ExecuteAsync(async () =>
        {
            var conditions = new List<string> { "TABLE_TYPE = 'BASE TABLE'" };
            if (!string.IsNullOrWhiteSpace(schema))
            {
                var safeSchema = schema.Trim().Replace("'", "''");
                conditions.Add($"TABLE_SCHEMA = '{safeSchema}'");
            }
            if (!string.IsNullOrWhiteSpace(database))
            {
                var safeDb = database.Trim().Replace("'", "''");
                conditions.Add($"TABLE_CATALOG = '{safeDb}'");
            }

            var where = string.Join(" AND ", conditions);
            var sql = $"SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE {where} ORDER BY TABLE_SCHEMA, TABLE_NAME";

            return await RunMetadataQueryAsync(options.Value, sql, database, cancellationToken);
        });
    }

    public async Task<string> ListViewsAsync(string? database = null, string? schema = null, CancellationToken cancellationToken = default)
    {
        return await MssqlExecutor.ExecuteAsync(async () =>
        {
            var conditions = new List<string> { "TABLE_TYPE = 'VIEW'" };
            if (!string.IsNullOrWhiteSpace(schema))
            {
                var safeSchema = schema.Trim().Replace("'", "''");
                conditions.Add($"TABLE_SCHEMA = '{safeSchema}'");
            }
            if (!string.IsNullOrWhiteSpace(database))
            {
                var safeDb = database.Trim().Replace("'", "''");
                conditions.Add($"TABLE_CATALOG = '{safeDb}'");
            }

            var where = string.Join(" AND ", conditions);
            var sql = $"SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE {where} ORDER BY TABLE_SCHEMA, TABLE_NAME";

            return await RunMetadataQueryAsync(options.Value, sql, database, cancellationToken);
        });
    }

    public async Task<string> ListProceduresAsync(string? database = null, string? schema = null, CancellationToken cancellationToken = default)
    {
        return await MssqlExecutor.ExecuteAsync(async () =>
        {
            var conditions = new List<string> { "ROUTINE_TYPE = 'PROCEDURE'" };
            if (!string.IsNullOrWhiteSpace(schema))
            {
                var safeSchema = schema.Trim().Replace("'", "''");
                conditions.Add($"ROUTINE_SCHEMA = '{safeSchema}'");
            }
            if (!string.IsNullOrWhiteSpace(database))
            {
                var safeDb = database.Trim().Replace("'", "''");
                conditions.Add($"ROUTINE_CATALOG = '{safeDb}'");
            }

            var where = string.Join(" AND ", conditions);
            var sql = $"SELECT ROUTINE_CATALOG, ROUTINE_SCHEMA, ROUTINE_NAME, CREATED, LAST_ALTERED FROM INFORMATION_SCHEMA.ROUTINES WHERE {where} ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";

            return await RunMetadataQueryAsync(options.Value, sql, database, cancellationToken);
        });
    }

    public async Task<string> ListFunctionsAsync(string? database = null, string? schema = null, CancellationToken cancellationToken = default)
    {
        return await MssqlExecutor.ExecuteAsync(async () =>
        {
            var conditions = new List<string> { "ROUTINE_TYPE = 'FUNCTION'" };
            if (!string.IsNullOrWhiteSpace(schema))
            {
                var safeSchema = schema.Trim().Replace("'", "''");
                conditions.Add($"ROUTINE_SCHEMA = '{safeSchema}'");
            }
            if (!string.IsNullOrWhiteSpace(database))
            {
                var safeDb = database.Trim().Replace("'", "''");
                conditions.Add($"ROUTINE_CATALOG = '{safeDb}'");
            }

            var where = string.Join(" AND ", conditions);
            var sql = $@"
SELECT 
    ROUTINE_CATALOG, 
    ROUTINE_SCHEMA, 
    ROUTINE_NAME, 
    DATA_TYPE,
    CASE WHEN DATA_TYPE = 'TABLE' THEN 'TABLE-VALUED' ELSE 'SCALAR' END AS FUNCTION_TYPE,
    CREATED, 
    LAST_ALTERED 
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE {where} 
ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";

            return await RunMetadataQueryAsync(options.Value, sql, database, cancellationToken);
        });
    }

    public async Task<string> DescribeTableAsync(string table, string? database = null, string? schema = null, CancellationToken cancellationToken = default)
    {
        return await MssqlExecutor.ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(table))
            {
                throw new ArgumentException("table parameter is required.", nameof(table));
            }

            var sql = $@"
SELECT c.COLUMN_NAME, c.ORDINAL_POSITION, c.DATA_TYPE, c.IS_NULLABLE, c.CHARACTER_MAXIMUM_LENGTH, c.NUMERIC_PRECISION, c.NUMERIC_SCALE
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = @tbl AND (@schema IS NULL OR c.TABLE_SCHEMA = @schema)
ORDER BY c.ORDINAL_POSITION";
            sql = sql.Trim();

            var parameters = new Dictionary<string, object?>()
            {
                { "@schema", schema },
                { "@tbl", table   }
            };

            return await RunMetadataQueryAsync(options.Value, sql, parameters, database, cancellationToken);
        });
    }

    public async Task<string> QueryAsync(
        string sql,
        string? database = null,
        string? parametersJson = null,
        int? maxRows = null,
        CancellationToken cancellationToken = default)
    {
        return await MssqlExecutor.ExecuteAsync(async () =>
        {
            SqlValidation.ValidateReadOnly(sql);
            var effectiveOptions = options.Value;

            var effectiveHardMaxRows = effectiveOptions.HardMaxRows;
            var requested = maxRows ?? effectiveOptions.DefaultMaxRows;
            var capped = Math.Clamp(requested, 1, effectiveHardMaxRows);

            var connectionString = UseDatabase(effectiveOptions.ConnectionString, database);
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = effectiveOptions.CommandTimeoutSeconds;

            BindParameters(cmd, parametersJson);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            var columns = reader.GetColumnSchema()
                .Select(c => new Column
                {
                    Name = c.ColumnName ?? string.Empty,
                    Ordinal = c.ColumnOrdinal ?? 0,
                    DataTypeName = c.DataTypeName,
                    AllowDbNull = c.AllowDBNull
                })
                .ToArray();

            var rows = new List<object?[]>(capacity: Math.Min(capped, 1024));
            var truncated = false;

            while (await reader.ReadAsync(cancellationToken))
            {
                if (rows.Count >= capped)
                {
                    truncated = true;
                    break;
                }

                var row = new object?[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    if (await reader.IsDBNullAsync(i, cancellationToken))
                    {
                        row[i] = null;
                        continue;
                    }

                    object value = reader.GetValue(i);

                    // Keep payload safe-ish for an MVP:
                    // - represent binary as base64
                    // - keep other scalar types as-is (JSON serializer will handle)
                    if (value is byte[] bytes)
                    {
                        row[i] = Convert.ToBase64String(bytes);
                    }
                    else
                    {
                        row[i] = value;
                    }
                }

                rows.Add(row);
            }

            var meta = new ResponseMeta
            {
                Truncated = truncated,
                MaxRows = capped
            };

            return ToolResponse.Create(columns, [.. rows], meta);
        });
    }

    private static Task<ToolResponse> RunMetadataQueryAsync(McpMssqlOptions options, string sql, CancellationToken cancellationToken = default)
        => RunMetadataQueryAsync(options, sql, null, null, cancellationToken);

    private static Task<ToolResponse> RunMetadataQueryAsync(McpMssqlOptions options, string sql, string? database = null, CancellationToken cancellationToken = default)
        => RunMetadataQueryAsync(options, sql, null, database, cancellationToken);

    private static async Task<ToolResponse> RunMetadataQueryAsync(McpMssqlOptions options, string sql, IDictionary<string, object?>? parameters, string? database, CancellationToken cancellationToken = default)
    {
        var connectionStr = UseDatabase(options.ConnectionString, database);
        await using var conn = new SqlConnection(connectionStr);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
            }
        }

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var columns = reader.GetColumnSchema()
            .Select(c => new Column
            {
                Name = c.ColumnName ?? string.Empty,
                Ordinal = c.ColumnOrdinal ?? 0
            })
            .ToArray();

        var rows = new List<object?[]>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new object?[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[i] = await reader.IsDBNullAsync(i, cancellationToken) ? null : reader.GetValue(i);
            }
            rows.Add(row);
        }

        return ToolResponse.Create(columns, [.. rows]);
    }

    private static string UseDatabase(string connectionString, string? database = null)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrWhiteSpace(database))
        {
            builder.InitialCatalog = database.Trim();
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Binds optional JSON parameters to the command. Parameter names in SQL use @name; JSON keys are used as-is (no @ prefix required).
    /// </summary>
    private static void BindParameters(SqlCommand cmd, string? parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            return;
        }

        using var doc = JsonDocument.Parse(parametersJson);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var prop in root.EnumerateObject())
        {
            var paramName = prop.Name.Trim();
            if (string.IsNullOrEmpty(paramName))
            {
                continue;
            }

            // SQL Server expects @paramName
            var sqlParamName = paramName.StartsWith('@') ? paramName : "@" + paramName;
            var value = JsonElementToClr(prop.Value);
            cmd.Parameters.AddWithValue(sqlParamName, value ?? DBNull.Value);
        }
    }

    private static object? JsonElementToClr(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            case JsonValueKind.String:
                return el.GetString();
            case JsonValueKind.Number:
                if (el.TryGetInt32(out var i32))
                {
                    return i32;
                }
                if (el.TryGetInt64(out var i64))
                {
                    return i64;
                }
                if (el.TryGetDouble(out var d))
                {
                    return d;
                }
                return el.GetDecimal();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            default:
                return el.GetRawText();
        }
    }
}
