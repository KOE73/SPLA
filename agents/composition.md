# Agent Context Composition

Read this before touching `IAgentContributor`, `AgentContextComposer`, any contributor under
`SPLA.Agent/Composition`, the system prompt, or the debug prompt view.

Why it is built this way: [`ADR_20260803_agent_typed-contributions`](../docs/adr/ADR_20260803_agent_typed-contributions.md).

---

## The shape

Everything that reaches the model as text comes from one mechanism:

```csharp
public interface IAgentContributor
{
    string Id { get; }                                            // "mode", "core", "skills", "plugins"
    AgentContribution Contribute(AgentContributionContext ctx);
}

public sealed record AgentContribution
{
    IReadOnlyList<ContextItem> Context { get; init; }
}

public sealed record ContextItem
{
    string           Source      { get; init; }   // which piece: a feature id, a skill id, a plugin id
    string           Title       { get; init; }
    string           Body        { get; init; }   // clean content
    string           Prefix      { get; init; }   // separators / machine headers
    string           Suffix      { get; init; }
    ContextPlacement Placement   { get; init; }   // SystemPrompt | TurnMessage
    string           Contributor { get; init; }   // stamped by the composer, never by the contributor
    string           Text        => Prefix + Body + Suffix;
}
```

Three rules hold this together:

- **One mechanism, several types of contribution.** `AgentContribution` carries `Context` today.
  Tools, tool middleware and policies will join as their **own types**, not as more context — "adds
  800 tokens" and "executes PowerShell" must stay distinguishable, because trust, permissions and
  context cost are all decided on that difference. A field is added when it has a producer and a
  consumer, not before.
- **Text is split, not pre-rendered.** `Prefix`/`Body`/`Suffix` is what lets anything downstream
  measure, group or shorten a body while the framing stays intact.
- **Attribution is stamped by the composer.** A contributor cannot claim to be someone else, which is
  what makes the manifest worth reading.

`AgentContributionContext` carries only settings and the working directory. Per-chat state (the
active skill, this chat's working memory) is resolved by the contributor through the ambient
`AgentSessionScope` — contributors are process-wide, chats are not.

Contributors are **synchronous**. Nothing here does I/O beyond reading an instruction file, and the
surface is recomposed on every iteration of the agent loop. A contributor that genuinely must await
(git state, a remote index) is the signal to add an async variant — not a reason to make every call
site await now.

---

## The contributors

Declared once, in order, in `AgentContributors.Default` (`SPLA.Agent/Composition`). Order is
authority order, top-down, and belongs to the composition root — never to a contributor.

| Id | Contributes | Conditional on |
|---|---|---|
| `mode` | the mode preamble | — |
| `core` | one item per enabled `IAgentFeature` that carries prompt text | the feature set |
| `instructions` | each existing instruction file from settings | — |
| `custom-prompt` | `agent.custom_prompt` | — |
| `skills` | active skill body, on-demand catalog (shelf + tag cloud) | `core.skills` |
| `toolsets` | one declaration line per set the agent may raise and has not | `core.toolsets` |
| `plugins` | each enabled plugin's own prompt | — |
| `plugin-commands` | the `plugin_run_command` list | — |
| `working-memory` | the live `context:*` snapshot, as a **turn message** | `core.memory` |

The conditional entries are gated on exactly the decision that gated their tools: a capability
that is off leaves **no contributor behind**, so the prompt can never describe a tool that was not
registered.

---

## Placement

| Placement | Where it goes |
|---|---|
| `SystemPrompt` | concatenated, in order, into the single system message |
| `TurnMessage` | its own system-role message, inserted after the prompt, never persisted |

Only working memory uses `TurnMessage`, and deliberately: the snapshot is worded as data rather than
instruction, and folding it into the prompt is what made weak models start "maintaining" it.

`ConversationOrchestrator` therefore takes **one** provider — `Context` — instead of separate prompt
and memory hooks. It delivers what the composer produced, addressed by placement, and knows nothing
about which sources of context exist.

---

## When it is composed

`ConversationOrchestrator.Context` is invoked on **every iteration** of the agent loop, inside the
turn's `AgentSessionScope`, and its result replaces the leading system message of the assembled list
(a fresh message — the stored conversation is never written to).

Per-iteration rather than per-turn because `skill_activate` is a move the model makes *mid-turn*: a
surface composed once before the loop would inject the procedure only from the user's next message,
by which point the model has already had to act without it.

A spawned sub-agent composes **once**, up front, with its skill session passed explicitly: it runs one
pinned procedure, cannot activate another, and must describe its own skill while running inside the
parent's async flow.

---

## The manifest

`CompositionManifest` is the report of what the surface is made of — one line per contribution:
contributor, source, title, placement, estimated size. It answers "why is this text in the prompt,
and what does it cost" without reading four classes.

Where it surfaces:

- **debug → prompt** (`DebugKinds.Prompt`): the list, with bodies expanding on click. Composed in the
  chat's scope when a chat is in focus, so the active skill is visible.
- **log, once at startup**: `SPLA.Agent.Composition` at Information — the line a service log has to
  carry.
- **log, per composition**: Debug (counts) and Trace (full table). Off in normal operation.

A contributor that throws does **not** take the turn down, and does not vanish either: the composer
logs a warning and writes a manifest line with the reason. Missing text with an explanation beats
missing text.

### Token figures are estimates, and are for attribution only

`TokenEstimate` is ~4 characters per token, defined once so two places cannot disagree.

**Nothing decides what to send based on it.** There is no per-contributor token budget and nothing is
truncated or dropped: a local budget can only guess, while the provider knows — the model's window
comes from its catalog (`IModelCatalogInfo` / `IModelManagementService`) and the real size of a
request comes back as `prompt_tokens`. We send what was composed; if it does not fit, the API says so.
The estimate exists to answer "which contributor is eating the window", where being off by ten per
cent changes nothing.

---

## Adding a source of context

1. Write an `IAgentContributor` in `SPLA.Agent/Composition` — one class, no core changes.
2. Add one line to `AgentContributors.Default`, in the right place in the order.
3. If it belongs to a `core.*` capability, gate it there on the same id that gates its tools.

If the prompt's wording changes as a result, the golden test
(`tests/SPLA.Tests/SystemPromptGoldenTests.cs`) will fail — that is the point. Delete the golden file
and run twice to re-approve, so the change is reviewed in a diff.
