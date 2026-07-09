# Skill System Architecture

Read this before touching `SkillManager`, `SystemPromptBuilder`, skill tool implementations, or any UI that reflects skill state.

---

## Overview

Skills are on-demand procedures. A skill body is loaded into the agent's context only when explicitly activated — it is never part of the base assembled prompt. This keeps the base prompt lean and prevents skill rules from conflicting with each other or with the global prompt.

The skill system has three layers:

- **Skill files** — markdown files with YAML frontmatter, loaded from plugin directories.
- **SkillManager** — discovers, parses, and serves skill metadata and bodies.
- **Skill lifecycle** — the runtime state machine that controls which skill (if any) is active.

---

## Skill File Format

```markdown
---
id: plugin.skill-name
description: One or two sentences for auto-selection matching.
enabled: true
preloaded: false
---

Skill body starts here. This is what the agent receives when the skill is activated.
Step 1: ...
Step 2: ...
```

Frontmatter is metadata. Body is the procedure. They are structurally separate (see `sys_prompt_rules.md` Rule S1).

---

## Lifecycle State Machine

```
         user request matches skill
                    │
                    ▼
              [ Idle ] ──────────────────────────────────┐
                    │                                     │
         agent calls skill_activate(id)                  │
                    │                                     │
                    ▼                                     │
         [ Confirming ] ← agent_clarify shown             │
                    │                                     │
         user confirms                                    │
                    │                                     │
                    ▼                                     │
             [ Active ] ── skill body injected into prompt│
                    │                                     │
      skill calls skill_deactivate                       │
      OR user clicks "Unload skill"                      │
      OR agent_spawn completes                           │
                    │                                     │
                    └────────────────────────────────────┘
```

**Single active skill invariant**: at most one skill is Active at any time. Calling `skill_activate` while another skill is Active is an error — the agent must call `skill_deactivate` first.

---

## Where Skill State Lives

`ISkillSession` — a domain interface owned by the conversation/agent core.

```csharp
// src/core/SPLA.Domain/Agent/ISkillSession.cs
string? ActiveSkillId { get; }
void Activate(string skillId);
void Deactivate();
event Action? Changed;
```

One instance per chat session. Lives alongside the KV stores in the domain, not in any ViewModel. `Changed` fires on every transition — UI and assembler subscribe to it.

This is a domain object, not a KV entry. It requires change notification for real-time UI updates (the "Unload skill" button) and for the assembler to rebuild the prompt after state changes.

---

## Skill Tools

### `skill_activate`

Activates a skill. Deterministic transition: Idle → Active.

- Validates the skill id via `SkillManager.Find`.
- Calls `ISkillSession.Activate(id)`.
- Gated by mode: in Chat mode routes through the confirmation gate (`PermissionResult.Ask` → `McpHost.OnPermissionRequested` → user confirms → activation proceeds). This is the mechanism that prevents "skill launches without user awareness."
- Returns `ok: activated <id>` or `error: unknown skill <id>` with suggestions.
- Calling while another skill is active returns `error: skill <current> is already active — deactivate first`.

### `skill_deactivate`

Deactivates the current skill. Deterministic transition: Active → Idle.

- `ToolScope.Agent` — always allowed in every mode. Stopping is always safe.
- No parameters. Returns `ok: deactivated <id>`.
- No-op if no skill is active (returns `ok: no active skill`).

### `agent_info` (existing)

Reused for skill body preview without activation. No new tool needed for skill discovery — `agent_info` already handles it. Do not duplicate (Rule 2 from `sys_prompt_rules.md`).

### `agent_clarify`

General-purpose structured question tool. Used as the confirmation gate before `skill_activate`, and for any other structured disambiguation the agent needs.

- Emits a `ClarifyRequest` (question + options) on an `AsyncLocal` channel (same pattern as `ToolProgressScope`).
- UI renders a picker; CLI renders a numbered prompt.
- Awaits the user's answer, returns the selected option.
- `ToolScope.Agent` — always allowed. It is an information-gathering step, not a side-effecting action.

---

## Assembly Order with Active Skill

When a skill is active, `SystemPromptBuilder` injects the skill body after the global rules and before plugin prompts:

```
[ Global prompt ]           ← governs all below; describes ACTIVE SKILL semantics
[ System tool schemas ]
[ Plugin: X header ]
[ Plugin X prompt ]
[ Plugin X tool schemas ]
[ === ACTIVE SKILL: id === ]   ← injected when ISkillSession.ActiveSkillId != null
[ skill body ]
[ === END ACTIVE SKILL === ]
[ Skill metadata list ]     ← on-demand list; excluded while a skill is active to reduce noise
```

The global prompt contains a standing description:
> "The ACTIVE SKILL block below is the current procedure. Follow its steps. It does not override global ordering or safety rules. Call `skill_deactivate` when the procedure is complete."

The skill body itself must not repeat this — it is stated once in the global prompt (Rule 2).

---

## `agent_spawn` — Autonomous Skill Run

Spawns a headless agent instance to run a skill to completion and return the result.

```
agent_spawn(mode: AgentMode, skill: string, input: object) → string
```

- Constructs `new Agent(mode)` — same code as the interactive agent, different entry point.
- Seeds context with `input`.
- Calls `skill_activate` internally (no confirmation gate in autonomous mode — the caller provides input).
- Runs the skill to completion.
- Returns the result when `skill_deactivate` is called by the skill's final step.
- The spawned agent's mode can be stricter than the parent's (`AgentMode.Edit` spawning an `AgentMode.Research` sub-agent is valid).

Autonomous deactivation: when `skill_deactivate` fires in a spawned agent, it signals completion — no UI button needed.

---

## Hot Reload

Skills are loaded from disk at startup. Hot reload allows changes to be picked up without restart.

**Mechanisms:**
- `SkillManager.Reload()` — re-scans skill directories, rebuilds the list.
- `FileSystemWatcher` on skill directories — fires `Reload()` on file changes, debounced.
- Explicit "Reload skills" command in UI and CLI.

**Safety rule:** if `ISkillSession.ActiveSkillId` is set when a reload is triggered, the reload is deferred until the skill is deactivated (`ISkillSession.Changed` → null).

---

## Permission Matrix

| Tool | ToolScope | Chat | Research | Inspect | Edit | Agent |
|---|---|---|---|---|---|---|
| `skill_activate` | Local | Ask | Deny | Ask | Allow | Allow |
| `skill_deactivate` | Agent | Allow | Allow | Allow | Allow | Allow |
| `agent_clarify` | Agent | Allow | Allow | Allow | Allow | Allow |
| `agent_spawn` | Agent | Ask | Deny | Deny | Allow | Allow |

"Ask" = routes through `McpHost.OnPermissionRequested` (user sees a confirmation dialog).
Exact permission rules live in `agents/security.md` and the `PermissionManager` implementation.
