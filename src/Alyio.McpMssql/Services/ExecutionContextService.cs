// MIT License

using Alyio.McpMssql.Models;
using Alyio.McpMssql.Options;
using Microsoft.Extensions.Options;
using ExecutionContext = Alyio.McpMssql.Models.ExecutionContext;

#pragma warning disable IDE0130
namespace Alyio.McpMssql.Services;
#pragma warning restore IDE0130

internal sealed class ExecutionContextService(IOptions<McpMssqlOptions> options) : IExecutionContextService
{
    private readonly McpMssqlOptions _options = options.Value;

    public ValueTask<ExecutionContext> GetContextAsync(CancellationToken cancellationToken = default)
    {
        var context = new ExecutionContext
        {
            Select = new SelectExecutionContext
            {
                DefaultMaxRows = new OptionDescriptor<int>
                {
                    Value = _options.Select.DefaultMaxRows,
                    Description =
                        "Used when an inspection query does not explicitly specify a row limit, " +
                        "to prevent accidental large result sets.",
                    IsOverridable = true,
                    Scope = "select",
                },

                HardRowLimit = new OptionDescriptor<int>
                {
                    Value = SelectExecutionOptions.HardRowLimit,
                    Description =
                        "Absolute maximum number of rows that may be returned for any inspection query, " +
                        "regardless of request.",
                    IsOverridable = false,
                    Scope = "select",
                },

                CommandTimeoutSeconds = new OptionDescriptor<int>
                {
                    Value = _options.Select.CommandTimeoutSeconds,
                    Description =
                        "Maximum execution time allowed for an interactive inspection query before it is terminated.",
                    IsOverridable = false,
                    Scope = "select",
                }
            }
        };

        return ValueTask.FromResult(context);
    }
}

