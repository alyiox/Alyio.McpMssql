// MIT License

using Alyio.McpMssql.Services;

namespace Alyio.McpMssql.Tests.Unit;

public class PlanStoreTests
{
    private const string SampleXml = "<ShowPlanXML/>";

    [Fact]
    public void Save_Returns_NonEmpty_Id()
    {
        var store = new PlanStore();

        var id = store.Save(SampleXml);

        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }

    [Fact]
    public void TryGet_Returns_Saved_Xml()
    {
        var store = new PlanStore();
        var id = store.Save(SampleXml);

        var result = store.TryGet(id);

        Assert.Equal(SampleXml, result);
    }

    [Fact]
    public void TryGet_Returns_Null_For_Unknown_Id()
    {
        var store = new PlanStore();

        var result = store.TryGet("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public void Save_Produces_Unique_Ids()
    {
        var store = new PlanStore();

        var id1 = store.Save(SampleXml);
        var id2 = store.Save(SampleXml);

        Assert.NotEqual(id1, id2);
    }
}
