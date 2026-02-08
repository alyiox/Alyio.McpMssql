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
}
