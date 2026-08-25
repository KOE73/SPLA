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


## Git: a branch per piece of work, `work` integrates, `main` releases

**The owner routinely runs several agents on this repo at once.** One shared working tree cannot
hold that — uncommitted changes from one piece of work block another. So: a non-trivial piece of
work gets its own branch `<area>/<short-name>` (e.g. `security/connection-secrets`) in its own
`git worktree` under `.claude/worktrees/<name>`, not the shared checkout.

Those branches start from **`work`** and merge back into **`work`**. `work` is the integration
branch and carries the detailed history — every feature branch, every fix, every intermediate
commit. It is where the project actually lives day to day, and it is the branch a session is
normally on.

**`main` is not an integration branch.** It receives whole releases, not individual pieces of work:
the owner decides when a batch is worth releasing, opens a pull request `work → main` on GitHub, and
squash-merges it there. One squash = one commit on `main` = one release point, so `main` reads as a
list of releases rather than a transcript of how they were built. This replaces the older "merge
each branch into `main`" rule, which in turn had replaced "everything on main" — `main` stopped
being the merge target the moment releases got their own meaning.

**Never merge, push, or fast-forward anything into `main` yourself, and never open or merge the
pull request.** Not when the work is finished, not when its tests pass, not when the user says a
piece of work is done. Deciding that a release exists is the owner's call, taken on GitHub, and it
is deliberately outside what an agent does here. Merging a feature branch into `work` is a normal
action you may take **when asked**; `main` is not.

A quick fix that touches one or two files and is going to be committed in the same turn does not
need the branch/worktree ceremony — commit it on `work` and use judgement. When in doubt, branch; a
spurious branch costs a merge, a missing one costs someone else's uncommitted work.

**A push to `work` can be rejected as diverged even when nothing is actually lost.** The owner
merges pull requests on GitHub (including squash-merges) between sessions, which gives `origin/work`
commits your local branch never saw under those hashes — a normal, expected outcome of this
workflow, not a sign of conflicting work. Before asking the owner how to reconcile, check it
yourself: diff the subjects/content of the commits `origin/work` has that you don't
(`git log HEAD..origin/work`) against your own local history (`git log --all --grep=<key phrase>`,
or just read the diffs). If the content already exists locally under different hashes (rebase,
squash, or a same-change commit from elsewhere), say so and proceed to reconcile — merge or
force-push as the situation calls for — without making the owner re-derive what you can already see.
Only ask when the remote-only commits contain something genuinely not present locally.

**State the current branch near the start of a session — one of the first sentences, not buried.**
With several worktrees around, "which checkout is this" is not obvious from the chat alone, and the
cost of assuming wrong (editing on the wrong branch, merging the wrong thing) is high enough that a
one-line `git branch --show-current` up front is cheap insurance. Re-state it if the session switches
worktrees or branches mid-conversation — the same reasoning applies at that point, not just at
session start.

Alongside it, run `git worktree list` and `git branch --no-merged work` once and mention what they
show — what else is checked out, and what else has unmerged work — so the user does not have to ask
"what's out there" separately. Skip the mention only if both come back empty/trivial (just `work`,
nothing unmerged); a one-liner beats silence, but two empty tables are noise.

**On completion, and only when asked:** `git merge --no-ff <branch>` into **`work`**, then
immediately `git worktree remove` and `git branch -d`. Do not leave a merged branch/worktree lying
around — check `git branch --no-merged work` before assuming a branch is safe to drop, same as
before. Nothing about finishing a branch involves `main`.

**Do not commit, amend, push, tag, or reset anything unless the user asks for it in the message you
are answering.** Finishing a piece of work is not permission to record it. Neither is the work being
correct, tested, and obviously ready. This applies on a feature branch exactly as it does on `work`
— a branch is not a lower-stakes place to commit unasked. **Merging into `work` is its own action
and needs its own ask**, separate from the ask (if any) that authorized commits on the branch.
Anything aimed at `main` is not an ask you can satisfy at all (see above).

- A commit requested earlier authorizes **that** commit only. It does not stand for the next one, or
  for "everything from now on". If in doubt, you were not asked.
- Leave the work in the working tree and say what changed. The user decides when it becomes history.
- **Never stage with `git add -A` or `git add .`** when the tree holds changes you did not make —
  concurrent work by the user or another agent is normal here. Stage explicit paths, and check
  `git status` for files you never touched before every commit.
- Same rule for anything else that leaves the machine or is hard to undo: pushing, force-pushing,
  deleting branches, rewriting history.
