// MIT License

using System.Globalization;
using System.Xml.Linq;
using Alyio.McpMssql.Models;

namespace Alyio.McpMssql.Internal;

/// <summary>
/// Parses SQL Server showplan XML into a compact <see cref="AnalyzeResult"/>.
/// Stateless; all methods are pure functions over <see cref="XDocument"/>.
/// </summary>
internal static class PlanParser
{
    private static readonly XNamespace Ns =
        "http://schemas.microsoft.com/sqlserver/2004/07/showplan";

    private const int TopOperatorCount = 5;
    private const double CardinalityThreshold = 10.0;

    /// <summary>
    /// Parses the showplan XML into an <see cref="AnalyzeResult"/>.
    /// The <see cref="AnalyzeResult.PlanUri"/> field is left empty;
    /// the caller is responsible for setting it.
    /// </summary>
    public static AnalyzeResult Parse(XDocument doc)
    {
        var stmt = doc.Descendants(Ns + "StmtSimple").First();
        var queryPlan = stmt.Descendants(Ns + "QueryPlan").First();
        var rootRelOp = queryPlan.Element(Ns + "RelOp")!;
        var rootSubtreeCost = Double(rootRelOp, "EstimatedTotalSubtreeCost");

        var allOps = CollectOperators(rootRelOp, rootSubtreeCost);

        return new AnalyzeResult
        {
            PlanUri = string.Empty,
            Statement = ParseStatement(stmt, queryPlan),
            TopOperators = ParseTopOperators(allOps),
            CardinalityIssues = ParseCardinalityIssues(allOps),
            Warnings = ParseWarnings(queryPlan, allOps),
            MissingIndexes = ParseMissingIndexes(queryPlan),
            WaitStats = ParseWaitStats(queryPlan),
            Statistics = ParseStatistics(queryPlan),
        };
    }

    private static StatementSummary ParseStatement(XElement stmt, XElement queryPlan)
    {
        var memoryGrant = queryPlan.Element(Ns + "MemoryGrantInfo");
        var queryTime = queryPlan.Element(Ns + "QueryTimeStats");

        return new StatementSummary
        {
            EstimatedCost = Double(stmt, "StatementSubTreeCost"),
            EstimatedRows = Double(stmt, "StatementEstRows"),
            OptimizationLevel = Str(stmt, "StatementOptmLevel") ?? "UNKNOWN",
            CeVersion = Int(stmt, "CardinalityEstimationModelVersion"),
            QueryHash = Str(stmt, "QueryHash"),
            PlanHash = Str(stmt, "QueryPlanHash"),
            BatchModeOnRowStore = Bool(stmt, "BatchModeOnRowStoreUsed"),
            DegreeOfParallelism = NullableInt(queryPlan, "DegreeOfParallelism"),
            NonParallelReason = Str(queryPlan, "NonParallelPlanReason"),
            CpuTimeMs = queryTime is not null ? NullableInt(queryTime, "CpuTime") : null,
            ElapsedTimeMs = queryTime is not null ? NullableInt(queryTime, "ElapsedTime") : null,
            MemoryGrant = memoryGrant is not null ? ParseMemoryGrant(memoryGrant) : null,
        };
    }

    private static MemoryGrantInfo ParseMemoryGrant(XElement el) => new()
    {
        GrantedKb = Int(el, "GrantedMemory"),
        DesiredKb = Int(el, "DesiredMemory"),
        MaxUsedKb = Int(el, "MaxUsedMemory"),
        GrantWaitTimeMs = Int(el, "GrantWaitTime"),
        FeedbackAdjusted = Str(el, "IsMemoryGrantFeedbackAdjusted"),
    };

    private sealed class OperatorInfo
    {
        public required XElement Element { get; init; }
        public required int NodeId { get; init; }
        public required string PhysicalOp { get; init; }
        public required string LogicalOp { get; init; }
        public required double EstimatedCostPct { get; init; }
        public required double EstimatedRows { get; init; }
        public double? EstimatedRowsRead { get; init; }
        public bool IsParallel { get; init; }
        public string? ExecutionMode { get; init; }
        public string? ObjectName { get; init; }

