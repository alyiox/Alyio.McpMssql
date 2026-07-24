// MIT License

namespace Alyio.McpMssql.Configuration;

/// <summary>
/// Server-enforced execution defaults for write commands (DDL/DML).
/// Writes and migrations frequently run longer than bounded interactive
/// queries, so a dedicated, higher timeout is used.
/// </summary>
/// <remarks>
/// These options only take effect for profiles that opt in via
/// <see cref="McpMssqlProfileOptions.AllowWrite"/>.
/// </remarks>
public sealed class WriteOptions
{
    /// <summary>
    /// Maximum execution time for a write command, in seconds.
    /// Clamped to <see cref="HardCommandTimeoutSeconds"/>.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Absolute, non-configurable hard limit for write command execution time.
    /// </summary>
    internal const int HardCommandTimeoutSeconds = 600;
}
