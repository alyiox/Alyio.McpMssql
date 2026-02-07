using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Alyio.McpMssql.Tests.Infrastructure.Transports;

internal sealed class InMemoryClientTransport : IClientTransport
{
    private readonly Stream _input;
    private readonly Stream _output;
    private readonly ILoggerFactory? _loggerFactory;

    public InMemoryClientTransport(Stream input, Stream output, ILoggerFactory? loggerFactory)
    {
        _input = input;
        _output = output;
        _loggerFactory = loggerFactory;
    }

    public string Name => "mcp-mssql-test-client-transport";

    public ITransport Transport { get; private set; } = null!;

    public Task<ITransport> ConnectAsync(CancellationToken cancellationToken = default)
    {
        Transport = new StreamServerTransport(_input, _output, "mcp-mssql-test-client", _loggerFactory);
        return Task.FromResult(Transport);
    }
}
