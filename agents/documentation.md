# SPLA Documentation Layout

SPLA separates user-facing documentation from agent-facing instructions.

## Folders

- `agents/` contains agent-facing documentation: architecture notes, implementation rules, permission models, project conventions, and operational instructions for AI agents. Files in this folder are written in English.
- `docs/` contains user-facing documentation. User documentation files should be named `readme_*.md` for English documents and `readme_*_ru.md` for Russian documents.

## Translation Rule

When the user asks to translate documentation without explicitly naming a source file, target file, or folder, assume the request applies only to user-facing README-style files.

Default behavior:

- Look in `docs/` for matching `readme_*.md` and `readme_*_ru.md` files.
- If there is an obvious matching pair, translate between those two files.
- If there are several possible candidates, use file names and last modified dates to infer the intended source, but only when the choice is clear.
- If the target is still ambiguous, ask a concise clarification question before editing.
- Do not translate or rewrite `agents/` files unless the user explicitly asks for agent documentation.
