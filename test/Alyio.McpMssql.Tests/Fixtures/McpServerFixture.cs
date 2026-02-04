// MIT License

using Alyio.McpMssql.Tests.Helpers;

namespace Alyio.McpMssql.Tests.Fixtures;

/// <summary>
/// xUnit class fixture for sharing a single MCP server instance across multiple test methods.
/// This improves test performance by avoiding repeated server setup/teardown.
/// </summary>
public sealed class McpServerFixture : IAsyncLifetime
{
    /// <summary>
    /// The shared MCP harness (client + server) used by all tests in the class.
    /// </summary>
    public McpHarness Harness { get; private set; } = null!;

    /// <summary>
    /// Called once before any test in the class runs.
    /// Initializes the MCP server and client.
    /// </summary>
    public async Task InitializeAsync()
    {
        Harness = await McpHarness.StartAsync();
    }

    /// <summary>
    /// Called once after all tests in the class have completed.
    /// Disposes the MCP server and client.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (Harness != null)
        {
            await Harness.DisposeAsync();
        }
    }
}
