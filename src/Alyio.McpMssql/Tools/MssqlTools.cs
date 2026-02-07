// MIT License

using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Tools;

/// <summary>
/// MCP tools for SQL Server connectivity and querying.
/// </summary>
[McpServerToolType]
public static class MssqlTools
{
    /// <summary>
    /// Gets the SQL Server version.
    /// </summary>
    [McpServerTool, Description("Gets the SQL Server version.")]
    public static Task<string> GetServerVersionAsync(
        ISqlServerService server,
        CancellationToken cancellationToken = default)
    {
        return server.GetServerVersionAsync(cancellationToken);
    }

    /// <summary>
    /// Lists databases visible to the current connection.
    /// </summary>
    [McpServerTool, Description("Lists all databases on the server.")]
    public static Task<string> ListDatabasesAsync(
        ISqlServerService server,
        CancellationToken cancellationToken = default)
    {
        return server.ListDatabasesAsync(cancellationToken);
    }

    /// <summary>
    /// Lists schemas in the current database (or a specified database).
    /// </summary>
    [McpServerTool, Description("Lists all schemas in a database.")]
    public static Task<string> ListSchemasAsync(
        ISqlServerService server,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        CancellationToken cancellationToken = default)
    {
        return server.ListSchemasAsync(database, cancellationToken);
    }

    /// <summary>
    /// Lists base tables in the current database, optionally filtered by schema.
    /// </summary>
    [McpServerTool, Description("Lists all base tables (excludes views). Optionally filter by database and/or schema.")]
    public static Task<string> ListTablesAsync(
        ISqlServerService server,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return server.ListTablesAsync(database, schema, cancellationToken);
    }

    /// <summary>
    /// Lists views in the current database, optionally filtered by schema.
    /// </summary>
    [McpServerTool, Description("Lists all views. Optionally filter by database and/or schema.")]
    public static Task<string> ListViewsAsync(
        ISqlServerService server,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return server.ListViewsAsync(database, schema, cancellationToken);
    }

    /// <summary>
    /// Lists stored procedures in the current database, optionally filtered by schema.
    /// </summary>
    [McpServerTool, Description("Lists all stored procedures. Use EXEC/EXECUTE to call these. Optionally filter by database and/or schema.")]
    public static Task<string> ListProceduresAsync(
        ISqlServerService server,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return server.ListProceduresAsync(database, schema, cancellationToken);
    }

    /// <summary>
    /// Lists user-defined functions in the current database, optionally filtered by schema.
    /// </summary>
    [McpServerTool, Description("Lists all user-defined functions (scalar and table-valued). Can be used in queries. Optionally filter by database and/or schema.")]
    public static Task<string> ListFunctionsAsync(
        ISqlServerService server,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return server.ListFunctionsAsync(database, schema, cancellationToken);
    }

    /// <summary>
    /// Returns column metadata for a table or view.
    /// </summary>
    [McpServerTool, Description("Describes table or view columns with data types, nullability, and precision.")]
    public static Task<string> DescribeTableAsync(
        ISqlServerService server,
        [Description("Table or view name.")] string table,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, uses default schema resolution.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return server.DescribeTableAsync(table, database, schema, cancellationToken);
    }

    /// <summary>
    /// Executes a SELECT-only query against SQL Server and returns JSON results.
    /// </summary>
    [McpServerTool, Description("Executes a read-only SELECT query. Supports parameterized queries using @paramName syntax.")]
    public static Task<string> SelectAsync(
        ISqlServerService server,
        [Description("SQL SELECT statement. Use @paramName for parameters.")] string sql,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional JSON object mapping parameter names to values, e.g. {\"id\": 1}.")] string? parametersJson = null,
        [Description("Maximum rows to return. Default 200, maximum 5000.")] int? maxRows = null,
        CancellationToken cancellationToken = default)
    {
        return server.QueryAsync(sql, database, parametersJson, maxRows, cancellationToken);
    }
}

