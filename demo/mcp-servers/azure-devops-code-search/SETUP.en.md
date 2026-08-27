# SETUP — connecting Azure DevOps Code Search as an MCP server

[Русский](SETUP.ru.md)

A runbook, not a ready project: the server is somebody else's package (`@ahouben/azure-devops-mcp`),
the address and collection are always your own. Every step but one explicitly marked step can be
carried out by an agent with shell and filesystem access. The secret itself is created and entered by
a human, out of band — see step 4.

Based on a live verification run against an on-prem TFS 2012 (`docs/secrets.md`, `agents/secrets.md`
— the sources of truth for secrets and their references).

## What you need to know up front

- **Azure DevOps / TFS collection URL.** Public `dev.azure.com/<org>` or an on-prem server address
  (`https://<host>/<collection>`).
- **Node.js `^20.19.0 || ^22.12.0 || >=23`** (an actual dependency of the package via `yargs`, not
  an arbitrary "20+"). Check with `node --version`.
- Read access to whatever repos/projects you intend to search — on the account whose PAT will be
  used.

## Step 1 — project folder and package (agent)

Create (or reuse) a working folder — it becomes the SPLA project's `workspace`:

```powershell
New-Item -ItemType Directory -Force <project-folder>
cd <project-folder>
npm init -y
npm install @ahouben/azure-devops-mcp
```

Pin the version in `package.json`/`package-lock.json` — don't rely on `npx`, which re-fetches the
package on every start and does not guarantee the same version.

Verify the package installs and understands its own syntax, without touching a real server:

```powershell
node node_modules/@ahouben/azure-devops-mcp/dist/index.js --help
```

## Step 2 — the `.spla` project file (agent)

If there's no project yet, run `spla init` in this folder; otherwise open the existing
`<name>.spla` and add an `mcp.servers` section. `AZURE_DEVOPS_PAT` is a **reference**, not a token —
the agent writes the reference itself; the token, it does not (step 4):

```yaml
mcp:
  servers:
    - id: azdevops
      name: Azure DevOps Code Search
      enabled: false          # turn on at step 5, once the secret is saved
      transport: stdio
      command: node
      cwd: '<full-path-to-project-folder>'
      args:
        - node_modules/@ahouben/azure-devops-mcp/dist/index.js
        - <collection-URL>       # https://dev.azure.com/<org> or https://<host>/<collection>
        - '-a'
        - env
        - '-d'
        - core
        - search
        - repositories
      env:
        AZURE_DEVOPS_PAT: secret:user:devops:azdevops-pat#token
      origin: unnamed
```

Notes on the values that are easy to get wrong:

- `-a env` makes the package read `AZURE_DEVOPS_PAT` from the process environment; without a PAT in
  that variable it silently falls back to Azure Identity (managed identity), which **does not work**
  against on-prem TFS and usually doesn't work for plain PAT-only access either.
- `AZURE_DEVOPS_PAT: secret:user:devops:azdevops-pat#token` is not a Windows environment variable —
  it's a reference into SPLA's secret store. **The scope (`user`) in the reference must match, word
  for word, the scope the secret is actually saved under in step 4** — there is no search or
  fallback between scopes; a mismatch looks exactly like "secret not found" and falls back to Azure
  Identity the same way a missing PAT does.
- The key `devops:azdevops-pat` and the field `#token` are convention, not a package requirement —
  name them however you like, as long as the reference and the actual record match verbatim.
- `plugins`/`permissions` can stay at project defaults, but note: this client tags **every** foreign
  MCP tool as `Foreign/Write/High`, including plain reads. `permissions.write: ask` is the working
  minimum (confirm the first call in chat); `write: deny` or `Research` mode blocks search too.

## Step 3 — TLS for an on-prem server with an internal CA (agent, if applicable)

Skip this for public `dev.azure.com`. For an on-prem server behind a corporate CA, Node does not
trust the chain by default (`UNABLE_TO_VERIFY_LEAF_SIGNATURE`), even when Windows does. Don't disable
verification — hand Node a trusted PEM instead:

```yaml
      env:
        AZURE_DEVOPS_PAT: secret:user:devops:azdevops-pat#token
        NODE_EXTRA_CA_CERTS: 'C:\path\to\corporate-ca.pem'
```

The package exposes `AZURE_DEVOPS_IGNORE_SSL_ERRORS=true`, but that disables certificate checking
entirely — don't use it as a workaround for this.

## Step 4 — the secret: human only, out of band

The agent does not perform this step and does not ask the user to paste the PAT into chat — that is
a direct invariant of the secret store (`agents/secrets.md` §6).

1. The human creates a read-only Personal Access Token: `Code (Read)`, and `Project and Team (Read)`
   if needed. Page: `<collection-URL>/_usersSettings/tokens` (for `dev.azure.com`:
   `https://dev.azure.com/<org>/_usersSettings/tokens`). No Write/Manage/Full access.
