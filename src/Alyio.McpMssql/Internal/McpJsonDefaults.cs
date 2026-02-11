// MIT License

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alyio.McpMssql.Internal;

/// <summary>
/// Provides default JSON serialization settings used by MCP components.
/// 
/// A new <see cref="JsonSerializerOptions"/> instance is returned on each access
/// to avoid shared mutable state.
/// </summary>
internal static class McpJsonDefaults
{
#pragma warning disable IDE0032 // Use auto property
    private static readonly JsonSerializerOptions s_template = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };
#pragma warning restore IDE0032 // Use auto property

    /// <summary>
    /// Gets a new instance of the default MCP <see cref="JsonSerializerOptions"/>.
    /// </summary>
    public static JsonSerializerOptions Options => new(s_template);
}

