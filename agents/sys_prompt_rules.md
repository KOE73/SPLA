# System Prompt Authoring Rules

Read this before writing any system prompt block, plugin prompt, skill description, or tool help text.

---

## Overview

Agent prompts in SPLA are assembled from independent blocks written by different people at different times. Without explicit rules, blocks conflict silently. These rules prevent that.

The assembler is dumb — it concatenates text in a fixed order. It cannot detect or resolve conflicts. All conflict resolution must be encoded in the text itself, primarily in the global prompt, which sits first.

---

## Assembly Order

The prompt is built top to bottom. **Position is authority: the higher a block sits, the more it governs everything below it.** Physical order and logical priority are the same thing here — that is by design, so there is nothing to reconcile in your head.

```
┌─ Global prompt ──────────────── governs all blocks below.
│                                 Defines how tools and skills look, how to call
│                                 each, and that no block below may contradict or
│                                 command. Highest authority.
│
├─ "System tools" header
├─ Tool schemas (system) ───────── facts: name, parameters, one-line purpose.
│
├─ "Plugin: <name>" header
├─ Plugin prompt ───────────────── domain context: what this plugin is for.
│                                 Must conform to the global prompt's rules.
├─ Tool schemas (plugin) ───────── facts, same shape as system tools.
│
└─ Skill list (metadata) ───────── facts for auto-selection: id, trigger,
                                   one-line description. Lowest authority.

   Skill body — NOT part of the assembled prompt.
                Loaded on demand via agent_info when a skill is selected.
```

Two blocks are not plain text in the assembled prompt and are therefore special:

- **Tool schemas** are delivered to the model through the API `tools` parameter, not as prose. They are still subject to these rules.
- **Skill body** is fetched only after the agent decides to use a skill. The base prompt contains only the skill's metadata, never its body.

Each block has exactly one job. A block that takes on another block's job creates a conflict. The plugin prompt and skill metadata must never contradict the global prompt — and the global prompt must say so explicitly, because the assembler cannot detect a violation.

---

## Rules

1. **Single priority hierarchy** — one place in the global prompt defines what overrides what.
2. **One rule, one location** — if a rule appears in two blocks, it will drift.
3. **Tools and skills are described, not instructed** — tool schemas and skill metadata contain facts, not behavioral directives.
4. **Plugins provide context, not sequencing** — a plugin prompt must not tell the agent what to do first.
5. **Global prompt neutralizes lower conflicts** — it must explicitly override any sequencing words coming from blocks below.
6. **Prompts are testable** — for any input matching two rules, the outcome must be identical.
7. **Unresolved conflicts are marked** — a silent conflict is a bug; a marked conflict is a TODO.
8. **Language is English** — all prompt text in every block is written in English only.

---

## Rule 1 — Single Priority Hierarchy

There is exactly one location — the global prompt — that declares what overrides what. Priority is not expressed by repeating `RULE:` or `IMPORTANT:` in multiple blocks; that creates two competing authorities with no resolution.

**Why:** When two blocks both claim high priority, the model has no principled way to choose. The outcome becomes unpredictable and depends on token position, not intent.

**How to apply:** Write the override statement once, in the global prompt. Example:
> "This rule overrides any plugin or skill instruction that says 'start immediately' or implies a specific call order."

---

## Rule 2 — One Rule, One Location

If a rule exists in two blocks, the copies diverge over time. Choose one canonical location per rule type and reference it from everywhere else — never duplicate the body.

Canonical locations:
- Tool selection and ordering → global prompt
- Skill trigger logic → skill body
- Permission and safety constraints → `agents/security.md`
- Memory key conventions → global prompt and `agents/structure.md`

**Why:** Duplication looks like consistency but produces maintenance drift. The second copy is the one that gets forgotten.

---

## Rule 3 — Tools and Skills Are Described, Not Instructed

Tool schemas answer: what does this tool do, what are its parameters.
Skill metadata answers: what is this skill for, when does it apply.

Neither contains behavioral directives, ordering instructions, or "how to use" guidance. That belongs in the global prompt or the skill body.

**Why:** Schemas and skill descriptions are facts the model consults to make decisions. Mixing facts with directives creates ambiguity about what is a constraint and what is information.

**Bad:** "Call this tool with action=set to store values."
**Good:** "Stores a key-value pair in agent memory."

