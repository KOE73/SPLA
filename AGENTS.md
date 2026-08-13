# SPLA Agents Configuration

## Doctrine (read first — the frame every change must fit)

SPLA is **not a chat client for a model. It is an environment where an agent lives on a project
and acts within it through its own tools.** The bet, inverted from frontier agents: **move the
intelligence out of the model and into the tool.** A small local model should win not by being
smarter, but by acting as a **dispatcher** over narrow, typed, deterministic tools that each
collapse a long reasoning chain into one call. Such a tool stays valuable when a large model
later arrives — it becomes an accelerator and a determinism/safety layer instead of a crutch.

Guardrails for any work here:

- **Build only what frontier clients structurally cannot have.** Anything commodity already does
  well (chat chrome, themes, markdown) — borrow or ignore, don't reinvent.
- **The moat is curation + tool interface design, not tool count.** A junk drawer of 50 mediocre
  tools hurts a small model more than 8 sharp ones. Narrow the interface so a weak model *cannot*
  misuse it; digest the output so it doesn't blow the context; route bulk data by handle, not
  through the model's window.
- **The window is just a window.** Authority, permissions, secrets, and memory belong to the
  agent on the project, never to a client/UI.
- **Judge every new tool by:** *which reasoning chain does it extract from the model, and will it
  still pay off on a large model?* If neither — it's probably a junk-drawer feature, not a tool.

Full text: [`docs/Doctrine.en.md`](docs/Doctrine.en.md) · [`docs/Doctrine.ru.md`](docs/Doctrine.ru.md).
If a change doesn't advance this doctrine, question whether it should be built.

---

For comprehensive details on agent permission models, tool matrices, autonomy configurations, and documentation layout, refer to the agent documentation:

- **[Agent Security & Permission Modes](agents/security.md)**: Describes the 5 operational modes (`Chat`, `Research`, `Inspect`, `Edit`, `Agent`), allowed actions, and execution risks.
- **[Avalonia UI Development Rules](agents/avalonia.md)**: Mandatory structure rules for Avalonia UI, including the requirement to create non-trivial views in `AXAML` immediately.
- **[UI Theming & Density Guidelines](agents/ui-theming.md)**: Rules for UI styling, themes, spacing, and avoiding hardcoded layout properties.
- **[Observability](agents/observability.md)**: OpenTelemetry-ready logging, tracing, metrics, log destinations, and correlation rules.
- **[`.spla` Project File Format](agents/spla-file.md)**: Specification for the `.spla` project entry point — workspace, mode, instructions, permissions, and settings cascade.
- **[Project Structure](agents/structure.md)**: Overview of the solution layout and module responsibilities.
- **[Plugin System & Tool Naming](agents/plugins.md)**: Rules for creating plugins, extending the system prompt, and standardizing tool names (`[plugin].[domain].[action]`).
- **[Documentation Layout](agents/documentation.md)**: Where a document goes and how it is named. Documents are separated by how long they stay true, not by topic: `ADR_` decisions (never edited — they are the record of how the thinking evolved), `PLAN_` work plans, `IDEA_` notes for the future, `readme_` user guides, and `agents/` rules that must match the code. Naming is `GENRE_YYYYMMDD_zone_short-name.md`. Read before creating any file under `docs/`.
- **[Tool Sets: Levels & Activation](agents/toolsets.md)**: STOP — read this before touching `ToolSetRegistry`, `ToolSetSession`, tool gating in `McpHost`, the `toolset_*` tools, or tool documentation. A tool set is the unit the user levels and the agent activates; how much of it reaches the model follows from its level. There is no help tool: a tool's `Details` are disclosed with the tool itself.
- **[Data Ownership Rules](agents/data-ownership.md)**: STOP — read this before adding any registry, flag, or discovery logic. UI ViewModels must not own domain data. Violations cause data loss on restart, CLI blindness, and untestable state.
- **[System Prompt Authoring Rules](agents/sys_prompt_rules.md)**: STOP — read this before writing any system prompt block, skill description, tool help text, or plugin prompt. Defines how to avoid logical contradictions between rules. Russian translation: [`docs/sys_prompt_rules_ru.md`](docs/sys_prompt_rules_ru.md).
- **[Agent Context Composition](agents/composition.md)**: STOP — read this before touching `IAgentContributor`, `AgentContextComposer`, any contributor under `SPLA.Agent/Composition`, the system prompt, or the debug prompt view. Everything that reaches the model as text comes from one mechanism with typed contributions; the composition manifest says which contributor is answerable for which piece, and why there is deliberately no local token budget.
- **[Skill System Architecture](agents/skills.md)**: STOP — read this before touching `SkillLibrary`, `SystemPromptBuilder`, skill tool implementations (`skill_activate`, `skill_deactivate`, `agent_clarify`, `agent_spawn`), or any UI that reflects skill state. Defines the lifecycle state machine, assembly order, permission matrix, and hot reload behavior.
- **[Secrets, Credentials & API Keys](agents/secrets.md)**: STOP — read this before writing any code that touches a password, API key, token, private key, or connection string. The canonical policy: what the store is, how config references it (`secret:`/`env:`/`credential:`), when resolution happens, what may never cross the client/server boundary or enter the model's context, and the list of known open leaks. Every other mention in the repo is a one-line restatement plus a pointer here.
- **[Wire Protocol & Event Registry](agents/protocol.md)**: STOP — read this before adding, renaming, or removing any WebSocket message type, payload, or client bus event. Message names are soft strings on the JS side; this is the registry of every `MessageTypes` constant, payload, fan-out semantics, domain events (`ServiceEvents`), and client-local bus events that keeps both sides in sync.


