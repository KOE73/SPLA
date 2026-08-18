<!-- covers: 2026-08-18 -->

# Summary — unreleased

The prose account of the current cycle: what changed and why it matters, organised by theme rather
than by date. Rewritten from scratch before each push — never appended to. On release it is frozen
into `CHANGELOGS/<version>.md` and this file starts empty again.

The `covers:` marker on the first line records the latest `current-log.md` date this text accounts
for. CI compares the two: if the log has moved on, this summary is stale and is left out of the
release rather than published as if it were current.

---

## The agent becomes a service

Two months of work, and one change underneath nearly all of it: SPLA stopped being a desktop
application that talks to a model and became **a service that clients connect to**. The desktop app
is now one of those clients. So are browser tabs, torn-off panels, the CLI, and — new in this
cycle — anything speaking MCP.

Everything below follows from that, or from the second theme: **things that were implicit became
explicit** — what a tool returns, where a file may be read from, where a secret lives, where a skill
comes from.

## The service and its clients

The agent runs behind a WebSocket protocol. A client is a window, and a window is a view onto a
chat, not an owner of one. The Avalonia app became a window manager over a single web renderer
instead of a second, parallel UI — the native chat implementation was deleted rather than kept in
sync.

Panels tear off into their own windows and keep following the chat they belong to. A change like
theme or density fans out over an event bus and reaches every open window at once. A client targets
a local or remote service through the same code path; "embedded" means the client starts the service
itself.

Chat state belongs to the chat. A running turn in one chat can no longer lock the composer in
another — a bug that came back twice before the state moved where it belongs.

## Settings moved into the web client

All of it, as one tabbed surface with sidebar navigation: LLM connections and models, agent mode and
permissions, plugins, appearance, skills. Reversible preferences apply as you change them; anything
transactional or dangerous still waits for an explicit save. The model picker is filterable and
resizable, and connections became a tree of connection → models rather than a flat list.

## Projects became storage brokers

A project no longer *holds* things; it *provides* places to put them. Chat images, telemetry logs
and plugin data all go through named buckets, which means the same agent core works against a local
folder, a server-side per-user area, or an in-memory backend without knowing which it has.

On top of that seam: multiple projects in one service, a project picker in the client, and a project
id on every chat-scoped message so nothing is addressed to "whatever is open".

**Mounts.** A project can declare folders outside its root as named mounts under `mnt/`. Each mount
is its own zone in the security model, so reaching outside the root is something you grant
deliberately rather than something that either works or does not.

## Security: zones, islands and edges

The security model stopped being a set of scattered checks and became a shape you can name.

Four hand-rolled path checks — which disagreed with each other in edge cases — became **one**
boundary. A call is treated as a **movement** from one zone to another, and it says which; grants
are on the edge, not on the caller. An island is defined by its substance, not by a label attached
to it. Data carries where it came from, and the chat remembers — a trust flag now survives a reload
instead of quietly resetting at exit. The sandbox seam reaches the chat, and `.spla` itself is no
longer readable through it.

## Secrets stop being fields

A DPAPI-backed secret store with per-entry ACLs and explicit scopes, and — deliberately — no way to
search it. Provider connection keys became *references*: the settings editor never receives the
value, only the pointer. Plugins get a host credential control instead of asking for a raw field.

## Skills: from plugin payload to a library

Skills used to arrive attached to plugins. Now they come from **declared sources**, and the library
is a project of its own. A librarian answers by subject and hands over cards, so the catalog stops
growing with the collection; behind the word-matching librarian there is now one that reads the
question. A skill is handed out with its appendices, and the permission gate is on taking the skill
on — not on each page of it. A person can hand a skill to a chat directly; a running skill is
pinned and can be ended.

## Tools: a call now has a contract

The largest internal change, and the one most visible to plugin authors. A tool returns a
**`ToolResult`**, not a string. A permission verdict is a pure function that comes with a reason,
and a refusal speaks in terms of what was denied rather than describing the host's plumbing. Eight
concerns that used to be wired in by hand became a pipeline; the outcome of a call reaches observers
and is covered by tests. A tool set is levelled by the user and raised by the chat. An **exposure
profile** decides what an outside caller is allowed to see — which is what made **MCP** possible:
SPLA tools are now served over stdio end to end, so an external agent can use them.

## The LLM path is a pipeline

`ILLMService` was replaced by a middleware pipeline behind a single gateway, with each stage named
and diagrammed. Providers became projects dispatched by `provider`, which is how LocalAI, OpenRouter
and LM Studio now coexist without special cases.

A **loop guard** catches degenerate, repeating generation in the output stage instead of letting it
run until the context ends. The **reasoning lever** is driven by what the provider actually
advertises rather than an assumed standard — there isn't one, and three providers disagreed about it
in measurable ways. OpenRouter context windows feed the status bar's occupancy pill, and the panel
says why a figure is missing instead of showing nothing.

## Plugins

**SSH** gained live pty sessions with streaming output, SFTP transfer staged through `.tar`
containers, upload as the true mirror of download, and a terminal that follows the window instead of
the size its pty was born with. **Roslyn** builds, runs and tests .NET projects as tools. **OneC**
got a Vue configuration browser; its dead Avalonia layer was removed. **Browser** landed the first
wave of Playwright automation with a screencast panel.

All plugin panels moved from Avalonia to the web client, so a plugin ships one UI, not two.

## Server, CLI and multi-user

Domain identity over NTLM, per-user file areas, group sharing, and a pluggable identity provider.
Projects can be created by name into the calling user's own area, and token/`Origin` authentication
gaps in the service were closed. The CLI moved to `Spectre.Console.Cli` and gained a headless batch
runner, so it can be driven by another program rather than only by a person.

## Build and release

This cycle is the first with **continuous integration**: every push to `work` and every pull request
into `main` builds the solution, runs the .NET tests, type-checks and bundles the web client, and
runs its vitest suite. **Releases are automated** — a tag or a manual run re-runs those checks
against the exact commit being released, publishes apps and plugins, and attaches `SPLA.zip` to a
GitHub release, removing its own tag again if the publish fails.

Alongside that, published builds stamp the branch they came from so an experimental build cannot be
mistaken for a working one, web dependencies install when the manifests change rather than once per
checkout, and a publish no longer fails just because `git` is missing.

## Structure and documentation

Projects were reorganized into a layered `src/` tree, `SPLA.Runtime` was extracted so a headless
worker can reference the runtime without the CLI, service or UI, and `docs/` was split by lifetime
into ADR (why), PLAN (how) and IDEA (maybe). Three demo workers show the boundaries hold —
`VisionAgent`, `LogSentry` and `Summarizer` — plus a `RemoteWeb` project demonstrating browser
automation over the LAN.

## Breaking changes

- **A tool result is a `ToolResult`, not a string.** Plugins returning strings must be updated.
- **A project's root is its manifest's own directory,** and nothing can move it. Use mounts to reach
  outside.
- **`.spla/skills` is gone.** Skills come from declared sources.
- **`.spla` is not readable through the sandbox** the way it used to be.
