// MIT License

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Store for query result snapshots (CSV), keyed by a short identifier.
/// Snapshots are ephemeral and subject to time-based eviction.
/// Empty by design: <see cref="IContentStore"/> provides the full contract;
/// this interface exists solely for DI disambiguation.
/// </summary>
public interface ISnapshotStore : IContentStore
{
}
