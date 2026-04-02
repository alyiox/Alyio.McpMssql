// MIT License

using System.ComponentModel;
using Alyio.McpMssql.Internal;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Features;

/// <summary>
/// Resource for retrieving the full XML execution plan by identifier.
/// </summary>
[McpServerResourceType]
public static class PlanResources
{
    /// <summary>
    /// Retrieve the full XML execution plan.
    /// </summary>
    [McpServerResource(
        Name = "plan",
        UriTemplate = "mssql://plans/{id}",
        MimeType = "application/xml")]
    [Description(
        "[MSSQL] Retrieve full XML execution plan by ID. " +
        "Use the id from analyze_query (plan_uri); entries expire and may return not found.")]
    public static async Task<string> GetPlanAsync(
        IPlanStore planStore,
        [Description("Opaque id from analyze_query (plan_uri path segment). Src: analyze_query.")]
        string id,
        CancellationToken cancellationToken = default)
    {
        return await McpExecutor.RunAsTextAsync(ct =>
        {
            var xml = planStore.TryGet(id)
                ?? throw new InvalidOperationException($"Plan '{id}' not found or has expired.");

            return Task.FromResult(xml);
        }, cancellationToken).ConfigureAwait(false);
    }
}
