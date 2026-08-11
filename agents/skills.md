# Skill System Architecture

Read this before touching `SkillLibrary`, any `ISkillSource`, `SkillsContributor`, the skill tools,
or any UI that reflects skill state. How the prompt is assembled around them: [`composition.md`](composition.md).

---

## Overview

Skills are on-demand procedures. **A skill is loaded or it is not**, exactly one at a time, and its
body reaches the prompt only through activation. There is no third state.

There used to be one — `preloaded: true` put a body into the base prompt forever — and it is gone.
It answered "is this text in the base prompt", which describes prompt assembly rather than the
document, and it showed: a preloaded skill was not indexed, not activatable, not deactivatable, and
bypassed the source level. Text that must always be in the prompt is `agent.instructions`, which owns
that job with its own settings key and its own contributor.

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
  Librarians/ ITagLibrarian, TagLibrarian, IAgentLibrarian, AgentLibrarian
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
  inherit_defaults: true          # false = a white list: only what is named here
  sources:
    - id: ops                     # the key every layer merges on
      type: directory
      path: /srv/shared-skills
      level: in-catalog           # absent = on-shelf
      label: Shared
    - id: local                   # switching an inherited branch off is a complete statement
      enabled: false
  items:
    builtin:network.host-audit:   # a full address selects one edition...
      enabled: false
    network.host-audit:           # ...a bare id reaches every edition of that name
      enabled: false
  policy:                         # honoured only from the machine layer
    max_trust: untrusted          # nothing may exceed this, grants included
    user_may_vouch: false         # default: true locally, false on a server
  librarian:                      # absent = off; skill_find stays deterministic
    enabled: true
    model: gpt-oss-120b           # null = the project's default, i.e. the chat's own model