        public long? ActualRows { get; init; }
        public long? ActualRowsRead { get; init; }
        public long? ActualExecutions { get; init; }
        public long? ActualElapsedMs { get; init; }
    }

    private static List<OperatorInfo> CollectOperators(XElement root, double rootSubtreeCost)
    {
        var ops = new List<OperatorInfo>();
        CollectOperatorsRecursive(root, rootSubtreeCost, ops);
        return ops;
    }

    private static void CollectOperatorsRecursive(
        XElement relOp,
        double rootSubtreeCost,
        List<OperatorInfo> ops)
    {
        var cpuCost = Double(relOp, "EstimateCPU");
        var ioCost = Double(relOp, "EstimateIO");
        var ownCost = cpuCost + ioCost;
        var costPct = rootSubtreeCost > 0 ? ownCost / rootSubtreeCost * 100.0 : 0.0;

        var runtime = relOp.Element(Ns + "RunTimeInformation");
        var threads = runtime?.Elements(Ns + "RunTimeCountersPerThread");

        long? actualRows = null;
        long? actualRowsRead = null;
        long? actualExecutions = null;
        long? actualElapsedMs = null;

        if (threads is not null)
        {
            long totalRows = 0, totalRowsRead = 0, totalExec = 0, maxElapsed = 0;
            bool hasData = false;

            foreach (var t in threads)
            {
                hasData = true;
                totalRows += Long(t, "ActualRows");
                totalRowsRead += Long(t, "ActualRowsRead");
                totalExec += Long(t, "ActualExecutions");
                var elapsed = Long(t, "ActualElapsedms");
                if (elapsed > maxElapsed) maxElapsed = elapsed;
            }

            if (hasData)
            {
                actualRows = totalRows;
                actualRowsRead = totalRowsRead > 0 ? totalRowsRead : null;
                actualExecutions = totalExec;
                actualElapsedMs = maxElapsed;
            }
        }

        var objectName = ExtractObjectName(relOp);
        var estimatedRowsRead = NullableDouble(relOp, "EstimatedRowsRead");

        ops.Add(new OperatorInfo
        {
            Element = relOp,
            NodeId = Int(relOp, "NodeId"),
            PhysicalOp = Str(relOp, "PhysicalOp") ?? "Unknown",
            LogicalOp = Str(relOp, "LogicalOp") ?? "Unknown",
            EstimatedCostPct = Math.Round(costPct, 1),
            EstimatedRows = Double(relOp, "EstimateRows"),
            EstimatedRowsRead = estimatedRowsRead,
            IsParallel = Bool(relOp, "Parallel") ?? false,
            ExecutionMode = Str(relOp, "EstimatedExecutionMode"),
            ObjectName = objectName,
            ActualRows = actualRows,
            ActualRowsRead = actualRowsRead,
            ActualExecutions = actualExecutions,
            ActualElapsedMs = actualElapsedMs,
        });

        foreach (var child in relOp.Elements(Ns + "RelOp"))
        {
            CollectOperatorsRecursive(child, rootSubtreeCost, ops);
        }
    }

    private static string? ExtractObjectName(XElement relOp)
    {
        var obj = relOp.Descendants(Ns + "Object").FirstOrDefault();
        if (obj is null) return null;

        var schema = Str(obj, "Schema");
        var table = Str(obj, "Table");
        var index = Str(obj, "Index");

        if (table is null) return null;

        return index is not null
            ? $"{schema}.{table}.{index}"
            : $"{schema}.{table}";
    }

    private static List<PlanOperator> ParseTopOperators(List<OperatorInfo> ops)
    {
        return ops
            .OrderByDescending(o => o.EstimatedCostPct)
            .Take(TopOperatorCount)
            .Select(o => new PlanOperator
            {
                NodeId = o.NodeId,
                PhysicalOp = o.PhysicalOp,
                LogicalOp = o.LogicalOp,
                EstimatedCostPct = o.EstimatedCostPct,
                EstimatedRows = o.EstimatedRows,
                ActualRows = o.ActualRows,
                EstimatedRowsRead = o.EstimatedRowsRead,
                ActualRowsRead = o.ActualRowsRead,
                ActualExecutions = o.ActualExecutions,
                ActualElapsedMs = o.ActualElapsedMs,
                ExecutionMode = o.ExecutionMode,
                IsParallel = o.IsParallel,
                ObjectName = o.ObjectName,
            })
            .ToList();
    }

