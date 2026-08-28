# Changes — unreleased

The scannable list: one line per change, no dates, no detail. Derived from the entry headings in
`current-log.md` rather than written independently, so the two cannot drift apart. Frozen into
`CHANGELOGS/<version>.md` on release, then started empty again.

This list and the summary are what CI puts into the release body; the full log stays in the
repository and is linked from it.

**Covers work since `v0.2.4`**, frozen in [`CHANGELOGS/v0.2.4.md`](v0.2.4.md).

---

### Added

- A `documents` plugin: `document_extract` reads a Word `.docx` for its meaning (markdown, plain text
  or a typed JSON block tree), and `spreadsheet_inspect` / `spreadsheet_read_rows` /
  `spreadsheet_append_rows` read and extend `.xlsx`/`.csv` by column header rather than by cell
  address. The same extraction registers as `docx → markdown | text | json` pairs in the core
  converter registry.
- A tool call can run detached from its turn (`background: true`) and deliver its result as a
  message on the chat's next turn; `task_list` / `task_output` / `task_cancel` manage what is
  running. `system_run_shell`, `agent_spawn`, `agent_spawn_batch`, `web_fetch` and
  `ssh_session_exec` opt in; a background task's live progress reaches the chat window and survives
  the human's next turn instead of being cleared with it.
- Architecture diagram visualizer unified to modular project format, filters panel added, and unsaved changes protected.

### Fixed

- A shell command that asks a question (`Overwrite? [y/N]`, credentials, `Do you want to continue?`)
  no longer hangs forever: the run comes back with the question, a session id and
  `Status: waiting_for_input`, and `system_resume_shell` / `system_kill_shell` answer it or end it.

- `v0.2.4`'s release notes were wrong, and are now corrected in the repository.
- Parallel diagram connections no longer overlap into indistinguishable single lines.
