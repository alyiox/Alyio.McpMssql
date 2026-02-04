// MIT License

using Alyio.McpMssql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

// IMPORTANT: MCP stdio transport uses stdout for protocol messages.
// Send logs to stderr to avoid corrupting the JSON-RPC stream.
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.Services
    .AddMcpMssqlOptions(builder.Configuration)
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

