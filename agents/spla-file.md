# `.spla` Project File Format

The `.spla` file is the entry point for a project. Double-click it (or pass it as an argument) to launch SPLA with full project context — mode, instructions, and permissions.

**The project root is the directory this file sits in.** It is not configurable and there is no
field for it: a second definition of "where the agent works" would make every boundary drawn on the
first one negotiable. Manifests still carrying the old `workspace:` key load fine — it is ignored.

## Example

```yaml
version: 1

name: My Project

mounts:
  - name: AAA
    type: file-system
    path: ../AAA          # relative to THIS file's directory
    access: read          # read (default) | write
    trust: trusted        # trusted (default) | untrusted
    description: reference Linux settings — the canonical copy, do not edit

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
| `mounts` | No | Folders outside the project root, named here and addressed as `mnt/<name>/...`. See [Mounts](#mounts). |
| `agent.mode` | No | Default mode: `Chat`, `Research`, `Inspect`, `Edit`, `Agent`. |
| `agent.instructions` | No | Markdown files injected into the system prompt. Paths relative to the project root. |
| `agent.capabilities` | No | Enabled built-in `core.*` capabilities. Missing = all; `[]` = pure chat with no built-in tools. |
| `llm.provider` | No | LLM provider. Currently only `lmstudio`. |
| `llm.endpoint` | No | API base URL. |
| `llm.model` | No | Model name. `auto` = use whatever is loaded. |
| `connections` | No | Named connection list (merged over defaults by `id`); each entry: `id`, `name`, `provider`, `endpoint`, `api_key`, `model`, `context_length`, `lock_model`, `swap_model`. When absent, a default connection is synthesized from `llm.*`. |
| `connections[].context_length` | No | Manual context-window override in tokens. Unset/0 = auto-detect from the provider (LM Studio native API reports the loaded instance's configured window; vLLM reports `max_model_len`). |
| `connections[].models[].reasoning_options` | No | Manual declaration of the model's reasoning options, in the provider's own words (`[off, low, medium, xhigh, on]`). Same precedence as `context_length`: a declaration wins over whatever the provider advertises, and it is the only way to get the lever for a server that describes nothing — most OpenAI-compatible endpoints, LocalAI and plain vLLM among them. Unset = take the provider's word, or leave the lever unavailable. See [ADR_20260817](../docs/adr/ADR_20260817_llm_reasoning-lever.md). |
| `connections[].models[].reasoning_default` | No | The option the model uses when asked for nothing. Read only alongside `reasoning_options`. |
| `llm.temperature` | No | Sampling temperature (default `0.7`). Layered defaults → project → chat; a chat's own value is written into its YAML by the status bar. |
| `llm.reasoning_level` | No | Reasoning selection: empty (model's own default), `off`, `on`, an effort word in the provider's vocabulary (`low`/`medium`/`xhigh`/…), or `budget:N` tokens. Nothing is sent to a provider that never described the model's reasoning channel — see the ADR above for why that matters. |
| `ui.theme` | No | Color theme: `Dark`, `Light`, `Cream`, `Emerald`. |
| `ui.density` | No | UI density: `norm`, `mini`, `nano`, `max`. |
| `permissions.*` | No | Per-effect overrides: `allow`, `ask`, `deny`. Overrides the mode's default matrix. |
| `toolsets.<id>` | No | How far a tool set may reach the model: `disabled`, `skill_demand`, `agent_demand`, `enabled`. Absent = derived from the supplier's `plugins.<id>.enabled` flag, so projects written before tool sets are unaffected. `on`/`off` are refused — YAML reads them as booleans. See [Tool Sets](toolsets.md). |
| `plugins.<id>.enabled` | No | Enables/disables one plugin by id. An entry naming the plugin always wins. |
| `plugins."*".enabled` | No | Enables/disables every plugin that has no entry of its own. See [The `*` plugin entry](#the--plugin-entry). |
| `docs` | No | Documentation directories to index. |
| `ignore` | No | Directories/files the agent will never touch. |

## Mounts

A project sometimes needs a folder that is not inside it — a reference tree shared by several
experiments, a deployment directory, a drop box. Neither ordinary way of naming one works: a relative
path in an instruction has no base (relative to the process's directory? the project root? the file
the instruction is written in?), and an absolute path breaks on the second machine and cannot be
expressed on a server at all.

A mount gives such a folder a name. **The name travels in git, the target is a property of the
machine** — which is the only split under which the address is both stable in an instruction and
portable between checkouts.

```yaml
mounts:
  - name: AAA
    type: file-system
    path: ../AAA
    access: read
    trust: trusted
    description: reference Linux settings — the canonical copy, do not edit
