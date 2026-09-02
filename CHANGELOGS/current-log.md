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
