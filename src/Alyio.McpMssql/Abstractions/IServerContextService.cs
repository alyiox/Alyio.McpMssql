// MIT License
using Alyio.McpMssql.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Alyio.McpMssql;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides server-scoped, read-only SQL Server context information
/// derived from safe system metadata and suitable for AI reasoning.
///
/// This service exposes non-identifying environment data only.
/// </summary>
/// <remarks>
/// Implementations MUST NOT expose server names, instance names,
/// hostnames, network topology, credentials, or user identity.
/// </remarks>
public interface IServerContextService
{
    /// <summary>
    /// Retrieves stable SQL Server engine metadata such as version,
    /// edition, and engine type derived from SERVERPROPERTY.
    /// </summary>
    /// <param name="profile">
    /// Optional profile name. If null or empty, the default profile is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    Task<ServerPropertiesContext> GetPropertiesAsync(
        string? profile = null,
        CancellationToken cancellationToken = default);

    // --- Future expansion (intentionally explicit & typed) ---

    // Task<ServerCapabilitiesContext> GetCapabilitiesAsync(
    //     CancellationToken cancellationToken = default);

    // Task<ServerLimitsContext> GetLimitsAsync(
    //     CancellationToken cancellationToken = default);
}
