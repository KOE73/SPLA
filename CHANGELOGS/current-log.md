# Log — unreleased

The detailed record, appended to as work happens and grouped by date. Never reordered, never
rewritten — this is the working log, not a narrative. Frozen into `CHANGELOGS/<version>.md` on
release, then started empty again.

An entry earns its place if someone outside your own head would notice the change. Time spent is
not the test: an hour's fix that changes visible behaviour gets an entry, two days of refactoring
that changes nothing observable does not. See the changelog rules in `AGENTS.md`.

Each entry is **a bold sentence saying what changed**, optionally followed by the detail. The bold
sentences are what `current-list.md` is built from, which is why they have to stand on their own.

> **Entries through 2026-08-21 were released as `v0.2.4`** and are frozen in
> [`CHANGELOGS/v0.2.4.md`](v0.2.4.md). Everything here is unreleased.

---

## 2026-08-24

- **Word documents can be read for their meaning, and rows can be appended to a spreadsheet by
  column name.** A new `documents` plugin (native backend: Open XML SDK + ClosedXML, both MIT, no
  Office on the machine) adds `document_extract`, which turns a `.docx` into markdown, plain text or
  a typed JSON block tree — headings, paragraphs, list nesting, tables, page breaks and image
  references, with fonts, colours and tracked changes dropped — and three spreadsheet tools,
  `spreadsheet_inspect` / `spreadsheet_read_rows` / `spreadsheet_append_rows`, that address `.xlsx`
  and `.csv` by COLUMN HEADER rather than by cell address. An append writes under the last used row
  and leaves every other cell, format and formula as it was; an unknown column is refused with the
  sheet's real columns listed rather than silently added, a `.csv`'s delimiter and encoding are
  preserved, and numbers stay numbers. The same extraction is registered into the core converter
  registry as three `(docx → markdown | text | json)` pairs, so `resource_read … as: text/markdown`
  works on a `.docx` address wherever `agent.unified_resources` is on. Which document backend serves
  those pairs is a plugin folder, not a host decision — the semantic tree
  (`SPLA.Documents.Model`) travels inside each backend, and only bytes plus a MIME type cross the
  plugin boundary. A skill, `documents.docx-to-registry`, writes the procedure down: inspect
  the target sheet first, extract, map facts to columns without inventing any, append in one call,
  read the tail back.

- **A shell command that asks a question no longer hangs forever — the agent sees the question and
  answers it.** `system_run_shell` used to return only when the process exited, so anything that
  stopped to ask (`ffmpeg` on `Overwrite? [y/N]`, `git` on credentials, an installer on
  `Do you want to continue?`) waited on stdin that nothing was connected to, holding its question in
  a pipe nobody would read until an exit that never came. The run now also returns when the command
  stops and waits: the reply carries whatever it has printed, `Status: waiting_for_input` and a
  `Session` id, and two new tools drive it from there — `system_resume_shell` sends the answer (or
  waits longer, with no input), `system_kill_shell` ends the command and everything it started.
  A question is told apart from ordinary slowness by the shape of the output: a tail with no line
  break is a prompt sitting with the cursor after it, and that is reported after two seconds, while
  total silence is reported as `Status: running` only after two minutes — so a quiet build is not
  mistaken for a question, and a question is not sat on for two minutes. `dotnet_build` and
  `dotnet_test`, which legitimately go quiet for minutes but never ask anything, keep only the
  prompt detector armed. Answering is the fallback, not the plan: the agent is told to prefer `-y`,
  `--yes` and `--force` where it knows a question is coming.

- **A tool call can now run detached from its turn.** A tool that opts in (none do yet — see below)
  reads `background: true` in its arguments, gets a `bg_N` id back immediately instead of blocking
  the turn, and keeps running; the result — success, failure, or cancellation, whichever — arrives
  as a message at the top of the chat's next turn, verified live end-to-end (schema flag reaching a
  real model, the model choosing to set it, the detached run finishing, delivery landing in the
  conversation) before this shipped. Three new tools work with it: `task_list` shows what is running
  or recently finished, `task_output` reads one task's result (repeatably — unlike the one-time
  delivery message), `task_cancel` stops one. A chat caps itself at 8 live background tasks and
  refuses the ninth rather than queuing it. A background call cannot ask a person anything —
  permission and clarification requests inside it are answered "no" automatically, since nobody is
  left to ask — so only a call that needs no mid-flight confirmation is a candidate for
  backgrounding at all. Closing a chat cancels every background task it still had running, and each
  chat's shell sessions are now its own (`ISandbox.ForChat()`) rather than shared across every open
  chat, which is what makes "close this chat, stop its work" possible in the first place.
  **Five built-in tools now opt in**: `system_run_shell`, `agent_spawn`, `agent_spawn_batch`,
  `web_fetch`, `ssh_session_exec` — the user's own call, made deliberately rather than by defaulting
  every tool in.

- **A background task's progress reaches the chat window live, and survives the human's next turn.**
  The subscription that turns `chat.Progress.NodeChanged` into `progress.node` frames moved from
  per-turn (`ClientConnection.BuildCallbacks`) to per-chat (`ChatRegistry.RuntimeOpened` →
  `SplaServiceHost.WireChatProgress`), since a detached task's tree outlives the turn that started
  it. That surfaced a collision: `ProgressTree` numbers its nodes `n1`, `n2`... starting fresh in
  each tree, so a turn's tree and a background task's tree running at the same time produced the
  same ids on the wire. Node ids are now namespaced `"{treeId}:{nodeId}"`, and `llm.turn.start`
  carries the new tree's id (`progressTreeId`) so the client knows which prefix just finished —
  it clears only that prefix from its node map on the next turn, not every node it's holding, so a
  background task's tree stays on screen across a turn boundary instead of vanishing and
  reappearing from nothing.

---

## 2026-08-25

- **`v0.2.4`'s release notes were wrong, and are now corrected in the repository.** The merge that
  reconciled `work` with `origin/work` resolved a conflict in `CHANGELOGS/` in favour of `main` —
  i.e. in favour of the files already emptied by the `v0.2.3` freeze — which silently dropped the
  log's 2026-08-19..21 entries before `v0.2.4` was squashed from that state. The published summary
  claimed to cover 2026-08-21 over a log that in fact stopped on the 18th, and `v0.2.4` was never
  frozen into `CHANGELOGS/` at all — the working files kept claiming to cover work since `v0.2.3`
  after `v0.2.4` had already shipped. `CHANGELOGS/v0.2.4.md` now exists, built from what the release
  actually contained (up to commit `d671ab9`, before the bad merge), and the working files carry only
  what has landed since. The GitHub release page itself was left as published — this fixes the
  repository's own record, not history.
