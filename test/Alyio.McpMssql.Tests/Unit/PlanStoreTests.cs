// MIT License

using Alyio.McpMssql.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Alyio.McpMssql.Tests.Unit;

public class PlanStoreTests : IDisposable
{
    private const string SampleXml = "<ShowPlanXML/>";
    private const string PlanFileExtension = ".sqlplan.xml";
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    private readonly string _plansDirectory = Path.Combine(Path.GetTempPath(), $"mcpmssql-plans-{Guid.NewGuid():N}");
    private readonly PlanStore _store;

    public PlanStoreTests()
    {
        _store = new PlanStore(_plansDirectory, NullLogger<PlanStore>.Instance);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_plansDirectory))
            {
                Directory.Delete(_plansDirectory, recursive: true);
            }
        }
        catch
        {
            // Best effort test cleanup.
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Save_Returns_NonEmpty_Id()
    {
        var id = await _store.SaveAsync(SampleXml, CancellationToken);

        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }

    [Fact]
    public async Task TryGet_Returns_Saved_Xml()
    {
        var id = await _store.SaveAsync(SampleXml, CancellationToken);

        var result = await _store.TryGetAsync(id, CancellationToken);

        Assert.Equal(SampleXml, result);
    }

    [Fact]
    public async Task TryGet_Returns_Null_For_Unknown_Id()
    {
        var result = await _store.TryGetAsync("nonexistent", CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task Save_Produces_Unique_Ids()
    {
        var id1 = await _store.SaveAsync(SampleXml, CancellationToken);
        var id2 = await _store.SaveAsync(SampleXml, CancellationToken);

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task TryGet_Returns_Null_And_Deletes_Expired_Plan_File_On_Initial_Load()
    {
        var id = await _store.SaveAsync(SampleXml, CancellationToken);
        var planPath = Path.Combine(_plansDirectory, $"{id}{PlanFileExtension}");
        File.SetLastWriteTimeUtc(planPath, DateTime.UtcNow - PlanStore.Ttl - TimeSpan.FromDays(1));

        var reloaded = new PlanStore(_plansDirectory, NullLogger<PlanStore>.Instance);
        var result = await reloaded.TryGetAsync(id, CancellationToken);

        Assert.Null(result);
        Assert.False(File.Exists(planPath));
    }

    [Fact]
    public async Task TryGet_Loads_Existing_Plan_Into_Memory()
    {
        const string id = "deadbeef";
        var planPath = Path.Combine(_plansDirectory, $"{id}{PlanFileExtension}");
        Directory.CreateDirectory(_plansDirectory);
        await File.WriteAllTextAsync(planPath, SampleXml, CancellationToken);

        var reloaded = new PlanStore(_plansDirectory, NullLogger<PlanStore>.Instance);
        var firstRead = await reloaded.TryGetAsync(id, CancellationToken);
        Assert.Equal(SampleXml, firstRead);

        File.Delete(planPath);
        var result = await reloaded.TryGetAsync(id, CancellationToken);

        Assert.Equal(SampleXml, result);
    }

    [Fact]
    public async Task TryGet_Evicts_Expired_Existing_File_On_First_Load()
    {
        const string id = "cafebabe";
        var planPath = Path.Combine(_plansDirectory, $"{id}{PlanFileExtension}");
        Directory.CreateDirectory(_plansDirectory);
        await File.WriteAllTextAsync(planPath, SampleXml, CancellationToken);
        File.SetLastWriteTimeUtc(planPath, DateTime.UtcNow - PlanStore.Ttl - TimeSpan.FromDays(1));

        var reloaded = new PlanStore(_plansDirectory, NullLogger<PlanStore>.Instance);

        var result = await reloaded.TryGetAsync(id, CancellationToken);

        Assert.Null(result);
        Assert.False(File.Exists(planPath));
    }
}
