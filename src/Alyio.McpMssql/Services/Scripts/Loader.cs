// MIT License

using System.Collections.Concurrent;
using System.Reflection;

namespace Alyio.McpMssql.Services.Scripts;

internal static class Loader
{
    private static readonly ConcurrentDictionary<string, string> s_cache = new();
    private static readonly Assembly s_assembly = typeof(Loader).Assembly;

    /// <summary>
    /// Reads the contents of an embedded SQL script by file name. Result is cached per file.
    /// </summary>
    /// <param name="fileName">Script file name (e.g. "constraints.sql").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The script text.</returns>
    /// <exception cref="FileNotFoundException">The embedded resource was not found.</exception>
    public static async Task<string> ReadText(string fileName, CancellationToken cancellationToken = default)
    {
        if (s_cache.TryGetValue(fileName, out string? value))
        {
            return value;
        }

        var stream = s_assembly.GetManifestResourceStream(typeof(Loader), fileName)
            ?? throw new FileNotFoundException($"Embedded script '{fileName}' not found.");

        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        s_cache[fileName] = text;
        return text;
    }
}
