# MCP SQL Server Tool

Read-only Model Context Protocol (MCP) server for Microsoft SQL Server. Exposes safe metadata discovery and parameterized `SELECT` queries over stdio.

## Features

- Read-only SQL validation (blocks DML/DDL and multi-statement batches)
- Parameterized queries with `@name` syntax
- Metadata discovery: databases, schemas, tables, views, functions, procedures
- Built with official `ModelContextProtocol` C# SDK

## Requirements

- .NET 10.0 SDK/runtime
- SQL Server connection string

## Quick Start

The `@modelcontextprotocol/inspector` is a CLI tool facilitating the inspection, interaction, and streamlined testing of Model Context Protocol servers.

Note: The `--prerelease` flag is used to include pre-release versions of the tool.

Run directly using `dotnet dnx` (no installation required):

```bash
npx -y @modelcontextprotocol/inspector \
  -e MCP_MSSQL_CONNECTION_STRING="Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;" \
  dotnet dnx Alyio.McpMssql --prerelease
```

### Alternative: Global Install

```bash
dotnet pack -c Release -o ./nupkg
dotnet tool install --global Alyio.McpMssql --add-source ./nupkg
npx -y @modelcontextprotocol/inspector \
  -e MCP_MSSQL_CONNECTION_STRING="Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;" \
  mcp-mssql
```

## Configuration

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `MCP_MSSQL_CONNECTION_STRING` | (required) | SQL Server connection string |
| `MCP_MSSQL_DEFAULT_MAX_ROWS` | 200 | Default row limit for queries |
| `MCP_MSSQL_HARD_MAX_ROWS` | 5000 | Maximum row limit (enforced) |
| `MCP_MSSQL_COMMAND_TIMEOUT_SECONDS` | 30 | Query timeout (enforced) |

### Development Configuration

For local development, the application's configuration, especially the `MCP_MSSQL_CONNECTION_STRING`, can be provided using either environment variables or .NET user secrets.

To set the SQL Server connection string using user secrets for local development:

```bash
dotnet user-secrets set "MCP_MSSQL_CONNECTION_STRING" "Server=...;Database=..." --project src/Alyio.McpMssql
```

To run the application with the Model Context Protocol Inspector (MCP Inspector):

```bash
npx -y @modelcontextprotocol/inspector dotnet run --project src/Alyio.McpMssql
```

### Testing Configuration

For integration tests, the SQL Server connection string (`MCP_MSSQL_CONNECTION_STRING`) can be configured using either environment variables or .NET user secrets. For example, to set it via user secrets:

```bash
dotnet user-secrets set "MCP_MSSQL_CONNECTION_STRING" "Server=...;Database=..." --project test/Alyio.McpMssql.Tests
```

## MCP Client Configuration

Example `mcp.json`:

```json
{
  "mcpServers": {
    "mcp-mssql": {
      "command": "dotnet",
      "args": ["dnx", "Alyio.McpMssql", "--prerelease"],
      "env": {
        "MCP_MSSQL_CONNECTION_STRING": "Server=127.0.0.1;User ID=sa;Password=<YourStrong@Passw0rd>;Encrypt=True;TrustServerCertificate=True;"
      }
    }
  }
}
```

## Available Tools

### `select`
Execute parameterized `SELECT` statements.

```json
{
  "tool": "select",
  "sql": "SELECT * FROM Users WHERE Id = @id",
  "parametersJson": "{\"id\": 42}",
  "maxRows": 100
}
```

### Metadata Tools

- `ping` - Test connectivity
- `list_databases` - List all databases
- `list_schemas` - List schemas in a database
- `list_tables` - List tables in a schema
- `list_views` - List views in a schema
- `list_functions` - List user-defined functions
- `list_procedures` - List stored procedures
- `describe_table` - Show table/view columns with types

## Security

- **Read-only**: Only `SELECT` statements allowed
- **Parameterized**: Use `@name` parameters to prevent SQL injection
- **No secrets in code**: Use environment variables or user secrets

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
