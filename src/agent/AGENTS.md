# src/agent — the agent loop

`SPLA.Agent` (orchestration, prompt assembly, spawned sub-agents) and `SPLA.MCP.BasicTools`
(built-in fs/shell/build/web tools). Read the root `AGENTS.md` first.

## Invariants

- **One agent loop.** `ConversationOrchestrator.RunAsync` is the single place the model is called,
  tools are run, and anti-loop/checkpoint rules apply. CLI, service, and spawned sub-agents all go
  through it. Do not fork a second loop for a new entry point — inject behaviour via
  `AgentCallbacks` / the orchestrator's `init` hooks instead.
- **One prompt assembly path.** `SystemPromptBuilder` produces the layered `PromptSegment` list that
  both the LLM prompt and the UI preview derive from — "what you see is what is sent." If you add a
  prompt source, add a segment; don't concatenate strings on the side. (Note: the per-chat active
  skill currently builds a prompt through a second path in `SpawnedAgentRunner` — converging these
  is a known refactor, not a licence to add a third.)
- **Tools reach the system only through `HostServices.Sandbox`.** A basic tool must not touch
  `System.IO`/`Process` directly — go through `IWorkspace`/`IShell`/`ICapabilityGate` so the server
  sandbox can replace the mechanism without editing the tool.
- **Tool results are the model's ground truth.** Today a result is a bare string and error detection
  is a substring heuristic in the orchestrator — when you touch this, move toward a structured
  result (`Ok`/payload/error-kind), don't add more string sniffing.

## Adding a basic tool

- Declare `Scope`/`Effect`/`Risk` honestly — they drive the permission matrix (`ToolModeFilter` +
  `PermissionManager`). A shell/execute tool is `Scope.Shell`; a file write is `Effect.Write`.
- Narrow the schema so a weak model *cannot* misuse it; route bulk output by handle
  (`DataChannel`/`output='blob'`), not through the model's context window (see the Doctrine).
