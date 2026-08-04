# Skill System Architecture

Read this before touching `SkillLibrary`, any `ISkillSource`, `SkillsContributor`, the skill tools,
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
- **Library** (`SkillLibrary`) — collects, layers settings, resolves state, answers queries.
- **Session** (`ISkillSession`) — which skill, if any, is active in a given chat.

### Where the code lives

```
src/core/SPLA.Library/          the library itself — no dependency on the tool host
  SkillLibrary.cs               holdings + catalog
  ISkillCapabilityProbe.cs      the one question it asks the outside world
  Catalog/    SkillCard, SkillState, SkillTag, TagVocabulary, CatalogView
  Sources/    ISkillSource, SourceLevel, DirectorySkillSource, PluginSkillSource, SkillSourceRegistry
  Format/     SkillFrontmatter
```

`ISkillSession` stays in `SPLA.Domain/Agent/` rather than moving here, and that is a dependency fact
rather than a preference: `AgentSession` and `AgentSessionScope` in `SPLA.Domain` compose it, while
`SkillLibrary` needs `SPLA.Domain.Settings` to layer `skills.items`. Putting the session in
`SPLA.Library` would close the loop `Domain → Library → Domain`. The loan lives with the chat; the
library is what the chat borrows from.

The skill tools (`skill_activate`, `skill_deactivate`, `skill_read_resource`) stay in
`SPLA.MCP.Core/Tools/` with every other tool — they are tools first, and `SPLA.MCP.Core` references
`SPLA.Library`, never the reverse.

---

## Skill file format

```markdown
---
id: network.host-audit
description: One or two sentences used for matching a user request to this skill.
tags: [ssh, linux, audit]      # optional — subject words for catalog lookup
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

`tags` are normalised at the door by `SkillTag.Normalize` — lower-case, kebab-case, punctuation
collapsed to one dash, duplicates dropped, and anything that normalises to nothing thrown away. So
`SSH`, `ssh_access` and `SSH---Access` cannot become three subjects. What normalisation cannot catch
is `ssh` and `ssh-access` being two words for one thing; the settings panel therefore shows the whole
vocabulary at once, because seeing them side by side is the only way that becomes noticeable.

Tags are **not** recovered by the lenient parser. Like `requires`, they are structured, and the
existing rule holds: guessing at the shape of a malformed list is worse than treating the skill as
untagged and letting the author fix the quoting. An untagged skill is not an error — it simply cannot
be found by subject, and therefore never leaves the shelf (see below).

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
    IReadOnlyList<string> ListResources(string skillRef);          // default: []
    string?    ReadResource(string skillRef, string resourcePath); // default: null
    event Action? Changed;
}
```

Deliberately minimal — enumerate and read, nothing else. `Ref` is **opaque to everything outside the
owning source**: a relative path for a folder, a primary key for a database, a URL for a server. The
core never interprets it, which is what makes a non-filesystem provider possible. A skill is
addressed by `SourceId` + `Ref`; `SkillCard` carries no file path.

`IEditableSkillSource` is the optional second role for sources that can be written back. It exists so
an editor can be added without reopening `ISkillSource`. Only `DirectorySkillSource` implements it,
and no UI calls it yet.

The two resource members have interface defaults, so a source that serves bare text implements
nothing. Resolution happens **inside** the source for the same reason `Ref` is opaque: only the source
knows what its refs mean, and only it can tell one skill's attachment from anything else it can reach.
A file-shaped source must refuse a path that escapes the skill's own folder — these strings arrive
from a model.

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

`ListResources` is that same rule read from the other side: for a folder skill it returns everything
beside the `SKILL.md`, walked recursively with the same exclusions, `SKILL.md` itself left out. A bare
`name.md` therefore has **no** resources — its neighbours are other people's skills, not its own
attachments, and carrying attachments is exactly what the folder layout is for.

A plugin skill is a single file, so its attachments live in the folder named after it beside it:
`skills/host-audit.md` is served by `skills/host-audit/`. That folder is invisible to `Enumerate`,
which reads only top-level `*.md`, so the convention cannot turn an appendix into a skill of its own.

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
      level: in-catalog          # absent = on-shelf
      label: Shared
  items:
    network.host-audit:
      enabled: false
