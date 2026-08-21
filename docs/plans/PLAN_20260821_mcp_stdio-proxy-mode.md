# PLAN 2026-08-21 — MCP: stdio proxy mode for a shared instance

## Problem

`SPLA.CLI.exe mcp` builds its own `AgentRuntime` and holds the project's writer
lease for the life of the pipe ([McpCommand.cs](../../src/apps/SPLA.CLI/Cli/McpCommand.cs)
— the "own body" deployment). A second MCP client pointed at the same project
folder cannot attach: it is not a second writer, and stdio has no address to
share.

Trigger: Antigravity ran `spla mcp` on a project and held it; a second client
(a ChatGPT-side agent, stdio-only, cannot be pointed at a URL) had nowhere to
connect — `endpoint: none`.

## What already exists (done 2026-08-21, see [[mcp-http-endpoint]])

`POST /mcp` on `spla serve` — a plain JSON-RPC request/response endpoint that
dispatches against the runtime `serve` already has open, so any client that
*can* be pointed at a URL now shares one writer instead of each taking its own
lease. This plan is about the case that endpoint does not cover: a client that
only speaks stdio and cannot be reconfigured to hit a URL.

## What this plan covers: the stdio bridge

A `spla mcp` mode that, instead of building its own `AgentRuntime`, discovers
an already-running instance for the project (via the same lock-file/registry
mechanism `spla ps`/`spla stop` use — see [[project-entry-and-instances]]) and
proxies stdio to it: reads a JSON-RPC line, forwards it to the running
instance's `/mcp` (or a dedicated internal channel), writes the line back.

Open questions to resolve before implementing:

1. **Discovery**: reuse `IInstanceRegistry`/the lock file directly, or shell
   out to the same resolution `spla ps` does? Should not duplicate the lookup
   logic.
2. **Transport to the running instance**: proxy over HTTP to its `/mcp` (reuses
   what's already built, simplest) vs. a lower-latency internal channel. HTTP
   is probably right — no new server-side surface needed.
3. **Fallback**: what happens when no instance is running? Two reasonable
   answers — fall back to today's "own body" behavior (build a runtime, take
   the lease), or refuse with a speaking error telling the caller to start one
   first. Needs a decision; falling back silently could surprise someone who
   *wanted* to share.
4. **Progress**: the proxied `/mcp` endpoint doesn't stream progress (see
   [[mcp-http-endpoint]]) — a proxied stdio client loses progress notifications
   it would have gotten talking to the "own body" mode directly. Acceptable,
   or does the bridge need its own SSE-over-`/ws` path to recover it?

## Not started

This is a plan only — owner review needed before implementation. Flagged by
the user as "запиши в план и перепроверит реализацию" (write it down, revisit
later; tokens were tight this session for a second architecture change).

Related: [[project-entry-and-instances]], [[service-architecture]].
