// MIT License

using System.ComponentModel;
using System.Text.Json;
using Alyio.McpMssql.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Tools;

/// <summary>
/// MVP MCP tools for SQL Server connectivity and querying.
/// </summary>
[McpServerToolType]
public static class MssqlTools
{
    /// <summary>
    /// Database connectivity health check.
    /// </summary>
    [McpServerTool, Description("Database connectivity check. Returns SQL Server version information.")]
    public static async Task<string> Ping(
        IOptions<McpMssqlOptions> options,
        CancellationToken cancellationToken = default)
    {
        return await ToolExecutor.ExecuteAsync(async () =>
        {
            var connectionString = ApplyConnectionOptions(options.Value.ConnectionString);
            const string sql = "SELECT @@VERSION AS version";
            return await RunMetadataQueryAsync(connectionString, sql, cancellationToken);
        });
    }

    /// <summary>
    /// Lists databases visible to the current connection.
    /// </summary>
    [McpServerTool, Description("Lists all databases on the server.")]
    public static async Task<string> ListDatabases(
        IOptions<McpMssqlOptions> options,
        CancellationToken cancellationToken = default)
    {
        return await ToolExecutor.ExecuteAsync(async () =>
        {
            var connectionString = ApplyConnectionOptions(options.Value.ConnectionString);
            const string sql = "SELECT name, database_id, create_date FROM sys.databases ORDER BY name";
            return await RunMetadataQueryAsync(connectionString, sql, cancellationToken);
        });
    }

    /// <summary>
    /// Lists schemas in the current database (or a specified database).
    /// </summary>
    [McpServerTool, Description("Lists all schemas in a database.")]
    public static async Task<string> ListSchemas(
        IOptions<McpMssqlOptions> options,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        CancellationToken cancellationToken = default)
    {
        return await ToolExecutor.ExecuteAsync(async () =>
        {
            var connectionString = ApplyConnectionOptions(options.Value.ConnectionString, database);
            var sql = "SELECT CATALOG_NAME, SCHEMA_NAME, SCHEMA_OWNER FROM INFORMATION_SCHEMA.SCHEMATA;";
            return await RunMetadataQueryAsync(connectionString, sql, cancellationToken);
        });
    }

    /// <summary>
    /// Lists base tables in the current database, optionally filtered by schema.
    /// </summary>
    [McpServerTool, Description("Lists all base tables (excludes views). Optionally filter by database and/or schema.")]
    public static async Task<string> ListTables(
        IOptions<McpMssqlOptions> options,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return await ToolExecutor.ExecuteAsync(async () =>
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

            var connectionString = ApplyConnectionOptions(options.Value.ConnectionString, database);
            return await RunMetadataQueryAsync(connectionString, sql, cancellationToken);
        });
    }

    /// <summary>
    /// Lists views in the current database, optionally filtered by schema.
    /// </summary>
    [McpServerTool, Description("Lists all views. Optionally filter by database and/or schema.")]
    public static async Task<string> ListViews(
        IOptions<McpMssqlOptions> options,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return await ToolExecutor.ExecuteAsync(async () =>
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

            var connectionString = ApplyConnectionOptions(options.Value.ConnectionString, database);
            return await RunMetadataQueryAsync(connectionString, sql, cancellationToken);
        });
    }

    /// <summary>
    /// Lists stored procedures in the current database, optionally filtered by schema.
    /// </summary>
    [McpServerTool, Description("Lists all stored procedures. Use EXEC/EXECUTE to call these. Optionally filter by database and/or schema.")]
    public static async Task<string> ListProcedures(
        IOptions<McpMssqlOptions> options,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return await ToolExecutor.ExecuteAsync(async () =>
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

            var connectionString = ApplyConnectionOptions(options.Value.ConnectionString, database);
            return await RunMetadataQueryAsync(connectionString, sql, cancellationToken);
        });
    }

    /// <summary>
    /// Lists user-defined functions in the current database, optionally filtered by schema.
    /// </summary>
    [McpServerTool, Description("Lists all user-defined functions (scalar and table-valued). Can be used in queries. Optionally filter by database and/or schema.")]
    public static async Task<string> ListFunctions(
        IOptions<McpMssqlOptions> options,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return await ToolExecutor.ExecuteAsync(async () =>
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

            var connectionString = ApplyConnectionOptions(options.Value.ConnectionString, database);
            return await RunMetadataQueryAsync(connectionString, sql, cancellationToken);
        });
    }

    /// <summary>
    /// Returns column metadata for a table or view.
    /// </summary>
    [McpServerTool, Description("Describes table or view columns with data types, nullability, and precision.")]
    public static async Task<string> DescribeTable(
        IOptions<McpMssqlOptions> options,
        [Description("Table or view name.")] string table,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, uses default schema resolution.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return await ToolExecutor.ExecuteAsync(async () =>
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

            var connectionString = ApplyConnectionOptions(options.Value.ConnectionString, database);
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@schema", (object?)schema ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tbl", table);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var columns = reader.GetColumnSchema()
                .Select(c => new Column
                {
                    Name = c.ColumnName ?? string.Empty,
                    Ordinal = c.ColumnOrdinal ?? 0,
                    DataTypeName = c.DataTypeName
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

            return ToolResponse.Create(columns, [.. rows]).ToJson();
        });
    }

    /// <summary>
    /// Executes a SELECT-only query against SQL Server and returns JSON results.
    /// </summary>
    [McpServerTool, Description("Executes a read-only SELECT query. Supports parameterized queries using @paramName syntax.")]
    public static async Task<string> Select(
        IOptions<McpMssqlOptions> options,
        [Description("SQL SELECT statement. Use @paramName for parameters.")] string sql,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional JSON object mapping parameter names to values, e.g. {\"id\": 1}.")] string? parametersJson = null,
        [Description("Maximum rows to return. Default 200, maximum 5000.")] int? maxRows = null,
        CancellationToken cancellationToken = default)
    {
        return await ToolExecutor.ExecuteAsync(async () =>
        {
            SqlValidation.ValidateReadOnly(sql);
            var effectiveOptions = options.Value;

            var effectiveHardMaxRows = effectiveOptions.HardMaxRows;
            var requested = maxRows ?? effectiveOptions.DefaultMaxRows;
            var capped = Math.Clamp(requested, 1, effectiveHardMaxRows);

            var connectionString = ApplyConnectionOptions(effectiveOptions.ConnectionString, database, effectiveOptions.CommandTimeoutSeconds);
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

            return ToolResponse.Create(columns, [.. rows], meta).ToJson();
        });
    }

    private static async Task<string> RunMetadataQueryAsync(string connectionString, string sql, CancellationToken cancellationToken = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
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

        return ToolResponse.Create(columns, [.. rows]).ToJson();
    }

    /// <summary>
    /// Applies optional database and timeout overrides to a connection string.
    /// </summary>
    private static string ApplyConnectionOptions(string connectionString, string? database = null, int commandTimeout = 30)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrWhiteSpace(database))
        {
            builder.InitialCatalog = database.Trim();
        }

        builder.CommandTimeout = commandTimeout;

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

