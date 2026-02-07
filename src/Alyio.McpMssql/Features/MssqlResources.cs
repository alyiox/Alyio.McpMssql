// MIT License

using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// 
/// </summary>
[McpServerResourceType]
public class MssqlResources
{
    /// <summary>
    /// Lists all databases on the server.
    /// </summary>
    /// <param name="server">The SQL Server service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string representing the list of databases.</returns>
    [McpServerResource(UriTemplate = "mssql://databases", Name = "List Databases")]
    [Description("Lists all databases on the server.")]
    public static Task<string> ListDatabasesAsync(ISqlServerService server, CancellationToken cancellationToken = default)
    {
        return server.ListDatabasesAsync(cancellationToken);
    }

    /// <summary>
    /// Lists schemas in the current database (or a specified database).
    /// </summary>
    /// <param name="server">The SQL Server service.</param>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string representing the list of schemas.</returns>
    [McpServerResource(UriTemplate = "mssql://schemas{?database}", Name = "List Schemas")]
    [Description("Lists all schemas in a database.")]
    public static Task<string> ListSchemasAsync(
        ISqlServerService server,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        CancellationToken cancellationToken = default)
    {
        return server.ListSchemasAsync(database, cancellationToken);
    }

    /// <summary>
    /// Lists base tables in the current database (or a specified database), optionally filtered by schema.
    /// </summary>
    /// <param name="server">The SQL Server service.</param>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="schema">Optional schema name. If omitted, lists from all schemas.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string representing the list of tables.</returns>
    [McpServerResource(UriTemplate = "mssql://tables{?database,schema}", Name = "List Tables")]
    [Description("Lists all base tables (excludes views) in a database. Optionally filter by schema.")]
    public static Task<string> ListTablesAsync(
        ISqlServerService server,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return server.ListTablesAsync(database, schema, cancellationToken);
    }

    /// <summary>
    /// Lists views in the current database (or a specified database), optionally filtered by schema.
    /// </summary>
    /// <param name="server">The SQL Server service.</param>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="schema">Optional schema name. If omitted, lists from all schemas.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string representing the list of views.</returns>
    [McpServerResource(UriTemplate = "mssql://views{?database,schema}", Name = "List Views")]
    [Description("Lists all views in a database. Optionally filter by schema.")]
    public static Task<string> ListViewsAsync(
        ISqlServerService server,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return server.ListViewsAsync(database, schema, cancellationToken);
    }

    /// <summary>
    /// Lists stored procedures in the current database (or a specified database), optionally filtered by schema.
    /// </summary>
    /// <param name="server">The SQL Server service.</param>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="schema">Optional schema name. If omitted, lists from all schemas.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string representing the list of stored procedures.</returns>
    [McpServerResource(UriTemplate = "mssql://procedures{?database,schema}", Name = "List Procedures")]
    [Description("Lists all stored procedures in a database. Optionally filter by schema.")]
    public static Task<string> ListProceduresAsync(
        ISqlServerService server,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return server.ListProceduresAsync(database, schema, cancellationToken);
    }

    /// <summary>
    /// Lists user-defined functions in the current database (or a specified database), optionally filtered by schema.
    /// </summary>
    /// <param name="server">The SQL Server service.</param>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="schema">Optional schema name. If omitted, lists from all schemas.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string representing the list of user-defined functions.</returns>
    [McpServerResource(UriTemplate = "mssql://functions{?database,schema}", Name = "List Functions")]
    [Description("Lists all user-defined functions in a database. Optionally filter by schema.")]
    public static Task<string> ListFunctionsAsync(
        ISqlServerService server,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, lists from all schemas.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return server.ListFunctionsAsync(database, schema, cancellationToken);
    }

    /// <summary>
    /// Lists column metadata for a table or view.
    /// </summary>
    /// <param name="server">The SQL Server service.</param>
    /// <param name="table">Table or view name.</param>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="schema">Optional schema name. If omitted, uses default schema resolution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Column metadata for the specified table or view in JSON format.</returns>
    [McpServerResource(UriTemplate = "mssql://tables/{table}/columns{?database,schema}", Name = "Describe Table")]
    [Description("Describes table or view columns with data types, nullability, and precision.")]
    public static Task<string> DescribeTableAsync(
        ISqlServerService server,
        [Description("Table or view name.")] string table,
        [Description("Optional database name. If omitted, uses current database.")] string? database = null,
        [Description("Optional schema name. If omitted, uses default schema resolution.")] string? schema = null,
        CancellationToken cancellationToken = default)
    {
        return server.DescribeTableAsync(table, database, schema, cancellationToken);
    }
}
