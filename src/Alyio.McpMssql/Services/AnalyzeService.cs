// MIT License

using System.Xml.Linq;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using Microsoft.Data.SqlClient;

namespace Alyio.McpMssql.Services;

internal sealed class AnalyzeService(
    IProfileService profileService,
    IPlanStore planStore) : IAnalyzeService
{
    public async Task<AnalyzeResult> AnalyzeAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        string? profile = null,
        bool estimated = false,
        CancellationToken cancellationToken = default)
    {
        SqlReadOnlyValidator.Validate(sql);
        var resolved = profileService.Resolve(profile);

        using var conn = new SqlConnection(resolved.ConnectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        var timeoutSeconds = resolved.Analyze.CommandTimeoutSeconds;

        var xmlPlan = estimated
            ? await CaptureEstimatedPlanAsync(conn, sql, parameters, timeoutSeconds, cancellationToken)
                .ConfigureAwait(false)
            : await CaptureActualPlanAsync(conn, sql, parameters, timeoutSeconds, cancellationToken)
                .ConfigureAwait(false);

        var doc = XDocument.Parse(xmlPlan);
        var result = PlanParser.Parse(doc);

        var id = planStore.Save(xmlPlan);

        return new AnalyzeResult
        {
            PlanUri = $"mssql://plans/{id}",
            Statement = result.Statement,
            TopOperators = result.TopOperators,
            CardinalityIssues = result.CardinalityIssues,
            Warnings = result.Warnings,
            MissingIndexes = result.MissingIndexes,
            WaitStats = result.WaitStats,
            Statistics = result.Statistics,
        };
    }

    /// <summary>
    /// Executes the query with <c>SET STATISTICS XML ON</c> and extracts
    /// the actual plan XML (with runtime statistics) from the result sets.
    /// </summary>
    private static async Task<string> CaptureActualPlanAsync(
        SqlConnection conn,
        string sql,
        IReadOnlyDictionary<string, object>? parameters,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SET STATISTICS XML ON;\n{sql};\nSET STATISTICS XML OFF;";
        cmd.CommandTimeout = commandTimeoutSeconds;
        AddParameters(cmd, parameters);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken)
            .ConfigureAwait(false);

        string? xmlPlan = null;

        // The XML plan is emitted as a single-row, single-column
        // nvarchar(max) result set after the data result set(s).
        do
        {
            if (reader.FieldCount == 1 && reader.GetFieldType(0) == typeof(string))
            {
                if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var value = reader.GetString(0);
                    if (value.StartsWith('<'))
                    {
                        xmlPlan = value;
                    }
                }
            }
            else
            {
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                }
            }
        }
        while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));

        return xmlPlan
            ?? throw new InvalidOperationException(
                "No execution plan XML was returned. Ensure the query is a valid SELECT.");
    }

    /// <summary>
    /// Compiles (but does not execute) the query with <c>SET SHOWPLAN_XML ON</c>
    /// and returns the estimated plan XML. No runtime statistics are available.
    /// </summary>
    private static async Task<string> CaptureEstimatedPlanAsync(
        SqlConnection conn,
        string sql,
        IReadOnlyDictionary<string, object>? parameters,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        // SHOWPLAN_XML is a session-level setting. While ON, all submitted
        // SQL returns the estimated plan XML without execution.
        // We must turn it off before returning the connection.
        using var enableCmd = conn.CreateCommand();
        enableCmd.CommandText = "SET SHOWPLAN_XML ON;";
        enableCmd.CommandTimeout = commandTimeoutSeconds;
        await enableCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = commandTimeoutSeconds;
            AddParameters(cmd, parameters);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken)
                .ConfigureAwait(false);

            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return reader.GetString(0);
            }

            throw new InvalidOperationException(
                "No estimated plan XML was returned. Ensure the query is a valid SELECT.");
        }
        finally
        {
            using var disableCmd = conn.CreateCommand();
            disableCmd.CommandText = "SET SHOWPLAN_XML OFF;";
            await disableCmd.ExecuteNonQueryAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    private static void AddParameters(SqlCommand cmd, IReadOnlyDictionary<string, object>? parameters)
    {
        var sqlParams = SqlParameterHelper.Build(parameters);
        if (sqlParams is not null)
        {
            foreach (var p in sqlParams)
            {
                cmd.Parameters.Add(p);
            }
        }
    }
}
