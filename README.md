# MCP SQL Server Tool

[![Build Status](https://github.com/alyiox/Alyio.McpMssql/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/alyiox/Alyio.McpMssql/actions/workflows/ci.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Alyio.McpMssql.svg)](https://www.nuget.org/packages/Alyio.McpMssql)

A small, focused Read-only Model Context Protocol (MCP) server for Microsoft SQL Server. It exposes safe metadata discovery and parameterized `SELECT` queries over stdio so it can be used by MCP-compatible agents and tools.

Key highlights:
- **Profile-based configuration**: Multiple named connection profiles (e.g. `default`, `warehouse`); tools and resources accept an optional profile so hosts and agents can target a specific connection.
- Strictly read-only: only `SELECT` queries are permitted and the server blocks DML/DDL and multi-statement batches.
- Parameterized queries using `@paramName` to minimize risk of SQL injection.
- Dedicated catalog and server-context services for safe metadata discovery (databases, schemas, relations, routines, server version, execution context, and available profiles).
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
- The `@modelcontextprotocol/inspector` CLI (MCP Inspector) is a separate tool used to interact with MCP servers.
- The `--prerelease` flag is used in examples to allow pre-release server builds.
- `dotnet dnx` is the command entrypoint used by the tool package in this repository; installed tool exposes the `mcp-mssql` command shown above.

## Why not Data API Builder?

Microsoft's Data API Builder (DAB) provides a full-featured REST/GraphQL layer over SQL Server with built-in authorization, CRUD, and policy configuration. That makes DAB an excellent choice for applications needing:

- Full CRUD (create/update/delete) and complex authorization rules
- REST or GraphQL endpoints with declarative mapping and policies
- A production-grade, configurable API gateway for multiple data sources

This project intentionally targets a different niche:

- Lightweight, audit-friendly MCP server focused on read-only access for agent workflows
- Small dependency surface and predictable behavior (stdio transport, parameterized SELECT only)
- Easier to run locally as a dotnet tool and simpler to reason about security for agent use

When to choose this project vs Data API Builder

- Choose this project when you need a tiny, read-only MCP server for agents, fast startup, and low operational complexity.
- Choose Data API Builder when you need CRUD operations, rich authorization, REST/GraphQL endpoints, or advanced policy configuration.

## Configuration

The server uses **profile-based configuration**: each named profile has a connection string and optional execution options. When a client calls a tool without a `profile` argument or reads a resource, the server uses the **default profile**—the profile whose name is set in `DefaultProfile` (if not set, the profile named `"default"` is used).

### Single connection (simplest)

For one SQL Server connection, set the **MCP-specific environment variable** and nothing else. The server creates a single profile (named `default`) from it. No config file required.

| Variable | Description |
|----------|-------------|
| `MCP_MSSQL_CONNECTION_STRING` | SQL Server connection string (required). |
| `MCP_MSSQL_SELECT_DEFAULT_MAX_ROWS` | Default row limit when no limit is provided (default `100`). |
| `MCP_MSSQL_SELECT_MAX_ROWS` | Max rows per SELECT (default `5000`). |
| `MCP_MSSQL_SELECT_COMMAND_TIMEOUT_SECONDS` | Query timeout in seconds (default `30`). |

Example: pass this env when starting the server (e.g. in MCP host config or inspector).

```bash
export MCP_MSSQL_CONNECTION_STRING="Server=127.0.0.1;User ID=sa;Password=...;Encrypt=True;TrustServerCertificate=True;"
```

**Local development with user-secrets**

```bash
dotnet user-secrets set "MCP_MSSQL_CONNECTION_STRING" "Server=...;Database=...;User ID=...;Password=...;" --project src/Alyio.McpMssql
```

### Multiple profiles

For more than one connection (e.g. default + warehouse), define each profile. **Prefer environment-based configuration**: this server runs as a local stdio process, and MCP hosts/agents typically pass env when they start the process, so env vars are the natural place for profile data. Config files (appsettings, user-secrets) work too.

**Environment variables** (recommended for multiple profiles)

Use the .NET convention: double underscore `__` for each level. The `Profiles` section is keyed by profile name.

```bash
# Which profile is used when the client doesn't specify one (default: "default")
export McpMssql__DefaultProfile=default

# Profile "default"
export McpMssql__Profiles__default__ConnectionString="Server=...;User ID=...;Password=...;"
export McpMssql__Profiles__default__Description="Primary connection"
export McpMssql__Profiles__default__Select__DefaultMaxRows=100
export McpMssql__Profiles__default__Select__MaxRows=5000

# Profile "warehouse"
export McpMssql__Profiles__warehouse__ConnectionString="Server=warehouse.example.com;..."
export McpMssql__Profiles__warehouse__Description="Read-only warehouse"
```

If you also set `MCP_MSSQL_CONNECTION_STRING`, it overrides the **default profile’s** connection string (so you can still drive the default profile from that one key if you like).

**Config file** (e.g. appsettings.json)

Same structure under the `McpMssql` section; use colon `:` in keys for user-secrets or command-line.

```json
{
  "McpMssql": {
    "DefaultProfile": "default",
    "Profiles": {
      "default": {
        "ConnectionString": "Server=...;User ID=...;Password=...;",
        "Description": "Primary connection",
        "Select": { "DefaultMaxRows": 100, "MaxRows": 5000, "CommandTimeoutSeconds": 30 }
      },
      "warehouse": {
        "ConnectionString": "Server=...;",
        "Description": "Read-only warehouse"
      }
    }
  }
}
```

**Run in development mode with the inspector**

```bash
npx -y @modelcontextprotocol/inspector -e DOTNET_ENVIRONMENT=Development dotnet run --project src/Alyio.McpMssql
```

## Integration testing

Integration tests use a real SQL Server instance and the **default profile**: the connection string is read from `MCP_MSSQL_CONNECTION_STRING` (user secrets or environment).

### IMPORTANT: TEST DATABASE REQUIREMENTS

The integration test suite **requires a **hardcoded** and not configurable database named `McpMssqlTest`**.

- The connection string **must include** `Initial Catalog=McpMssqlTest`, as the **active catalog** at connection time
- The test infrastructure will **create, seed, and drop** `McpMssqlTest` automatically during test execution

If the database does not exist or the catalog is omitted, tests will fail with errors similar to: `Cannot open database "McpMssqlTest" requested by the login.`.

### Configuration

Set the connection string using environment variables or user secrets for the test project.

User secrets example:

```bash
dotnet user-secrets set "MCP_MSSQL_CONNECTION_STRING" \
  "Server=localhost,1433;User ID=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;Encrypt=True;Initial Catalog=McpMssqlTest;" \
  --project test/Alyio.McpMssql.Tests
```

Environment variable example:

```bash
export MCP_MSSQL_CONNECTION_STRING="Server=localhost,1433;User ID=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;Encrypt=True;Initial Catalog=McpMssqlTest;"
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

## Available tools and resources

All tools accept an optional **`profile`** argument; when omitted, the default profile is used. Use the `list_profiles` tool or `mssql://profiles` resource to discover valid profile names.

**Profile discovery**
- **Tool** `list_profiles`: Returns configured profile names and the default profile name.
- **Resource** `mssql://profiles`: Same as the tool (server-level metadata; no profile in the URI).

**Core query tool**
- `select`: Executes read-only parameterized `SELECT` statements and returns tabular results. Optional args: `profile`, `catalog`, `parameters`, `maxRows`.

**Catalog discovery tools**
- `list_catalogs`, `list_schemas`, `list_relations`, `list_routines`, `describe_relation`: Metadata for databases, schemas, tables/views, routines, and column descriptions. Each accepts an optional `profile` (and other args as relevant).

**Execution and server context**
- **Tool** `get_execution_context`: Returns execution context (row limits, timeouts) for the given profile. Optional `profile`.
- **Resources** (all use profile as the first path segment; use `default` for the default profile):
  - `mssql://{profile}/context/server/properties` – SQL Server instance properties (engine edition, version, etc.)
  - `mssql://{profile}/context/execution` – execution context (defaults, limits, timeouts)
  - `mssql://{profile}/catalogs` – list databases
  - `mssql://{profile}/catalogs/{catalog}/schemas` – list schemas
  - `mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations` – list relations (tables/views)
  - `mssql://{profile}/catalogs/{catalog}/schemas/{schema}/relations/{name}` – describe a relation
  - `mssql://{profile}/catalogs/{catalog}/schemas/{schema}/routines` – list routines (procs/functions)

Example: to read server properties for the default profile, use the resource URI `mssql://default/context/server/properties`.

## Security

- Read-only: only `SELECT` statements are allowed.
- Parameterized queries: use `@name` parameters to reduce injection risk.
- Never commit secrets into code; use environment variables or user secrets.

## Contributing

Contributions are welcome. Please open issues for bugs or feature requests and submit pull requests for fixes. Follow the existing coding style and add tests where appropriate.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

