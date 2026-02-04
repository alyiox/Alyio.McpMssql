// MIT License

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Alyio.McpMssql.Tests.Helpers;

/// <summary>
/// Loads test configuration from environment variables and user secrets.
/// </summary>
public class ConfigurationLoader
{
    /// <summary>
    /// Configuration key for connection string (hierarchical format for user secrets).
    /// </summary>
    public const string ConnectionStringKey = "MCP_MSSQL_CONNECTION_STRING";

    /// <summary>
    /// Loads configuration for tests.
    /// </summary>
    public static ConfigurationManager Load()
    {
        var builder = Host.CreateEmptyApplicationBuilder(settings: null);

        builder.Configuration
            .AddEnvironmentVariables()
            .AddUserSecrets<ConfigurationLoader>(optional: true);

        return builder.Configuration;
    }
}