    private static List<CardinalityIssue> ParseCardinalityIssues(List<OperatorInfo> ops)
    {
        var issues = new List<CardinalityIssue>();

        foreach (var op in ops)
        {
            if (op.ActualRows is null || op.EstimatedRows <= 0) continue;

            var ratio = (double)op.ActualRows.Value / op.EstimatedRows;

            if (ratio >= CardinalityThreshold || ratio <= 1.0 / CardinalityThreshold)
            {
                var label = op.ObjectName is not null
                    ? $"{op.PhysicalOp} on {op.ObjectName}"
                    : $"{op.PhysicalOp} ({op.LogicalOp})";

                issues.Add(new CardinalityIssue
                {
                    NodeId = op.NodeId,
                    Operator = label,
                    EstimatedRows = op.EstimatedRows,
                    ActualRows = op.ActualRows.Value,
                    Ratio = Math.Round(ratio, 1),
                });
            }
        }

        return issues;
    }

    private static List<PlanWarning> ParseWarnings(
        XElement queryPlan,
        List<OperatorInfo> ops)
    {
        var warnings = new List<PlanWarning>();

        // Plan-level warnings
        var planWarnings = queryPlan.Element(Ns + "Warnings");
        if (planWarnings is not null)
        {
            foreach (var convert in planWarnings.Elements(Ns + "PlanAffectingConvert"))
            {
                var issue = Str(convert, "ConvertIssue");
                var expr = Str(convert, "Expression");
                warnings.Add(new PlanWarning
                {
                    Kind = "ImplicitConversion",
                    NodeId = null,
                    Operator = null,
                    Detail = $"{issue}: {expr}",
                });
            }

            foreach (var mgw in planWarnings.Elements(Ns + "MemoryGrantWarning"))
            {
                var kind = Str(mgw, "GrantWarningKind") ?? "";
                var granted = Str(mgw, "GrantedMemory");
                var used = Str(mgw, "MaxUsedMemory");
                var warningKind = kind.Contains("Excessive", StringComparison.OrdinalIgnoreCase)
                    ? "MemoryGrantExcessive"
                    : "MemoryGrantIncrease";

                warnings.Add(new PlanWarning
                {
                    Kind = warningKind,
                    NodeId = null,
                    Operator = null,
                    Detail = $"Granted {granted} KB, used {used} KB",
                });
            }
        }

        // Operator-level warnings
        foreach (var op in ops)
        {
            var opWarnings = op.Element.Element(Ns + "Warnings");
            if (opWarnings is null) continue;

            if (opWarnings.Element(Ns + "SpillToTempDb") is { } spill)
            {
                var level = Str(spill, "SpillLevel");
                var threadCount = Str(spill, "SpilledThreadCount");
                var detail = $"Spill level {level ?? "?"}";
                if (threadCount is not null)
                {
                    detail += $", {threadCount} threads spilled";
                }

                warnings.Add(new PlanWarning
                {
                    Kind = "SpillToTempDb",
                    NodeId = op.NodeId,
                    Operator = op.PhysicalOp,
                    Detail = detail,
                });
            }

            if (opWarnings.Element(Ns + "NoJoinPredicate") is not null)
            {
                warnings.Add(new PlanWarning
                {
                    Kind = "NoJoinPredicate",
                    NodeId = op.NodeId,
                    Operator = op.PhysicalOp,
                    Detail = "No join predicate — possible cartesian product",
                });
            }

            if (opWarnings.Element(Ns + "ColumnsWithNoStatistics") is { } noStats)
            {
                var cols = noStats.Elements(Ns + "ColumnReference")
                    .Select(c => $"{Str(c, "Table")}.{Str(c, "Column")}")
                    .ToList();

                warnings.Add(new PlanWarning
                {
                    Kind = "ColumnsWithNoStatistics",
                    NodeId = op.NodeId,
                    Operator = op.PhysicalOp,
                    Detail = string.Join(", ", cols),
                });
            }

            if (opWarnings.Element(Ns + "UnmatchedIndexes") is not null)
            {
                warnings.Add(new PlanWarning
                {
                    Kind = "UnmatchedIndexes",
                    NodeId = op.NodeId,
                    Operator = op.PhysicalOp,
                    Detail = "Filtered index exists but could not be used",
                });
            }
        }

        return warnings;
    }

