// MIT License

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alyio.McpMssql.Tests.Helpers
{
/// <summary>
/// Factory for creating service scopes in tests.
/// </summary>
    internal static class ServiceScopeFactory
    {
        private static readonly IServiceScopeFactory s_factory;

        static ServiceScopeFactory()
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Configuration.AddUserSecrets(typeof(ServiceScopeFactory).Assembly);

            builder.Services.AddMcpMssqlOptions(builder.Configuration);

            var host = builder.Build();

            s_factory = host.Services.GetRequiredService<IServiceScopeFactory>();
        }

        /// <summary>
        /// Creates a new service scope.
        /// </summary>
        /// <returns>A new <see cref="IServiceScope"/>.</returns>
        public static IServiceScope Create()
        {
            return s_factory.CreateScope();
        }
    }
}

