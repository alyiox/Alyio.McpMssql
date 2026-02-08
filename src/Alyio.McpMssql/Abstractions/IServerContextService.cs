// MIT License
using Alyio.McpMssql.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides connection-level context and environment metadata for SQL Server.
///
/// This service exposes stable, read-only information about the active
/// connection, such as server identity, user context, and engine version.
/// </summary>
public interface IServerContextService
{
    /// <summary>
    /// Retrieves metadata describing the active SQL Server connection.
    /// </summary>
    Task<ServerConnectionContext> GetConnectionContextAsync(CancellationToken cancellationToken = default);
}

