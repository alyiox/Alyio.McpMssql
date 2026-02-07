// MIT License

using System.Text.Json.Serialization;

namespace Alyio.McpMssql.Models;

/// <summary>
/// Represents connectivity and environment metadata for the SQL Server connection.
/// </summary>
public class SqlConnectionContext
{
    /// <summary>The hostname or IP address of the database server.</summary>
    [JsonPropertyName("server")]
    public string Server { get; set; } = string.Empty;

    /// <summary>The TCP port used for the connection (default 1433).</summary>
    [JsonPropertyName("port")]
    public string Port { get; set; } = "1433";

    /// <summary>The name of the currently active database (Initial Catalog).</summary>
    [JsonPropertyName("database")]
    public string Database { get; set; } = string.Empty;

    /// <summary>The effective user identity on the server (via SUSER_SNAME).</summary>
    [JsonPropertyName("user")]
    public string User { get; set; } = string.Empty;

    /// <summary>The full SQL Server version string and engine details.</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

