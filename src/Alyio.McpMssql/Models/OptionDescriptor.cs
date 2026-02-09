// MIT License

namespace Alyio.McpMssql.Models;

/// <summary>
/// Describes a server-enforced option for inspection and reasoning.
/// </summary>
public sealed class OptionDescriptor<T>
{
    /// <summary>
    /// The effective value enforced by the server.
    /// </summary>
    public required T Value { get; init; }

    /// <summary>
    /// Human- and AI-readable explanation of what this option controls.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Indicates whether the client or tool may override this value.
    /// </summary>
    public bool IsOverridable { get; init; }

    /// <summary>
    /// Logical scope of the option (e.g. "select", "analyze").
    /// </summary>
    public required string Scope { get; init; }
}

