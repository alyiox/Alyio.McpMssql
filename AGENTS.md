# Agent Instructions

Rules AI agents must follow when working in this repository.

---

## Commit messages

Use **Conventional Commits**.

### Header

* Format: `<type>(optional scope): summary`
* Use lowercase types (`feat`, `fix`, `ci`, `chore`, `docs`)
* Use scopes when relevant
* Write summaries in lowercase, imperative mood

### Body

* Leave a blank line after the header
* Explain **why**, not what
* Use imperative, present tense
* Wrap lines at ~72 characters

The body is optional for trivial changes.

---

## Commits (automation)

When generating commits via a shell:

* Do **not** pass generated messages directly to `git commit -m`
* Write the commit message to a file or standard input
* Use `git commit -F <file>` or `git commit -F -`
* Disable shell expansion when writing commit messages

This avoids issues with backticks, quotes, and other shell-expanded
characters in generated commit messages.

---

## Code style

Follow existing project conventions.

* Match formatting, naming, and file structure already in use
* Do not reformat unrelated code
* Prefer small, focused changes
* Avoid introducing new patterns without clear benefit

### Language-specific rules

* If a formatter or linter exists, follow it
* Respect `.editorconfig` when present
* Do not disable lint rules without justification
* Prefer explicit, readable code over clever abstractions

---

## MCP metadata (normative)

**Intro.** These requirements apply to all tool and resource metadata (names,
titles, descriptions, parameter descriptions, URI templates) exposed by this
server.
They assume hosts use a unified tool/resource pool and shared context. Goal:
unambiguous backend and scope so models select this server's tools correctly
and avoid cross-server leakage.

**Definitions.** *Backend*: Microsoft SQL Server or Azure SQL Database.
*Metadata*: tool/resource names, titles, descriptions, params, URI templates.
*Scope parameter*: narrows the target (e.g. profile, catalog, schema).
*Name parameter*: identifies a specific object (e.g. relation name, table name, routine name).
*This server*: Alyio.McpMssql.

**Requirements.**

1. **Backend identification.** Every tool and resource description MUST
   identify the backend in plain language using at least one of: **Microsoft
   SQL Server**, **Azure SQL Database**, **T-SQL**.

2. **Locality.** MUST NOT describe anything as global or cross-server. Profiles
   and scope parameters are scoped to this server only; state or imply where
   relevant.

3. **Scope and name parameters.** For each scope or name parameter, the
   description MUST state where valid values come from (this server's tools or
   `mssql://` resources) and that it is optional when optional. Name the
   specific tool(s) or resource(s) that provide those values (e.g. for
   profile: `list_profiles` or `mssql://context/profiles`).

4. **SQL parameters.** Parameters that accept or describe SQL MUST state the
   language is T-SQL. For read-only tools (e.g. SELECT only), MUST state
   read-only T-SQL.

5. **Resource URI scheme.** All resource URI templates MUST use `mssql://`.
   MUST NOT use another scheme.

6. **Cross-server isolation.** Metadata MUST make it unambiguous (a) which
   server a tool or resource belongs to, and (b) which arguments apply to this
   server—so the model does not pick another server's tool or pass this
   server's arguments (e.g. profile names) to another.

7. **Precedence.** In metadata, clarity and platform specificity over brevity.

