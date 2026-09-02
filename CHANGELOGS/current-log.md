# Log — unreleased

The detailed record, appended to as work happens and grouped by date. Never reordered, never
rewritten — this is the working log, not a narrative. Frozen into `CHANGELOGS/<version>.md` on
release, then started empty again.

An entry earns its place if someone outside your own head would notice the change. Time spent is
not the test: an hour's fix that changes visible behaviour gets an entry, two days of refactoring
that changes nothing observable does not. See the changelog rules in `AGENTS.md`.

Each entry is **a bold sentence saying what changed**, optionally followed by the detail. The bold
sentences are what `current-list.md` is built from, which is why they have to stand on their own.

> **Entries through 2026-09-01 were released as `v0.2.5`** and are frozen in
> [`CHANGELOGS/v0.2.5.md`](v0.2.5.md). Everything here is unreleased.

---


## 2026-09-02

- **A sub-agent can be spawned as a named role, and the role is a boundary rather than a request.**
  A role is the settings type for an actor: its body lives in `roles/<name>.yaml` beside the manifest
  and travels in git, and `agent:` in the manifest is read as the default role, so existing projects
  mean exactly what they meant before. The manifest names the roles it accepts — a role file that
  arrives in a pull request and that nobody named does not act, which is the same rule as "no walking
  up the tree" and for the same reason. `agent_spawn` and `agent_spawn_batch` take an optional
  `role`; an unknown one is refused with the available names listed rather than quietly falling back
  to the default, because a silent fallback runs the work with the wrong capabilities and looks like
  success. When a role is named its mode governs the run and its tool selection actually narrows what
  the run is offered — a role a caller could widen by passing `mode` would not be a boundary at all,
  and a reviewer told not to fix things needs the tools withheld, not the instruction repeated. The
  narrowing is built per run through the orchestrator's tool filter, so it never touches the shared
  registry every other chat reads.

- **A spawned run is now a real, readable session instead of an in-memory record that vanished on
  restart.** It gets what every chat already had — a chat id, a file on disk, an inbox, a progress
  tree, token accounting — and `subagent.get`/`subagent.result` read that file directly rather than a
  fixed-size in-memory ring, so a client can watch one mid-run (`outcome: "running"`) and not only
  after it finishes. It stays off the ordinary chat list — `chat.list` still shows human chats only —
  and is capped instead by `agent.spawned_retention` (200 by default): the newest finished spawned
  sessions are kept and older ones ring out, but a session with a run still in progress is never
  touched, however old.

- **Two actors can now hold a correspondence and write to each other by name, instead of one side
  only ever being a task and a result.** `agent_correspond` opens an address to another role; once
  open, the chat gets its own `reply_<role>[_<topic>]` tool for that one correspondence, and the tool
  name — not a chat id in the argument list, not a line in the prompt — is the address for as long as
  the correspondence lives. Calling it returns a delivery receipt, never the correspondent's answer:
  the reply is not blocked on it, and their actual words arrive later as an ordinary message on this
  chat's own turn. A correspondent that goes quiet (archived, deleted) is announced in the chat and its
  reply tool stops appearing, rather than the next call looking like a hallucinated tool name.

- **An exchange between two correspondents now decays instead of running forever.** Nothing stops a
  reply from prompting a reply — the failure mode is two actors that never run out of things to say
  to each other on their own token budget. Four settings under `agent:`
  (`peer_debounce_base`/`peer_debounce_max`/`peer_depth_ceiling`/`peer_hard_cap`, all overridable per
  role) govern a regulator in the turn pump: the wait before waking a turn for a reply doubles with
  how many replies have already gone back and forth since a human last spoke or a background task last
  finished, up to a ceiling past which a reply no longer wakes a turn of its own — it queues and rides
  whatever turn happens for some other reason, so the conversation slows to the pace of outside events
  rather than being cut off. A hard cap exists only as an emergency stop that should never actually
  fire.

- **The chat list is a tree, and a reply now renders as speech instead of as an ordinary message from
  a stranger.** A spawned session hangs under the chat that gave it its errand, tagged with its role,
  rather than being hidden from the list or dumped in among the human chats; a sessions panel lists
  what is running — role, parent, status, model, tokens spent — across the whole tree. An incoming
  reply across a correspondence renders "from `<role>`" in its own bubble the moment it lands, live,
  and an outgoing one renders "to `<role>`" with its delivery receipt hidden — the wire message is
  still an ordinary user turn underneath, unchanged, so only the client's rendering tells the two
  apart. Opening a spawned session in its own window stays a filter over one connection, never a
  second instance, and `ui.auto_open_subagents` (off by default) is the only thing that would ever pop
  one there by itself.
