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

## A unified address space for resources — one address, one verb set, any scheme

This is the largest piece of this cycle and the least visible from the outside, because it ships
**inert**: with `agent.unified_resources` off (the default), the system prompt is byte-for-byte what
it was before, and none of the model's existing tools change shape. Everything below exists so that
switching it on is a single flag, not a redesign.

**The problem it replaces.** Every scheme SPLA already talks to — the workspace filesystem, an SFTP
host, eventually a browser tab, a memory store — had grown its own one-off tool family (`fs_read`,
`ssh_download`, …), each with its own idea of what "read" and "list" mean. A model has to learn each
family separately, and a new backend means a new tool family rather than a new row in a table.

**The address.** `ResourceUri` (`scheme://authority/path`) is parsed by hand rather than through
`System.Uri`, because the BCL type normalizes as it parses — collapses `..`, lowercases the host,
rewrites percent-encoding — and a silently rewritten path is exactly the mechanism a path escapes its
boundary through. The type hands back the three parts as given; the provider is where getting that
judgment wrong would actually cost something. An opaque form (`blob:handle123`, no `//`) is supported
by the same parser for content with no path at all.

**The verbs, and the rule for adding one.** A base set — `Read`, `Exists`, `List` — is mandatory: a
provider that cannot do all three does not register at all, rather than registering and failing verb
calls at runtime (which just moves the discovery problem from "ask the registry" to "find out by
erroring"). An extended set — `Write`, `Delete`, `MakeDir` — is each its own interface
(`IResourceWriter`, `IResourceRemover`, `IResourceContainerMaker`), and support is read off the
provider's actual type (`provider is IResourceWriter`) rather than a self-reported flag, so a
declared capability cannot drift from a real one. The written acceptance rule for any future verb:
*if using it needs caveats, per-scheme exceptions, or a bag of options, it's a bad verb or a bad
schema, and it doesn't get added* — which is why there is no `Query` or `Search`: "name what's at
this address" is a verb, "find what matches this condition" is a query language, and `List` stays a
flat `ls` and nothing more.

**The registry is a lookup table, not a gate.** `ResourceRegistry` maps scheme to provider, one per
project, and classifies nothing — whether a call is an allowed movement between security zones stays
entirely the permission pipeline's job, the same one every other tool answers to. A second place that
can say "no" would be a second place that can disagree with the first.

**Two schemes ship:** `file://` over the existing `IWorkspace`, and `sftp://` over `SftpTransfer`
(which gained the ability to read a remote file into memory — it previously could not). Mounts
deliberately do not get their own scheme: the workspace's path space already routes `mnt/<name>/…`
before any path joining happens, so `file:///mnt/AAA/nginx.conf` already **is** the mount's logical
path, and inventing a second address for the same file is exactly what the mount resolver already
forbids at load time. `file://` lets a `PathBoundaryException` through uncaught, the same way the
existing `Fs*` tools do, so the pipeline turns it into a structured refusal rather than a raw error.

**Content type is not a scheme.** The instinct to add an `image://` scheme died on the browser case:
a tab can hand back a screenshot or its HTML, so a type-as-scheme design would need `image://tab1`
*and* `text://tab1` — two addresses for one source, and overlapping sets in `list()`. The address
would stop being an address. The web solved exactly this with a `Content-Type` header instead of a
URL scheme, so a read now returns `ResourceContent(byte[] Bytes, string ContentType)`. Three
different pieces know three different things and none of them is a universal sniffer in the middle:
the provider knows what it actually produced (a `browser://` provider doesn't guess — it made the
PNG itself), a static `ContentTypes.Resolve` guesses from what's available (a library, not a pipeline
stage), and the calling *tool* decides the byte's fate, since only it has the chat's context. The
registry itself never touches bytes — `TryResolve` hands back a provider and steps out of the way.

**A format-converter registry sits beside the scheme registry, same shape.** Register by `(source,
target)` MIME pair; one hop only, deliberately no path search across registered pairs — that
open-ended search is the exact mistake that rotted ImageMagick's delegate chain. `SPLA.Domain` reads
labels, never content: signatures and MIME types live in `Domain`, decoders and demuxers stay outside
it, so an eleven-project-deep shared assembly never grows a dependency for the sake of telling `rar`
from `7z`. `resource_read` takes an optional `as` — the *requested* type, distinct from the
*determined* one: the same `.docx` read as bytes gets a blob handle and stays out of context, read as
text gets decoded whole, and only the model choosing `as` knows which it needs right now. Omit it and
the default has to be safe on its own: text if it's text, a handle if it's binary, so the first `read`
on a large `.mp4` doesn't fill the window by accident. Three converters carry real traffic from day
one rather than leaving the registry empty: identity for `image/*` (what `image_view` already did,
now reached through the registry instead of dead-ending on "not a viewable image"), a UTF-8 decoder
that fails loudly on non-text bytes, and JSON→YAML — picked specifically because identity and a raw
decode don't exercise what a real transform has to get right (parsing, a different size, its own
failure mode).

**What earned its own verb-and-tool cost:** six `resource_*` tools (`read/exists/list/write/delete/
mkdir`), one per verb rather than one tool with a mode argument, because Effect/Risk differ per verb
and a single tool would have to declare the worst case across all six.

**What's written down but not built.** `memory://` is the strongest remaining candidate — routing the
memory tools' `scope` argument into the address itself would delete five copies of that flag — but it
touches five existing tools and is its own piece of work. A `blob:` *provider* has no code behind it
yet, only the address form. Video/audio addressing ("continuum" resources — a frame is a fragment
like `#t=12.5`, a window is a `from/step/count` triplet) is deliberately deferred: which frames are
needed depends on what the model already saw, which is a feedback loop, and a one-shot converter
cannot bake one in. A live measurement against LM Studio (`qwen3-vl-8b`) confirms today's actual
constraint: `video_url` gets a flat 400, only `text` and `image_url` are accepted, and a batch of
several `image_url` parts in one message does work.

## MCP gets an HTTP door

`spla serve` can now expose MCP over HTTP (`POST /mcp`, off by default) so multiple stdio-proxy
clients can share one running instance instead of each taking its own writer lease.

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
