# SPLA Agents Configuration

## Doctrine (read first — the frame every change must fit)

SPLA is **not a chat client for a model. It is an environment where an agent lives on a project
and acts within it through its own tools.** The bet, inverted from frontier agents: **move the
intelligence out of the model and into the tool.** A small local model should win not by being
smarter, but by acting as a **dispatcher** over narrow, typed, deterministic tools that each
collapse a long reasoning chain into one call. Such a tool stays valuable when a large model
later arrives — it becomes an accelerator and a determinism/safety layer instead of a crutch.

Guardrails for any work here:

- **Build only what frontier clients structurally cannot have.** Anything commodity already does
  well (chat chrome, themes, markdown) — borrow or ignore, don't reinvent.
- **The moat is curation + tool interface design, not tool count.** A junk drawer of 50 mediocre
  tools hurts a small model more than 8 sharp ones. Narrow the interface so a weak model *cannot*
  misuse it; digest the output so it doesn't blow the context; route bulk data by handle, not
  through the model's window.
- **The window is just a window.** Authority, permissions, secrets, and memory belong to the
  agent on the project, never to a client/UI.
- **Judge every new tool by:** *which reasoning chain does it extract from the model, and will it
  still pay off on a large model?* If neither — it's probably a junk-drawer feature, not a tool.

Full text: [`docs/Doctrine.en.md`](docs/Doctrine.en.md) · [`docs/Doctrine.ru.md`](docs/Doctrine.ru.md).
If a change doesn't advance this doctrine, question whether it should be built.

---

This file is the core and the index. Everything else lives in `agents/` and in the `AGENTS.md` next
to the code, and is read **when the task calls for it** — see the table below. The lines in the next
two sections are short forms kept here because they must hold even when nobody opens the file they
point to; that file is still the rule.

## Never optional

- **Git: do not commit, amend, push, merge, tag, or reset unless the user asked for it in the
  message you are answering.** Never merge, push, or open a pull request into `main` — a release is
  the owner's call. Stage explicit paths, never `git add -A` / `git add .`. Before deleting a branch,
  check nothing unique is lost. → [agents/git.md](agents/git.md)
- **Every push to `work`: in the same turn, ask the owner whether this batch is a release.** →
  [agents/release.md](agents/release.md#before-pushing-to-work-ask-whether-this-is-a-release)
- **Commit subjects, branch and PR titles are English, typed `<type>(<scope>): <subject>`** — the
  type decides what reaches the changelog. → [agents/git.md](agents/git.md#commit-messages-the-type-is-load-bearing)
- **Text that reaches a model (prompts, skill metadata, tool help) is English only.** →
  [agents/sys_prompt_rules.md](agents/sys_prompt_rules.md#rule-8--language-is-english)
- **An `ADR_` is never edited**; a changed decision gets a new ADR. →
  [agents/documentation.md](agents/documentation.md)

## At the start of a session

- State the current branch in one of your first sentences; run `git worktree list` and
  `git branch --no-merged work` and mention what they show. → [agents/git.md](agents/git.md)
- Check that every release tag has its `CHANGELOGS/<version>.md`. →
  [agents/release.md](agents/release.md#freezing-a-release)

## Read before you act

STOP-files: when the task matches the left column, read the file before the first edit — it is
authoritative over that code.

| When you are about to… | Read |
|---|---|
| commit, branch, merge, push, delete a branch, write a commit message | [agents/git.md](agents/git.md) |
| push to `work`; touch `CHANGELOGS/`, the version in `Directory.Build.props`, `.github/workflows/` | [agents/release.md](agents/release.md) |
| write or review C# | [agents/csharp.md](agents/csharp.md) |
| edit code under `src/<layer>/` | that layer's `AGENTS.md` (`src/core`, `src/agent`, `src/service`, `src/apps`, `src/plugins`) |
| edit `web/src` | [web/AGENTS.md](web/AGENTS.md) |
| create or move a file under `docs/`; merge a branch that touched `docs/` or `agents/`; translate | [agents/documentation.md](agents/documentation.md) |
| write Russian prose (ADR, plan, readme, diagram text, UI string) or translate a term of art | [agents/glossary.md](agents/glossary.md) |
| write any system prompt block, skill description, tool help text, or plugin prompt | [agents/sys_prompt_rules.md](agents/sys_prompt_rules.md) |
| touch `IAgentContributor`, `AgentContextComposer`, `SPLA.Agent/Composition`, the system prompt, the debug prompt view | [agents/composition.md](agents/composition.md) |
| touch `ToolSetRegistry`, `ToolSetSession`, tool gating in `McpHost`, the `toolset_*` tools, tool documentation | [agents/toolsets.md](agents/toolsets.md) |
| touch `SkillLibrary`, `SystemPromptBuilder`, skill tools (`skill_activate`, `skill_deactivate`, `agent_clarify`, `agent_spawn`), UI reflecting skill state | [agents/skills.md](agents/skills.md) |
| add any registry, flag, or discovery logic | [agents/data-ownership.md](agents/data-ownership.md) |
| touch a password, API key, token, private key, or connection string | [agents/secrets.md](agents/secrets.md) |
| add, rename, or remove a WebSocket message type, payload, or client bus event | [agents/protocol.md](agents/protocol.md) |
| touch `ChatFeed`, `ChatEvents`, a feed subscriber, `ChatFeedWireSubscriber`, how a turn's events leave the chat | [agents/chat-feed.md](agents/chat-feed.md) |
| change permission modes (`Chat`, `Research`, `Inspect`, `Edit`, `Agent`) | [agents/security.md](agents/security.md) |
| create or extend a plugin, name a tool | [agents/plugins.md](agents/plugins.md) |
| design tool arguments | [agents/tool-args.md](agents/tool-args.md) |
| build Avalonia UI | [agents/avalonia.md](agents/avalonia.md), [agents/ui-theming.md](agents/ui-theming.md) |
| add logging, tracing, or metrics | [agents/observability.md](agents/observability.md) |
| change the `.spla` project file format or the settings cascade | [agents/spla-file.md](agents/spla-file.md) |
| find where something lives in the solution | [agents/structure.md](agents/structure.md) |
