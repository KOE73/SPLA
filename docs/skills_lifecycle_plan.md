# Skill Lifecycle — Implementation Plan

Target executor: a Sonnet agent, one chunk per session. Each chunk is self-contained:
it compiles, has acceptance criteria, and does not require holding other chunks in context.

Read `agents/sys_prompt_rules.md` (layer model) and `agents/skills.md` (once Chunk 0 creates it)
before implementing. Honor `agents/data-ownership.md`: domain owns state, UI only observes.

## Design decisions (locked)

- **State**: a per-conversation `ActiveSkillId` lives in the domain (not in a ViewModel).
- **Activation = tool call**, the only deterministic state transition. The assembler reads state; it never guesses.
- **Single active skill**: at most one skill body injected at a time. No nesting.
- **Tool namespace `skill.*`**: `skill_activate`, `skill_deactivate`. Listing/body reuse the existing `agent_info` (do not duplicate — Rule 2).
  - `skill_activate` — gated by mode; in Chat mode requires a confirmation gate.
  - `skill_deactivate` — `ToolScope.Agent`, always allowed (stopping is always safe).
- **Confirmation before activation**: always, even with a single candidate (prevents "ran, user said no, re-selected, ran again").
- **agent_clarify**: general-purpose structured-question tool. Tool emits a structured request on a channel; UI/CLI render it (mirror the existing `ToolProgressScope` pattern). Used for skill disambiguation and any other clarification.
- **Deactivation triggers**: skill calls `skill_deactivate` in its final step; UI "Unload skill" button; autonomous run ends on completion.
- **agent_spawn**: `new Agent(mode)` instance with `skill` + `input`, runs headless to completion, returns result. Same code as the interactive agent, different entry.
- **Hot reload**: `SkillManager.Reload()` + FileSystemWatcher; deferred while a skill is active; plus an explicit command.

---

## Chunk 0 — Documentation (no code)

**Goal:** capture the architecture so later chunks have a reference.

**Files:**
- Create `agents/skills.md` — skill system architecture: lifecycle state machine (Idle → confirming → Active → Idle), the `skill.*` and `agent_clarify` tools, `agent_spawn`, hot reload, single-active-skill invariant, where state lives.
- Add a `## Skill Lifecycle` section to `agents/sys_prompt_rules.md` (and mirror in `docs/sys_prompt_rules_ru.md`): rules S7 "Single active skill, explicit lifecycle" and S8 "Activation is a tool call, deactivation is explicit".
- Update `AGENTS.md` — add link to `agents/skills.md`.
- Update `SPLA.slnx` — add `agents/skills.md` to `/agents/`, `docs/skills_lifecycle_plan.md` to `/docs/`.

**Acceptance:** docs exist, AGENTS.md links them, slnx lists them. No build impact.

---

## Chunk 1 — Activation state (domain)

**Goal:** one source of truth for the active skill, observable by assembler + UI + CLI.

**Files:**
- `SPLA.Domain/Agent/ISkillSession.cs` (new) — `string? ActiveSkillId { get; }`, `void Activate(string skillId)`, `void Deactivate()`, `event Action? Changed`.
- `SPLA.Domain/Agent/SkillSession.cs` (new) — trivial implementation, raises `Changed`.
- Wire one instance per chat (DI / construction in CLI `Program.cs` and UI `MainWindowViewModel`).

**Notes:** Do NOT store this in the KV store as a string — it needs change notification for the UI button. Keep it a dedicated observable domain object, alongside the KV stores.

**Acceptance:** unit test in `SPLA.Tests` — Activate sets id + raises Changed; Deactivate clears + raises Changed.

**Depends on:** none.

---

## Chunk 2 — `skill_activate` / `skill_deactivate` tools

**Goal:** deterministic state transitions exposed to the model.

**Files:**
- `SPLA.MCP.Core/Tools/SkillTool.cs` (new) — two `IMcpTool`s or one tool with an `action` param. Prefer two distinct tools for clear permissions:
  - `skill_activate` — param `id`. Validates via `SkillManager.Find`; on success `ISkillSession.Activate(id)`. `Effect=Write`, mode-gated (see below). Returns `ok: activated <id>` or `error: unknown skill <id>` with suggestions.
  - `skill_deactivate` — no params. `ISkillSession.Deactivate()`. `Scope=ToolScope.Agent` (always allowed). Returns `ok: deactivated`.
- Register both in `McpHost` wiring (CLI `Program.cs`, UI `MainWindowViewModel`) — same place `agent_info` is registered externally.

**Confirmation gate:** route `skill_activate` through the existing permission path so that in Chat mode it resolves to `PermissionResult.Ask`, triggering `McpHost.OnPermissionRequested`. Add the policy entry in `SPLA.MCP.Core/Permissions` (find where per-tool/per-mode rules live; mirror an existing Write tool that asks in Chat).

