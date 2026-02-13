// MIT License

using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace Alyio.McpMssql.Tests;

internal static class ResultJsonExtensions
{
    public static string ReadAsText(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result switch
        {
            CallToolResult toolResult => toolResult.Content
                .OfType<TextContentBlock>()
                .FirstOrDefault()?.Text,

            ReadResourceResult resourceResult => resourceResult.Contents
                .OfType<TextResourceContents>()
                .FirstOrDefault()?.Text,

            _ => throw new NotSupportedException(
                $"Result type '{result.GetType().FullName}' is not supported.")
        } ?? throw new InvalidOperationException("Result does not contain text content.");
    }

    public static JsonElement ReadJsonRoot(this Result result)
    {
        var text = result.ReadAsText();

        using var doc = JsonDocument.Parse(text);
        return doc.RootElement.Clone();
    }

    public static (JsonElement columns, JsonElement rows) ReadColumnRows(this JsonElement root)
    {
        return (
            root.GetProperty("columns"),
            root.GetProperty("rows")
        );
    }

    /// <summary>Reads columns and rows from a nested table (e.g. SchemaResult.columns).</summary>
    public static (JsonElement columns, JsonElement rows) ReadColumnRowsFrom(this JsonElement root, string tableKey)
    {
        var table = root.TryGetProperty(tableKey, out var t) ? t : root.GetProperty(tableKey);
        return (table.GetProperty("columns"), table.GetProperty("rows"));
    }

    public static (bool truncated, int rowLimit) ReadMeta(this JsonElement root)
    {
        return (
            root.GetProperty("truncated").GetBoolean(),
            root.GetProperty("rowLimit").GetInt32()
        );
    }

    /// <summary>
    /// Gets a property by name, trying exact match then case-insensitive match.
    /// </summary>
    public static bool TryGetPropertyIgnoreCase(this JsonElement element, string name, out JsonElement value)
    {
        if (element.TryGetProperty(name, out value))
            return true;

        foreach (var p in element.EnumerateObject())
        {
            if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = p.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}

