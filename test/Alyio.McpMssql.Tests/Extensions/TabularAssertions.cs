// MIT License

using System.Text.Json;

namespace Alyio.McpMssql.Tests;

internal static class TabularAssertions
{
    public static void AssertHasColumns(this JsonElement columns, params string[] expectedColumns)
    {
        var actual = columns
            .EnumerateArray()
            .Select(c => c.GetString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var column in expectedColumns)
        {
            Assert.Contains(column, actual);
        }
    }

    public static void AssertHasColumns(this IReadOnlyList<string> columns, params string[] expectedColumns)
    {
        Assert.NotNull(columns);

        foreach (var expected in expectedColumns)
        {
            Assert.Contains(expected, columns);
        }
    }

    /// <summary>
    /// Splits the header line from a CSV string into column names.
    /// Assumes simple values with no embedded commas or quotes.
    /// </summary>
    public static IReadOnlyList<string> ParseCsvHeaders(string csv)
    {
        var firstLine = csv.Split('\n')[0];
        return firstLine.Split(',');
    }

    /// <summary>
    /// Returns each data row (after the header) as a string array.
    /// Assumes simple values with no embedded commas or quotes.
    /// </summary>
    public static IReadOnlyList<string[]> ParseCsvDataRows(string csv)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
            return [];

        return lines[1..].Select(l => l.Split(',')).ToArray();
    }
}
