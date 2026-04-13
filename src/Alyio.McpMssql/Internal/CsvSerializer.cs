// MIT License

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Alyio.McpMssql.Internal;

/// <summary>
/// Serializes tabular data (column headers and row values) to
/// RFC 4180-compliant CSV using <see cref="CsvHelper"/>.
/// </summary>
internal static class CsvSerializer
{
    private static readonly CsvConfiguration s_config = new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        NewLine = "\n",
    };

    /// <summary>
    /// Converts columns and rows into a CSV string.
    /// <c>null</c> values are written as empty fields.
    /// </summary>
    public static string Serialize(
        IReadOnlyList<string> columns,
        IReadOnlyList<object?[]> rows)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, s_config);

        foreach (var col in columns)
        {
            csv.WriteField(col);
        }

        csv.NextRecord();

        foreach (var row in rows)
        {
            foreach (var val in row)
            {
                csv.WriteField(val);
            }

            csv.NextRecord();
        }

        return writer.ToString();
    }
}
