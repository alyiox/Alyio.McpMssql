// MIT License

using Alyio.McpMssql.Internal;

namespace Alyio.McpMssql.Tests.Unit;

public class CsvSerializerTests
{
    [Fact]
    public void Serialize_Empty_Result_Returns_Header_Only()
    {
        var columns = new[] { "Id", "Name" };
        var rows = Array.Empty<object?[]>();

        var csv = CsvSerializer.Serialize(columns, rows);

        Assert.Equal("Id,Name\n", csv);
    }

    [Fact]
    public void Serialize_Basic_Rows_Returns_Header_And_Rows()
    {
        var columns = new[] { "Id", "Name" };
        var rows = new[]
        {
            new object?[] { 1, "Alice" },
            new object?[] { 2, "Bob" },
        };

        var csv = CsvSerializer.Serialize(columns, rows);

        Assert.Equal("Id,Name\n1,Alice\n2,Bob\n", csv);
    }

    [Fact]
    public void Serialize_Null_Value_Writes_Empty_Field()
    {
        var columns = new[] { "Id", "Name" };
        var rows = new[]
        {
            new object?[] { 1, null },
        };

        var csv = CsvSerializer.Serialize(columns, rows);

        Assert.Equal("Id,Name\n1,\n", csv);
    }

    [Fact]
    public void Serialize_Value_With_Comma_Is_Quoted()
    {
        var columns = new[] { "Description" };
        var rows = new[]
        {
            new object?[] { "foo,bar" },
        };

        var csv = CsvSerializer.Serialize(columns, rows);

        Assert.Equal("Description\n\"foo,bar\"\n", csv);
    }

    [Fact]
    public void Serialize_Value_With_Quote_Is_Escaped()
    {
        var columns = new[] { "Name" };
        var rows = new[]
        {
            new object?[] { "say \"hello\"" },
        };

        var csv = CsvSerializer.Serialize(columns, rows);

        Assert.Equal("Name\n\"say \"\"hello\"\"\"\n", csv);
    }

    [Fact]
    public void Serialize_Header_Only_Result_Has_Trailing_Newline()
    {
        var csv = CsvSerializer.Serialize(["Col"], []);

        Assert.EndsWith("\n", csv);
    }

    [Fact]
    public void Serialize_Uses_Lf_Not_Crlf()
    {
        var csv = CsvSerializer.Serialize(["A", "B"], [["x", "y"]]);

        Assert.DoesNotContain("\r\n", csv);
        Assert.Contains("\n", csv);
    }
}
