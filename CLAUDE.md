# Repository Agent Instructions

The binding instructions for this repo are in **[AGENTS.md](AGENTS.md)**.

## When to read it

- Before **commits or branches** (Conventional Commits format, `Assisted-by` trailer required, use `git commit -F` — not `git commit -m` for generated messages).
- Before **code changes** (match existing style; small diffs; respect linters and `.editorconfig`).
- Before **release tags** (bare version, no `v` prefix; annotated tags only).
- Before **MCP metadata** changes (description must start with `[MSSQL]`; use `Src:` tag for entity-referencing parameters).

If you have not consulted `AGENTS.md` in this conversation for the task at hand, **read it** and apply it.
