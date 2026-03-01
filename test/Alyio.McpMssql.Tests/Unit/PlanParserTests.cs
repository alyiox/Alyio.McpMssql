// MIT License

using System.Reflection;
using System.Xml.Linq;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;

namespace Alyio.McpMssql.Tests.Unit;

public class PlanParserTests
{
    /// <summary>
    /// Minimal valid showplan XML with no warnings, missing indexes,
    /// wait stats, or statistics. Used to test empty-collection paths.
    /// </summary>
    private const string MinimalPlanXml = """
        <?xml version="1.0" encoding="utf-16"?>
        <ShowPlanXML xmlns="http://schemas.microsoft.com/sqlserver/2004/07/showplan" Version="1.6" Build="16.0.4125.3">
          <BatchSequence><Batch><Statements>
            <StmtSimple StatementEstRows="5" StatementOptmLevel="TRIVIAL" CardinalityEstimationModelVersion="160" StatementSubTreeCost="0.003" StatementType="SELECT" QueryHash="0xABCD" QueryPlanHash="0x1234">
              <QueryPlan DegreeOfParallelism="1">
                <QueryTimeStats CpuTime="0" ElapsedTime="1" />
                <RelOp NodeId="0" PhysicalOp="Clustered Index Scan" LogicalOp="Clustered Index Scan" EstimateRows="5" EstimatedTotalSubtreeCost="0.003" EstimateCPU="0.0001" EstimateIO="0.0029" Parallel="false" EstimatedExecutionMode="Row">
                  <RunTimeInformation>
                    <RunTimeCountersPerThread Thread="0" ActualRows="5" ActualRowsRead="5" ActualEndOfScans="1" ActualExecutions="1" ActualElapsedms="0" ActualCPUms="0" />
                  </RunTimeInformation>
                  <IndexScan Ordered="true" ScanDirection="FORWARD" Storage="RowStore">
                    <Object Database="[McpMssqlTest]" Schema="[dbo]" Table="[Users]" Index="[PK__Users__1788CC4C]" IndexKind="Clustered" Storage="RowStore" />
                  </IndexScan>
                </RelOp>
              </QueryPlan>
            </StmtSimple>
          </Statements></Batch></BatchSequence>
        </ShowPlanXML>
        """;

    // --- Statement ---

    [Fact]
    public async Task ParseStatement_Extracts_Cost_And_Level()
    {
        var result = await ParseFixtureAsync();

        Assert.Equal(12.458, result.Statement.EstimatedCost, 3);
        Assert.Equal(3, result.Statement.EstimatedRows);
        Assert.Equal("FULL", result.Statement.OptimizationLevel);
        Assert.Equal(170, result.Statement.CeVersion);
        Assert.Equal("0xA1B2C3D4E5F60718", result.Statement.QueryHash);
        Assert.Equal("0x18F6E5D4C3B2A100", result.Statement.PlanHash);
        Assert.True(result.Statement.BatchModeOnRowStore);
        Assert.Equal(4, result.Statement.DegreeOfParallelism);
    }

    [Fact]
    public async Task ParseStatement_Extracts_QueryTimeStats()
    {
        var result = await ParseFixtureAsync();

        Assert.Equal(3890, result.Statement.CpuTimeMs);
        Assert.Equal(412, result.Statement.ElapsedTimeMs);
    }

    [Fact]
    public async Task ParseStatement_Extracts_MemoryGrant()
    {
        var result = await ParseFixtureAsync();

        Assert.NotNull(result.Statement.MemoryGrant);
        Assert.Equal(80260, result.Statement.MemoryGrant.GrantedKb);
        Assert.Equal(80260, result.Statement.MemoryGrant.DesiredKb);
        Assert.Equal(5120, result.Statement.MemoryGrant.MaxUsedKb);
        Assert.Equal(0, result.Statement.MemoryGrant.GrantWaitTimeMs);
        Assert.Equal("No: First Execution", result.Statement.MemoryGrant.FeedbackAdjusted);
    }

    [Fact]
    public void ParseStatement_Minimal_Has_No_MemoryGrant()
    {
        var result = ParseInline(MinimalPlanXml);

        Assert.Null(result.Statement.MemoryGrant);
        Assert.Equal("TRIVIAL", result.Statement.OptimizationLevel);
        Assert.Equal(160, result.Statement.CeVersion);
    }

    // --- Top Operators ---

    [Fact]
    public async Task ParseOperators_Returns_Multiple_Ranked_By_Cost()
    {
        var result = await ParseFixtureAsync();

        Assert.True(result.TopOperators.Count > 1);
        var first = result.TopOperators[0];
        var second = result.TopOperators[1];
        Assert.True(first.EstimatedCostPct >= second.EstimatedCostPct);
    }

    [Fact]
    public async Task ParseOperators_Users_Scan_Has_Highest_Cost()
    {
        var result = await ParseFixtureAsync();

        var usersScan = result.TopOperators.First(o => o.NodeId == 3);
        Assert.Equal("Clustered Index Scan", usersScan.PhysicalOp);
        Assert.Equal("[dbo].[Users].[PK__Users__1788CC4C]", usersScan.ObjectName);
        Assert.True(usersScan.IsParallel);
        Assert.Equal("Row", usersScan.ExecutionMode);
    }

    [Fact]
    public async Task ParseOperators_Sums_ActualRows_Across_Threads()
    {
        var result = await ParseFixtureAsync();

        // NodeId=3 (Users scan): Thread 0=0, Thread 1=1, Thread 2=0
        var usersScan = result.TopOperators.First(o => o.NodeId == 3);
        Assert.Equal(1, usersScan.ActualRows);
    }