```

`type` selects an `ISkillSourceFactory`; the factory validates its own fields. Adding a source kind
(server, database, git) means registering a factory — no core changes.

**A branch declares its name.** `id` is the key, and everything follows from having one: adding a
folder is one entry in any layer, dropping an inherited one is `enabled: false`, clearing the set is
`inherit_defaults: false`. An entry without `id` falls back to a name derived from its location —
that is a fallback, not the identity, and a path is now an ordinary field.

An overlay may omit everything but the `id`: type is validated on the *merged* entry, never per
layer, which is what makes `- id: local` + `enabled: false` complete. Position is fixed by first
appearance, so editing a label cannot silently reorder the fond.

**Two stores, one model.** Prescribed entries live in the settings layers and travel with the project;
granted ones live in `<personal dir>/skills.yaml`, are stamped `Granted` on the way in, and come last
in the fold. That ordering is what lets the panel switch an inherited branch off without writing to a
committed file — it records an override under the same id, in the person's own store. The personal
dir is `~/.spla` locally and `{server root}/users/{userKey}` on a server.

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

- **`sources` merges by `id`**, like every other named collection here. It used to replace wholesale,
  and the comment justifying that named the symptom: an entry had no name, so there was no key to
  merge on. The name removed the reason.
- **`items` merges by key**, project wins.

**List order is display order and the tie-break in `skill_find` ranking — not priority.** Two branches
holding the same skill id both keep their book; see *Addressing* below.

### Addressing: a book is a branch plus a number

A skill's identity is `SkillCard.Address` — `branch:id`. `Id` alone is not identity: two branches may
each hold a book of the same name, and that is a normal state of a fond rather than a conflict.
Resolving a name by which shelf answered first is exactly the shape of dependency confusion, so it is
not done.

`SkillLibrary.Resolve` has **three** answers, not two: found, not found, and *ambiguous with the
candidates attached*. The third is the point — a librarian who picks for you will be wrong once,
quietly, and nobody will think to check. Every refusal must print the addresses; an ambiguity error
without alternatives is a dead end with extra words.

Ambiguity is judged **among the usable books first**. Two editions where only one is available is not
a question worth asking; two available ones is. When none is available the single match is still
returned, so the caller reports the real reason instead of "unknown skill".

Addresses are matched **whole, never split on `:`**. Source ids carry colons of their own — a plugin
branch is literally `plugin:network`, so `plugin:network:net.audit` has two — and any rule about which
colon separates what is a rule someone eventually gets wrong.

The model reads `DisplayId`: the bare id while it occurs once in the holdings, the full address once
it occurs twice. Computed once per rebuild, so the prompt, the panel and what `skill_activate` accepts
cannot drift apart. Always printing the address would be correct and would charge every prompt for a
branch name nobody needed.

`skills.items` keys are **predicates, not addresses**: a full address selects one edition, a bare id
reaches every edition of that name and is never ambiguous. "The skill called foo is not for me" is a
statement about a name; needing exactly one book is a property of borrowing, which is where ambiguity
is an error. The panel still writes addresses — a row is one edition, and a bare key is a fine thing
to write by hand and a terrible one to produce by clicking.

There is no `Superseded` state and no override-by-id. To replace a shipped skill, switch it off by
address and add your own under its own name — one place to grep instead of a resolution rule to know.

`builtin` is an **ordinary named entry** in the built-in set, switchable one at a time like any other.
It used to be appended unconditionally after the configured list, so that a project replacing
`sources` could not lose the shipped skills silently; merging by name makes losing anything silently
impossible, and the special case had nothing left to protect.

Plugin branches are still appended and are still not declarable in `skills.sources` — a plugin's
skills follow that plugin's own toggle, so there is exactly **one** switch per plugin rather than two.

---

## State resolution

```csharp
public enum SkillState { Available, MissingPrerequisites, DisabledByUser, DisabledByTrust }
```

Resolved in this order — an explicit user decision outranks trust, and both outrank a missing tool,
because telling someone their switched-off skill also lacks a plugin is noise:

1. **`DisabledByUser`** — a matching `skills.items` key says `enabled: false`, or the skill's own
   frontmatter does.
2. **`DisabledByTrust`** — untrusted source. **There is no per-skill way past this.** Switching one
   skill on used to lift it and no longer does: if the branch as a whole is not trusted, one
   arbitrarily trusted book out of it is a contradiction — the contents arrived together, from the
   same place, on the same terms. The two ways up are trusting the source (below) and copying the
   skill into a branch you already trust, which is one act visible in a diff.
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

### Trust: declared nowhere, granted from outside

A skill body becomes part of the system prompt, so an untrusted source is a prompt-injection surface:
reading an unfamiliar book aloud in the middle of your own flat. Trust is therefore never something a
source can state about itself. Three rules, applied in this order:

**1. Where the folder is.** Anything inside the workspace arrives with the clone — whether a `.spla`
named it or it was simply sitting in `skills/` — and starts `Untrusted`. The second case is the one
authorship rules miss entirely: nobody declared anything, so nobody could be caught claiming trust.
The installation folder is excluded even when it sits inside the workspace, which it does whenever
this product is developed on itself; build output is not somebody else's content.

**2. Which layer declared it.** A project `.spla` may *ask* for trust and may not decide it, because
it travels with a repository that may not be yours. The machine layer is the person at the keyboard.
`SourceOrigin` is stamped during resolution and never read from a file — an entry that could name its
own origin could name a privileged one. Standing belongs to whoever last chose the content (set
`path` or `trust`), in both directions; restating a path that *resolves* to where the entry already
pointed is not a choice, or every project writing its list out in full would untrust itself.

**3. The grant.** Only a record in `<personal dir>/skills.acl.yaml` lifts a source, and its key is the
**resolved path**, not the id. Rename the folder and approval does not follow — what is there now is
not what was read; rename the entry and approval stays with the folder, so a renamed entry cannot
inherit somebody else's decision. This is [`SecretAcl`](../src/core/SPLA.Domain/Secrets/SecretAcl.cs)'s
rule for a different object: *an ACL lives beside the store, never inside it.* Self-signed trust is not
trust, which is why `git` refuses a config from a repository owned by someone else and `apt` keeps
repository keys in a keyring the repository does not control.

Then two ceilings the administrator owns, read **only** from the machine layer — locally the person's
own home, on a server the service account's, unwritable by users:

| Key | Effect |
|---|---|
| `policy.max_trust: untrusted` | nothing reaches `Trusted`, grants included |
| `policy.user_may_vouch: false` | a person may add branches but not vouch for them; both the entry's claim and their own grant stop working |

`user_may_vouch` defaults to *true locally, false on a server*, derived from one fact: a deployment
that resolves personal directories has more than one person in it. The cut is on the trust level, not
on the right to write — a user adding a branch in their own area is doing nothing dangerous.

Plugin branches carry no grant key, deliberately: enabling a plugin already trusted it with executable
code, which is strictly more than trusting its text, and a second switch for one decision is a way to
have the two disagree.

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

### Starting one on the user's say-so

`ChatRuntime.ActivateSkill(id)` is the loan desk — `chat.skill.activate` over the protocol, the
`+ skill` picker in the status bar. It is the third of the ADR's three ways to take a book out, and
the cheapest: **the catalog is suppressed while a skill is active, so a handed-out chat carries no
index at all.** That is what closes the "weak model, small context" case outright, and it sidesteps
the failure the stage-4 runs found — nothing has to be chosen by the model, so no competing tool can
distract it from choosing.

Two rules, and the asymmetry between them is the point:

- **Level is not consulted.** A person picking from a list they can see is exactly what an
  `out-of-catalog` source is for: invisible to the model, fully visible to its owner.
- **State is.** A skill that is switched off, untrusted, or missing its tools must not slip in through
  a different door than the model's, so the refusal is reported rather than swallowed.

It raises the same `SkillDemand` tool sets `skill_activate` would have: a procedure handed over by a
person must arrive able to run.

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
- **`skill_find`** — asks the tag librarian what the fond has on a subject, and returns **cards, never
  bodies**: a stack of annotations to pick from, not procedures. Handing back bodies would put the
  full text of skills nobody chose straight back into the context, which is the thing this whole
  design exists to avoid. Read-effect like `skill_read_resource`, so asking costs no prompt.
  An empty result distinguishes "nobody wrote a skill for this" from "that is not a word here" —
  only the second is fixable by asking again, so the answer says which happened.
- **`agent_clarify`** — the confirmation gate before activation, and general structured questions.

### Two librarians behind one tool

The ADR calls them layers, not competitors, and `skill_find` is where that becomes real. There is
deliberately **no second search tool**: the stage-4 runs showed a weak model already struggles to call
this one at all, so adding a sibling would make selection worse rather than better.

| | Where the index lives | Cost | Reaches |
|---|---|---|---|
| `TagLibrarian` | nowhere — set intersection on the spot | 0–3 ms | only what an author tagged or wrote |
| `AgentLibrarian` | its own throwaway system prompt | 1.4–2.7 s, one LLM call | meaning: synonyms nobody tagged |

The tag pass always runs first and free. The model-backed one runs **only when that found nothing**,
and only when `skills.librarian.enabled` is on — it is off by default because it costs a call before
any work begins.

`AgentLibrarian` is **not** built on `agent_spawn`, despite the plan's wording: a spawn is a full
agent loop and it runs a *skill*, which a lookup is not. One `ILlmGateway` call is the whole thing,
with accounting and quotas already in that path.

**Its answer is never trusted as text.** The model returns ids; every one is looked up in the holdings
and anything that does not resolve is dropped. A hallucinated id is the obvious failure of this
approach, and mapping back through the library is what makes it impossible rather than merely
unlikely. `OutOfCatalog` skills never enter its prompt and would be refused coming back — the level
boundary holds here exactly as it does for tags.

It has its own `model:` because the catalog goes into *its* prompt, not the chat's. That is what makes
"weak model in the chat, a competent one at the desk" a setting rather than a compromise.

| Tool | ToolScope | Chat | Research | Inspect | Edit | Agent |
|---|---|---|---|---|---|---|
| `skill_activate` | Skill | Ask | Deny | Ask | Allow | Allow |
| `skill_find` | Skill | Allow | Deny | Allow | Allow | Allow |
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

When the cloud is non-empty the section also spells out the sequence — subject → `skill_find` →
`skill_activate` — in those words. That is not decoration: two-step selection introduces one new way
to fail, a model that never asks, and runs showed the wording carries real weight.

**What the runs actually showed** (`gemma-4-26b-a4b-qat`, 2026-08-04). In a clean context the weak
model handles the two steps first try: `skill_find` then `skill_activate`. What breaks it is **tool
competition** — with ninety plugin tools registered it went straight to `ssh_list_hosts` and never
consulted the catalog at all. That is the same failure the index rule about "start immediately"
already names, not a new one `skill_find` created; the prompt does not currently close it. A weak
model also sometimes guesses an id before searching, which is why the refusal names `skill_find`
rather than leaving a dead end, and why suggestions are filtered by level: a wrong guess must not
become a way to enumerate the fond.

Nothing bypasses this. When a skill is active the section is suppressed entirely and **no other
skill's body reaches the prompt by any route** — the removal of `preloaded` closed the last one,
which used to inject bodies even into a chat that already had a skill running.

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
regardless — see "The body is pinned for the run" above; `SkillLibrary.IsSkillActive` (wired by
`ChatRegistry`, any open chat counts) additionally **defers** the rebuild while a skill runs, and
`ApplyDeferredRebuild` lands it when the book comes back. Deferring rather than skipping matters now
that the source *list* can change: a lost file event is repaired by the next save, but a folder added
mid-procedure would otherwise never appear at all.

`SetSources` replaces the whole branch set — unsubscribing and disposing the old sources, because a
watcher nobody reads is a handle nobody closes — and `AgentRuntime.RebuildSkillSources` is what the
settings panel calls after editing the list or moving a grant. `SkillLibrary.Reloaded` is broadcast to
every client as `skills.result`, for every cause at once.

Plugin packages do not change under a running process, so `PluginSkillSource` never raises `Changed` —
the registry is rebuilt on a plugin load pass instead.

`DirectorySkillSource` watches its root when it exists, and the nearest existing **ancestor** when it
does not, swapping to the real watcher the moment the folder appears. A missing folder is the normal
state of `skills/` right up until the user writes a first one there, and requiring a restart at
exactly that moment is the opposite of hot reload. Deleting the root re-arms the ancestor watch,
so delete-and-recreate is survivable rather than one-way.

---

## Where to put a skill

| It belongs to… | Put it in | Reaches the agent via |
|---|---|---|
| one plugin's tools | that plugin's `skills/` subfolder | `plugin:<id>`, gated by the plugin's toggle |
| this project / repo | `skills/` at the repo root, committed | `repo` |
| you, everywhere | `<SPLA home>/skills` | `machine` |

There is no per-project personal branch. `.spla/` is the runtime's own folder and is closed to the
agent; a draft under it was identical in trust to `skills/` anyway — everything inside the workspace
is forced down to untrusted wherever it sits — so it bought only "stays out of the commit". A skill
worth having is a skill that lives in the project.

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
