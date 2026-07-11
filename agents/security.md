# SPLA Agent Modes & Capabilities

SPLA uses a 5-tier permission model. The mode can be switched per chat. This file describes the
**actual** decision logic in `PermissionManager` (`src/service/SPLA.MCP.Core/Permissions/PermissionManager.cs`)
and the mode-based tool visibility in `ToolModeFilter` (`src/agent/SPLA.Agent/ToolModeFilter.cs`).
Keep it in sync with those two files — they are the source of truth.

> **Note:** this describes the local/trusted enforcement today. The capability gate (`ICapabilityGate`)
> that will bound file/network access by path/host is still `AllowAllGate` everywhere, and the
> server deployment runs without an execution sandbox. See
> `docs/reviews/2026-07-09-fable5-security-review.md` for the gaps and the target model, and
> `docs/roadmap.md` (sections B/E) for the planned work.

## How a decision is made

For each tool call, `PermissionManager.CheckPermission(mode, toolMetadata, argsJson)` returns
`Allow` / `Ask` / `Deny`, evaluated in this order:

1. **`Scope == Agent`** (memory, `agent_info`, datetime, context) → **Allow in every mode**, including
   Chat. These touch only the agent's own state.
2. **`Scope == Skill`** (skill activation) → `Chat`/`Inspect` = Ask, `Edit`/`Agent` = Allow, otherwise
   Deny (Research and unknown). Skill *deactivation* uses `Scope.Agent` and is always allowed.
3. **Project policy override** (Settings → Agent → `perm_read`/`perm_write`/`perm_shell`/`perm_internet`
   = allow/deny/ask) → a hard floor/ceiling for that category, applied in **every** mode including
   Agent, and it wins over a session "remembered" grant.
4. **Session "remembered" decisions** — only consulted when `mode != Agent`. A remembered
   allow/deny for the tool (matched by tool name and either `*` or an exact-arguments match) decides.
5. **Mode matrix** (below).

## Modes (mode matrix)

`Scope ∈ { Agent, Skill, Project, Local, Internet, Shell }`, `Effect ∈ { Read, Write, Execute }`,
`Risk ∈ { Low, Medium, High, Danger }`.

### Chat
Deny everything except `Scope.Agent` (rule 1) and skill-activation Ask (rule 2).

### Research
- `Project`/`Local` + `Read` → Allow
- `Internet` → Allow
- everything else → Deny (no writes, no shell)

### Inspect
- `Project`/`Local` + `Read` → Allow
- `Internet` → **Ask**
- everything else → Deny

### Edit
- `Project` + `Read` → Allow; `Project` + `Write` → **Ask**
- `Local` + `Read` → Allow
- `Internet` → Allow
- `Shell`: `Read` → Allow; `Risk == Danger` → Deny; otherwise → **Ask**
- everything else → Deny

### Agent
- `Project` (read or write) → Allow
- `Internet` → Allow
- `Shell`: `Risk == Danger` → **Ask**; otherwise → **Allow**
- `Local`: `Read` → Allow; write → Ask
- anything else → Ask

## Tool visibility vs. enforcement

`ToolModeFilter` decides which tools are even *offered* to the model in a mode (Chat sees only
`Scope.Agent`; Internet needs Research/Edit/Agent; Read needs Inspect/Edit/Agent; Write needs
Edit/Agent; Execute needs Agent). This is a UX filter. **Enforcement is `PermissionManager` at
execution time** (`McpHost.ExecuteToolAsync`): a `Deny` returns an error to the model, an `Ask`
routes an interactive prompt via `PermissionScope` to the initiating client and blocks on the answer.

## Tool metadata (declared per tool)

| Tool | Scope | Effect | Risk |
|------|-------|--------|------|
| `system_list_files` | Project | Read | Low |
| `system_read_file` | Project | Read | Low |
| `system_search_text` | Project | Read | Low |
| `system_find_files` | Project | Read | Low |
| `system_create_file` | Project | Write | Medium |
| `system_write_file` | Project | Write | Medium |
| `system_patch_file` | Project | Write | Medium |
| `system_delete_file` | Project | Write | Medium |
| `system_run_shell` | Shell | Execute | High |
| `web_fetch` | Internet | Read | Low |
| `agent_*` (memory/info/context/datetime) | Agent | Read | Low |
| `skill_activate` | Skill | — | — |

## Caveats agents must know (not obvious from the matrix)

- **No tool currently declares `Risk == Danger`.** So in Agent mode the only "Ask on shell" branch
  never fires — `system_run_shell` (`Risk == High`), `roslyn_script_run`, and the
  `roslyn_project_build` / `roslyn_project_run` / `roslyn_project_test` tools (all `Scope.Shell`,
  `Risk == High`, invoking the `dotnet` SDK — a build or `dotnet run` executes arbitrary
  project/MSBuild code) are **auto-allowed in Agent mode without a prompt**. If you want a shell
  command to require confirmation in Agent mode, it must be classified `Danger` (or the mode logic
  must change).
- **`Scope == Agent` is unconditional Allow.** A tool that declares `Scope.Agent` runs in *every*
  mode, including Chat, with no prompt. Do not give a plugin tool `Scope.Agent`/`Scope.Skill` — those
  are core-only classifications (see `src/plugins/AGENTS.md`).
- **Remembered grants are runtime-scoped**, i.e. shared across all chats of a project (and, on a
  shared server project, across users). Exact-argument matching rarely re-matches, so a remembered
  grant is usually `*` (broad). Treat "remember" as coarse.
- **Project policy overrides win over Agent mode too** — an `perm_shell: ask` override makes shell
  Ask even in Agent, and `perm_write: deny` blocks writes in every mode.
