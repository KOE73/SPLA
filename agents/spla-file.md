# `.spla` Project File Format

The `.spla` file is the entry point for a project. Double-click it (or pass it as an argument) to launch SPLA with full project context — mode, instructions, and permissions.

**The project root is the directory this file sits in.** It is not configurable and there is no
field for it: a second definition of "where the agent works" would make every boundary drawn on the
first one negotiable. Manifests still carrying the old `workspace:` key load fine — it is ignored.

## Example

```yaml
version: 1

name: My Project

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

toolsets:
  ssh: agent_demand
  network: skill_demand
  roslyn: disabled

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
| `agent.mode` | No | Default mode: `Chat`, `Research`, `Inspect`, `Edit`, `Agent`. |
| `agent.instructions` | No | Markdown files injected into the system prompt. Paths relative to the project root. |
| `agent.capabilities` | No | Enabled built-in `core.*` capabilities. Missing = all; `[]` = pure chat with no built-in tools. |
| `llm.provider` | No | LLM provider. Currently only `lmstudio`. |
| `llm.endpoint` | No | API base URL. |
| `llm.model` | No | Model name. `auto` = use whatever is loaded. |
| `connections` | No | Named connection list (merged over defaults by `id`); each entry: `id`, `name`, `provider`, `endpoint`, `api_key`, `model`, `context_length`, `lock_model`, `swap_model`. When absent, a default connection is synthesized from `llm.*`. |
| `connections[].context_length` | No | Manual context-window override in tokens. Unset/0 = auto-detect from the provider (LM Studio native API reports the loaded instance's configured window; vLLM reports `max_model_len`). |
| `ui.theme` | No | Color theme: `Dark`, `Light`, `Cream`, `Emerald`. |
| `ui.density` | No | UI density: `norm`, `mini`, `nano`, `max`. |
| `permissions.*` | No | Per-effect overrides: `allow`, `ask`, `deny`. Overrides the mode's default matrix. |
| `toolsets.<id>` | No | How far a tool set may reach the model: `disabled`, `skill_demand`, `agent_demand`, `enabled`. Absent = derived from the supplier's `plugins.<id>.enabled` flag, so projects written before tool sets are unaffected. `on`/`off` are refused — YAML reads them as booleans. See [Tool Sets](toolsets.md). |
| `docs` | No | Documentation directories to index. |
| `ignore` | No | Directories/files the agent will never touch. |

## Settings Cascade

```
Hardcoded Defaults  →  ~/.spla/defaults.yaml  →  project.spla  →  Runtime UI
```

Each layer only overrides the keys it explicitly sets. Missing keys inherit from the previous layer.

## Global Defaults

Located at `~/.spla/defaults.yaml`. Same YAML format but without project-specific fields (`instructions`, `docs`, `ignore`).

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

## Personal state — the layer that is not a settings file

Some things belong to the person rather than to the project or the machine's configuration, and must
never be committed. They live as separate files in the **personal directory** — `~/.spla` locally,
`{server root}/users/{userKey}` on a server, so one server's users share none of it.

| File | Holds | Written by |
|---|---|---|
| `secrets.yaml` | credential values | secrets UI |
| `secrets.acl.yaml` | who may use/manage each secret | secrets UI |
| `skills.yaml` | skill branches this person added | Settings → Skills |
| `skills.acl.yaml` | folders this person approved, keyed by resolved path | Settings → Skills |

The pattern is the same in both pairs and worth keeping: **the list and the permission over it are two
files, never one.** A list is edited as data; an approval is a decision about safety, and one document
for both is a way to grant yourself something by editing the field next door.

These are not a settings layer in the cascade sense — they hold what the cascade cannot, namely a
decision that would be wrong to deliver to everyone who clones the repository. `skills.yaml` does
merge into the source list as the most specific layer, which is what lets the panel switch an
inherited branch off without touching the project file.

## Usage

### CLI
```bash
# Explicit
spla run my-project.spla

# Auto-detect (looks for *.spla in CWD)
spla

# Web service; create a chat and send its first message when the first client connects
spla serve --new-chat "Introduce this project"
```

### GUI
Double-click `my-project.spla` → SPLA opens with full context.
