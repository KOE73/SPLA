# `.spla` Project File Format

The `.spla` file is the entry point for a project. Double-click it (or pass it as an argument) to launch SPLA with full project context — workspace, mode, instructions, and permissions.

## Example

```yaml
version: 1

name: My Project

workspace: .

agent:
  mode: Edit
  instructions:
    - AGENTS.md

llm:
  provider: lmstudio
  endpoint: http://localhost:1234/v1
  model: auto

ui:
  theme: Emerald

permissions:
  read: allow
  write: ask
  shell: ask
  internet: allow

docs:
  - docs/

ignore:
  - bin/
  - obj/
  - .git/
  - node_modules/
```

## Fields

| Field | Required | Description |
|-------|----------|-------------|
| `version` | Yes | Format version. Currently `1`. |
| `name` | No | Human-readable project name. |
| `workspace` | No | Working directory, relative to the `.spla` file. Default: `.` |
| `agent.mode` | No | Default mode: `Chat`, `Research`, `Inspect`, `Edit`, `Agent`. |
| `agent.instructions` | No | Markdown files injected into the system prompt. Paths relative to workspace. |
| `llm.provider` | No | LLM provider. Currently only `lmstudio`. |
| `llm.endpoint` | No | API base URL. |
| `llm.model` | No | Model name. `auto` = use whatever is loaded. |
| `connections` | No | Named connection list (merged over defaults by `id`); each entry: `id`, `name`, `provider`, `endpoint`, `api_key`, `model`, `context_length`, `lock_model`, `swap_model`. When absent, a default connection is synthesized from `llm.*`. |
| `connections[].context_length` | No | Manual context-window override in tokens. Unset/0 = auto-detect from the provider (LM Studio native API reports the loaded instance's configured window; vLLM reports `max_model_len`). |
| `ui.theme` | No | Color theme: `Dark`, `Light`, `Cream`, `Emerald`. |
| `ui.density` | No | UI density: `norm`, `mini`, `nano`, `max`. |
| `permissions.*` | No | Per-effect overrides: `allow`, `ask`, `deny`. Overrides the mode's default matrix. |
| `docs` | No | Documentation directories to index. |
| `ignore` | No | Directories/files the agent will never touch. |

## Settings Cascade

```
Hardcoded Defaults  →  ~/.spla/defaults.yaml  →  project.spla  →  Runtime UI
```

Each layer only overrides the keys it explicitly sets. Missing keys inherit from the previous layer.

## Global Defaults

Located at `~/.spla/defaults.yaml`. Same YAML format but without project-specific fields (`workspace`, `instructions`, `docs`, `ignore`).

```yaml
version: 1

llm:
  provider: lmstudio
  endpoint: http://127.0.0.1:1234/v1
  api_key: lm-studio
  model: auto
  temperature: 0.7

agent:
  mode: Edit

ui:
  theme: Dark
```

## Usage

### CLI
```bash
# Explicit
spla run my-project.spla

# Auto-detect (looks for *.spla in CWD)
spla
```

### GUI
Double-click `my-project.spla` → SPLA opens with full context.