**Acceptance:** tests — activate sets `ISkillSession.ActiveSkillId`; deactivate clears it; unknown id returns error; activate in Chat mode returns `Ask`.

**Depends on:** Chunk 1.

---

## Chunk 3 — Assembler injection

**Goal:** when a skill is active, its body is injected deterministically with a marker; the global prompt explains how to treat it.

**Files:**
- `SPLA.Agent/SystemPromptBuilder.cs` — constructor gains `ISkillSession`. In `Build`, after the mode preamble / global rules and before plugin prompts, if `ActiveSkillId` is set:
  ```
  === ACTIVE SKILL: <id> ===
  <body from SkillManager.LoadBody>
  === END ACTIVE SKILL ===
  ```
  Add a one-time meta-description in the global section: how to treat the ACTIVE SKILL block (follow it as the current procedure; it does not override global ordering/safety rules; call `skill_deactivate` when done).
- `AppendSkills` — keep the on-demand list. The conflict-prone lines 94-95 ("call agent_info FIRST … overrides 'start immediately'") should be reconciled with the new model per `sys_prompt_rules.md` Rule 5 (global neutralization).

**Acceptance:** existing prompt tests updated; new test — prompt contains the ACTIVE SKILL block iff `ActiveSkillId` set. Wiring updated everywhere `SystemPromptBuilder` is constructed.

**Depends on:** Chunk 1.

---

## Chunk 4 — `agent_clarify` tool + channel

**Goal:** structured question → user picks → answer returned. Tool decoupled from rendering.

**Files:**
- `SPLA.Domain/Tools/` — a `ClarifyRequest` model (questions, each with options) + an async channel/scope mirroring `ToolProgressScope.cs` (`AsyncLocal` → callback). Define `ClarifyScope` + a callback the host attaches.
- `SPLA.MCP.Core/Tools/AgentClarifyTool.cs` (new) — `ToolScope.Agent`. Emits `ClarifyRequest` on the channel, awaits the answer, returns the chosen option(s) as text.
- Hosts attach a handler: UI shows a picker, CLI shows a numbered prompt. (CLI handler can be minimal here; full UI in Chunk 5.)

**Acceptance:** unit test with a fake handler that auto-answers; tool returns the selected option.

**Depends on:** none (parallel to 1-3). Pattern reference: `tool-progress-channel` memory note.

---

## Chunk 5 — UI: unload button + clarify picker

**Goal:** user-facing controls.

**Files:**
- `SPLA.UI.Avalonia/Views/Status/StatusView.axaml` + `StatusViewModel.cs` — "Unload skill" button bound to `ISkillSession.ActiveSkillId != null`; click invokes the `skill_deactivate` path. Subscribe to `ISkillSession.Changed`.
- Clarify picker view + VM rendering `ClarifyRequest`; CLI numbered prompt in `SPLA.CLI`.

**Acceptance:** `/verify` or manual — activate a skill, button appears; click unloads; clarify shows options and returns the pick.

**Depends on:** Chunks 1, 4.

---

## Chunk 6 — Hot reload

**Goal:** skill file changes picked up without restart.

**Files:**
- `SPLA.MCP.Core/Plugins/SkillManager.cs` — extract load into `Reload()` (re-run `LoadSkills` on the known directory; store the dir).
- A watcher service (host side) using `FileSystemWatcher` on the skills directories; debounce; if `ISkillSession.ActiveSkillId` set, defer reload until `Changed`→null.
- Explicit "Reload skills" command in UI + CLI.

**Acceptance:** add/edit a skill `.md`; after reload it appears in `agent_info` index. Reload deferred while a skill is active.

**Depends on:** Chunks 1, 2.

---

## Chunk 7 — `agent_spawn` (autonomous run)

**Goal:** headless agent instance that runs a skill to completion and returns the result.

**Files:**
- `SPLA.MCP.Core/Tools/AgentSpawnTool.cs` (new) — params `mode` (AgentMode enum), `skill` (id), `input`. Constructs a new agent instance (`new Agent(mode)` semantics via the agent core / `ConversationOrchestrator`), seeds context with `input`, activates `skill`, runs to completion, returns result text. In autonomous spawn, `agent_clarify` is skipped or answered from `input`.
- Autonomous deactivation = run completion (no UI button).

**Acceptance:** integration test — spawn runs a simple skill headless and returns a result; spawned `mode` can be stricter than the parent.

**Depends on:** Chunks 1, 2, 3 (and ideally 4 for clarify-skip behavior).

---

## Suggested order

`0 → 1 → 2 → 3` is the MVP (state → tools → assembler injection). Then `4 → 5` (clarify + UI),
`6` (hot reload), `7` (spawn). Chunk 4 can run in parallel with 1-3.
