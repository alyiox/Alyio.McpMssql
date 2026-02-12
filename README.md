# MCP SQL Server Tool

[![Build Status](https://github.com/alyiox/Alyio.McpMssql/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/alyiox/Alyio.McpMssql/actions/workflows/ci.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Alyio.McpMssql.svg)](https://www.nuget.org/packages/Alyio.McpMssql)

A read-only MCP server for Microsoft SQL Server: metadata discovery and parameterized `SELECT` over stdio. Profile-based config, no DML/DDL; uses the official Model Context Protocol C# SDK.

**Requirements:** .NET 10.0 SDK, SQL Server, and a connection string.

## Quick start

Set `MCPMSSQL_CONNECTION_STRING` and run the server (direct or as a global tool).

```bash
# Run directly (e.g. with MCP Inspector)
npx -y @modelcontextprotocol/inspector \
  -e MCPMSSQL_CONNECTION_STRING="Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;" \
  dotnet dnx Alyio.McpMssql --prerelease
```

```bash
# Or install and run as a tool
dotnet tool install --global Alyio.McpMssql --prerelease
npx -y @modelcontextprotocol/inspector -e MCPMSSQL_CONNECTION_STRING="..." mcp-mssql
```

Use `--prerelease` for pre-release builds. The tool entrypoint is `dotnet dnx Alyio.McpMssql`; the installed command is `mcp-mssql`.

## Configuration

All settings use the **MCPMSSQL** prefix: flat environment variables (e.g., `MCPMSSQL_CONNECTION_STRING`) for a single connection, or the environment variable (e.g., `MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING`) for profile-based config.

**Single server / one connection:** Set the following as environment variables.

```bash
# Connection string (required). 
export MCPMSSQL_CONNECTION_STRING="Server=...;User ID=...;Password=...;"

# Optional description for the default profile (tooling/AI discovery). 
export MCPMSSQL_DESCRIPTION="Primary connection"

# Optional default row limit (default `100`). 
export MCPMSSQL_SELECT_DEFAULT_MAX_ROWS="200"

# Optional max rows per SELECT (default `5000`).
export MCPMSSQL_SELECT_MAX_ROWS="5000"

# Optional query timeout in seconds (default `30`). 
export MCPMSSQL_SELECT_COMMAND_TIMEOUT_SECONDS="60"
```

**Multiple servers or connections:** Use environment variables or the same structure in `appsettings.json`.

- Use environment variables with the **MCPMSSQL** prefix, then `__` for each level (e.g. `MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING`).
- For each profile, set `MCPMSSQL__PROFILES__<name>__CONNECTIONSTRING`, and optionally add `__DESCRIPTION` and `__SELECT`.
- The default profile is the one named `default`. Define it with `MCPMSSQL__PROFILES__DEFAULT__...` or use the single-connection variables (`MCPMSSQL_CONNECTION_STRING`, etc.) to create or override the default profile when set.

Example (environment variables):

```bash
# Default profile
export MCPMSSQL__PROFILES__DEFAULT__CONNECTIONSTRING="Server=...;User ID=...;Password=...;"
export MCPMSSQL__PROFILES__DEFAULT__DESCRIPTION="Primary connection"
export MCPMSSQL__PROFILES__DEFAULT__SELECT__DEFAULTMAXROWS="200"
export MCPMSSQL__PROFILES__DEFAULT__SELECT__MAXROWS="5000"
export MCPMSSQL__PROFILES__DEFAULT__SELECT__COMMANDTIMEOUTSECONDS="60"

# Named profile
export MCPMSSQL__PROFILES__WAREHOUSE__CONNECTIONSTRING="Server=warehouse.example.com;..."
export MCPMSSQL__PROFILES__WAREHOUSE__SELECT__MAXROWS="10000"
export MCPMSSQL__PROFILES__WAREHOUSE__SELECT__COMMANDTIMEOUTSECONDS="120"

# Single-connection (flat) keys create or override the default profile:
export MCPMSSQL_CONNECTION_STRING="Server=...;User ID=...;Password=...;"
export MCPMSSQL_DESCRIPTION="Primary connection"
export MCPMSSQL_SELECT_DEFAULT_MAX_ROWS="200"
export MCPMSSQL_SELECT_MAX_ROWS="5000"
export MCPMSSQL_SELECT_COMMAND_TIMEOUT_SECONDS="60"
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

## Tools and resources

All tools accept an optional `profile`; when omitted, the default profile is used. Discover profiles via the `list_profiles` tool or the **Profile context** resource `mssql://context/profiles`.

- **Profile context:** Tool `list_profiles`; resource `mssql://context/profiles` — configured profiles.
- **Query:** `select` — parameterized `SELECT`; optional `profile`, `catalog`, `parameters`, `maxRows`.
- **Catalog:** `list_catalogs`, `list_schemas`, `list_relations`, `list_routines`, `describe_columns`, `describe_indexes`, `describe_constraints`, `get_routine_definition` — optional `profile` and other args.
- **Context:** Tool `get_execution_context` (optional `profile`). Resources (use `{profile}` in path, e.g. `default`):
  - `mssql://{profile}/context/server/properties` — server properties, such as product version, and edition
  - `mssql://{profile}/context/execution` — execution context, such as row limits and command timeout
  - `mssql://{profile}/catalogs` — list databases
  - `mssql://{profile}/catalogs/{catalog}/schemas` — list schemas
  - `mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations` — list relations
  - `mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/columns` — describe columns
  - `mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/indexes` — describe indexes
  - `mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}/constraints` — describe constraints (PK, UQ, FK, CHECK, DEFAULT; tables only)
  - `mssql://{profile}/catalogs/{catalog}/schemas/{schema}/routines` — list routines
  - `mssql://{profile}/catalogs/{catalog}/schemas/{schema}/routines/{name}/definition` — routine T-SQL body

## Security

Read-only (`SELECT` only); parameterized `@paramName`. Use environment variables or user-secrets for connection strings—never commit secrets.

## MCP host examples

Snippets for common MCP clients. Copy one and replace the connection string; ensure `dotnet` is on your PATH.

### Cursor / Gemini / Codex

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

### OpenCode

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

### GitHub Copilot (agent)

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

Config file location depends on the client (e.g. Cursor: `.cursor/mcp.json`).

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
