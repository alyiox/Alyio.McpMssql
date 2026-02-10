// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Describes a server-enforced execution option for inspection and reasoning.
///
/// This model is intended to be surfaced to MCP clients to explain
/// effective limits, defaults, and safety constraints applied by the server.
/// </summary>
public sealed class OptionDescriptor<T>
{
    /// <summary>
    /// The effective value enforced by the server.
    /// </summary>
    public required T Value { get; init; }

    /// <summary>
    /// Description of what this option controls, intended for humans
    /// and AI agents.
    ///
    /// This value is informational only and does not influence behavior.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Indicates whether the client or tool may request an override
    /// for this value.
    /// </summary>
    public bool IsOverridable { get; init; }

    /// <summary>
    /// Logical scope in which this option applies
    /// (for example, <c>"select"</c> or <c>"analyze"</c>).
    /// </summary>
    public required string Scope { get; init; }
}
