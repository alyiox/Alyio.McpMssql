// MIT License

using Alyio.McpMssql.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
var userConfigPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".config",
    "mcpmssql",
    "appsettings.json");

// IMPORTANT: MCP stdio transport uses stdout for protocol messages.
// Send logs to stderr to avoid corrupting the JSON-RPC stream.
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: false)
    .AddJsonFile(userConfigPath, optional: true, reloadOnChange: false);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(
        System.Reflection.Assembly.GetExecutingAssembly(),
        optional: true,
        reloadOnChange: false);
}

builder.Configuration
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services
    .AddMcpMssql(builder.Configuration)
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(serializerOptions: McpJsonDefaults.Options)
    .WithResourcesFromAssembly();

await builder.Build().RunAsync();

