// MIT License

using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Models;

#pragma warning disable IDE0130
namespace Alyio.McpMssql.Services;
#pragma warning restore IDE0130

internal sealed class ExecutionContextService(IProfileService profileService) : IExecutionContextService
{
    public ValueTask<ExecutionLimits> GetLimitsAsync(
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        var resolved = profileService.Resolve(profile);
        var limits = new ExecutionLimits
        {
            Query = new QueryLimits
            {
                DefaultMaxRows = new OptionDescriptor<int>
                {
                    Value = resolved.Query.DefaultMaxRows,
                    Description =
                        "Used when a query does not explicitly specify a row limit, " +
                        "to prevent accidental large result sets.",
                    IsOverridable = true,
                    Scope = "query",
                },

                HardRowLimit = new OptionDescriptor<int>
                {
                    Value = QueryOptions.HardRowLimit,
                    Description =
                        "Absolute maximum number of rows that may be returned for any query, " +
                        "regardless of request.",
                    IsOverridable = false,
                    Scope = "query",
                },

                CommandTimeoutSeconds = new OptionDescriptor<int>
                {
                    Value = resolved.Query.CommandTimeoutSeconds,
                    Description =
                        "Maximum execution time allowed for an interactive query before it is terminated.",
                    IsOverridable = false,
                    Scope = "query",
                }
            }
        };

        return ValueTask.FromResult(limits);
    }
}