2. The human saves it into SPLA's store — terminal, hidden input, the value never lands in
   arguments, shell history, or chat:

   ```powershell
   spla secret set devops:azdevops-pat --field token --user
   ```

   The scope (`--user`/`--project`/`--shared`) must match the reference from step 2, verbatim. The
   same can be done through Settings → Secrets in the UI.

## Step 5 — enable the server and restart (agent)

```yaml
mcp:
  servers:
    - id: azdevops
      enabled: true    # was false in step 2
```

**The secret reference is resolved exactly once, at the server's first connection within an already
running SPLA process.** Editing `.spla` on the fly and the "Reconnect" button in the MCP panel do not
rebuild that connection — it needs a full restart of the process holding the project:

```powershell
spla stop <manifest.spla>     # or with no argument, from inside the project folder
spla start <manifest.spla>
```

`spla ps` will show the new PID and port (`ENDPOINT`) the project came back up on.

## Step 6 — verifying without a human (agent)

An actual tool call (`azdevops_core_list_projects` etc.) is `Foreign/Write/High`; it needs chat
confirmation, and a headless request has nobody to confirm it. That's expected, not a bug. But you
can verify that **the PAT actually resolves and the server comes up** without a single human click:

1. The process log, `<workspace>/.spla/logs/spla-<date>.log`. Search for `azdevops`:

   ```powershell
   Select-String -Path ".spla\logs\spla-*.log" -Pattern "azdevops"
   ```

   Success looks like:
   ```
   MCP server started. Server=azdevops Command=node
   MCP server ready. Server=azdevops Name=Azure DevOps MCP Server Tools=26
   ```
   A PAT failure doesn't show up in SPLA's log — it shows up on the first real tool call, as
   `ChainedTokenCredential authentication failed. CredentialUnavailableError: EnvironmentCredential is
   unavailable.` If you see that, the secret didn't resolve (check the scope in the reference, and
   that a real restart happened — not just a file edit).

2. The tool list, through this project's own outward MCP endpoint (needs `mcp.enabled: true` and
   `mcp.port` in `.spla` — don't confuse this with the foreign server the project *consumes*; this
   is the endpoint the project *exposes itself as*):

   ```powershell
   curl -s -X POST "http://127.0.0.1:<mcp.port>/mcp" -H "Content-Type: application/json" `
     -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}' | Select-String azdevops
   ```

   Twenty-six `azdevops_*` entries in the response means the server connected and handed over its
   tools. That doesn't yet prove the PAT is valid against the real Azure DevOps (only step 1 does —
   no auth error in the log after a real call), but it confirms the whole chain — config → secret →
   process → MCP client — is wired correctly.

3. The final proof that the PAT is actually accepted by the server is one message a human sends in
   chat (e.g. "find every use of SqlConnection across all repos"), confirming the first tool call and
   looking at the result. No further confirmations are needed within that same chat/session.

## What's next

A ready prompt to paste into chat once verification passes:

```
Read-only. Find every use of <class/function> across all available repositories;
show the project, repository, and path.
```

Don't approve branch creation, PRs, comments, or other write operations through this server — the
`repositories` domain in `-d core search repositories` includes the package's write methods too, it
is not a read-only allowlist by itself; the real boundary is what the PAT is allowed to do in Azure
DevOps.

## Potential feature: multiple collections

Not implemented, but it fits the existing model with no changes to SPLA — noting it here as a
starting point.

The package accepts exactly one collection per process: `organization` is a required positional
argument, and there's no list within a single run (`node .../index.js --help`). A second collection
(another TFS server, or `dev.azure.com/<another-org>`) doesn't need a second package — it needs a
second entry in `mcp.servers`, a second node process alongside the first, with its own `id`:

```yaml
mcp:
  servers:
    - id: azdevops                 # first collection, as in step 2
      args: [..., <collection-URL-1>, ...]
      env:
        AZURE_DEVOPS_PAT: secret:user:devops:azdevops-pat#token

    - id: azdevops2                # second — its own id = its own tool prefix (azdevops2_*)
      name: Azure DevOps Code Search (second collection)
      transport: stdio
      command: node
      cwd: <same-project-folder>
      args:
        - node_modules/@ahouben/azure-devops-mcp/dist/index.js
        - <collection-URL-2>
        - '-a'
        - env
        - '-d'
        - core
        - search
        - repositories
      env:
        AZURE_DEVOPS_PAT: secret:user:devops:azdevops2-pat#token   # same secret if one PAT covers both
      level: enabled
```

Details:

- `id` must be unique — it's also the tool prefix; two entries sharing an `id` breaks naming rather
  than one simply overwriting the other.
- One `package.json`/`node_modules` for the whole project — no need to install the package twice; the
  second entry just points at the same `command`/`cwd`.
- The PAT can be the same secret if the account is valid in both collections — both entries then
  reference the same `secret:...` reference; if the logins differ, run a separate `spla secret set`
  under a separate key — step 4 repeats verbatim for the second key.
- Step 5 (restart) and step 6 (verification via log/`tools/list`) don't change — the log just gets a
  second pair of `MCP server started/ready. Server=azdevops2` lines, and `tools/list` returns a second
  set of `azdevops2_*` entries.
