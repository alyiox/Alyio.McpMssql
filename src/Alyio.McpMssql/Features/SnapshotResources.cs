// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Resource for retrieving a query result snapshot as CSV by identifier.
/// </summary>
[McpServerResourceType]
public static class SnapshotResources
{
    /// <summary>
    /// Retrieve a query result snapshot as CSV.
    /// </summary>
    [McpServerResource(
        Name = "snapshot",
        UriTemplate = "mssql://snapshots/{id}",
        MimeType = "text/csv")]
    [Description(
        "[MSSQL] Retrieve full query result snapshot as CSV by ID. " +
        "Use the id from run_query (snapshot_uri); entries expire after 1 day and may return not found.")]
    public static async Task<string> GetSnapshotAsync(
        ISnapshotStore snapshotStore,
        [Description("Opaque id from run_query (snapshot_uri path segment). Src: run_query.")]
        string id,
        CancellationToken cancellationToken = default)
    {
        return await McpExecutor.RunAsTextAsync(async ct =>
        {
            var csv = await snapshotStore.TryGetAsync(id, ct).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Snapshot '{id}' not found or has expired.");

            return csv;
        }, cancellationToken).ConfigureAwait(false);
    }
}
