A shell command does not always finish in one call. `system_run_shell` returns as soon as the command exits — but also when the command stops and waits, so its output never sits unseen behind a process that will not end.

When the result has no `ExitCode` and shows `Status:` and `Session:` instead, the command is **still running**:

- `Status: waiting_for_input` — it printed something and left the cursor mid-line, which is what a question looks like: `Overwrite? [y/N]`, `Continue? (y/n)`, `Username:`. The question is in `Output`. Answer it with `system_resume_shell` using that `Session` id, or end the command with `system_kill_shell`.
- `Status: running` — it has simply gone quiet for a while. This is normal for builds, installs and downloads, and is **not** a question. Call `system_resume_shell` with no `input` to keep waiting.

Two rules that matter:

**Never start the command again while a session is open.** A second `system_run_shell` runs the whole thing a second time — re-encoding the file, re-installing the package — while the first copy still holds the first question. Drive the session you already have.

**Close what you open.** A session you stop answering keeps its process alive on the host. Once you no longer need it, end it with `system_kill_shell`.

Prefer flags that avoid the question in the first place when you know one is coming (`-y`, `--yes`, `--force`, `-NonInteractive`). Answering is the fallback for the questions you did not anticipate — which is most of them.