    [Fact]
    public async Task ParseOperators_Reports_RowsRead_For_Scan()
    {
        var result = await ParseFixtureAsync();

        var usersScan = result.TopOperators.First(o => o.NodeId == 3);
        Assert.Equal(100000, usersScan.EstimatedRowsRead);
        Assert.Equal(5, usersScan.ActualRowsRead);
    }

    [Fact]
    public async Task ParseOperators_Reports_Parallel_And_ExecutionMode()
    {
        var result = await ParseFixtureAsync();

        var hashAgg = result.TopOperators.First(o => o.NodeId == 1);
        Assert.True(hashAgg.IsParallel);
        Assert.Equal("Batch", hashAgg.ExecutionMode);
        Assert.Equal("Hash Match", hashAgg.PhysicalOp);
        Assert.Equal("Aggregate", hashAgg.LogicalOp);
    }

    [Fact]
    public async Task ParseOperators_Orders_Scan_Not_Parallel()
    {
        var result = await ParseFixtureAsync();

        var ordersScan = result.TopOperators.First(o => o.NodeId == 4);
        Assert.False(ordersScan.IsParallel);
        Assert.Equal("[dbo].[Orders].[PK__Orders__C3905BCF]", ordersScan.ObjectName);
        Assert.Equal(7, ordersScan.ActualRows);
    }

    // --- Cardinality Issues ---

    [Fact]
    public void ParseCardinalityIssues_Minimal_Returns_Empty()
    {
        var result = ParseInline(MinimalPlanXml);

        Assert.Empty(result.CardinalityIssues);
    }

    // --- Warnings ---

    [Fact]
    public async Task ParseWarnings_Extracts_SpillToTempDb()
    {
        var result = await ParseFixtureAsync();

        var spill = result.Warnings.FirstOrDefault(w => w.Kind == "SpillToTempDb");
        Assert.NotNull(spill);
        Assert.Equal(2, spill.NodeId);
        Assert.Equal("Hash Match", spill.Operator);
        Assert.Contains("level 1", spill.Detail);
        Assert.Contains("2 threads", spill.Detail);
    }

    [Fact]
    public async Task ParseWarnings_Extracts_ImplicitConversion()
    {
        var result = await ParseFixtureAsync();

        var convert = result.Warnings.FirstOrDefault(w => w.Kind == "ImplicitConversion");
        Assert.NotNull(convert);
        Assert.Null(convert.NodeId);
        Assert.Contains("Cardinality Estimate", convert.Detail);
        Assert.Contains("UserName", convert.Detail);
    }

    [Fact]
    public void ParseWarnings_Minimal_Returns_Empty()
    {
        var result = ParseInline(MinimalPlanXml);

        Assert.Empty(result.Warnings);
    }

    // --- Missing Indexes ---

    [Fact]
    public async Task ParseMissingIndexes_Extracts_Columns_And_Impact()
    {
        var result = await ParseFixtureAsync();

        Assert.Single(result.MissingIndexes);
        var mi = result.MissingIndexes[0];
        Assert.Equal("[dbo].[Users]", mi.Table);
        Assert.Equal(["[UserName]"], mi.EqualityColumns);
        Assert.Empty(mi.InequalityColumns);
        Assert.Contains("[UserId]", mi.IncludeColumns);
        Assert.Contains("[Email]", mi.IncludeColumns);
        Assert.Equal(78.32, mi.Impact);
    }

    [Fact]
    public void ParseMissingIndexes_Minimal_Returns_Empty()
    {
        var result = ParseInline(MinimalPlanXml);

        Assert.Empty(result.MissingIndexes);
    }

    // --- Wait Stats ---

    [Fact]
    public async Task ParseWaitStats_Returns_Top_Waits_Ordered()
    {
        var result = await ParseFixtureAsync();

        Assert.NotEmpty(result.WaitStats);
        Assert.True(result.WaitStats.Count <= 5);

        var top = result.WaitStats[0];
        Assert.Equal("SOS_SCHEDULER_YIELD", top.WaitType);
        Assert.Equal(3142, top.WaitTimeMs);
        Assert.Equal(178, top.WaitCount);

        for (int i = 1; i < result.WaitStats.Count; i++)
        {
            Assert.True(result.WaitStats[i - 1].WaitTimeMs >= result.WaitStats[i].WaitTimeMs);
        }
    }

    [Fact]
    public void ParseWaitStats_Minimal_Returns_Empty()
    {
        var result = ParseInline(MinimalPlanXml);

        Assert.Empty(result.WaitStats);
    }

    // --- Statistics ---

    [Fact]
    public async Task ParseStatistics_Flags_Low_Sampling()
    {
        var result = await ParseFixtureAsync();

        Assert.Single(result.Statistics);
        var stat = result.Statistics[0];
        Assert.Equal("[Users]", stat.Table);
        Assert.Equal("[_WA_Sys_00000002_Users]", stat.Name);
        Assert.True(stat.AutoCreated);
        Assert.Equal(4.51, stat.SamplingPct, 2);
    }

    [Fact]
    public void ParseStatistics_Minimal_Returns_Empty()
    {
        var result = ParseInline(MinimalPlanXml);

        Assert.Empty(result.Statistics);
    }

    // --- Helpers ---

    private static async Task<AnalyzeResult> ParseFixtureAsync()
    {
        var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Alyio.McpMssql.Tests.Unit.Plans.complex_plan.xml")
            ?? throw new FileNotFoundException("Embedded plan fixture not found.");

        using var reader = new StreamReader(stream);
        var xml = await reader.ReadToEndAsync();
        return ParseInline(xml);
    }

    private static AnalyzeResult ParseInline(string xml)
    {
        var doc = XDocument.Parse(xml);
        return PlanParser.Parse(doc);
    }
}