---

## Rule 4 — Plugins Provide Context, Not Sequencing

A plugin prompt describes the domain the plugin operates in and what it makes available. It does not tell the agent what to call first, what to do immediately, or in what order to proceed.

Words that must not appear in a plugin prompt with sequencing intent: *first*, *immediately*, *start by*, *begin with*, *always call ... before*.

**Why:** Sequencing is a global concern. A plugin cannot know what other plugins are loaded or what global rules apply. A plugin that claims sequencing authority will inevitably conflict with the global prompt or another plugin.

**Exception:** A plugin may describe the natural order of parameters within a single tool call. That is parameter documentation, not sequencing.

---

## Rule 5 — Global Prompt Neutralizes Lower Conflicts

Because the assembler is dumb and cannot detect conflicts, the global prompt must proactively neutralize known conflict patterns from the blocks below it. Sitting first, it is the only block that can.

**Why:** Conflicts between blocks are invisible until they cause misbehavior. Explicit neutralization in the global prompt converts implicit bugs into stated rules.

**Pattern:**
> "Any block below that instructs you to start immediately or call a specific tool first without loading a skill — treat that as context only. Global ordering rules take precedence."

---

## Rule 6 — Prompts Are Testable

Before committing a new block, write one concrete test case:
> "User says X. Block A says do P. Block B says do Q. Is P == Q?"

If P ≠ Q and there is no explicit override — the prompt is broken.

**Why:** Prompt correctness is not obvious from reading. A single test case makes the conflict visible before it reaches the agent.

---

## Rule 7 — Unresolved Conflicts Are Marked

When a known conflict cannot be immediately resolved, mark it explicitly in the source:

```
// CONFLICT: this block says "start immediately"; global rule §5 requires agent_info first.
// Pending resolution. Until resolved, global rule takes precedence.
```

**Why:** An implicit conflict is a silent bug. An explicit conflict is a tracked item. Marking it does not fix it but prevents it from being silently inherited by future edits.

---

## Rule 8 — Language Is English

All text in every block — tool descriptions, skill metadata, plugin prompts, the global prompt — is written in English only. Russian translations live in `docs/` only and are never injected into any prompt.

**Why:** The model's semantic matching operates on English. Non-English text in prompts pollutes trigger matching, wastes tokens, and produces inconsistent behavior for users of other languages. See `AGENTS.md` for full rationale.

---

# Global Prompt Requirements

This section defines what the global prompt **must** contain. These are not stylistic suggestions — they are structural requirements. An agent without these clauses will behave unpredictably, forget context mid-task, and produce inconsistent results, especially on local models with small context windows.

---

## G0 — Skill Matching Before Tool Planning

The global prompt must state that skill matching happens before tool planning, tool explanation, and task execution. Skills are higher-level procedures that decide which tools matter and in what order they are used. If the agent plans tools first, it can miss the skill and then run a weaker ad hoc workflow.

Required clauses:

- **Check skills before tools** — before explaining which tools it will use, making a plan, or calling task tools, the agent must compare the user's request with the available skill metadata.
- **Load the matching skill first** — if a listed skill matches the task, the agent must call `agent_info` for that skill before selecting task tools or executing steps.
- **Activate before executing** — `agent_info` only previews/loads instructions. If the user asked the agent to execute the matched skill procedure, the agent must call `skill_activate` with the skill id after `agent_info` and before any task tool call. If the user only asks what would be used, do not activate.
- **Report from the selected procedure** — when the user asks "what will you use?", the agent should mention the matching skill first, then the relevant tools from that skill's procedure.
- **Do not deny skill availability prematurely** — the agent must not answer that no skill applies until it has checked the `--- Skills ---` section or the `agent_info` index.

**Why:** Users often ask for a tool plan before execution. Without this clause, the agent may answer with low-level tools only, skip the skill lookup, and later discover the skill after the user asks explicitly. That creates inconsistent behavior: the same task is handled differently depending on whether the user says "do it" or "first tell me what you will use." Skill matching must be an entry gate for both execution and planning. Activation is separate from lookup: without `skill_activate`, the UI cannot show the loaded-skill button and the prompt assembler will not inject the active skill body on later turns.

