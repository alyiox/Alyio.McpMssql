// MIT License

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alyio.McpMssql.Tests.Helpers;

/// <summary>
/// Provides a centralized way to build and configure the service collection 
/// for the MCP MSSQL server using standard host defaults.
/// </summary>
internal static class ServiceBuilder
{
    /// <summary>
    /// Creates an <see cref="IServiceCollection"/> initialized with default configuration, 
    /// logging, user secrets, and MSSQL-specific options.
    /// </summary>
    /// <returns>A configured service collection ready for provider building.</returns>
    public static IServiceCollection Build()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration.AddUserSecrets(typeof(ServiceBuilder).Assembly);

        builder.Services.AddMcpMssql(builder.Configuration);

        return builder.Services;
    }
}