- **Before deleting any branch, check what would be lost.** `git branch --no-merged work` and
  `git log work..<branch>`. A branch whose commits are all in `work` is free to delete; one with
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

## Commit messages: the type is load-bearing

`<type>(<scope>): <subject>`, with `!` before the colon for a breaking change —
`feat(tools)!: a tool result is a ToolResult, not a string`.

| Type | Use for | Reaches the changelog |
| --- | --- | --- |
| `feat` | new observable behaviour | yes |
| `fix` | corrected observable behaviour | yes |
| `refactor` | internal restructuring, nothing observable changes | no |
| `docs`, `test`, `chore`, `build` | documentation, tests, housekeeping, build plumbing | no |
| any type with `!` | breaking change | yes, under Breaking |

This used to be a habit — pleasant, optional, and unevenly followed. It is now **load-bearing**: the
type decides whether a change is written into `CHANGELOGS/`, so getting it wrong is how work goes
missing from a release rather than merely how a log looks untidy. Scope is the area (`tools`,
`llm`, `skills`, `security`, `web`, …), matching the zone names used in `docs/` filenames.

Subjects are English, like branch and pull-request titles. Prose documentation stays Russian.

## CI and releases: the release is decided in a commit, not by CI

Two workflows under `.github/workflows/`, both on `windows-latest` (`SPLA.Tests` targets
`net10.0-windows` and proves itself against a real `WindowsIdentity`; `PublishAll.ps1` is
PowerShell and registers a file association):

- **`ci.yml`** — pushes to `main`/`work`, and pull requests into `main`. Builds the solution, runs
  `SPLA.Tests`, runs the web client's vitest suite, and checks that the changelog summary is
  current. The web type-check and bundle come free with the solution build, which already shells out
  to npm through `Exec` targets. On `work` this is a signal; on a pull request into `main` it is the
  gate.
- **`release.yml`** — any push to `main`, or a manual run from the Actions tab. Under this
  branching model `main` receives nothing but releases (see above), so a push to it *is* the release
  decision, already taken when the pull request was squash-merged. There is no `paths` filter and no
  other trigger deciding whether "this counts" — every push to `main` is a release by definition.

**CI does not decide the version, does not write the changelog, and does not compose the release
body. All three are decided locally, before the push that makes them real, and CI only checks that
what was decided is actually there:**

| What | Decided where | CI's job |
| --- | --- | --- |
| Version number | `Directory.Build.props`, by hand, in the release commit | read it, refuse if that tag already exists |
| Frozen changelog | `CHANGELOGS/v<version>.md`, written in the release commit | refuse if it is missing |
| Release body | `CHANGELOGS/v<version>-notes.md`, written in the release commit | attach it verbatim (`generate_release_notes: false`) — CI does not assemble prose from `current-*.md` |
| Working changelog reset | `current-log.md` / `current-list.md` / `current-summary.md` emptied in the release commit | refuse if any still holds content past its preamble |

CI's role is a gate that either finds all four already true, or fails without touching anything.
Nothing it computes is allowed to change what gets published — that was exactly the failure mode
behind [`v0.2.4`](CHANGELOGS/v0.2.4.md): a `run_number` nobody could reproduce locally, and a release
body assembled from working files whose staleness check the actual damage slipped past (see
`ADR_20260818-3_deterministic-release-commit`, which supersedes
[`ADR_20260818-2_build_versioning-and-changelog`](docs/adr/ADR_20260818-2_build_versioning-and-changelog.md)
on this point).

**The version still has three parts**, `0.<minor>.<build>`, and Major/Minor are still moved by hand,
rarely, to mark a new chapter. What changed is `<build>`: it is now a plain number set by hand in the
same release commit as everything else in the table above, not `release.yml`'s `run_number`. A local
build with `PublishAll.cmd` still falls back to `0.<minor>.0` — the build number only means something
once it has been chosen for an actual release.

**Making a release, end to end:**

1. **Ask first — see "Before pushing to `work`" below.** Everything after this point only happens
   once the owner has said yes.
2. In one commit on `work`: bump `<build>` in `Directory.Build.props`; freeze `current-log.md` /
   `current-list.md` / `current-summary.md` into `CHANGELOGS/v<version>.md` and empty the three
   working files; write `CHANGELOGS/v<version>-notes.md` — the literal release body, not assembled by
   CI. Push.
3. The owner opens the `work → main` pull request and squash-merges it on GitHub, same as any other
   release — see "Git: a branch per piece of work" above. Nothing about this step changed.
