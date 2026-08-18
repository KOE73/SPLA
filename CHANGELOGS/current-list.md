# Changes — unreleased

The scannable list: one line per change, no dates, no detail. Derived from the entry headings in
`current-log.md` rather than written independently, so the two cannot drift apart. Frozen into
`CHANGELOGS/<version>.md` on release, then started empty again.

This list and the summary are what CI puts into the release body; the full log stays in the
repository and is linked from it.

---

### Added

- Continuous integration: solution build, .NET tests, web type-check, bundle and vitest, on every
  push to `work` and every pull request into `main`.
- Release automation: tag or manual run re-runs the checks, publishes apps and plugins, and attaches
  `SPLA.zip` to a GitHub release.
- MCP: SPLA tools served over stdio, usable by an external agent.
- Mounts: folders outside the project root, declared by name, each its own security zone.
- Multi-project service: several projects at once, with a project picker in the client.
- Server deployment: domain identity over NTLM, per-user file areas, group sharing.
- DPAPI secret store with explicit scopes and per-entry ACLs.
- Skill library as a project of its own, fed by declared sources, with a librarian that answers by
  subject and one that reads the question.
- SSH: live pty sessions, SFTP transfer, upload as the mirror of download.
- Roslyn plugin: build, run and test .NET projects as tools.
- Browser plugin: first wave of Playwright automation with a screencast panel.
- OneC: Vue configuration browser.
- Headless batch runner in the CLI, now on `Spectre.Console.Cli`.
- Loop guard against degenerate LLM generation.
- Reasoning lever driven by what the provider advertises.
- Branch stamp on published builds.

### Changed

- The agent runs as a service; windows, terminals and remote clients are its clients.
- Avalonia became a window manager over one web renderer; the parallel native chat was deleted.
- All settings moved into the web client as one tabbed surface.
- Projects became storage brokers handing out named buckets instead of holding files.
- Four hand-rolled path checks became one boundary; a call is a movement between zones.
- Connection keys became secret references the settings editor never sees.
- Tool calls went through a pipeline instead of eight hand-wired concerns.
- `ILLMService` became a middleware pipeline behind one gateway; providers dispatch by `provider`.
- Plugin panels moved from Avalonia to the web client.
- Projects reorganized into a layered `src/` tree; `SPLA.Runtime` extracted.
- `docs/` split by lifetime into ADR, PLAN and IDEA.
- Version scheme is now `0.<minor>.<build>`, with the build number assigned by CI.

### Fixed

- A running turn in one chat no longer locks the composer in another.
- A trust flag survives a reload instead of resetting at exit.
- Web dependencies install when the manifests change, not once per checkout.
- A publish no longer fails because `git` is missing.
- The SSH terminal follows the window instead of the size its pty was born with.
- An SSH session can no longer wedge on a marker that never prints.
- The project tree shows every file rather than an extension whitelist.

### Breaking

- A tool result is a `ToolResult`, not a string.
- A project's root is its manifest's own directory and cannot be moved.
- `.spla/skills` is gone; skills come from declared sources.
- `.spla` is no longer readable through the sandbox.
