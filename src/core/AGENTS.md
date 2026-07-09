# src/core — foundation layer

`SPLA.Domain`, `SPLA.MCP.Core`, `SPLA.Observability`. This is the most stable layer: everything
else depends on it, it depends on nothing above it. Read the root `AGENTS.md` first — this file is
the delta for this direction only.

## Invariants (do not break without a deliberate, reviewed decision)

- **No dependencies upward or outward.** Core must not reference ASP.NET, Avalonia, the LLM
  provider, any plugin, or the service/app layers. If you reach for one of those here, the type
  belongs in a higher layer, not in core.
- **Ambient scopes are the only channel for per-chat context.** `AgentSessionScope`,
  `ToolHostScope`, `ClarifyScope`, `ProgressScope`, `PermissionScope` (in MCP.Core) carry state to
  tools without changing tool signatures. Do not add a competing static/global or thread it through
  method parameters — use the scope.
- **`IMcpTool` / `ISandbox` / `IWorkspace` / `ICapabilityGate` are contracts shared by every tool
  and plugin.** Changing their shape is a breaking change: it forces a sweep of all basic tools and
  every plugin. Treat such edits as an ADR-level change, not a local tweak.
- **The capability gate is where an action is *decided*.** Enforcement of what an agent may touch
  (read/write/execute/network) belongs at this layer or the host boundary — never as a check
  scattered in a single tool. See `docs/reviews/2026-07-09-fable5-security-review.md` for the target
  model (one gate, policy fed from the mode/permission layer).
- **Classification must be trustworthy.** A tool's `Scope`/`Effect`/`Risk` drives permission
  decisions; do not let a lower-trust component self-declare a privileged `Scope` (`Agent`/`Skill`)
  and have core believe it. Built-in classification is a core concern.

## When adding to core

- Prefer pure, unit-testable rules (see `ContextAssembler`, `PermissionManager`) over stateful
  services. Core is the layer with the least excuse for hidden state.
- Anything disk- or platform-specific (`MapToHostDirectory`, PowerShell shell) is a *seam*, not the
  contract — keep the interface backend-agnostic so the server/virtual backends slot in behind it.
