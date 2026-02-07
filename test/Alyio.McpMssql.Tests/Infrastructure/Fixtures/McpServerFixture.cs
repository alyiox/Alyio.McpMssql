// MIT License
using System.IO.Pipelines;
using Alyio.McpMssql.Features;
using Alyio.McpMssql.Tests.Infrastructure.Database;
using Alyio.McpMssql.Tests.Infrastructure.DependencyInjection;
using Alyio.McpMssql.Tests.Infrastructure.Transports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Tests.Infrastructure.Fixtures;

/// <summary>
/// A shared xUnit fixture that manages the lifecycle of an MCP Server and Client connected via in-memory pipes.
/// </summary>
public sealed class McpServerFixture : IAsyncLifetime, IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private Task? _serverRunTask;
    private ServiceProvider? _serviceProvider;
    private ITransport? _clientTransport;
    private ITransport? _serverTransport;

    /// <summary>
    /// Gets the initialized MCP Client instance.
    /// </summary>
    public McpClient Client { get; private set; } = null!;

    /// <summary>
    /// Gets the initialized MCP Server instance.
    /// </summary>
    public McpServer Server { get; private set; } = null!;

    /// <summary>
    /// Initializes the in-memory pipes, server task, and client connection.
    /// </summary>
    public async Task InitializeAsync()
    {
        // 1. Setup bidirectional in-memory pipes
        var clientToServer = new Pipe();
        var serverToClient = new Pipe();

        var serverInput = clientToServer.Reader.AsStream();
        var serverOutput = serverToClient.Writer.AsStream();
        var clientInput = serverToClient.Reader.AsStream();
        var clientOutput = clientToServer.Writer.AsStream();

        // 2. Configure Services
        var services = ServiceBuilder.Build();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));

        services.AddMcpServer()
            .WithStreamServerTransport(serverInput, serverOutput)
            .WithToolsFromAssembly(typeof(MssqlTools).Assembly)
            .WithResourcesFromAssembly(typeof(MssqlResources).Assembly);

        _serviceProvider = services.BuildServiceProvider();

        // 3. Start Server
        Server = _serviceProvider.GetRequiredService<McpServer>();
        _serverTransport = _serviceProvider.GetRequiredService<ITransport>();
        _serverRunTask = Server.RunAsync(_cts.Token);

        // 4. Start Client
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        var clientTransportWrapper = new InMemoryClientTransport(clientInput, clientOutput, loggerFactory);
        _clientTransport = clientTransportWrapper.Transport;

        Client = await McpClient.CreateAsync(clientTransportWrapper, loggerFactory: loggerFactory);
    }

    /// <summary>
    /// Gracefully shuts down the server task and disposes of all transports and service providers.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // Signal shutdown
        await _cts.CancelAsync();

        // Dispose in order: Client -> Server -> Transports
        if (Client != null) await Client.DisposeAsync();
        if (Server != null) await Server.DisposeAsync();
        if (_clientTransport != null) await _clientTransport.DisposeAsync();
        if (_serverTransport != null) await _serverTransport.DisposeAsync();

        if (_serverRunTask != null)
        {
            try { await _serverRunTask.WaitAsync(TimeSpan.FromSeconds(2)); }
            catch (OperationCanceledException) { }
        }

        if (_serviceProvider != null) await _serviceProvider.DisposeAsync();
        _cts.Dispose();
    }

    /// <inheritdoc />
    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();
}
