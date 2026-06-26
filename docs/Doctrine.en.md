# SPLA Doctrine

> Why this thing exists and which bet it serves. If a new feature doesn't advance this
> doctrine, it probably shouldn't be built.

## Thesis

**SPLA is not a chat client for a model. It is an environment where an agent lives on a
project and acts within it through its own tools.**

The first goal is the combination of a **local model** and a set of **task-specialized
tools**, such that:

1. a small model has an **easier** time (the task is simplified at the input, rather than the
   model being made smarter);
2. even once a larger model arrives later, the mass of specialized tools **remains** and keeps
   paying off;
3. tools are **extensible** — a bank that keeps accruing.

(And no accursed Python 🙂 — see below; under the joke there's a real engineering reason.)

## The bet, inverted from frontier agents

Codex / Claude Code / Antigravity optimize for **"best model + general tools"**: give the
frontier `bash`/`edit`/`grep` and rely on the model's intelligence to walk a long reasoning
chain by itself. That works because the model is huge.

SPLA bets the opposite: **move the intelligence out of the model and into the tool.** Not "give
the model bash and let it figure it out," but a narrow, typed tool (`sql_table_describe`,
`roslyn_compile_check`, `1c_*`) that **collapses twenty reasoning steps into one deterministic
call**. Competence lives in the tool, not in the weights. A small model doesn't need to be
smart — it needs to be a **dispatcher** that picks the right thick tool.

This is an under-explored seam: the big labs don't go there because they don't need to — their
model carries it. We have no frontier on hand, and out of that necessity comes an architecture
that will **outlive** the arrival of the frontier.

## Why the bet won't go stale

When a large model arrives, the tools **do not become garbage**. Frontier + a deterministic SQL
tool still beats frontier + raw bash: the tool removes a whole class of errors and burns less
context. The same asset pays off in both regimes:

- a **crutch for a weak model** — today;
- an **accelerator and a determinism/safety layer for a strong one** — tomorrow.

This is a rare property: most architectures are tuned to the current model generation and age
with it. This one doesn't.

## The blade: where the real work is

The danger isn't in the idea — it's that **"extensible tools" degenerate into a junk drawer**.
50 mediocre tools hurt a small model more than 8 sharp ones — it drowns in the choice. So the
real work is not "write more," but two things:

1. **Tool interface design.** Narrow and typed enough that a weak model **cannot** misuse it;
   output digested enough that it doesn't blow the context. The tool validates its own input and
   returns a "next step" hint, turning the model into a router.
   *(Bulk data travels by handle, past the context — not as JSON in the model's window; see
   data-channel.)*
2. **Curation and surfacing.** Which tools are even live in this chat. Per-chat modes, skill
   sessions, `context:` injection, progressive disclosure of tools per task — **that is the
   moat**, not the tool count.

**The metric.** Not the number of tools. The number of tasks where a small model **stopped
making mistakes** thanks to a thick, deterministic tool.

## Why .NET is not a matter of taste

For this specific bet, .NET is objectively better than Python:

- **Roslyn** — the model compiles and safely runs C#;
- **ALC** — real plugin isolation (down to native DLLs);
- **native interop** — 1C, SQL Server (SNI), system APIs;
- a single typed binary, no venv zoo.

For a **tool-heavy agent on the Windows enterprise stack**, this is the right substrate, not a
whim.

## The principle (hold it in mind for every feature)

> **Intelligence moves into the tool. The model is a dispatcher. Every sharp, deterministic tool
> is a piece of intelligence extracted from the model and deposited in a bank that pays interest
> both on the small model today and the large model tomorrow.**

Consequences for decisions:

- Anything commodity already does well (chat chrome, themes, markdown) — **borrow or ignore**,
  don't build. Build only what frontier clients structurally cannot have.
- Judge every new tool by: *"which reasoning chain does it extract from the model — and will it
  still pay off on the large model?"* If not, it may be a junk-drawer feature, not a tool.
- The window is just a window. Authority, permissions, secrets, memory belong to the agent on
  the project.
