// MIT License

using System.IO.Pipelines;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Tests.Helpers;

public sealed class McpHarness : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _serverRunTask;
    private readonly ITransport _clientTransport;
    private readonly ITransport _serverTransport;

    public required McpClient Client { get; init; }
    public required McpServer Server { get; init; }

    private McpHarness(ITransport clientTransport, ITransport serverTransport, Task serverRunTask)
    {
        _clientTransport = clientTransport;
        _serverTransport = serverTransport;
        _serverRunTask = serverRunTask;
    }

    public static async Task<McpHarness> StartAsync()
    {
        // Use two pipes to connect client <-> server bidirectionally.
        var clientToServer = new Pipe();
        var serverToClient = new Pipe();

        var serverInput = clientToServer.Reader.AsStream();
        var serverOutput = serverToClient.Writer.AsStream();
        var clientInput = serverToClient.Reader.AsStream();
        var clientOutput = clientToServer.Writer.AsStream();

        using var loggerFactory = LoggerFactory.Create(b =>
        {
            // Keep test logs quiet by default.
            b.SetMinimumLevel(LogLevel.Warning);
        });

        var serviceProvider = BuildServices();

        var serverTransport = new StreamServerTransport(serverInput, serverOutput, "mcp-mssql-test-server", loggerFactory);
        var serverOptions = CreateServerOptions(serviceProvider);

        var server = McpServer.Create(serverTransport, serverOptions, loggerFactory, serviceProvider);
        var serverRunTask = server.RunAsync();

        // Client side: implement an IClientTransport that returns a stream-backed ITransport.
        var clientTransport = new InMemoryClientTransport(clientInput, clientOutput, loggerFactory);
        var client = await McpClient.CreateAsync(clientTransport, loggerFactory: loggerFactory);

        return new McpHarness(clientTransport.Transport, serverTransport, serverRunTask)
        {
            Client = client,
            Server = server,
        };
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            _cts.Cancel();
        }
        catch
        {
            // Ignore.
        }

        await Client.DisposeAsync();
        await Server.DisposeAsync();

        await _clientTransport.DisposeAsync();
        await _serverTransport.DisposeAsync();

        try
        {
            await _serverRunTask.WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // Ignore: server may already be stopped.
        }
    }

    private static ServiceProvider BuildServices()
    {
        var configuration = ConfigurationLoader.Load();

        var services = new ServiceCollection();
        services.AddMcpMssqlOptions(configuration);

        return services.BuildServiceProvider();
    }

    private static McpServerOptions CreateServerOptions(IServiceProvider serviceProvider)
    {
        var toolCollection = new McpServerPrimitiveCollection<McpServerTool>();
        var toolCreateOptions = new McpServerToolCreateOptions
        {
            Services = serviceProvider,
        };

        // Discover tool methods on our tool type and register each tool.
        var toolType = typeof(Alyio.McpMssql.Tools.MssqlTools);
        foreach (var method in toolType.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (method.GetCustomAttribute<McpServerToolAttribute>() is null)
            {
                continue;
            }

            var tool = McpServerTool.Create(method, target: null, toolCreateOptions);
            toolCollection.Add(tool);
        }

        return new McpServerOptions
        {
            Capabilities = new ServerCapabilities { Tools = new() },
            ToolCollection = toolCollection,
        };
    }
}
