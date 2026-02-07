// MIT License

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder();

// IMPORTANT: MCP stdio transport uses stdout for protocol messages.
// Send logs to stderr to avoid corrupting the JSON-RPC stream.
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpMssql(builder.Configuration)
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly();

await builder.Build().RunAsync();

