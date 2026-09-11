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
  agents_md: inject

roles: [reviewer]

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
| `agent.instructions` | No | Markdown files injected into the system prompt. Paths relative to the project root. An entry named `AGENTS.md` (any casing, any subdirectory) is skipped with a logged warning — that file has its own mechanism, `agent.agents_md`, and would otherwise reach the prompt twice. |
| `agent.agents_md` | No | How the project's `AGENTS.md` tree reaches the prompt: `inject` (default — root file today, nested files as folders are visited in a later wave) or `ignore` (SPLA never reads it, root or nested). Per-role override with the same key under `roles/<name>.yaml`; absent there inherits the project's value. See [`ADR_20260911-2_agent_agents-md-scopes`](../docs/adr/ADR_20260911-2_agent_agents-md-scopes.md). |
| `agent.capabilities` | No | Enabled built-in `core.*` capabilities. Missing = all; `[]` = pure chat with no built-in tools. |
| `agent.spawned_retention` | No | How many finished spawned sessions to keep on disk, newest first (default 200). `0` keeps none; negative disables trimming entirely. Never touches a session with a run still in progress. Project-level only — not a per-role setting; retention is a disk policy of the project, not a behaviour a role narrows. See [Roles](#roles). |
| `agent.peer_debounce_base` / `agent.peer_debounce_max` / `agent.peer_depth_ceiling` / `agent.peer_hard_cap` | No | The correspondence decay regulator — how fast an exchange between two actors slows down and where it is cut off. See [Correspondence decay](#correspondence-decay). |
| `agent.self_feeding_cap` | No | How many consecutive turns with no human message the chat pump allows itself before it stops waking a turn and posts a notice instead. Unset or `0` — disabled, no cap. See [Correspondence decay](#correspondence-decay). |
| `roles` | No | Names of the roles this project has, e.g. `[reviewer, architect]`. Each name pairs with a body at `roles/<name>.yaml`, next to this manifest. See [Roles](#roles). |
| `llm.provider` | No | LLM provider. Currently only `lmstudio`. |
| `llm.endpoint` | No | API base URL. |
| `llm.model` | No | Model name. `auto` = use whatever is loaded. |
| `connections` | No | Named connection list (merged by `id` across layers — see [Connection scopes](#connection-scopes)); each entry: `id`, `name`, `provider`, `endpoint`, `api_key`, `model`, `context_length`, `lock_model`, `swap_model`. When no layer declares one, a default connection is synthesized from `llm.*`. |
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

## Connection scopes

A connection is an endpoint plus a credential — properties of an *account*, not of a repository. So
it lives in one of the same three layers a secret does, and the layer **is** the file it is written
in:

| Scope | File | Meaning |
|---|---|---|
| `shared` | `<sharedDir>/connections.shared.yaml` | Administered, shared between people. |
| `user` | `<personalDir>/connections.yaml` — `~/.spla` locally, the caller's own area on a server | Yours, never committed, present in every project you open. |
| `project` | the `connections:` block in this manifest | Travels with the repository. |

They merge by `id`, least authoritative first:
`shared` → `defaults.yaml`'s own `connections:` → `connections.yaml` → this manifest. A later layer
replaces an entry **wholesale**, not field by field. `connections:` in `defaults.yaml` still works
and counts as `user` — that file belongs to the person at the keyboard — and `connections.yaml` is
the file the settings panel writes, so an `id` in both resolves to the newer one.

The scope is never a key inside a connection entry: it is the file, and a second answer that can
disagree with the first is one answer too many. Saving from the settings panel routes each entry
back to its own file, so moving a connection between scopes moves it out of the old file.

See [`ADR_20260904_core_connection-scopes`](../docs/adr/ADR_20260904_core_connection-scopes.md).

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

## Roles

A role is a **named type of actor settings** — the same kind of thing `agent:` above already is, per
[`ADR_20260827-2_core_roles`](../docs/adr/ADR_20260827-2_core_roles.md) §2.1. `agent:` is a project's
*default* role — "role zero" — and needs no entry anywhere; any other role is declared twice, in two
different places, for two different reasons:

1. **The manifest names it**, under `roles:`. This is what makes the role *exist* — a manifest that
   never mentions `reviewer` has no `reviewer` role, no matter what sits on disk.
2. **A file holds its body**, at `roles/<name>.yaml`, next to the manifest. This travels in git —
   unlike `.spla/`, which is closed as a whole and holds nothing that acts.

```yaml
# project.spla
roles: [reviewer, architect]
```

```yaml
# roles/reviewer.yaml
mode: Research
instructions:
  - docs/review-checklist.md
capabilities:
  - core.read
model: fast-local
toolsets:
  roslyn: agent_demand
islands:
  - sql:staging-db
```

**A file in `roles/` that the manifest does not name does not act.** This is a security property, not
tidiness: a role file arriving in someone else's pull request must not become a new actor with access
the moment it lands on disk. It is exactly the same logic as "no walking up the tree" in
[Usage](#usage) below — nothing acts that nobody named.

**A role's body has the same shape as `agent:`**, plus three fields of its own:

| Field | Meaning |
|---|---|
| `mode`, `instructions`, `capabilities`, `custom_prompt`, `agents_md`, `loop_guard*`, `ask_timeout_minutes`, `shell_timeout_seconds`, `trusted_domains`, `save_tool_calls`, `save_attempts`, `peer_debounce_base`, `peer_debounce_max`, `peer_depth_ceiling`, `peer_hard_cap` | Same meaning as the identically-named `agent.*` field above. Absent on the role = inherit the project's own value, same as every other field here. (`spawned_retention` is the one exception — project-level only, see the fields table above.) |
| `connections` | Which connections this role may use, by `id` or by scope name (`user`, `project`, `shared` — a whole layer in one word). Absent/empty = every connection resolved for the project. A *selection*, not a grant, the same as `islands` below: there is no endpoint or credential in this list to declare one with. A named `id` that does not exist is an error; a scope with no entries is not (that is a fact about the machine). |
| `model` | Which of the resolved `connections:` models this role runs on. A role does not declare its own connection — the layers declare what is reachable at all, a role only chooses among it. Refused when the role's own `connections:` selection excludes it. |
| `toolsets` | Same shape as the top-level `toolsets:` section, merged over it key by key — a role that mentions one set narrows (or widens, within what the capability gate still allows) only that set. |
| `islands` | Which of the project's already-reachable islands (a database, a host, a foreign tool server — see `SPLA.Domain.Security.IslandIdentity`) this role's prompt and tool surface mention. A *selection*, not a grant: this list narrows what is shown, it never widens what is actually reachable — the capability gate is still the only place a reach is decided. |

**A role is not a subset of the project's own permissions.** `capabilities` on a role *replaces* the
project's list for that role — a role may declare a capability `agent:` never mentioned, and gets it.
The ceiling on what a role may actually reach is the directory root and the owner's grants, never the
union of what `agent:` happened to declare. Requiring "every role's capabilities ⊆ `agent:`'s" would
make the project itself the maximally privileged entity — the exact failure
[`ADR_20260819`](../docs/adr/ADR_20260819_core_project-entry.md) describes for project-less mode.

**A role is not a second axis of permissions.** It has no `permissions:` block of its own: it picks a
`mode`, the same way `agent:` does, and narrows what runs inside that mode. Two overlapping permission
systems would immediately raise "who overrides whom" — the ADR rejected that outright.

**A role is not self-assigned.** The manifest's owner writes `roles:` and each `roles/<name>.yaml`;
nothing an agent can compute or say at runtime substitutes for that. There is no tool and no
configuration path that lets a running agent invent a role or take one it was not given — this is
called out in the ADR as "the only real invariant", everything else being a question of grants.

## Correspondence decay

A [correspondence](composition.md#correspondents-are-deliberately-not-a-contributor) between two
actors (`agent_correspond`, and the `reply_<role>[_<n>]` tool it opens — see
[`toolsets.md`](toolsets.md#virtual-reply-tools-are-outside-this-system)) is internal circulation: a
reply to a reply needs no person watching either chat to keep going, which is exactly what makes an
unbounded exchange possible. Four settings, all under `agent:` (and overridable per role, alongside
every other `agent.*` field a role narrows — see [Roles](#roles) above), are the regulator that keeps
one from running forever:

```yaml
agent:
  peer_debounce_base: 2      # seconds
  peer_debounce_max: 300     # seconds
  peer_depth_ceiling: 6
  peer_hard_cap: 24
  self_feeding_cap: 0        # 0 = disabled, no cap
```

| Field | Default | Meaning |
|---|---|---|
| `peer_debounce_base` | `2` | Seconds the pump waits before waking a turn for the first reply after external energy (a human message or a finished background task resets the count to 0). |
| `peer_debounce_max` | `300` | Ceiling on the wait below — the doubling never waits longer than this between an incoming reply and the turn it wakes. |
| `peer_depth_ceiling` | `6` | How many consecutive replies (since the last human message or task result) may still raise a turn of their own. Past this depth a reply no longer wakes one — it stays queued and rides whatever turn happens for some other reason, so the exchange slows to the pace of outside events rather than stopping. |
| `peer_hard_cap` | `24` | Emergency stop — should never be reached in normal operation, since the debounce and depth ceiling above exist to keep depth from ever getting here. Reaching it is a defect in the regulator, not a normal outcome, and refuses the reply with a notice into the chat instead of silently continuing. |
| `self_feeding_cap` | unset (disabled) | A counter separate from correspondence: how many consecutive turns with no human message `ChatPump` allows itself before it refuses to wake another one and posts a notice. Unset or `0` — no cap, a turn can keep going as long as the `peer_*` regulator above lets it. Set a positive number if a particular project still wants a ceiling on an unattended run. |

The wait between replies is `peer_debounce_base · 2^depth`, floored at `peer_debounce_base` and
capped at `peer_debounce_max` — depth 0 (the reply right after external energy) always waits exactly
the base amount, and the wait only grows once a correspondence starts circulating on its own. A reply
past `peer_depth_ceiling` is never discarded, only left queued; only `peer_hard_cap` actually refuses
one. See `docs/adr/ADR_20260827-2_core_roles.md` §2.4 and `ChatPump.PeerWakePolicy`/`DecideWake` for
the regulator itself.

Until now `self_feeding_cap` was a constant baked into the code (`ChatPump.SelfFeedingCap = 3`), and it
counted a turn as "self-fed" even when a correspondent's reply was what woke it — an honest ping-pong
between two roles hit that ceiling on the third turn, before `peer_depth_ceiling`/`peer_hard_cap` above
ever got a chance to fire. It is now disabled by default: a correspondence is bounded by the `peer_*`
regulator alone, and `self_feeding_cap` is a separate, optional rein on top of it.

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