    private static List<MissingIndex> ParseMissingIndexes(XElement queryPlan)
    {
        var result = new List<MissingIndex>();

        foreach (var group in queryPlan.Descendants(Ns + "MissingIndexGroup"))
        {
            var impact = Double(group, "Impact");

            foreach (var mi in group.Elements(Ns + "MissingIndex"))
            {
                var schema = Str(mi, "Schema");
                var table = Str(mi, "Table");
                var tableName = schema is not null ? $"{schema}.{table}" : table ?? "?";

                var equality = new List<string>();
                var inequality = new List<string>();
                var include = new List<string>();

                foreach (var cg in mi.Elements(Ns + "ColumnGroup"))
                {
                    var usage = Str(cg, "Usage");
                    var cols = cg.Elements(Ns + "Column")
                        .Select(c => Str(c, "Name") ?? "?")
                        .ToList();

                    switch (usage)
                    {
                        case "EQUALITY": equality.AddRange(cols); break;
                        case "INEQUALITY": inequality.AddRange(cols); break;
                        case "INCLUDE": include.AddRange(cols); break;
                    }
                }

                result.Add(new MissingIndex
                {
                    Table = tableName,
                    EqualityColumns = equality,
                    InequalityColumns = inequality,
                    IncludeColumns = include,
                    Impact = Math.Round(impact, 2),
                });
            }
        }

        return result;
    }

    private static List<WaitStat> ParseWaitStats(XElement queryPlan)
    {
        var waitStats = queryPlan.Element(Ns + "WaitStats");
        if (waitStats is null) return [];

        return waitStats.Elements(Ns + "Wait")
            .OrderByDescending(w => Int(w, "WaitTimeMs"))
            .Take(TopOperatorCount)
            .Select(w => new WaitStat
            {
                WaitType = Str(w, "WaitType") ?? "Unknown",
                WaitTimeMs = Int(w, "WaitTimeMs"),
                WaitCount = Int(w, "WaitCount"),
            })
            .ToList();
    }

    private static List<StatisticsInfo> ParseStatistics(XElement queryPlan)
    {
        var statsUsage = queryPlan.Element(Ns + "OptimizerStatsUsage");
        if (statsUsage is null) return [];

        return statsUsage.Elements(Ns + "StatisticsInfo")
            .Where(s =>
            {
                var sampling = NullableDouble(s, "SamplingPercent");
                var modCount = Long(s, "ModificationCount");
                return (sampling.HasValue && sampling.Value < 50.0) || modCount > 0;
            })
            .Select(s =>
            {
                var name = Str(s, "Statistics") ?? "?";
                return new StatisticsInfo
                {
                    Table = Str(s, "Table") ?? "?",
                    Name = name,
                    AutoCreated = name.Contains("_WA_Sys_", StringComparison.Ordinal),
                    SamplingPct = Math.Round(NullableDouble(s, "SamplingPercent") ?? 0, 2),
                    ModificationCount = Long(s, "ModificationCount"),
                    LastUpdate = Str(s, "LastUpdate"),
                };
            })
            .ToList();
    }

    // --- Attribute helpers ---

    private static string? Str(XElement el, string attr) =>
        (string?)el.Attribute(attr);

    private static int Int(XElement el, string attr) =>
        int.TryParse((string?)el.Attribute(attr), NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
            ? v : 0;

    private static int? NullableInt(XElement el, string attr) =>
        int.TryParse((string?)el.Attribute(attr), NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
            ? v : null;

    private static long Long(XElement el, string attr) =>
        long.TryParse((string?)el.Attribute(attr), NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
            ? v : 0;

    private static double Double(XElement el, string attr) =>
        double.TryParse((string?)el.Attribute(attr), NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
            ? v : 0.0;

    private static double? NullableDouble(XElement el, string attr) =>
        double.TryParse((string?)el.Attribute(attr), NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
            ? v : null;

    private static bool? Bool(XElement el, string attr) =>
        bool.TryParse((string?)el.Attribute(attr), out var v) ? v : null;
}
