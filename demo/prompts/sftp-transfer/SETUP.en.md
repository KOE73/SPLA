# SETUP — rig for exercising the SFTP tools

[Русский](SETUP.ru.md) · [Prompt](PROMPT.en.md)

Without this the prompt goes nowhere: half the steps check refusals, and "the host is not configured"
is not the refusal we want to see.

## What you need

1. **A Linux host with working SSH** and the SFTP subsystem enabled. A typical OpenSSH already has
   it — `Subsystem sftp internal-sftp` (or `.../sftp-server`) in `/etc/ssh/sshd_config`.
   Rule of thumb from Windows: if `ssh user@host` works, `sftp user@host` almost certainly works too.
   An administrator can disable it separately — the tools will say plainly that the subsystem is
   unavailable, but the test ends there.
2. **A second host that is NOT allowed to be written to.** It exists for exactly one step: proving
   that `sftp_upload` refuses without `allow_write`. Any SSH-reachable device will do — a router, a
   NAS, a second VM. Nothing is changed on it and nothing is written to it.
3. **Credentials in the secret store**, not a password in the config. A password in `.spla` or in the
   chat is precisely what the plugin is designed to avoid.

## The secret

Via the CLI — one field per call, with the value entered at a hidden prompt (it cannot be passed as
an argument, so it never lands in shell history):

```bash
spla secret set my-host-ssh --field user --user
spla secret set my-host-ssh --field password --user
```

The scope flag is required and has no default: `--user`, `--project` or `--shared`. It also becomes
part of the reference used in the config — `secret:user:my-host-ssh` for the `--user` scope. Check
with `spla secret list --user`.

The executable is actually `SPLA.CLI.exe`; for the `spla` alias and the rest of the commands see
[README_CLI.en.md](../../../docs/README_CLI.en.md).

For key authentication use the `private_key` field instead of `password`, plus `passphrase` if the
key is encrypted. All of this is also available on the Settings → Secrets page.

## The project

In the `.spla` of the project you run from (paths in the prompt are relative to its `workspace`):

```yaml
permissions:
  read: allow
  write: allow
  shell: allow

plugins:
  ssh:
    enabled: true
    settings:
      timeout_seconds: 25
      hosts:
        my-host:                      # written to; allow_write is required
          host: 192.168.1.10
          credential: secret:user:my-host-ssh
          allow_write: true
        my-readonly-host:             # never written to — that is the point
          host: 192.168.1.254
          credential: secret:user:my-readonly-host-ssh
```

`allow_write: true` on the first host is not needed for transferring files (downloading is always
allowed). It is needed for two things: the preparation commands that create the test directory, and
steps 17–19, which upload a file.

Do **not** set `allow_write` on the second host — otherwise step 16 "passes" while checking nothing.

## Before running

- Replace the host names in the prompt (`my-host`, `my-readonly-host`) with your own.
- The plugin must be built and deployed. If SPLA was running during the build it holds the old dll in
  memory — **restart it**, or you will be testing the previous version.
- Verify the connection: Settings → SSH → Test connection, or ask the agent for `ssh_list_hosts`.

## After the run

The prompt cleans up on the remote host, but a local `check/` folder stays in the project (the
downloaded files and the container). Delete it by hand if you do not want it.
