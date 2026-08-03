# Skill System Architecture

Read this before touching `SkillManager`, any `ISkillSource`, `SkillsContributor`, the skill tools,
or any UI that reflects skill state. How the prompt is assembled around them: [`composition.md`](composition.md).

---

## Overview

Skills are on-demand procedures. A skill body is loaded into the agent's context only when the skill
is activated (or when it is marked `preloaded`) — it is never part of the base prompt by default.
That keeps the base prompt lean and stops skill rules from competing with each other.

A skill is **not owned by a plugin**. It comes from a *source*, declares what it *needs*, and the
runtime decides whether it can be offered. There are four layers:

- **Sources** (`ISkillSource`) — where skills come from: a folder, a plugin package, later a server.
- **Frontmatter** (`SkillFrontmatter`) — the file format, parsed outside the source.
- **Registry** (`SkillManager`) — collects, layers settings, resolves state, answers queries.
- **Session** (`ISkillSession`) — which skill, if any, is active in a given chat.

---

## Skill file format

```markdown
---
id: network.host-audit
description: One or two sentences used for matching a user request to this skill.
requires:                       # optional — omit entirely for a plain procedure
  tools: [dns_lookup, port_scan]
  features: [core.memory]
uses:                           # optional — nice to have, never gates availability
  tools: [tls_probe]
enabled: true                   # the skill's own default; settings can override
preloaded: false                # true = body goes into the base prompt, no activation step
---

Step 1: ...
Step 2: ...
```

Everything except the body is optional. **A markdown file with no frontmatter at all is a valid
skill** — its id is the file name, it requires nothing, and it is available immediately. That is the
normal shape for a user's own procedure ("how I write release notes") and the reason requirements
are opt-in: most skills are prose, not tool choreography.

`requires` is the only thing that can make a skill vanish. Declare a tool there when calling it *is*
the procedure; declare it in `uses` when the skill merely benefits from it.

---

## Sources

```csharp
public interface ISkillSource
{
    string     Id    { get; }          // "project", "machine", "plugin:network"
    string     Label { get; }
    SkillTrust Trust { get; }
    IReadOnlyList<SkillEntry> Enumerate();
    string?    ReadBody(string skillRef);
    event Action? Changed;
}
```

Deliberately minimal — enumerate and read, nothing else. `Ref` is **opaque to everything outside the
owning source**: a relative path for a folder, a primary key for a database, a URL for a server. The
core never interprets it, which is what makes a non-filesystem provider possible. A skill is
addressed by `SourceId` + `Ref`; `SkillMeta` carries no file path.

`IEditableSkillSource` is the optional second role for sources that can be written back. It exists so
an editor can be added without reopening `ISkillSource`. Only `DirectorySkillSource` implements it,
and no UI calls it yet.

Parsing is **not** a source's job — `SkillFrontmatter` does it on top. A future source serving a
different kind of prompt asset reuses the same source with a different parser.

### Implementations

| Type | Id | What it serves |
|---|---|---|
| `DirectorySkillSource` | `repo` | The project's own committed `skills/` folder. |
| `DirectorySkillSource` | `local` | `.spla/skills` — personal drafts. `.spla/` is local state and git-ignored in full. |
| `DirectorySkillSource` | `machine` | `<SPLA home>/skills` — the user's, across every project. |
| `DirectorySkillSource` | `builtin` | `<install>/skills` — shipped with the product, beside `plugins/`. |
| `PluginSkillSource` | `plugin:<id>` | Markdown at a package root (`type: skills` packages) or in a plugin's `skills/` subfolder. **Returns nothing while its plugin is disabled.** |

Change detection lives inside the source. A `FileSystemWatcher` is an implementation detail of
file-shaped sources; a server source will signal `Changed` by entirely different means.

### Folder layout

A folder tree, walked recursively. One rule decides what a subfolder is:

- **Contains `SKILL.md`** → the folder *is* one skill, and everything beside it is that skill's
  resources (scripts, references). The walk does not descend further.
- **No `SKILL.md`** → the folder is grouping (`skills/network/`, `skills/1c/`) and the walk continues.

So nesting is free for organisation and never ambiguous. `README.md` at any level is documentation
about the folder, not a skill; dot-folders, `bin`, `obj` and `node_modules` are skipped.

A file that declares no `id:` gets one from its path: `skills/network/dns.md` → `network.dns`. Folder
structure becomes an id namespace, which matches how skills are already named and stops two files
with the same leaf name from colliding. An explicit `id:` in the frontmatter always wins.

### Configuration

```yaml
skills:
  sources:                        # absent = the two defaults below
    - type: directory
      path: .spla/skills
    - type: directory
      path: /srv/shared-skills
      trust: untrusted
      label: Shared
  items:
    network.host-audit:
      enabled: false
```

`type` selects an `ISkillSourceFactory`; the factory validates its own fields. Adding a source kind
(server, database, git) means registering a factory — no core changes.

Layering across `~/.spla/defaults.yaml` and the project `.spla`:

- **`sources` replaces wholesale.** Merging would leave no way to drop an inherited source. A layer
  that omits `sources` keeps the inherited list.
- **`items` merges by id**, project wins. A project switches one skill off without restating the rest.

List order is priority order: the first source offering an id owns it, later ones are marked
`Shadowed` (still listed in the panel, so an override is visible).

The installation's own sources — `builtin` and one per plugin — are appended after the configured
list and are **not** declared in `skills.sources`, for the same reason plugins are discovered rather
than declared: they are what the product came with, and a project that replaces `sources` must not
lose them silently. Coming last, they are still overridable per skill: a project file reusing a
shipped skill's id wins, and the shipped one shows as `Shadowed`.

A plugin's skills follow that plugin's own toggle, so there is exactly **one** switch per plugin
rather than two.

---

## State resolution

```csharp
public enum SkillState { Available, MissingTools, DisabledByUser, DisabledByTrust, Shadowed }
```

Resolved in this order — an explicit user decision outranks trust, and both outrank a missing tool,
because telling someone their switched-off skill also lacks a plugin is noise:

1. **`DisabledByUser`** — `skills.items.<id>.enabled: false`, or the skill's own `enabled: false`.
2. **`DisabledByTrust`** — untrusted source and no explicit `enabled: true`. A skill body becomes part
   of the system prompt, so content the user did not write needs a deliberate opt-in; the file saying
   `enabled: true` about itself is not the user's decision.
3. **`MissingTools`** — some `requires.tools` is not registered, or some `requires.features` is not in
   the resolved `agent.capabilities`. The reason names the tools and the plugins that own them.
4. **`Available`** — otherwise. Empty requirements land here, which is the common case.

Only `Available` skills reach the prompt (`SkillManager.GetAvailable`), get previewed by
`agent_info`, or can be activated. `GetAll` keeps everything for the settings panel, so a skill that
is off explains itself instead of disappearing.

Capability answers come from `ISkillCapabilityProbe`, supplied via `SetProbe` once the tool host and
feature set exist. `SkillManager` therefore depends on neither.

### Fail-closed

A skill is offered only when a source vouches for it AND its requirements are met AND it is switched
on. There is no path by which an unowned skill defaults to enabled.

The previous design had one, and it is worth remembering: a directory scan ran *beside* plugin
registration and produced entries with no owner, which the filter then admitted via
`OwnerPlugin?.IsEffectivelyEnabled ?? true`. Disabled plugins kept injecting their skills into the
system prompt. Both the scan and the nullable owner are gone.

---

## Lifecycle

```
[ Idle ] ──skill_activate(id)──▶ [ Active ] ──skill_deactivate──▶ [ Idle ]
                                     │
                                     └── body injected into the prompt
```

At most one skill is Active per chat. `skill_activate` while another is active is an error.
`ISkillSession` (`src/core/SPLA.Domain/Agent/ISkillSession.cs`) holds the state, one instance per
chat, and raises `Changed` so the UI and the prompt assembler react.

### The body is pinned for the run

`Activate` takes the procedure text, not just the id, and the session holds it until `Deactivate`.
Everything downstream reads that snapshot; nothing re-fetches from the source while a skill is
active.

That is what reconciles the two halves of hot reload: a skill file can be edited, added or deleted
at any time and the registry follows along, but the procedure a model is *currently executing*
cannot be swapped out from under it mid-run. The edit takes effect at the next activation. A source
that cannot produce the body fails the activation outright rather than activating into an empty
block.

`SkillManager.IsSkillActive` still exists and still defers a source-triggered rebuild while set. It
is no longer what protects a running skill — the pin does that — so it is free to be used as a
plain flag.

### Ending a skill

Three ways out, and the last one matters more than it looks:

- the model calls `skill_deactivate` as its final step (the designed path);
- the chat is closed — `SkillSession` is in-memory and deliberately not persisted;
- **the user ends it** — `ChatRuntime.DeactivateSkill()`, reachable from every client:
  - `chat.skill.deactivate` over the protocol, answered with a `chat.skill.state` broadcast to the
    chat's watchers, so two windows on one chat never disagree;
  - the active-skill chip in the web status bar (which the desktop shell hosts, so that is the
    desktop control too);
  - `/skills unload` in the CLI REPL.

The user's exit is not a convenience. A model that simply never calls `skill_deactivate` wedges the
chat: the skills index is suppressed while a skill is active, so it cannot be told about another
one, and `skill_activate` refuses a second. With the tool as the only exit, the chat is stuck until
restart.

Note what the CLI command alone does *not* solve: a skill session lives in the `ChatRuntime` of the
process running it, so `/skills unload` reaches only chats inside that CLI. A chat wedged in the
desktop or web client needs the protocol op — which is why the escape hatch belongs there first.

Clients learn the state from `ChatOpenedPayload.ActiveSkillId` (attaching to a chat left mid-skill)
and `TurnCompletePayload.ActiveSkillId` (end of turn — the moment a forgotten `skill_deactivate`
becomes visible and actionable). Neither needs an event subscription kept alive across a turn.