4. The push to `main` runs `release.yml`. It checks the table above, runs the same tests as `ci.yml`,
   tags the `main` commit it is running against with the version from `Directory.Build.props`, runs
   `PublishAll.ps1 -VersionBuild <build>`, and creates the GitHub release from
   `CHANGELOGS/v<version>-notes.md` with `SPLA.zip` attached. If any check in the table fails, nothing
   is tagged or published.

The tag is created by the workflow, on the `main` commit it just built and tested — never by hand,
and never before that commit exists. This is different from step 2's local commit: that commit lives
on `work`, and a squash-merge gives it a new hash on `main`, so tagging it before the merge would tag
an object the release was never actually built from. **Do not tag and do not trigger a release**
yourself — same rule, and same reason, as not pushing to `main`.

### Before pushing to `work`: ask whether this is a release

**Every push to `work` — always, no exceptions for "just this once" — ask the owner in the same turn
whether this batch should become a release, before running `git push`.** Accumulating on `work`
without releasing is the normal state most of the time; the point of asking is that the owner decides
*when*, and a push is the last moment that decision is still open before the next session might not
think to raise it. A short one-line question is enough — "Push to `work`. Release this batch, or
keep accumulating?" — but it must be asked, not assumed either way. If the answer is yes, do step 2
above in the same push rather than a separate one; if the answer is no, push as normal and move on.

## Changelog: three working files under `CHANGELOGS/`

GitHub's generated notes list *merged pull requests*, and under this branching model there is
exactly one per release — the squash of `work → main`. Left alone, a release covering months of work
reads as a single line. So the notes are written, not generated:

| File | What it is | When it is written |
| --- | --- | --- |
| `current-log.md` | the detailed record, grouped by date | appended as work happens |
| `current-list.md` | one line per change, no dates | derived from the log's bold entry headings |
| `current-summary.md` | the prose account, by theme | rewritten from scratch **before each push** |

**What earns an entry:** something a person outside your own head would notice. Time spent is not
the test — an hour's fix that changes visible behaviour gets an entry, two days of refactoring that
changes nothing observable does not. In practice this is the commit-type table above.

**The list is derived, never authored twice.** Write the log entry as a bold sentence that stands on
its own, then take that sentence for the list. Two independently written files drift; a derived one
cannot.

**Rewrite `current-summary.md` before pushing to `work`.** Not on every log entry — nine rewrites out
of ten would have no reader, and the summary costs roughly fifty log entries to produce. The push is
the right moment to consider it because it is the boundary where a release becomes possible at all:
before it, nothing can be released; after it, anything can. Update the `<!-- covers: YYYY-MM-DD -->`
marker to the newest date in the log when you do.

That marker is what makes staleness checkable: `ci.yml` warns when it trails the log on a push to
`work`, so a stale summary is visible long before it could end up composed into anything. It is no
longer `release.yml`'s problem — the release body is a file written by hand in the release commit
(see "CI and releases" above), never assembled from `current-*.md` at publish time.

**Publishable content starts after the first `---`.** Everything above it in these files explains
how the file itself works. This still matters for readability, but nothing reads it out mechanically
any more — `CHANGELOGS/v<version>-notes.md` is written directly, by hand, in the release commit.

**Coverage is checked against `git log main..work`, never against conversation memory.** Multiple
people and multiple sessions commit to `work`; a session only knows what it did, not what landed
alongside it. Before opening the `work → main` pull request, diff the two branches and confirm every
`feat`/`fix`/`!` commit has a log entry — a commit that arrived from outside the current session is
exactly as reportable as one written in it, and the log has no way to notice a missing entry on its
own. This already happened once: a CLI flag landed in the same window as a CI change, from a
different piece of work, and was merged into `main..work` with no entry until the gap was caught
during PR review.

### Freezing a release

Freezing — merging the three working files into `CHANGELOGS/<version>.md` (sections `Summary`,
`Changes`, `Log`), emptying them again, and writing `CHANGELOGS/v<version>-notes.md` — is not a
follow-up step after the release. It **is** step 2 of "Making a release, end to end" above: it
happens locally, in the same commit that bumps the version, *before* the push that becomes the
release. CI checks that it happened; it never does it and never writes to `work`.

**Check whether a past release is missing its freeze at the start of a session, mechanically:** if
`git tag` holds a release tag with no matching `CHANGELOGS/<version>.md`, something was published
without this step — under the current algorithm that should be impossible (CI refuses to publish
without it), so treat it as a sign the release predates this rule, or that a manual run bypassed it.
Say so. This is a comparison, not a recollection — do not rely on having noticed the release, or on
being reminded.

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
  merging *any* branch into `work`, including one you did not author.

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
