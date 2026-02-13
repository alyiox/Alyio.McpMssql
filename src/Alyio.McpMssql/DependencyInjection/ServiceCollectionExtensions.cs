// MIT License

using Alyio.McpMssql;
using Alyio.McpMssql.Services;
using Microsoft.Extensions.Configuration;

#pragma warning disable IDE0130 // Intentional: extension methods for IServiceCollection
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Dependency injection helpers for the MCP SQL Server integration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all MCP SQL Server services and configuration.
    /// </summary>
    public static IServiceCollection AddMcpMssql(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMcpMssqlOptions(configuration);

        services.AddSingleton<IServerContextService, ServerContextService>();
        services.AddSingleton<ICatalogService, CatalogService>();
        services.AddSingleton<IQueryService, QueryService>();
        services.AddSingleton<IExecutionContextService, ExecutionContextService>();
        services.AddSingleton<IProfileService, ProfileService>();

        return services;
    }
}