## Git: a branch and a worktree per piece of work

**The owner routinely runs several agents on this repo at once.** One shared working tree cannot
hold that — uncommitted changes from one piece of work block another. So: a non-trivial piece of
work gets its own branch `<area>/<short-name>` (e.g. `security/connection-secrets`) in its own
`git worktree` under `.claude/worktrees/<name>`, not the shared checkout. `main` is a merge target,
never edited directly by a branch's own work — this replaces the older "everything on main" rule
that predates multi-agent use.

A quick fix that touches one or two files and is going to be committed in the same turn does not
need this ceremony — use judgement. When in doubt, branch; a spurious branch costs a merge, a
missing one costs someone else's uncommitted work.

**State the current branch near the start of a session — one of the first sentences, not buried.**
With several worktrees around, "which checkout is this" is not obvious from the chat alone, and the
cost of assuming wrong (editing on the wrong branch, merging the wrong thing) is high enough that a
one-line `git branch --show-current` up front is cheap insurance. Re-state it if the session switches
worktrees or branches mid-conversation — the same reasoning applies at that point, not just at
session start.

Alongside it, run `git worktree list` and `git branch --no-merged main` once and mention what they
show — what else is checked out, and what else has unmerged work — so the user does not have to ask
"what's out there" separately. Skip the mention only if both come back empty/trivial (just `main`,
nothing unmerged); a one-liner beats silence, but two empty tables are noise.

**On completion:** `git merge --no-ff <branch>` into `main`, then immediately `git worktree remove`
and `git branch -d`. Do not leave a merged branch/worktree lying around — check `git branch
--no-merged main` before assuming a branch is safe to drop, same as before.

**Do not commit, amend, push, tag, or reset anything unless the user asks for it in the message you
are answering.** Finishing a piece of work is not permission to record it. Neither is the work being
correct, tested, and obviously ready. This applies to work on a feature branch exactly as it applied
to `main` before — a branch is not a lower-stakes place to commit unasked. **Merging into `main` is
its own action and needs its own ask**, separate from the ask (if any) that authorized commits on
the branch.

- A commit requested earlier authorizes **that** commit only. It does not stand for the next one, or
  for "everything from now on". If in doubt, you were not asked.
- Leave the work in the working tree and say what changed. The user decides when it becomes history.
- **Never stage with `git add -A` or `git add .`** when the tree holds changes you did not make —
  concurrent work by the user or another agent is normal here. Stage explicit paths, and check
  `git status` for files you never touched before every commit.
- Same rule for anything else that leaves the machine or is hard to undo: pushing, force-pushing,
  deleting branches, rewriting history.
- **Before deleting any branch, check what would be lost.** `git branch --no-merged main` and
  `git log main..<branch>`. A branch whose commits are all in `main` is free to delete; one with
  unique commits is not — preserve it as a tag (`archive/<name>`), push the tag, and say what was in
  it. Never let a delete be the reason work disappears.

**If `git status` looks clean but a file is visibly new in the editor, suspect a stale untracked
cache before anything else.** `core.untrackedcache=true` is a per-clone, unversioned `.git/config`
setting — it does not travel with the repo. When enabled, `git status` can silently miss newly
created files while `git status <path>` (or `--untracked-files=all`) still finds them, and the IDE
(which watches the filesystem directly, not the git index) shows the truth. Fix per machine:
`git config core.untrackedcache false`. Do this once per clone if you hit the symptom; it is not a
repo-wide setting you can ship in tracked files.

**Do remind, though.** Silence while changes pile up is its own failure — a large mixed working tree
is hard to review and easy to lose. Say something (one line, not a nag) when:

- more than roughly ten files are uncommitted, or
- the changes span several areas at once (`src/`, `web/`, `docs/`, `agents/`, `tests/`), or
- a self-contained piece of work just built and passed its tests — the natural place to draw a line.

State what is uncommitted, in which areas, and offer to commit. Then wait.

## Docs across parallel branches (`docs/ideas`, `docs/plans`, `docs/adr`)

Branch-per-piece (above) solves code conflicts; these files fail differently — usually not a git
conflict at all, which is the dangerous case, since nothing forces anyone to notice.