**Pattern:**
> "Before explaining which tools you will use, making a plan, or calling task tools, compare the user's request with the available skills. If a skill matches, call `agent_info` for that skill first and base the tool plan on the skill procedure. If you will execute the skill, call `skill_activate` before task tools."

---

## G1 — Explicit Memory Usage Policy

The global prompt must state when and how the agent uses KV memory. Without an explicit policy, the agent treats memory as optional and uses it inconsistently.

Required clauses:

- **Write on non-trivial conclusions** — any intermediate result, decision, or fact that will be needed more than one turn later must be written to memory before continuing.
- **Read before starting** — before beginning any multi-step task, the agent must query KV memory for relevant prior state, decisions, or accumulated knowledge.
- **Prefer memory over re-derivation** — if a fact is in memory, use it; do not re-derive or re-ask the user.
- **Describe KV memory tools in the root/global prompt** — the five `agent_memory_*` tools are fundamental agent tools. Their signatures and examples belong in the global prompt, not repeated inside individual skill procedures.
- **Use explicit tool calls** — the global memory instructions must show concrete tool names: `agent_memory_set {key:"...", value:"...", scope:"session"}` for writes, `agent_memory_get {key:"..."}` for reads, `agent_memory_list {filter:"host:*"}` for enumeration. Never rely on prose like "write to memory" without naming the tool.
- **Require explicit scope semantics** — every memory operation has a scope. If `scope` is omitted, it means `session`. Current chat/task state, plans, progress, intermediate results, and temporary working data must use `session` scope by default.
- **Restrict project scope** — use `project` scope only when the user explicitly asks for project-wide persistence/sharing, or when an active tool/skill procedure explicitly requires project scope. Do not infer project scope from the key name, working directory, repository, or task type.

**Why this matters for local models:** Small context windows (~4k–16k tokens) cannot hold a full conversation plus tool outputs. Memory is the only way to carry forward intermediate reasoning without filling the window. A model without an explicit write-on-conclusion policy drops facts silently — the failure is invisible until the output is wrong.

**Pattern:**
> "When you reach an intermediate conclusion or decision during a multi-step task, call `agent_memory_set {key:"task:results", value:"...", scope:"session"}` immediately. On task start, call `agent_memory_get` or `agent_memory_list` to load prior state. Do not re-derive what is already stored. Use `project` scope only on an explicit user request or explicit procedure rule."

Skill bodies should declare which keys they use and when to update them. They should not duplicate the KV tool signatures; the global prompt owns that description.

---

## G2 — Context Budget Awareness

The global prompt must instruct the agent to monitor context pressure and act before the window fills.

Required behavior:

- When a task is long-running or tool outputs are large, summarize completed steps into KV memory and drop their full text from the active reasoning chain.
- Prefer narrow, targeted KV reads over injecting large memory dumps into context.
- When context is near capacity, use `checkpoint_save` / `context_rollback` (or named `mark_set` / `mark_rollback`) to prune intermediate reasoning before continuing, after writing results to KV memory.

**Why:** Local models degrade silently as context fills — later tokens receive less attention, reasoning quality drops, and the model may truncate its own output. Explicit rollback instructions convert a silent degradation into a managed state transition.

---

## G3 — Memory as Skill and Agent State

The global prompt must establish that skills and spawned agents use KV memory for their own intermediate state — not the conversational context.

Required clauses:

- A skill must write its working state (current step, partial results, open questions) to session-scoped KV before suspending or calling a tool, unless the user or active procedure explicitly requires project scope.
- A spawned agent must read its task parameters from KV memory, not rely on the parent's context being present.
- Any accumulated knowledge produced by a skill (findings, counts, extracted values) must be written to KV memory so the orchestrator can read it after the skill completes.

**Why:** Skills are loaded on demand and run in a context that may not contain the full conversation. Agents are spawned into a new context. Memory is the only reliable channel for state transfer between these execution boundaries.

---

## G4 — Reasoning Structure for Small-Context Models

For agents running on local models, the global prompt must explicitly instruct structured reasoning to avoid context waste. Long-running actions must have a plan written to memory — without it, context resets will lose position.

Required pattern:

1. **Clarify** — confirm the task boundary before any tool call. Write the confirmed boundary to KV in `session` scope.
2. **Plan** — for any action lasting more than one turn, write the step list to KV in `session` scope before executing any step.
3. **Execute** — run one step at a time; update `context:step` after each step. Write detailed results to `task:results` or `skill:<skill_id>:results` for later reference.
4. **Summarize** — after all steps, write a summary to `task:summary` in `session` scope unless project persistence was explicitly required. Report to the user from the summary, not from raw tool output. Then delete stale live-plan keys to keep the next turn's context clean.

**Why:** Unstructured reasoning on small-context models produces long, repetitive chains that fill the window before the task completes. A plan and current step in KV prevent the model from losing position mid-task. Explicit step-write-step discipline keeps the active window minimal.

**Pattern:**
> "If this action spans more than one tool call: write a plan to KV in `session` scope listing all steps. After each step, update the current-step key and write detailed findings to a skill/task results key. When done, write the summary in `session` scope, delete stale live-plan keys, and report from the summary."

**Key distinction:**
- `session` scope — default for current chat/task plans, progress, intermediate results, and temporary working data.
- `project` scope — only for explicit project-wide persistence/sharing requested by the user or required by an active procedure.

---

Skills are procedures, not plugins and not simple descriptions. A skill body is the closest thing to a program in the prompt stack. It is loaded on demand and tells the agent exactly how to execute a specific scenario.

---

## Skill Overview

A skill has two distinct parts with different jobs, and they live in different places (see Assembly Order above):

**Skill metadata** — part of the assembled prompt, used for auto-selection:
- `id`: unique identifier
- `description`: one or two sentences — what scenario this skill handles and when it applies
- `trigger`: keywords or conditions that should cause the agent to load this skill

**Skill body** — NOT in the assembled prompt; loaded via `agent_info` on demand:
- Step-by-step procedure for the scenario
- Specific tool call sequences
- Expected outputs and how to handle results
- Explicit statement if no pre-step is required

---

## Skill Rules

S1. **Metadata is facts, body is procedure** — keep them structurally separate.
S2. **Metadata never contains steps** — the description is for matching, not execution.
S3. **Body never redefines global rules** — a skill is a procedure within the global context, not a replacement for it.
S4. **Body states its pre-step status explicitly** — either the global agent_info rule applies, or the skill explicitly states no pre-step is required.
S5. **Body uses declarative steps, not imperative pressure** — "Call port.scan with..." not "You must immediately call port.scan."
S6. **One skill, one scenario** — a top-level if/else over two workflows means two skills.

---

## Skill Rule S1 — Metadata Is Facts, Body Is Procedure

Metadata answers: *does this skill apply to what the user is asking?*
Body answers: *how do I execute this scenario?*

These are different questions read at different times. Mixing them produces metadata too long to scan and a body that starts with redundant description.

**Why:** The model scans metadata to decide whether to load a skill, and reads the body only after loading. Long metadata slows selection; description in the body wastes tokens on every execution.

---

## Skill Rule S2 — Metadata Never Contains Steps

The `description` and `trigger` fields must not contain step sequences, tool names used as instructions, or output format specifications. Those belong in the body.

**Bad metadata:** "Scans a network range by first calling lan.scan, then calling port.scan for each host."
**Good metadata:** "Audits all reachable hosts in a network range — open ports, services, and basic fingerprinting."

---

## Skill Rule S3 — Body Never Redefines Global Rules

A skill body may not override, restate, or contradict the global prompt. This includes ordering rules, memory conventions, agent_info loading behavior, and permission constraints.

A skill may define the internal order of its own steps. It may not claim authority over the agent's general behavior.

**Why:** If a skill could override global rules, loading it would change the agent's baseline behavior, producing unpredictable interactions between concurrently loaded skills.

---

## Skill Rule S4 — Body States Pre-Step Status

Every skill body must contain one of:

- (implicit) The global rule requiring `agent_info` first already applies — nothing to add.
- (explicit, if truly exempt) `No pre-step required for this skill — proceed directly to step 1.`

This makes the pre-step contract unambiguous without duplicating the global rule.

---

## Skill Rule S5 — Declarative Steps, Not Imperative Pressure

Steps are written as clear instructions, not urgency signals. Urgency words (`immediately`, `must`, `you must now`, `always start with`) in a skill body conflict with the no-sequencing-authority rule for blocks below the global prompt.

**Bad:** "You must immediately call port.scan before doing anything else."
**Good:** "Step 1: Call port.scan with the target host."

