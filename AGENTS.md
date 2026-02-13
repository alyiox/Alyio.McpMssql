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

Normative, high-density metadata: enough for correct tool and parameter selection, minimal to reduce token cost.

- **The `DescriptionAttribute` statement MUST be a Verb-Object fragment that naturally includes the target platform (SQL Server or Azure SQL Database), e.g., `Execute read-only T-SQL SELECT against SQL Server or Azure SQL Database.`**
- **Use tag-based lineage (Src: <Entity>) for parameters that refer to server or database entities** (e.g. profile name → Src: profiles, catalog → Src: catalogs).

