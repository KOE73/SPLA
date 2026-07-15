Fundamental agent tools — context management (4 tools): the context-window counterpart to agent_memory. agent_memory persists state across rollbacks; these tools prune the conversation so it does not grow unbounded across a long loop.

How rollback works: when you call checkpoint_save or mark_set, an invisible label anchor is inserted into the conversation just before your current turn. When you call the matching rollback tool, everything after that label is discarded — your tool calls, reasoning, and results all disappear. The label itself stays but is never sent to the LLM. After rollback the orchestrator always injects a synthetic user message as an explicit call to action: "[Rolled back to checkpoint/mark]\n{resume}". KV memory is never affected by rollback — it is the only state that survives.

Two mechanisms — use whichever fits:

1. Unnamed checkpoint stack — `checkpoint_save {resume?}` / `context_rollback`: supports nesting. checkpoint_save inserts a label and pushes it; context_rollback pops the top label and truncates back to it. Loop shape: call `checkpoint_save` BEFORE processing one item → do the work → write ALL results to KV → call `context_rollback` ONCE at the end of that item. Skip rollback on the LAST item. Stack supports nested loops naturally — each save/rollback pair is independent. Each iteration inserts a new label (tiny, invisible to LLM).

2. Named marks — `mark_set {name, resume?}` / `mark_rollback {name}`: for complex workflows needing stable named re-entry points. mark_set inserts a label with a name; mark_rollback always returns to that same label. The same label is reused — no accumulation across iterations.

`resume` parameter: write a note to yourself that will appear after rollback — describe exactly what to do next (e.g. "Process next host from KV key loop:hosts. Write result to host:{ip}. Then call context_rollback."). Without resume the synthetic message is just "[Rolled back to checkpoint]".

Critical rules: write every result to KV BEFORE calling rollback; never call rollback mid-task before KV writes are done; skip rollback on the last iteration so the loop finishes naturally. Without context pruning a long loop fills the context window and the model degrades.