```

`type` selects an `ISkillSourceFactory`; the factory validates its own fields. Adding a source kind
(server, database, git) means registering a factory — no core changes.

### Level: how much of a source reaches the model

`SourceLevel` is the same ladder shape as [`ToolSetLevel`](../src/core/SPLA.MCP.Core/ToolSets/ToolSetLevel.cs)
and answers the same question — not "may these run" but "who is told they exist, and at what price".

| `level:` | In the prompt | Reached by |
|---|---|---|
| `out-of-catalog` | nothing at all | a person, handing the skill to a chat |
| `findable` | nothing at all | `skill_find` (stage 4; until then, same as above) |
| `in-catalog` | its tags, in the cloud | the model, in two steps |
| `on-shelf` *(default)* | id + description, every request | the model, in one step |

**The level is on the source, not the skill.** A hundred skills would be a hundred decisions; one
external repository is one decision, and that is the decision a person actually holds an opinion
about. An unparseable or absent value falls back to `on-shelf` — a typo must not silently hide a
source's skills, because hiding is the failure hardest to notice.

**Level is not trust.** Trust decides whether a skill may be used at all and needs the user's
consent; level decides only who is told. An `out-of-catalog` skill is still listed in the panel, still
activatable by a person, and still refused if its source is untrusted. Neither axis substitutes for
the other.

Layering across `~/.spla/defaults.yaml` and the project `.spla`:

- **`sources` replaces wholesale.** Merging would leave no way to drop an inherited source. A layer
  that omits `sources` keeps the inherited list.
- **`items` merges by id**, project wins. A project switches one skill off without restating the rest.

List order is priority order: the first source offering an id owns it, later ones are marked
`Superseded` (still listed in the panel, so an override is visible).

The installation's own sources — `builtin` and one per plugin — are appended after the configured
list and are **not** declared in `skills.sources`, for the same reason plugins are discovered rather
than declared: they are what the product came with, and a project that replaces `sources` must not
lose them silently. Coming last, they are still overridable per skill: a project file reusing a
shipped skill's id wins, and the shipped one shows as `Superseded`.

A plugin's skills follow that plugin's own toggle, so there is exactly **one** switch per plugin
rather than two.

---

## State resolution

```csharp
public enum SkillState { Available, MissingPrerequisites, DisabledByUser, DisabledByTrust, Superseded }
```

Resolved in this order — an explicit user decision outranks trust, and both outrank a missing tool,
because telling someone their switched-off skill also lacks a plugin is noise:

1. **`DisabledByUser`** — `skills.items.<id>.enabled: false`, or the skill's own `enabled: false`.
2. **`DisabledByTrust`** — untrusted source and no explicit `enabled: true`. A skill body becomes part
   of the system prompt, so content the user did not write needs a deliberate opt-in; the file saying
   `enabled: true` about itself is not the user's decision.
3. **`MissingPrerequisites`** — some `requires.tools` is not registered, or some `requires.features` is
   not in the resolved `agent.capabilities`. The reason names the tools and the plugins that own them.
4. **`Available`** — otherwise. Empty requirements land here, which is the common case.

Only `Available` skills reach the prompt (`SkillLibrary.Catalog()`), get previewed by
the index, or can be activated. `Holdings()` keeps everything for the settings panel, so a skill that
is off explains itself instead of disappearing.

Capability answers come from `ISkillCapabilityProbe`, supplied via `SetProbe` once the tool host and
feature set exist. `SkillLibrary` therefore depends on neither.

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

`SkillLibrary.IsSkillActive` still exists and still defers a source-triggered rebuild while set. It
is no longer what protects a running skill — the pin does that — so it is free to be used as a
plain flag.

### The loan slip: what is pinned about attachments

`Activate` also takes the skill's `SourceId`, its `Ref`, and the **list** of its resources; all three
go back to null/empty on `Deactivate`. That triple is the loan slip, and it decides two things:

- **Where attachments come from.** `skill_read_resource` addresses the source by the pinned id, not by
  a fresh lookup on `ActiveSkillId`. A rebuild that shadows the skill therefore cannot redirect a
  running procedure to a different edition's appendices — the same guarantee the pinned body gives.
- **What may be asked for.** A path not on the slip is refused before any source is touched, which is
  what stops one skill reaching another's references.

Only the list is pinned; **the text is fetched live at the moment it is asked for.** A procedure that
opens two references out of fourteen files should not pay for the other twelve at activation, and on a
server the whole set is not on the client's machine to begin with. The cost of the live read is the
"the shelf disappeared while the book is out" case: `skill_read_resource` then returns an error while
the pinned procedure keeps running, which is the honest answer rather than a wedged chat.

The consequence worth knowing: a resource **added** to the folder after activation is not on the slip
and is not readable until the next activation. That is the same shape as the body's pin, not a bug.

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

- **`skill_activate`** — validates via `SkillLibrary.Find`, refuses a non-`Available` skill *with its
  reason* ("needs `port_scan` — from plugin 'network'") rather than pretending it does not exist,
  then calls `ISkillSession.Activate`. Mode-gated (see the matrix below).
  It also loads the body only for `Available` skills, since handing back a procedure whose tools are
  missing just walks the model into failing calls, and it fills the loan slip (see above).
- **`skill_deactivate`** — `ToolScope.Agent`, allowed in every mode. Stopping is always safe.
- **`skill_read_resource`** — one attachment of the **currently active** skill, by a path from the
  loan slip. There is no argument naming a skill, which is why the model cannot ask for another one's;
  the source resolves the path itself, so escaping the folder fails inside the source rather than
  being checked in the tool. Both halves are needed: a loan check over a careless source would still
  serve `../../etc/passwd` to the one chat that is entitled.
  Skill-scoped like `skill_activate`, but `ToolEffect.Read`, and `PermissionManager` splits the scope
  on exactly that: **the gate is the activation, not each page.** Reading inside a skill the user
  already let in is not a second decision, and asking per reference would mean a dozen prompts for one
  step of one procedure — which trains the user to click through the prompt that does matter. Research
  still denies the whole scope: nothing can be activated there, so there is nothing to read.
- **`agent_clarify`** — the confirmation gate before activation, and general structured questions.

| Tool | ToolScope | Chat | Research | Inspect | Edit | Agent |
|---|---|---|---|---|---|---|
| `skill_activate` | Skill | Ask | Deny | Ask | Allow | Allow |
| `skill_read_resource` | Skill | Allow | Deny | Allow | Allow | Allow |
| `skill_deactivate` | Agent | Allow | Allow | Allow | Allow | Allow |
| `agent_clarify` | Agent | Allow | Allow | Allow | Allow | Allow |
| `agent_spawn` | Agent | Ask | Deny | Deny | Allow | Allow |

The table is `PermissionManager`, which runs at **execution** time. `ToolModeFilter` decides
**visibility** first, and in Chat mode it offers nothing but `Scope.Agent` — so the Skill-scoped rows
never come up there at all, and the Chat column describes a branch that is currently unreachable. See
the caveat in [`security.md`](security.md).

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

The index lists `Catalog()` only. The standing description of what an ACTIVE SKILL block means
lives once in the global prompt; skill bodies must not repeat it.

### What the index actually contains

`CatalogView.Build` splits the available skills into two, and the split is the point:

- **Shelf** — id and description per skill, the expensive half.
- **Cloud** — subject words with counts and nothing else, the half whose price stops tracking the
  size of the fond. Counts are printed because a bare word list would leave the model guessing
  whether a subject is one skill or forty — exactly the judgement it needs to decide whether asking
  is worth a turn.

Beyond `DefaultShelfLimit` (25) the shelf collapses: skills that would have been listed contribute
tags instead. **Untagged skills are never demoted** — a skill with no tags cannot be found by subject,
so moving it to the cloud would not summarise it, it would delete it. It keeps its place and keeps
costing what it costs. That is deliberately visible: the way to make a large fond cheap is to tag it,
and a project that has not should feel the price rather than lose the skills.

Level also outranks `preloaded`. A preloaded skill from an `out-of-catalog` source would be the
loudest possible way of telling the model about a source it is not supposed to know — the whole body,
unasked — so the contributor gates preloading on the level too.

The ACTIVE SKILL block ends with the skill's resource list when it has one — **names only**, never
contents. A procedure that opens two of fourteen files should not carry the other twelve in every
iteration, but it also cannot ask for what it does not know exists, so the list itself is not
optional. It is the catalogue card for the attachments.

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

Sources raise `Changed`; `SkillLibrary` re-enumerates and recomputes. A running procedure is safe
regardless — see "The body is pinned for the run" above; `SkillLibrary.IsSkillActive` additionally
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
