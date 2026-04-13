# MCP SQL Server Tool

[![Build Status](https://github.com/alyiox/Alyio.McpMssql/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/alyiox/Alyio.McpMssql/actions/workflows/ci.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Alyio.McpMssql.svg)](https://www.nuget.org/packages/Alyio.McpMssql)

A read-only [Model Context Protocol (MCP)](https://modelcontextprotocol.io) server for Microsoft SQL Server that supports metadata discovery, parameterized queries, and query analysis, with profile-based configuration and strict no-DML/DDL enforcement.

**Requirements:** .NET 10.0 SDK, SQL Server, and a connection string.

## Quick start

Set `MCPMSSQL_CONNECTION_STRING` and run the server in one of these ways:

```bash
# Option 1: Run from NuGet package (e.g. with MCP Inspector)
export MCPMSSQL_CONNECTION_STRING="Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
npx -y @modelcontextprotocol/inspector dotnet dnx Alyio.McpMssql --prerelease
```

```bash
# Option 2: Install and run as a global tool
dotnet tool install --global Alyio.McpMssql --prerelease
export MCPMSSQL_CONNECTION_STRING="Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
npx -y @modelcontextprotocol/inspector mcp-mssql
```

```bash
# Option 3: Run from source (clone repo, then)
export MCPMSSQL_CONNECTION_STRING="Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
npx -y @modelcontextprotocol/inspector dotnet run --project src/Alyio.McpMssql
```

Use `--prerelease` for pre-release builds. When using the package: entrypoint `dotnet dnx Alyio.McpMssql`; when installed as a tool: command `mcp-mssql`.

## Configuration

All settings use the **MCPMSSQL** prefix. **Flat** environment variables (e.g. `MCPMSSQL_CONNECTION_STRING`) are the straightforward way to configure the **default** profile when you have a single connection. **Hierarchical** env vars (e.g. `MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING`) spell out the same profile tree explicitly and are the natural choice when you define multiple named profiles. The same structure under key `MCPMSSQL` is also supported in a user-scoped `appsettings.json` file:

- Unix-like: `~/.config/mcp-mssql/appsettings.json`
- Windows: `%USERPROFILE%\.config\mcp-mssql\appsettings.json`

**Single server / one connection:** Set the following as environment variables.

```bash
# Connection string (required).
export MCPMSSQL_CONNECTION_STRING="Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"

# Optional description for the default profile (tooling/AI discovery).
export MCPMSSQL_DESCRIPTION="Primary connection"

# Optional max rows per interactive query (default `500`; hard ceiling `1000`).
export MCPMSSQL_QUERY_MAX_ROWS="500"

# Optional query timeout in seconds (default `30`).
export MCPMSSQL_QUERY_COMMAND_TIMEOUT_SECONDS="60"

# Optional max rows for snapshot queries (default `10000`; hard ceiling `50000`).
export MCPMSSQL_QUERY_SNAPSHOT_MAX_ROWS="10000"

# Optional snapshot query timeout in seconds (default `120`).
export MCPMSSQL_QUERY_SNAPSHOT_COMMAND_TIMEOUT_SECONDS="120"

# Optional analyze timeout in seconds (default `300`).
export MCPMSSQL_ANALYZE_COMMAND_TIMEOUT_SECONDS="300"
```

**Multiple servers or connections:** Use environment variables or the same structure in the user-scoped `appsettings.json` file.

- Use environment variables with the **MCPMSSQL** prefix, then `__` for each level (e.g. `MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING`).
- For each profile, set `MCPMSSQL__PROFILES__<name>__CONNECTIONSTRING`, and optionally add `__DESCRIPTION` and `__QUERY`.
- The default profile is the one named `default`. Define it with `MCPMSSQL__PROFILES__DEFAULT__...`, or use flat variables (`MCPMSSQL_CONNECTION_STRING`, etc.) for the same default profile in a single-connection setup.

Example (environment variables):

```bash
# Default profile
export MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING="Server=...;User ID=...;Password=...;"
export MCPMSSQL__PROFILES__DEFAULT__DESCRIPTION="Primary connection"
export MCPMSSQL__PROFILES__DEFAULT__QUERY__MAXROWS="500"
export MCPMSSQL__PROFILES__DEFAULT__QUERY__COMMANDTIMEOUTSECONDS="60"
export MCPMSSQL__PROFILES__DEFAULT__QUERY__SNAPSHOTMAXROWS="10000"
export MCPMSSQL__PROFILES__DEFAULT__QUERY__SNAPSHOTCOMMANDTIMEOUTSECONDS="120"
export MCPMSSQL__PROFILES__DEFAULT__ANALYZE__COMMANDTIMEOUTSECONDS="300"

# Named profile
export MCPMSSQL__PROFILES__WAREHOUSE__CONNECTIONSTRING="Server=warehouse.example.com;..."
export MCPMSSQL__PROFILES__WAREHOUSE__QUERY__MAXROWS="1000"
export MCPMSSQL__PROFILES__WAREHOUSE__QUERY__COMMANDTIMEOUTSECONDS="120"
export MCPMSSQL__PROFILES__WAREHOUSE__ANALYZE__COMMANDTIMEOUTSECONDS="600"

# Same default profile via flat variables (single connection):
export MCPMSSQL_CONNECTION_STRING="Server=...;User ID=...;Password=...;"
export MCPMSSQL_DESCRIPTION="Primary connection"
export MCPMSSQL_QUERY_MAX_ROWS="500"
export MCPMSSQL_QUERY_COMMAND_TIMEOUT_SECONDS="60"
export MCPMSSQL_QUERY_SNAPSHOT_MAX_ROWS="10000"
export MCPMSSQL_QUERY_SNAPSHOT_COMMAND_TIMEOUT_SECONDS="120"
export MCPMSSQL_ANALYZE_COMMAND_TIMEOUT_SECONDS="300"
```

Example (user-scoped `appsettings.json`):

```json
{
  "McpMssql": {
    "Profiles": {
      "default": {
        "ConnectionString": "Server=...;User ID=...;Password=...;",
        "Description": "Primary connection",
        "Query": {
          "MaxRows": 500,
          "CommandTimeoutSeconds": 60,
          "SnapshotMaxRows": 10000,
          "SnapshotCommandTimeoutSeconds": 120
        },
        "Analyze": {
          "CommandTimeoutSeconds": 300
        }
      },
      "warehouse": {
        "ConnectionString": "Server=warehouse.example.com;...",
        "Description": "Warehouse read-only"
      }
    }
  }
}
```

**Local development:** Set the connection string, then run with that environment:

- Store connection string in user-secrets (Development only).

```bash
dotnet user-secrets set "MCPMSSQL_CONNECTION_STRING" "..." --project src/Alyio.McpMssql
```

- Run server with MCP inspector (use `DOTNET_ENVIRONMENT=Development` so user-secrets load).

```bash
npx -y @modelcontextprotocol/inspector -e DOTNET_ENVIRONMENT=Development dotnet run --project src/Alyio.McpMssql
```

**Azure SQL / Microsoft Entra ID:** This MCP server uses [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient), which supports Microsoft Entra (Azure AD) authentication. Set the `Authentication` property in the connection string to a supported mode (e.g. `Active Directory Default`, `Active Directory Managed Identity`, or `Active Directory Interactive`) when connecting to Azure SQL. See [Connect to Azure SQL with Microsoft Entra authentication and SqlClient](https://learn.microsoft.com/en-us/sql/connect/ado-net/sql/azure-active-directory-authentication) for all modes and details.

## Breaking changes

Versioned breaking changes for this package and global tool. Review before upgrading.

### 1.0.0-beta.2

#### User-scoped `appsettings.json` directory renamed

**Before:** Configuration file lived next to the old folder name:

- Unix-like: `~/.config/mcpmssql/appsettings.json`
- Windows: `%USERPROFILE%\.config\mcpmssql\appsettings.json`

**After:** Use the directory that matches the global tool command name `mcp-mssql`:

- Unix-like: `~/.config/mcp-mssql/appsettings.json`
- Windows: `%USERPROFILE%\.config\mcp-mssql\appsettings.json`

**Migration:** Move `appsettings.json` from `.config/mcpmssql/` to
`.config/mcp-mssql/` (create the folder if needed). The JSON shape and keys are
unchanged; only the path changes.

**Rationale:** Aligns the on-disk config location with the shipped tool identity
and documented paths, and removes ambiguous dual-path loading.

## Tools and resources

All tools accept an optional `profile`; when omitted, the default profile is used.

**Tools**

| Tool | Description | Key params |
|---|---|---|
| **`list_profiles`** | List configured connection profiles. Call first when picking a non-default profile. | — |
| **`get_server_properties`** | Get server properties and execution limits (timeouts, row caps, guardrails). | `profile` |
| **`list_objects`** | List catalog metadata. `kind=catalog`: databases; `schema`: schemas; `relation`: tables/views; `routine`: procedures/functions. `catalog` omitted → active catalog (ignored for `kind=catalog`). `schema` omission depends on kind. | `kind`, `profile`, `catalog`, `schema` |
| **`get_object`** | Get metadata for one relation or routine. Use `list_objects` to resolve names. Returns empty detail payloads if `includes` is null. | `kind`, `name`, `profile`, `catalog`, `schema`, `includes` |
| **`run_query`** | Execute read-only T-SQL SELECT; only SELECT allowed (no DML/DDL). Returns results as CSV in the `data` field (inline) or a snapshot resource URI when `snapshot=true`. Inline limit: 500 rows (hard ceiling 1000). Snapshot limit: 10 000 rows. Prefer `analyze_query` for plan tuning. | `sql`, `profile`, `catalog`, `parameters`, `snapshot` |
| **`analyze_query`** | Analyze execution plan for a read-only SELECT. Returns compact JSON summary (cost, operators, cardinality, warnings, indexes, waits, stats). Fetch full XML from `plan_uri`; does not return result rows. | `sql`, `profile`, `catalog`, `parameters`, `estimated` |

- **`kind`** — `catalog`, `schema`, `relation`, or `routine`. For `get_object`, only `relation` or `routine`.
- **`includes`** — Array of detail sections: `columns`, `indexes`, `constraints` (relations only), `definition` (routines only).

**Resources**

| URI template | Description |
|---|---|
| `mssql://profiles` | List configured connection profiles. Same data as `list_profiles`. |
| `mssql://server-properties?{profile}` | Get server properties and execution limits. Same data as `get_server_properties`. |
| `mssql://objects?{kind,profile,catalog,schema}` | List catalog metadata. Schema omission behavior matches `list_objects`. |
| `mssql://objects/{kind}/{name}{?profile,catalog,schema,includes}` | Get metadata for one relation or routine. `includes` is required. |
| `mssql://plans/{id}` | Retrieve full XML execution plan by ID from `analyze_query`; entries expire after 7 days. |
| `mssql://snapshots/{id}` | Retrieve full query result as CSV by ID from `run_query` (snapshot=true); entries expire after 1 day. |

Resources mirror their corresponding tools and return JSON (except `mssql://plans/{id}` which returns XML and `mssql://snapshots/{id}` which returns CSV).

## Security

Read-only (`SELECT` only); parameterized `@paramName`. Use environment variables or user-secrets for connection strings—never commit secrets.

## MCP host examples

Snippets for common MCP clients. Copy one and replace the connection string; ensure `dotnet` is on your PATH.

Note: For security, store connection strings in environment variables or a secret manager (for example, user-secrets in development) instead of directly in config files.

### Cursor / Gemini

```json
{
  "mcpServers": {
    "mssql": {
      "command": "dotnet",
      "args": ["dnx", "Alyio.McpMssql", "--prerelease", "--yes"],
      "env": {
        "MCPMSSQL_CONNECTION_STRING": "Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
      }
    }
  }
}
```

### Codex

```toml
[mcp_servers.mssql]
command = "dotnet"
args = ["dnx", "Alyio.McpMssql", "--prerelease", "--yes"]
[mcp_servers.mssql.env]
MCPMSSQL_CONNECTION_STRING = "Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
```

### Open Code

```json
{
  "$schema": "https://opencode.ai/config.json",
  "mcp": {
    "mssql": {
      "type": "local",
      "enabled": true,
      "command": ["dotnet", "dnx", "Alyio.McpMssql", "--prerelease", "--yes"],
      "environment": {
        "MCPMSSQL_CONNECTION_STRING": "Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
      }
    }
  }
}
```

### Claude Code

```json
{
  "mcpServers": {
    "mssql": {
      "command": "dotnet",
      "args": ["dnx", "Alyio.McpMssql", "--prerelease", "--yes"],
      "env": {
        "MCPMSSQL_CONNECTION_STRING": "Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
      }
    }
  }
}
```

### GitHub Copilot

```json
{
  "inputs": [],
  "servers": {
    "mssql": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["dnx", "Alyio.McpMssql", "--prerelease", "--yes"],
      "env": {
        "MCPMSSQL_CONNECTION_STRING": "Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
      }
    }
  }
}
```

**Config file locations:** Cursor `.cursor/mcp.json`, Codex/Copilot/OpenCode vary by client; see your client’s MCP docs.

## Integration tests

Tests use a real SQL Server and the `default` profile (`MCPMSSQL_CONNECTION_STRING` from environment variables or user-secrets). The suite expects a database named **`McpMssqlTest`**: the connection string must include `Initial Catalog=McpMssqlTest`. The test infrastructure creates, seeds, and drops this database. Set the secret for the test project:

```bash
dotnet user-secrets set "MCPMSSQL_CONNECTION_STRING" \
  "Server=localhost,1433;User ID=sa;Password=...;TrustServerCertificate=True;Encrypt=True;Initial Catalog=McpMssqlTest;" \
  --project test/Alyio.McpMssql.Tests
```

## Why this instead of Data API Builder?

Data API Builder (DAB) is a full REST/GraphQL API with CRUD and auth. This project is a small, read-only MCP server for agents: stdio, parameterized SELECT only, minimal surface. Choose this for agent workflows and low operational overhead; choose DAB for CRUD, REST/GraphQL, and rich policies.

## Contributing

Open issues or PRs; follow existing style and add tests where appropriate.

## License

MIT. See [LICENSE](LICENSE).
