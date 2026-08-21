<!-- covers: 2026-08-21 -->

# Summary — unreleased

The prose account of the current cycle: what changed and why it matters, organised by theme rather
than by date. Rewritten from scratch before each push — never appended to. On release it is frozen
into `CHANGELOGS/<version>.md` and this file starts empty again.

The `covers:` marker on the first line records the latest `current-log.md` date this text accounts
for. CI compares the two: if the log has moved on, this summary is stale and is left out of the
release rather than published as if it were current.

---

## What this cycle is about

Everything here landed after `v0.2.3`. Two threads run through it: **an agent's lifecycle stopped
being tied to a window** (an instance is found and reused rather than duplicated, a run's progress
and transcript survive past the tool call that started it), and **the run itself became legible** —
what a batch run actually cost, what a spawned sub-agent is doing right now, what a resource read
actually returned.

## Run reports say what actually happened

`spla chat run --show-statistic` (plus `-file` and `-format`) prints or writes a per-cell report:
the model that actually answered — not necessarily the one asked for, under `model: auto` or a cloud
substitution — the connection, endpoint, sampling and reasoning settings, token totals, timing, and
where the output went. It reports the reasoning lever as `reasoning_requested` next to
`reasoning_wire`, observed off the actual payload rather than a provider profile, so an undeclared
model correctly shows `(nothing sent)`. Two bugs surfaced while building this: the accounting stage
was rebuilding the turn result field-by-field and silently dropping provider signals, and token
accounting itself was wired into six different call sites by hand — moved into the LLM pipeline as
`TokenAccountingMiddleware`, so a spawned sub-agent (previously uncounted) is billed like everything
else. `AgentCallbacks.OnTokenUsage` is gone with it; `OnLlmTurn` carries the same information whole.

## One writer per project, and a hub that finds it

A project is now owned by exactly one live instance: `.spla/instance.json` is held open with writes
denied, which answers both "who has it" and "what address to talk to instead" — over SMB as well as
locally, so a share protects against two machines, not only two processes. An instance holds a
**lease**, not ownership: it lives while somebody is connected or work is in flight, and only lets go
once neither is true, so switching between projects no longer kills whichever one you left. A
question — permission or clarification — now outlives the window that asked it, so closing a window
no longer auto-denies whatever it was waiting on. A folder entered without a manifest fails instead
of silently inheriting machine defaults; `spla init` now asks for a launch profile explicitly.

**`spla hub`** gives a machine, or a network, one place to see what is running, since a lock file
alone cannot answer that across machines. It now knows about **participants**, not only agents — a
registration carries a role (agent, window, or hub) — which is what lets Open raise an existing
window instead of opening a duplicate, and lets closing a project reach its windows as well as its
agent. The hub can start agents too, through a host-provided spawner, closing the gap where a machine
with no desktop had no way to bring a project up. A **project manager web page**, served by the hub
itself, lists every project the machine remembers next to what is currently running, reachable from a
tray that now lives one-per-session rather than one-per-window. A window whose agent disappears shows
a banner instead of retrying forever in silence.

## Progress becomes a tree, and a spawn is no longer a black box

Tool progress used to be a single flat line per top-level call. It is now a **tree**: a spawned
sub-agent's tool calls land as children of the node that spawned them, reaching every existing
surface — CLI status line, native and web tool trees, and MCP's own `notifications/progress` — with
no per-tool wiring. Alongside it, a spawned run keeps its transcript (bounded, in memory) so a run
that came back with something odd can still be inspected, and a context-fill percentage rides every
tick so a long-running sub-agent reads as "filling up" rather than "hung". MCP callers get the same
visibility a native client has: `tools/call` opens a progress tree when the client asks for one, and
a call can be cancelled mid-flight instead of blocking the read loop. The **loop guard** — degenerate
repeat detection — is on by default for chats now, not only for spawned runs, closing a gap that had
it backwards from the start.

## Resources get a type, and MCP gets an HTTP door

A resource read now returns its content **and** its type (`ResourceContent(Bytes, ContentType)`), not
a bare byte array — paired with a format-converter registry that turns one MIME type into another in
a single hop, deliberately not a path search. Six `resource_*` tools expose the address space to the
model one verb at a time, sitting behind `agent.unified_resources` (default off, verified inert when
so). Separately, `spla serve` can now expose MCP over HTTP (`POST /mcp`, off by default) so multiple
stdio-proxy clients can share one running instance instead of each taking its own writer lease.

## Smaller fixes and polish

A distributed build was serving 404 for every page — the web client had never actually been
embedded in `SPLA.Service.dll`, only building correctly on a checkout that happened to fall back to
`web/dist` on disk; the build now fails outright if the bundle is missing from the assembly.
`SPLA.CLI.exe` ships self-contained like the desktop app, so an extracted zip no longer needs the
ASP.NET Core runtime installed for the service behind a window to start, and a service child that
dies on startup now reports its exit code and output instead of a bare 30-second health timeout
(raised to 120 for a first run unpacking from a zip). A chat save writes to a temp file and renames
it into place, closing a race where a concurrent read could see a truncated file. CLI help text is
pinned to English regardless of the machine's UI culture, argument parsing is strict (a misspelled
option now exits non-zero instead of being silently ignored), and OS-specific desktop code moved into
its own `SPLA.Platform` library. The Built-in tools settings panel now explains what each `core.*`
toggle actually registers.

## Breaking changes

- **`AgentCallbacks.OnTokenUsage` is gone.** `OnLlmTurn` carries the whole turn outcome; recording
  usage is the pipeline's job now.
- **`projectId` is gone from the wire envelope.** A project belongs to the connection; a second
  project is a second connection.
- **A folder without a manifest is no longer entered silently.** A non-interactive run there now
  fails and names `--init` instead of guessing a profile.
