// MIT License

using Alyio.McpMssql.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides read-only access to SQL Server catalog metadata.
///
/// Supports discovery of catalogs (databases), schemas, tabular relations
/// (tables and views), and routines (procedures and functions).
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// Lists catalogs accessible to the current connection.
    /// </summary>
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only list of catalog (database) names.
    /// </returns>
    Task<TabularResult> ListCatalogsAsync(
        string? profile = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists schemas within a catalog.
    /// </summary>
    /// <param name="catalog">
    /// Optional catalog (database) name. If omitted, uses the active catalog.
    /// </param>
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only list of schema names within the specified catalog.
    /// </returns>
    Task<TabularResult> ListSchemasAsync(
        string? catalog = null,
        string? profile = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists tabular relations (tables and views) within a catalog.
    /// </summary>
    /// <param name="catalog">
    /// Optional catalog (database) name. If omitted, uses the active catalog.
    /// </param>
    /// <param name="schema">
    /// Optional schema name. If omitted, lists relations from all schemas.
    /// </param>
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only list of relation names accessible within the specified scope.
    /// </returns>
    Task<TabularResult> ListRelationsAsync(
        string? catalog = null,
        string? schema = null,
        string? profile = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists routines (procedures and functions) within a catalog.
    /// </summary>
    /// <param name="catalog">
    /// Optional catalog (database) name. If omitted, uses the active catalog.
    /// </param>
    /// <param name="schema">
    /// Optional schema name. If omitted, lists routines from all schemas.
    /// </param>
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only list of routine names accessible within the specified scope.
    /// </returns>
    Task<TabularResult> ListRoutinesAsync(
        string? catalog = null,
        string? schema = null,
        string? profile = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Describes the column-level structure of a tabular relation.
    /// </summary>
    /// <param name="name">
    /// Name of the relation (table or view).
    /// </param>
    /// <param name="catalog">
    /// Optional catalog (database) name. If omitted, uses the active catalog.
    /// </param>
    /// <param name="schema">
    /// Optional schema name. If omitted, uses default schema resolution.
    /// </param>
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only list of column metadata describing the relation structure.
    /// </returns>
    Task<TabularResult> DescribeColumnsAsync(
        string name,
        string? catalog = null,
        string? schema = null,
        string? profile = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Describes the indexes of a tabular relation (table or view).
    /// </summary>
    /// <param name="name">
    /// Name of the relation (table or view).
    /// </param>
    /// <param name="catalog">
    /// Optional catalog (database) name. If omitted, uses the active catalog.
    /// </param>
    /// <param name="schema">
    /// Optional schema name. If omitted, uses default schema resolution.
    /// </param>
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only list of index metadata (one row per index column).
    /// </returns>
    Task<TabularResult> DescribeIndexesAsync(
        string name,
        string? catalog = null,
        string? schema = null,
        string? profile = null,
        CancellationToken cancellationToken = default);
}

