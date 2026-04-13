# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this project is

A read-only [Model Context Protocol (MCP)](https://modelcontextprotocol.io) server for Microsoft SQL Server. It exposes SQL Server metadata discovery, parameterized query execution, and execution plan analysis to AI agents — with strict no-DML/DDL enforcement. Distributed as a .NET global tool (`mcp-mssql`).

**Requirements:** .NET 10.0 SDK.

## Commands

```bash
# Build
dotnet build --configuration Release

# Run all tests (requires SQL Server — see Integration tests below)
dotnet test --configuration Release --verbosity normal -- --coverage --coverage-output-format cobertura

# Run a single test by name
dotnet test --configuration Release -k "PlanStoreTests.Save_Returns_NonEmpty_Id" --verbosity normal

# Run tests matching a class
dotnet test --configuration Release --filter "FullyQualifiedName~PlanStoreTests" --verbosity normal

# Format check (CI enforces this)
dotnet format --verify-no-changes --verbosity normal

# Run from source with MCP Inspector
export MCPMSSQL_CONNECTION_STRING="Server=127.0.0.1;User ID=sa;Password=...;Encrypt=True;TrustServerCertificate=True;"
npx -y @modelcontextprotocol/inspector dotnet run --project src/Alyio.McpMssql

# Pack NuGet
dotnet pack --configuration Release --output ./artifacts
```

## Integration tests

Tests run against a real SQL Server and expect a database named **`McpMssqlTest`**. The connection string must include `Initial Catalog=McpMssqlTest`. The test infrastructure creates, seeds, and drops this database. Set the connection string via user-secrets:

```bash
dotnet user-secrets set "MCPMSSQL_CONNECTION_STRING" \
  "Server=localhost,1433;User ID=sa;Password=...;TrustServerCertificate=True;Encrypt=True;Initial Catalog=McpMssqlTest;" \
  --project test/Alyio.McpMssql.Tests
```

CI uses a SQL Server 2025 container (see `.github/workflows/ci.yml`).

## Architecture

The server is split across five layers inside `src/Alyio.McpMssql/`:

**`Features/`** — MCP tool and resource handlers. Each file is a thin, stateless class that binds MCP protocol calls to service calls. Tools are in `*Tools.cs`; resources (paginated/large results) are in `*Resources.cs`.

**`Services/`** — Business logic. Key services:
- `QueryService` — executes parameterized SELECT statements and produces execution plans
- `CatalogService` — introspects server metadata (databases, schemas, tables, columns, routines)
- `ProfileService` — resolves named connection profiles
- `ExecutionContextService` — enforces per-profile row limits and timeouts
- `PlanStore` — disk-backed execution plan cache (`~/.cache/mcpmssql/plans`), 7-day TTL, lazy-loaded into an in-memory dictionary
- `SnapshotStore` / `ContentStore` — query result snapshot caching

**`Internal/`** — Core utilities not exposed via DI: `SqlReadOnlyValidator` (T-SQL parse-time DML/DDL rejection), `PlanParser` (XML plan → compact JSON summary), `McpExecutor` (error wrapping), `CsvSerializer`.

**`Configuration/`** — `McpMssqlOptions` / `McpMssqlProfileOptions` typed options; all settings use the `MCPMSSQL` prefix.

**`DependencyInjection/ServiceCollectionExtensions.cs`** — single registration entry point wired from `Program.cs`.

**`test/Alyio.McpMssql.Tests/`** — three layers:
- `Unit/` — no infrastructure required
- `Functional/` — service-level tests, require SQL Server
- `E2E/` — full tool/resource tests against a running MCP server, require SQL Server

Test infrastructure lives in `Infrastructure/Fixtures/` (`SqlServerFixture`, `McpServerFixture`, `SqlServerCollection`).

## Conventions (from AGENTS.md)

### Commit messages
Use **Conventional Commits**: `<type>(scope): summary` — lowercase type and summary, imperative mood. Body explains *why*, not *what*, wrapped at ~72 chars.

When writing commits via shell: write the message to a file and use `git commit -F <file>` — never `git commit -m` with a generated string (shell expansion risk).

### Release tags
No `v` prefix — use the bare version (e.g. `1.0.0-beta.3`). Tags must be annotated (`git tag -a`).

### C# style
- Use trailing commas in multi-line collections, object initializers, and enums.
- Respect `.editorconfig`; do not reformat unrelated code.

### MCP metadata
- `[Description]` attributes on tools and parameters **must** start with `[MSSQL]` followed by a Verb-Object fragment (e.g. `[MSSQL] Execute Read-only T-SQL`).
- Parameters that refer to server/database entities **must** include a `Src:` tag (e.g. `Src: profiles`, `Src: catalogs`).