### Tools

- **`skill_activate`** — validates via `SkillManager.Find`, refuses a non-`Available` skill *with its
  reason* ("needs `port_scan` — from plugin 'network'") rather than pretending it does not exist,
  then calls `ISkillSession.Activate`. Mode-gated (see the matrix below).
- **`skill_deactivate`** — `ToolScope.Agent`, allowed in every mode. Stopping is always safe.
- **`agent_info`** — doubles as skill preview; returns a body only for `Available` skills, since
  handing back a procedure whose tools are missing just walks the model into failing calls.
- **`agent_clarify`** — the confirmation gate before activation, and general structured questions.

| Tool | ToolScope | Chat | Research | Inspect | Edit | Agent |
|---|---|---|---|---|---|---|
| `skill_activate` | Skill | Ask | Deny | Ask | Allow | Allow |
| `skill_deactivate` | Agent | Allow | Allow | Allow | Allow | Allow |
| `agent_clarify` | Agent | Allow | Allow | Allow | Allow | Allow |
| `agent_spawn` | Agent | Ask | Deny | Deny | Allow | Allow |

---

## Prompt assembly

```
[ Mode preamble ]
[ Core feature fragments ]      ← the skills contributor is gated on core.skills
[ Instructions / custom prompt ]
[ === ACTIVE SKILL: id === ]    ← when ISkillSession.ActiveSkillId != null
[ Preloaded skill bodies ]
[ Skills index ]                ← suppressed while a skill is active
[ Plugin prompts / commands ]
```

Skills are **one contributor among several** (`SkillsContributor`), not a branch of a prompt builder:
the assembly order above is declared in `AgentContributors.Default` and folded by
`AgentContextComposer`. See [`composition.md`](composition.md) for the mechanism; this section covers
only what the skill system puts in.

The index lists `GetAvailable()` only. The standing description of what an ACTIVE SKILL block means
lives once in the global prompt; skill bodies must not repeat it.

### When it is assembled

`ConversationOrchestrator.Context` is a provider invoked on **every iteration** of the agent loop, and
its system-prompt half replaces the leading system message of the assembled list (a fresh message —
the stored conversation is never written to).

Per-iteration rather than per-turn because `skill_activate` is a move the model makes *mid-turn*: a
prompt built once before the loop would inject the procedure only from the user's next message, by
which point the model has already had to act without it.

The provider runs inside the turn's `AgentSessionScope`, and that is what lets a runtime-wide
contributor render per-chat state at all. `SkillsContributor` resolves its session from the
constructor argument if given, else from the ambient scope — the same pattern the skill tools use.
Passing one explicitly still wins, which is how a spawned sub-agent keeps describing its own skill
while running inside the parent's async flow.

---

## Reload

Sources raise `Changed`; `SkillManager` re-enumerates and recomputes. A running procedure is safe
regardless — see "The body is pinned for the run" above; `SkillManager.IsSkillActive` additionally
defers the rebuild itself while set. Plugin packages do not change under a running process, so
`PluginSkillSource` never raises `Changed` — the registry is rebuilt on a plugin load pass instead.

`DirectorySkillSource` watches its root when it exists, and the nearest existing **ancestor** when it
does not, swapping to the real watcher the moment the folder appears. A missing folder is the normal
state of `.spla/skills` right up until the user writes a first draft there, and requiring a restart
at exactly that moment is the opposite of hot reload. Deleting the root re-arms the ancestor watch,
so delete-and-recreate is survivable rather than one-way.

---

## Where to put a skill

| It belongs to… | Put it in | Reaches the agent via |
|---|---|---|
| one plugin's tools | that plugin's `skills/` subfolder | `plugin:<id>`, gated by the plugin's toggle |
| this project / repo | `skills/` at the repo root, committed | `repo` |
| you, in this project | `.spla/skills` (git-ignored) | `local` |
| you, everywhere | `<SPLA home>/skills` | `machine` |

A separate `type: skills` plugin package still works (markdown at its root) but is no longer the
recommended shape: it means two switches for one capability.

## Deployment

`CopySkills.ps1` defines, once, what a skills tree is on disk and how to move one — the same
exclusions as the scanner (`README.md`, dot-folders, `bin`/`obj`/`node_modules`), structure preserved.
Dot-source it and call `Copy-Skills -From <dir> -To <dir> [-Clean]`; copying nothing is not an error.

`PublishAll.ps1` uses it twice: for each plugin's `Extras` (inside the parallel jobs, which need
their own dot-source), and for the repo's `skills/` → `.publish/work/skills`, which is where the
`builtin` source looks.

The single definition matters: the layout used to be encoded separately in the publish script and in
`SPLA.Skills.Network.csproj`, the two drifted (`network/skills/` vs `network.skills/`), and that
drift is exactly how a disabled plugin kept injecting its skills into the system prompt.
