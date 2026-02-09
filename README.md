# MCP SQL Server Tool

[![Build Status](https://github.com/alyiox/Alyio.McpMssql/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/alyiox/Alyio.McpMssql/actions/workflows/ci.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Alyio.McpMssql.svg)](https://www.nuget.org/packages/Alyio.McpMssql)

A small, focused Read-only Model Context Protocol (MCP) server for Microsoft SQL Server. It exposes safe metadata discovery and parameterized `SELECT` queries over stdio so it can be used by MCP-compatible agents and tools.

Key highlights:
- Strictly read-only: only `SELECT` queries are permitted and the server blocks DML/DDL and multi-statement batches.
- Parameterized queries using `@paramName` to minimize risk of SQL injection.
- Dedicated catalog and server-context services for safe metadata discovery (databases, schemas, relations, routines, server version, user, etc.).
- Implemented using the official `ModelContextProtocol` C# SDK.

## Requirements

- .NET 10.0 SDK/runtime
- A SQL Server instance and a connection string with credentials

## Quick start

There are two common ways to run the server locally: directly (no install) or as a global dotnet tool.

**Run directly** (recommended for development/testing)

```bash
npx -y @modelcontextprotocol/inspector \
  -e MCP_MSSQL_CONNECTION_STRING="Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;" \
  dotnet dnx Alyio.McpMssql --prerelease
```

**Install as a global tool** (convenient for repeated use)

```bash
# install (use --prerelease if you want pre-release builds)
dotnet tool install --global Alyio.McpMssql --prerelease

# run with the MCP Inspector
npx -y @modelcontextprotocol/inspector \
  -e MCP_MSSQL_CONNECTION_STRING="Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;" \
  mcp-mssql
```

Notes:
- The `@modelcontextprotocol/inspector` CLI (MCP Inspector) is a separate tool used to interact with MCP servers. The `--prerelease` flag is used in examples to allow pre-release server builds.
- `dotnet dnx` is the command entrypoint used by the tool package in this repository; installed tool exposes the `mcp-mssql` command shown above.

## Configuration

The application binds configuration from the `McpMssql` section of the .NET configuration system (appsettings, environment, user-secrets, etc.). In addition, a set of flat environment variables with the `MCP_MSSQL_` prefix can be used to override specific values.

Provide configuration via environment variables or .NET user secrets in development.

**Environment variables**

| Variable | Default | Description |
|----------|---------|-------------|
| `MCP_MSSQL_CONNECTION_STRING` | (required) | SQL Server connection string (e.g. `Server=...;User ID=...;Password=...;`) |
| `MCP_MSSQL_SELECT_DEFAULT_MAX_ROWS` | `100` | Default row limit returned when no explicit limit is provided (see `SelectExecutionOptions.DefaultMaxRows`). |
| `MCP_MSSQL_SELECT_ROW_LIMIT` | `5000` | Maximum number of rows that may be returned for a single SELECT (clamped by the server hard limit). |
| `MCP_MSSQL_SELECT_COMMAND_TIMEOUT_SECONDS` | `30` | Query timeout in seconds (clamped by server hard limit). |

Note: configuration is bound from the `McpMssql` section (for example in `appsettings.json`) and then overridden by the flat `MCP_MSSQL_*` environment variables listed above.

**Local development with user-secrets**

```bash
dotnet user-secrets set "MCP_MSSQL_CONNECTION_STRING" "Server=...;Database=...;User ID=...;Password=...;" --project src/Alyio.McpMssql
```

**Run in development mode with the inspector**

```bash
npx -y @modelcontextprotocol/inspector -e DOTNET_ENVIRONMENT=Development dotnet run --project src/Alyio.McpMssql
```

## Integration testing

Integration tests also use `MCP_MSSQL_CONNECTION_STRING`. Set it using environment variables or user secrets for the test project:

```bash
dotnet user-secrets set "MCP_MSSQL_CONNECTION_STRING" "Server=...;Database=...;User ID=...;Password=...;" --project test/Alyio.McpMssql.Tests
```

## MCP agents configuration examples

Below are common example snippets used by various agents to start this MCP server. Replace the example connection string with your own.

**OpenCode AI example**

```json
{
  "$schema": "https://opencode.ai/config.json",
  "mcp": {
    "mssql": {
      "type": "local",
      "enabled": true,
      "command": ["dotnet", "dnx", "Alyio.McpMssql", "--prerelease", "--yes"],
      "environment": {
        "MCP_MSSQL_CONNECTION_STRING": "Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
      }
    }
  }
}
```

**Cursor Agent / Gemini CLI example**

```json
{
  "mcpServers": {
    "mssql": {
      "command": "dotnet",
      "args": ["dnx", "Alyio.McpMssql", "--prerelease", "--yes"],
      "env": {
        "MCP_MSSQL_CONNECTION_STRING": "Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
      }
    }
  }
}
```

**GitHub Copilot (agent) example**

```json
{
  "inputs": [],
  "servers": {
    "mssql-us": {
      "type": "stdio",
      "command": "dotnet",
      "args": [ "dnx", "Alyio.McpMssql", "--prerelease", "--yes"],
      "env": {
        "MCP_MSSQL_CONNECTION_STRING": "Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
      }
    }
  }
}
```

## Available tools and endpoints

**Core query tool**
- `select`: Executes read-only parameterized `SELECT` statements and returns tabular results.

Example request

```json
{
  "tool": "select",
  "sql": "SELECT * FROM Users WHERE Id = @id",
  "parameters": { "id": 42 },
  "maxRows": 100
}
```

**Catalog discovery tools**
- `list_catalogs`: Lists accessible databases
- `list_schemas`: Lists schemas within a specified database
- `list_relations`: Lists tables and views within a specified schema
- `list_routines`: Lists stored procedures and functions within a specified schema
- `describe_relation`: Describes the columns of a specified table or view

**Server context endpoints (resources)**
- `mssql://context/connection` - current server connection context (server identity, user, database, version)
- `mssql://context/execution` - current execution context (defaults, limits, timeouts enforced by the server)
- `mssql://catalogs` - list databases
- `mssql://catalogs/{catalog}/schemas` - list schemas for a database
- `mssql://catalogs/{catalog}/schemas/{schema}/relations` - list relations (tables/views)
- `mssql://catalogs/{catalog}/schemas/{schema}/relations/{name}` - describe a relation
- `mssql://catalogs/{catalog}/schemas/{schema}/routines` - list routines (procs/functions)

## Security

- Read-only: only `SELECT` statements are allowed.
- Parameterized queries: use `@name` parameters to reduce injection risk.
- Never commit secrets into code; use environment variables or user secrets.

## Contributing

Contributions are welcome. Please open issues for bugs or feature requests and submit pull requests for fixes. Follow the existing coding style and add tests where appropriate.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
