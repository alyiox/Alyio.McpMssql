// MIT License

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Store for execution plan XML, keyed by a short identifier.
/// Plans are ephemeral and subject to time-based eviction.
/// </summary>
public interface IPlanStore
{
    /// <summary>
    /// Stores an XML execution plan and returns a unique identifier.
    /// </summary>
    /// <param name="xml">The raw showplan XML string.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A short hex identifier for retrieval.</returns>
    Task<string> SaveAsync(string xml, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a stored XML execution plan by identifier.
    /// </summary>
    /// <param name="id">The identifier returned by <see cref="SaveAsync"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The raw XML string, or <c>null</c> if the plan has expired or was not found.</returns>
    Task<string?> TryGetAsync(string id, CancellationToken cancellationToken = default);
}