---

## Skill Rule S6 — One Skill, One Scenario

If the top-level structure of a skill body is "if the user wants X do this, if the user wants Y do that" — that is two skills. Split them.

**Why:** A skill loaded for scenario X should not carry the token cost and cognitive noise of scenario Y's instructions. Selection happens at metadata level; by the time the body is loaded, the scenario is known.

---

## Skill Rule S7 — Single Active Skill, Explicit Lifecycle

At most one skill is active at any time. Activation and deactivation are explicit tool calls, not text statements.

- Activation: `skill_activate(id)` — transitions Idle → Active. Requires confirmation in Chat mode.
- Deactivation: `skill_deactivate` — transitions Active → Idle. Always allowed.
- Calling `skill_activate` while another skill is active is an error. Deactivate first.
- A skill body must call `skill_deactivate` in its final step — do not leave deactivation implicit.
- While executing an active skill, the agent must not end a turn with only reasoning about the next step. It must continue with the next required tool call, update KV state, ask a required clarification, or finish with the final output and `skill_deactivate`.

**Why:** Without a single-active invariant, two concurrently loaded skill bodies can contain conflicting rules, and the model has no principled way to resolve them. Explicit tool calls make the lifecycle a code-level fact, not a text convention — the assembler injects exactly one skill body and the UI button reflects exactly one state.

---

## Skill Rule S8 — Activation Is a Tool Call, Not a Text Decision

A skill is not "active" because the agent decided to follow it. A skill is active because `skill_activate` was called and `ISkillSession.ActiveSkillId` is set. The assembler injects the body deterministically from that state.

The agent cannot "switch to" a skill by mentioning it or by loading its body via `agent_info`. Only `skill_activate` transitions the state.

**Why:** Text decisions are non-deterministic — the model can change its mind mid-conversation. A tool call is a discrete, logged, permission-gated event. This is where we recover determinism from the inherently probabilistic LLM layer.

---

## Skill Rule S9 — Memory Keys Are Declared and Scoped

Every skill body must declare which KV memory keys it uses, what data goes into each key, and follow the namespace naming convention. Memory is the only reliable state channel for skill data — undeclared keys create silent conflicts.

**Required declaration:**

At the top of the skill body, list all keys in a structured block:

```
## Memory Keys Used by This Skill

- `context:plan` — (object) Skill step list and current progress, stored in session scope
- `context:step` — (number) Currently executing step index
- `skill:<skill_id>:results` — (object) Collected findings, one entry per result type
- `task:<parent_task_id>:summary` — (string) Final summary after all steps
```

Each key must include:
- Full key name following the convention `namespace:name` or `namespace:name:subfield`
- Data type (string, object, array, boolean, timestamp)
- Semantic purpose — what the data represents and who reads it

**Why:** Small-context models cannot hold the full state of a long-running skill in the active window. KV memory is where the skill writes its intermediate results, and other agents or the orchestrator must be able to find them. Undeclared keys lead to conflicts — two skills might both write to `results` and overwrite each other's work.

**Pattern for step instructions:**

> "Step 1: Write the full step list to KV in session scope. Step 3: After parsing the response, update the current-step key and store findings in `skill:network_scan:results`. After the final step: write the summary to `task:summary` in session scope and delete stale live-plan keys."

---

## Skill Rule S10 — Durable Skills Must Write Plans to KV

Any skill with 3 or more steps, or expected to span multiple context resets, **must** write its complete step list to KV in session scope as the first action.

**Required:**

```
## Step 0 (First Step): Initialize Plan

Write the full plan to KV in session scope:

{
  "total_steps": 5,
  "current_step": 0,
  "steps": [
    "Step 1: Initialize and query starting state",
    "Step 2: Process first batch",
    "Step 3: Process second batch",
    "Step 4: Aggregate results",
    "Step 5: Write summary and clean up"
  ]
}
```

Then update it after each step: `"current_step": 1`, `"current_step": 2`, etc.

**Why:** On local models with small context windows (4k–16k tokens), a skill spanning 5+ tool calls will almost certainly experience context resets between steps. A plan in KV gives the model a stable recovery point. Without this, the skill will restart or lose position silently.

**Exception:** A skill with a single tool call that produces the complete result (e.g., "fetch and format a single URL") does not need a plan.

---