```

Everything then addresses it as `mnt/AAA/nginx/nginx.conf`, and the file tools take that exactly as
they take a project path. So does `sftp_upload`, which is the case mounts were introduced for.

| Key | Required | Meaning |
|-----|----------|---------|
| `name` | Yes | The address segment. One plain segment — no slashes, no `..`. |
| `type` | No | Only `file-system` exists. The key is here so a second kind would be an addition rather than a break. |
| `path` | Yes | Where it points. Relative paths are resolved against **the directory holding this manifest**, never the current directory. |
| `access` | No | `read` (default) or `write`. Read-only unless opted in, the same as `allow_write` on an SSH host. |
| `trust` | No | `trusted` (default) or `untrusted`. See below. |
| `description` | Yes | What the folder is for. Goes into the system prompt. |

**`description` is required on purpose.** It is what the model reads beside the address; without a
line saying what the folder is, it opens the folder to find out.

**`trust: untrusted`** is for one situation: a folder other people put files into — a shared drop,
somebody else's export, a downloads directory. Reading from one raises the chat's doubt flag, which
costs a re-asked question when something later goes outward and nothing at all otherwise. Everything
you set up yourself is trusted by default, because a mount is a source you named, exactly like an SSH
host.

### `mnt` is reserved in the project root

A real folder called `mnt` in the root stops the project from opening. This is checked whether or not
anything is mounted, and that is the point: it is one condition for the life of the project, so
adding a mount never re-opens the question of what is already in the tree. The alternative — checking
each mount's name against the tree at load — would fail at a moment nobody chose, the day a `git
pull` lands a folder with a colliding name.

### What a mount does not do

- **It does not work in the shell.** `system_run_shell`, `roslyn_script_run`, git and compilers take
  a path from the model and never pass the file tools; the OS knows nothing of `mnt/`. The design
  ([ADR §7](../docs/adr/ADR_20260814_core_project-mounts.md)) points at `MapPathToHost` for this, but
  **no tool exposes it to the model today** — so for now a `mnt/` address is for the tools only, and
  the prompt says so. Non-file-system mount kinds would never work from a shell in any case; that is
  a consequence of what a shell is, not a gap.
- **It does not isolate.** Like everything else in the zone model, it holds until the first shell
  command.
- **It does not make a shared folder coherent.** Two projects writing into one deployment directory
  is still a race; a mount names it, it does not lock it.
- **A missing target is not an error at load.** The project opens and the mount is marked
  unavailable; reaching into it then says so by name, rather than reporting a missing file.

Design: [`ADR_20260814_core_project-mounts`](../docs/adr/ADR_20260814_core_project-mounts.md).

## Launch Profiles

A profile is a **CLI parameter and a template applied once, at creation.** It is not a field of the
manifest and nothing at load time reads it back — an existing project's behavior comes entirely from
the ordinary settings the profile wrote (`agent.capabilities`, `plugins:`), never from "which profile
made this". Storing the profile too would put a second source of truth beside those settings and
raise the question of which one wins; storing only its result cannot.

| Profile | Default | Writes | Meaning |
|---|---|---|---|
| `minimal` | Yes | `agent.capabilities: []` and a `plugins: { "*": { enabled: false } }` wildcard entry | No built-in `core.*` features and no plugins. The LLM connection still comes through — it is not a capability, and without it there is nothing to run. |
| `standard` | No | Nothing | Deliberately empty: an absent `agent.capabilities` key already means "everything", and an absent `plugins:` section already means every plugin runs. Spelling that out would freeze today's feature list into the manifest. |
| `inherit` | No | No manifest at all | Runs against `~/.spla/defaults.yaml` with no project and no path boundary — the historical behavior, reachable but never the default. |

Set a profile with `spla init --profile <name> [--name <name>] [directory]`, or with `--init[=<name>]`
in front of any other command to create-then-continue in one step (`spla --init chat run "..."`,
`spla --init=standard serve`). `--init` alone means `minimal`.

Running in a folder with no manifest no longer silently inherits machine defaults. An interactive
session (the REPL with no command, or `chat open`) asks the person what to do. A scripted or headless
invocation — `chat run`, `mcp`, `serve` — refuses and names `--init`, because a prompt in a process
nobody is watching just hangs.

Manifests written before profiles existed are unaffected: the absence of a profile marker means
exactly what it always meant, since there never was one to begin with.

### `minimal` result

```yaml
version: 1

name: My Project

agent:
  capabilities: []

plugins:
  "*":
    enabled: false
```

## The `*` plugin entry

`IsPluginEnabled` resolves in this order: an entry naming the plugin wins if it sets `enabled`;
otherwise an entry under the key `"*"` wins if it sets `enabled`; otherwise the plugin is enabled.

```yaml
plugins:
  "*":
    enabled: false      # every plugin without its own entry is off
  network:
    enabled: true        # named entry still wins over the wildcard
```

The wildcard exists instead of a generated list of every installed plugin because a manifest travels
in git and a list of one machine's plugins does not — a second machine with a different plugin set
would either be missing entries (silently inheriting "enabled") or carry stale ones for plugins it
never had. `"*"` says "off unless named" without naming anything, which is exactly what the `minimal`
launch profile writes. `standard` writes nothing here for the same reason it writes nothing under
`agent.capabilities`: an absent `plugins:` section already means every plugin runs.

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

# Auto-detect (looks for *.spla in CWD only — no walk up to parent directories,
# and two manifests in one directory is a refusal, not a coin toss)
spla

# Web service; create a chat and send its first message when the first client connects
spla serve --new-chat "Introduce this project"
```

### GUI
Double-click `my-project.spla` → SPLA opens with full context.
