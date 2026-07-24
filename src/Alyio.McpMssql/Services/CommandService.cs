// MIT License

using Alyio.McpMssql.Configuration;
using Alyio.McpMssql.Internal;
using Alyio.McpMssql.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Alyio.McpMssql.Services;

internal sealed partial class CommandService(IProfileService profileService, ILogger<CommandService> logger) : ICommandService
{
    public async Task<CommandResult> ExecuteAsync(
        string sql,
        string? catalog = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        string? profile = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("SQL command cannot be empty.", nameof(sql));
        }

        McpMssqlProfileOptions resolved = profileService.Resolve(profile);
        string profileName = string.IsNullOrWhiteSpace(profile)
            ? McpMssqlOptions.DefaultProfileName
            : profile;

        if (!resolved.AllowWrite)
        {
            LogRejected(profileName);

            throw new InvalidOperationException(
                $"Profile '{profileName}' does not permit write operations. " +
                "Enable AllowWrite for this profile to use run_command.");
        }

        using var conn = new SqlConnection(resolved.ConnectionString);

        var messages = new List<string>();
        conn.InfoMessage += (_, e) =>
        {
            foreach (SqlError error in e.Errors)
            {
                messages.Add(error.Message);
            }
        };

        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(catalog))
        {
            conn.ChangeDatabase(catalog);
        }

        using SqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandTimeout = resolved.Write.CommandTimeoutSeconds;

        IReadOnlyList<SqlParameter>? sqlParameters = SqlParameterHelper.Build(parameters);
        if (sqlParameters is not null)
        {
            foreach (SqlParameter p in sqlParameters)
            {
                cmd.Parameters.Add(p);
            }
        }

        int rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        LogExecuted(
            profileName,
            string.IsNullOrWhiteSpace(catalog) ? "<default>" : catalog,
            rowsAffected);

        return new CommandResult
        {
            RowsAffected = rowsAffected,
            Messages = messages.AsReadOnly(),
        };
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Rejected write command on profile '{Profile}': write operations are disabled.")]
    private partial void LogRejected(string profile);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Executed write command on profile '{Profile}' (catalog: {Catalog}); rows affected: {RowsAffected}.")]
    private partial void LogExecuted(string profile, string catalog, int rowsAffected);
}
