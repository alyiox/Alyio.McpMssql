// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// SQL Server environment metadata and execution limits.
/// Derived from SERVERPROPERTY and server configuration; safe for AI reasoning.
/// </summary>
public sealed class ServerProperties
{
    /// <summary>
    /// Full SQL Server product version (e.g. 16.0.4125.3).
    /// </summary>
    public string ProductVersion { get; init; } = string.Empty;

    /// <summary>
    /// Product servicing level (RTM, SP, CU).
    /// </summary>
    public string ProductLevel { get; init; } = string.Empty;

    /// <summary>
    /// Cumulative update level (e.g. CU12).
    /// </summary>
    public string ProductUpdateLevel { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable SQL Server edition string.
    /// </summary>
    public string Edition { get; init; } = string.Empty;

    /// <summary>
    /// Numeric engine edition identifier.
    /// Authoritative enum value from SERVERPROPERTY('EngineEdition').
    /// </summary>
    public int EngineEdition { get; init; }

    /// <summary>
    /// Human-readable engine edition name derived from EngineEdition.
    /// </summary>
    public string EngineEditionName { get; init; } = string.Empty;

    /// <summary>
    /// Server-enforced execution limits (row limits, timeouts, etc.).
    /// </summary>
    public required ExecutionLimits Limits { get; init; }
}
