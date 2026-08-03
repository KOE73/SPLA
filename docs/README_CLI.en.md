# SPLA.CLI — complete command reference

[Русский](README_CLI.ru.md)

> **This file must match the code.** It documents EVERY command and flag the CLI accepts. If you
> change argument parsing under `src/apps/SPLA.CLI/` — `Program.cs`, `Cli/CliBootstrap.cs`,
> `Cli/ServeCommand.cs`, `Cli/SecretCommands.cs`, `Cli/ChatCommands.cs`, `Cli/InteractiveRepl.cs` —
> update both references (`docs/README_CLI.ru.md` and `docs/README_CLI.en.md`) in the same commit. A
> reference that lies is worse than none: someone runs the documented command and gets
> `Scope is required` instead of a result.

The executable is **`SPLA.CLI.exe`**. Below it is written as `spla` for brevity — see
[Aliasing on Windows](#aliasing-on-windows).

## How the project is chosen

The project is resolved BEFORE the command is parsed, from the first argument:

| Invocation | What is used |
|------------|--------------|
| `spla <path>.spla …` | that exact file; **if it does not exist it is created** (`ScaffoldIfNew`) |
| `spla chat …` | search upward from the current directory for `*.spla` |
| everything else, including no arguments | search upward from the current directory for `*.spla` |

With no project file the CLI runs on global settings (`~/.spla/defaults.yaml`) — a working mode, not
an error. When a project is found, **the process working directory changes to the project's
`workspace`**, and relative paths are taken from there afterwards.

Startup prints: project file, project name, workspace, model endpoint, agent mode, and the number of
registered tools.

## Commands

### No command — interactive REPL

```
spla
spla myproject.spla
```

Creates a new chat and enters a "line in → agent turn" loop. Inside the REPL:

| Input | Effect |
|-------|--------|
| `/skills` | list available skills: `[on]`/`[off]`, id, description |
| `/skills load <id>` | load a skill's body into the current chat's context |
| `exit` / `quit` | leave |
| empty line | leave |

### `serve` — the WebSocket service

```
spla serve [--port <N>] [--bind <address>] [--token <string>] [--repl] [--new-chat "<text>"]
```

| Flag | Default | Meaning |
|------|---------|---------|
| `--port <N>` | `5050` | TCP port |
| `--bind <address>` | `127.0.0.1` | interface |
| `--token <string>` | none | access token for the socket |
| `--repl` | off | a parallel console session against the same runtime socket clients drive |
| `--new-chat "<text>"` | none | message the first chat starts with |

Stop with Ctrl+C. With `--repl`, `exit`/`quit` stops the service; if stdin merely closed, the service
keeps running.

Binding to a non-loopback address **without** `--token` prints a warning and is not blocked: anyone
who can reach the port controls the agent. That is a deliberate "warn, don't decide for you" — but do
not do it on a network.

### `secret` — the secret store

```
spla secret list                          --user | --project | --shared
spla secret set    <key> [--field <name>] --user | --project | --shared
spla secret delete <key> [--field <name>] --user | --project | --shared
```

The rules that most often make a first attempt fail:

- **The scope flag is required and has no default.** Deliberately so: a tool that picks the scope for
  you is how the same key ends up in two stores, one silently shadowing the other.
- **Only the first `--field` is read.** One field per call; two fields means two calls.
- **The value is entered at a hidden prompt** and cannot be passed as a command-line argument — it
  would land in shell history. For the same reason `list` prints keys and field names only, never
  values.
- `set` **merges** the field into an existing entry rather than replacing its siblings.
- `delete` with `--field` removes one field; without it, the whole entry. Removing the last field
  removes the entry.
- `--project` requires an open project, otherwise it refuses and says so.

The scope becomes part of the reference the config uses: a key `my-host-ssh` stored with `--user` is
addressed as `secret:user:my-host-ssh`.

Fields the SSH plugin understands: `user`, `password`, or `private_key` (+`passphrase`).

Exit codes: `0` success, `1` not found (on `delete`), `2` bad arguments.

### `chat` — saved chats

```
spla chat list
spla chat open <id>
spla chat fork <id> [--model <name>]
```

- `list` — id, title, last-modified time.
- `open <id>` — open a chat and enter the REPL. **An unknown id silently creates a new chat** —
  expect that; there is no separate error.
- `fork <id>` — a copy of the chat, optionally on a different model. `--model` is parsed positionally:
  it must follow the id directly (`chat fork <id> --model <name>`) and is matched lowercase.
- `spla chat` with no sub-command prints a usage line.

## Aliasing on Windows

Do **not** copy `SPLA.CLI.exe` elsewhere: the apphost looks for its managed assembly next to itself
and will not start from a foreign directory. Renaming it in place does work, but every build
recreates `SPLA.CLI.exe`, so the rename has to be redone each time.

What works is the `spla.cmd` shim. It already sits in the repository root: put the root on `PATH` (or
copy the file into a directory that is already there) and `spla ...` works from anywhere.

It locates the exe in this order: the `SPLA_CLI` variable → `.publish\work` → `bin\Release` →
`bin\Debug`, so it survives both a rebuild and a publish. For a build that lives elsewhere:

```bat
set SPLA_CLI=D:\somewhere\SPLA.CLI.exe
```

The shim deliberately does not change directory: the CLI searches upward from the current directory
for `*.spla`, so a `cd` inside the shim would quietly select the wrong project.

The PowerShell-only alternative is a line in `$PROFILE`:

```powershell
Set-Alias spla 'C:\path\to\SPLA.CLI.exe'
```

`doskey` is not suggested: it lives only inside a `cmd` session and has no effect in PowerShell.
