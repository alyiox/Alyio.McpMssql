// MIT License

using Microsoft.Extensions.Logging;

namespace Alyio.McpMssql.Services;

/// <summary>
/// Execution plan store backed by <see cref="ContentStore"/>.
/// Plans are cached under <c>~/.cache/mcp-mssql/plans/</c> with a 7-day TTL.
/// </summary>
internal sealed class PlanStore : ContentStore, IPlanStore
{
    private const string CacheRelativePath = ".cache/mcp-mssql/plans";
    private const string FileExtension = ".sqlplan.xml";
    internal static readonly TimeSpan Ttl = TimeSpan.FromDays(7);

    public PlanStore(ILogger<PlanStore> logger)
        : base(CacheRelativePath, FileExtension, Ttl, logger)
    {
    }

    /// <summary>
    /// Internal constructor for testing with an explicit directory path.
    /// </summary>
    internal PlanStore(string plansDirectory, ILogger<PlanStore> logger)
        : base(plansDirectory, FileExtension, Ttl, logger, default)
    {
    }
}
