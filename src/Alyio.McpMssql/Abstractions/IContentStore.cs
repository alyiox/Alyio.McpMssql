// MIT License

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Ephemeral content store keyed by a short identifier.
/// Entries are subject to time-based eviction.
/// </summary>
public interface IContentStore
{
    /// <summary>
    /// Stores content and returns a unique identifier.
    /// </summary>
    /// <param name="content">The content string to store.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A short hex identifier for retrieval.</returns>
    Task<string> SaveAsync(string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves stored content by identifier.
    /// </summary>
    /// <param name="id">The identifier returned by <see cref="SaveAsync"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The stored content string, or <c>null</c> if expired or not found.</returns>
    Task<string?> TryGetAsync(string id, CancellationToken cancellationToken = default);
}