- **Naming collisions.** `GENRE_YYYYMMDD_zone_short-name.md` (see
  [agents/documentation.md](agents/documentation.md)) already carries a `-N` suffix for same-day
  files (`IDEA_20260813-2_...`, `IDEA_20260813-4_...`) — use it. Before picking a slot, check the
  next free `-N` across **`git log --all`**, not just the current branch: two branches started the
  same day and merged later can otherwise both land on `-2` — git merges that cleanly as two
  distinct files with near-identical names, so the collision is invisible until a human reads them.
- **ADRs never get edited — including to resolve a conflict.** If a later ADR reaches a different
  conclusion than an earlier one, it says so explicitly ("supersedes ADR_YYYYMMDD_..."); it does not
  rewrite the old file's answer. This already follows from "ADR = record of how the thinking
  evolved", but the parallel-branch case is where forgetting it actually bites: two branches can
  each honestly believe their ADR is the current answer.
- **STOP-marked files under `agents/`** (protocol.md, secrets.md, toolsets.md, composition.md,
  skills.md, …) declare themselves authoritative over specific code. If your branch changed code a
  STOP-file governs, updating that file is part of the same merge, not a follow-up — a docs/code
  split that survives the merge is exactly the drift these files exist to prevent. Check this when
  merging *any* branch into `main`, including one you did not author.

## Web UI: Chat-Scoped State (recurring bug — do not regress)

The composer input, Send/Stop button, and every other per-conversation UI state belong to the
**current chat**, not the window. In `web/src` any such state MUST live in `store.ts` keyed by
`chatId` (e.g. `store.turnActiveByChat`) and be read via a computed over `store.currentChat`.
Never hold it in a component-local `ref` — it leaks across chat switches (a running turn in chat A
locked input in chat B, twice). Server events must be applied by `env.chatId` from the envelope,
never to whatever chat happens to be open.

## Translation Policy

Any file under `agents/` that is updated must have its Russian translation in `docs/` updated in the same commit.
Translation target: `docs/<same-name>_ru.md`.
Exception: files with no existing `_ru` counterpart do not require one unless explicitly requested.

## Skill & Plugin Authoring Language

**STOP: skill descriptions, trigger hints, and plugin prompts MUST be written in English only.**

Skills and plugin prompts are injected into the system prompt of a multilingual AI agent.
The model's vocabulary and semantic matching operate on English.
Adding text in any other language (Russian, German, French, etc.) pollutes the index,
wastes tokens, and breaks semantic search for other language users.

Rules:
- `description:` frontmatter in all `.md` skill files — English only.
- `default_prompt` / `custom_prompt` in `meta.yaml` plugin manifests — English only.
- `GetHelpText()` tool help bodies — English only.
- Trigger examples in skill bodies (`Run when the user asks...`) — English only.
- Do NOT add locale-specific keywords, phrases, or examples to any of the above.

The model handles multilingual input natively.
The system prompt is the contract layer — keep it language-neutral (English).

## Modern C# Language Usage

### Mandatory

Use the latest stable C# language features and .NET APIs available in the target project version.

Prefer concise language constructs that reduce code size, boilerplate and token usage while preserving readability and maintainability.

Actively use:

* Collection expressions (`[]`)
* Target-typed `new`
* Primary constructors where appropriate
* File-scoped namespaces
* Pattern matching and switch expressions
* Expression-bodied members
* `required` members
* `init` setters
* Collection initializers and spread operators
* `nameof`
* Raw string literals
* Inline `using` declarations
* Modern LINQ constructs
* `ArgumentNullException.ThrowIfNull`
* Static abstract interfaces when appropriate
* Record and record struct types where semantically correct
* Readonly structs and readonly members where beneficial

Avoid legacy syntax when a modern equivalent exists.

### Code Size

Minimize boilerplate.

Prefer shorter language constructs over verbose equivalents.

Do not generate code solely for stylistic consistency when the modern language provides a simpler alternative.

### Naming Quality (Critical)

Token savings MUST NEVER be achieved by shortening identifiers.

Names of:

* classes
* interfaces
* records
* structs
* methods
* properties
* fields
* local variables
* parameters
* generic type parameters

must be descriptive, explicit and self-documenting.

Bad:

```csharp
var d = Get();
var x = Process(d);
```

Good:

```csharp
var sourceImage = GetImage();
var detectionResults = ProcessDetections(sourceImage);
```

### Readability Rule

Prefer:

* shorter syntax
* fewer lines
* less boilerplate

while simultaneously keeping:

* semantic clarity
* explicit intent
* discoverability
* maintainability

If a shorter construct makes the code harder to understand, choose the clearer version.

### Generated Code Standard

Generated code should resemble code written by a senior modern C# developer in 2026:

* idiomatic
* concise
* allocation-aware
* maintainable
* production-ready

Use modern language features aggressively.

Use abbreviated syntax.

Never abbreviate business meaning.

## Imported Claude Cowork project instructions
