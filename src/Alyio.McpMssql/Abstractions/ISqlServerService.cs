// MIT License

using Alyio.McpMssql.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Defines the contract for interacting with SQL Server to retrieve metadata and execute queries.
/// </summary>
public interface ISqlServerService
{
    /// <summary>
    /// Retrieves the active database name, user identity, and SQL Server version for execution context and feature compatibility.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SQL Server connectivity and environment metadata.</returns>
    Task<SqlConnectionContext> GetConnectionContextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists databases visible to the current connection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of databases.</returns>
    Task<string> ListDatabasesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists schemas in the current database (or a specified database).
    /// </summary>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of schemas.</returns>
    Task<string> ListSchemasAsync(string? database = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists base tables in the current database, optionally filtered by schema.
    /// </summary>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="schema">Optional schema name. If omitted, lists from all schemas.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of tables.</returns>
    Task<string> ListTablesAsync(string? database = null, string? schema = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists views in the current database, optionally filtered by schema.
    /// </summary>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="schema">Optional schema name. If omitted, lists from all schemas.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of views.</returns>
    Task<string> ListViewsAsync(string? database = null, string? schema = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists stored procedures in the current database, optionally filtered by schema.
    /// </summary>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="schema">Optional schema name. If omitted, lists from all schemas.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of stored procedures.</returns>
    Task<string> ListProceduresAsync(string? database = null, string? schema = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists user-defined functions in the current database, optionally filtered by schema.
    /// </summary>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="schema">Optional schema name. If omitted, lists from all schemas.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of user-defined functions.</returns>
    Task<string> ListFunctionsAsync(string? database = null, string? schema = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists column metadata for a table or view.
    /// </summary>
    /// <param name="table">Table or view name.</param>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="schema">Optional schema name. If omitted, uses default schema resolution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Column metadata for the specified table or view.</returns>
    Task<string> DescribeTableAsync(string table, string? database = null, string? schema = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a SELECT-only query against SQL Server and returns JSON results.
    /// </summary>
    /// <param name="sql">SQL SELECT statement. Use @paramName for parameters.</param>
    /// <param name="database">Optional database name. If omitted, uses current database.</param>
    /// <param name="parametersJson">Optional JSON object of parameters to bind.</param>
    /// <param name="maxRows">Maximum rows to return. Default 200, maximum 5000.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Query results in JSON format.</returns>
    Task<string> QueryAsync(
        string sql,
        string? database = null,
        string? parametersJson = null,
        int? maxRows = null,
        CancellationToken cancellationToken = default);
}
