# Changes — unreleased

The scannable list: one line per change, no dates, no detail. Derived from the entry headings in
`current-log.md` rather than written independently, so the two cannot drift apart. Frozen into
`CHANGELOGS/<version>.md` on release, then started empty again.

This list and the summary are what CI puts into the release body; the full log stays in the
repository and is linked from it.

**Covers work since `v0.2.5`**, frozen in [`CHANGELOGS/v0.2.5.md`](v0.2.5.md).

---


- A sub-agent can be spawned as a named role, and the role is a boundary rather than a request.
- A spawned run is now a real, readable session instead of an in-memory record that vanished on restart.
- Two actors can now hold a correspondence and write to each other by name, instead of one side only ever being a task and a result.
- An exchange between two correspondents now decays instead of running forever.
- The chat list is a tree, and a reply now renders as speech instead of as an ordinary message from a stranger.
- A graph shows which actors talk to each other, and which one nobody answers.
- Opening an archived chat no longer quietly brings it back to life.
