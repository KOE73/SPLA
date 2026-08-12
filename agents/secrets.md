# Secrets, Credentials & API Keys — Canonical Policy

**STOP — read this before writing any code that touches a password, API key, token, private key,
passphrase, or connection string.** This file is the single source of truth. Every other mention in
the repo (XML-docs, plugin panels, protocol registry, design notes) states at most a one-line
restatement plus a pointer here. If you find a rule stated somewhere else and it contradicts this
file, this file wins and the other place is a bug.

Related: [`agents/protocol.md`](protocol.md) (wire boundary) · [`agents/spla-file.md`](spla-file.md)
(config format) · [`ADR_20260712_secrets_store-and-ssh`](../docs/adr/ADR_20260712_secrets_store-and-ssh.md) (why the
backends look like this — ADR, not policy) · [`readme_auth`](../docs/readme_auth.md) (server
login accounts, a different subject).

---

## 1. Invariants

These are absolute. There is no configuration, no debug flag, and no "just for local dev" exception.

1. **A secret value never leaves the host process.** Not in a protocol reply, not in a broadcast,
   not in a log line, not in an error message, not in telemetry, not in a tool result.
2. **A secret value never enters a committable file.** `*.spla`, `defaults.yaml`, plugin settings
   blobs, and anything else under version control hold a *reference*, never plaintext.
3. **A secret value never enters the model's context.** No MCP tool accepts a secret as an argument
   and no tool returns one. The agent may see key *names*, never values.
4. **A secret value is materialized at the point of use and dropped.** Resolution happens inside
   host code, at connect/invoke time, into a local variable or a clone — never onto stored config,
   never into a DTO, never into a cached object.
5. **Values are write-only from every UI.** They travel client → server. They never travel back.
6. **The transport is the store, not the config.** Config declares *which* entry to use; the store
   holds the material.

Anything that violates 1–6 is a defect regardless of how convenient it is.

---

## 2. The store

`ISecretStore` ([`src/core/SPLA.Domain/Secrets/ISecretStore.cs`](../src/core/SPLA.Domain/Secrets/ISecretStore.cs))
is the stable contract every consumer codes against — SQL, SSH, LLM connections and the host all use
the same store; it is global, not per-plugin. Changing its shape is a deliberate, whole-codebase
move, never a local convenience: the scope became mandatory on every method in one pass, because a
half-migrated store (explicit writes, searching reads) is worse than either end state.

**Unit of storage — the entry.** A named record of free-form fields (`user` + `password`, a lone
`token`, a PEM `private_key` + `passphrase`). Field names are conventions, not schema; see
[`SecretEntry.cs`](../src/core/SPLA.Domain/Secrets/SecretEntry.cs). Keys and field names are
case-insensitive.

**One entry = one credential.** There are only a few ways to authenticate, and each is already a
shape the store knows:

| Shape | Fields | Used by |
|---|---|---|
| a single secret (bearer / API key) | `token` | LLM connections, management keys, webhooks |
| user + password | `user` + `password` | SSH, SQL |
| private key | `private_key` (+ `passphrase`) | SSH |

A new subsystem picks one of these; it does not invent a fourth. Two credentials that merely belong
to the same account — an OpenRouter api key and its management key — are two entries, not one entry
with two fields. That is what keeps a bare `secret:<scope>:<key>` unambiguous: with one field it
resolves to it, and consumers never have to agree on a private field name to read each other's
credentials. `#field` stays available for the genuinely compound cases (`user`+`password`), where the
fields are parts of one credential rather than separate ones.

**Scopes.** Three, and the scope is **always stated explicitly**.

| Scope | Local | Server |
|---|---|---|
| `user` | `~/.spla/secrets.yaml` (`SPLA_HOME` overrides) | the caller's own private area |
| `project` | `<manifest dir>/.spla/secrets.yaml` | same — travels with the project |
| `shared` | `~/.spla/secrets.shared.yaml` | administered centrally, ACL-gated |

**There is no search order, because there is no search.** No overload takes a key without a scope,
nothing falls through from one scope to another, and nothing defaults. `user` means `user`.

This is not fussiness. A store that guesses eventually hands back the wrong credential — silently,
under a name that looked right: you set a password in one place, the UI writes it in another, and the
first one keeps winning while you edit the second. The same key in two scopes is now legal and
unambiguous, because each is only reachable by naming its scope.

Two consequences worth keeping in mind: a missing entry is an **error with a name in it**, never a
substitution; and on a server "machine-wide" would have meant *the server's* machine — that is,
everybody — which is exactly the hole `user` closes.

Never compute `~/.spla` anywhere except `ConfigLoader.GetDefaultsDir()`.

**Backends.** One active backend, no composites, no chains. Selected by `secrets.backend: file|dpapi`
in the machine-level `defaults.yaml` only — never in a committable project file, because the backend
is a property of the machine. `file` (plaintext YAML, gitignored) is a legitimate shipped default and
must not be removed: transparent dev/test storage is a feature. `dpapi` lives in a separate assembly
wired through a factory (no platform dependency in `SPLA.Domain`) and encrypts **values only** —
entry keys and field names stay verbatim so listing never decrypts. An unavailable backend degrades
to `file` with one warning and never crashes; a field that fails to decrypt is treated as absent and
its blob is never logged. There is no plaintext→DPAPI migration; the two files coexist.

**Listing never returns values.** `ListEntriesAsync` returns keys and field *names*. This is a
contract, not an implementation detail — a management UI is built on it.

---

## 3. References in config

Committable config holds pointers. Legal forms:

| Form | Meaning |
|---|---|
| `secret:<scope>:<key>` | default field of the entry — the only field, else `password`, else `token`, else `value` |
| `secret:<scope>:<key>#<field>` | a specific field |
| `env:VAR` | read from the environment |
| anything else | a literal |

**The scope is mandatory.** Making writes explicit while leaving reads to a search would move the
ambiguity, not remove it. A reference without a scope is a hard error that names the three valid ones.

**[`SecretRef`](../src/core/SPLA.Domain/Secrets/SecretRef.cs) is the single place the syntax exists.**
Every prefix and separator is a constant there, with `Format` / `TryParse` / `TryParseScope` beside
them. Nothing else may hard-code the prefix or the separators — parse and format through it, so
changing the grammar is one edit rather than a hunt. `ISecretResolver.IsReference` (which delegates
there) is the only test for pointer-vs-literal, and there is exactly one resolver implementation
([`SecretResolver.cs`](../src/core/SPLA.Domain/Secrets/SecretResolver.cs)).

Splitting happens at the *first* separator, so keys may contain `:` and `/` freely — `sql:mydb` and
`ssh/homelab/oleg` need no escaping.

**`credential: <reference>`** — the form for anything with more than one field. It holds a full
reference, exactly like every other pointer: one syntax everywhere, so nothing has to remember a
second way of naming the same thing. New subsystems use `credential:` and nothing else.

**Key naming.** Free-form keys, bound in config (the `~/.ssh/config` model — the store knows nothing
about "hosts" or "connections"). Established conventions:

| Subsystem | Key | Example reference |
|---|---|---|
| SQL | `sql:<name>` | `secret:project:sql:mydb` |
| SSH | `ssh/<host>/<user>` | `secret:shared:ssh/prod/root` |
| LLM connection | `llm:<connection-id>` | `secret:shared:llm:anthropic-main` |

---

## 3a. Who may use which entry (ACL)

An entry carries an ACL: an **owner** (whoever created it, who always holds every right and may grant
them on) plus additive grants of two rights to **principals** — user keys or group keys:

| Right | Means |
|---|---|
| `Use` | resolve it at point of use — connect with it, never see it |
| `Manage` | see it in listings, overwrite, delete, change its ACL |

`Manage` implies `Use`. There is no deny list: a deny that another grant can out-vote is a bug
generator.

Only `shared` needs this. `user` is isolated by living in the caller's own area and `project` by
being part of the project — and that rule lives once, in
[`ISecretAccessPolicy`](../src/core/SPLA.Domain/Secrets/ISecretAccessPolicy.cs), rather than being
re-derived by each caller. Local installs use `PermissiveSecretAccessPolicy` (one person, nothing to
arbitrate); a server uses `AclSecretAccessPolicy`. A `shared` entry with **no recorded ACL is closed**
to everyone but administrators — failing open would publish it.

Two rules that make this real rather than decorative:

1. **Enforced at resolve, not only when listing.** `SecretResolver` checks on every materialization,
   against the ambient `SecretCallerScope.Identity`. A filtered dropdown is not a permission.
2. **ACLs are stored in plaintext beside the store, never inside it** (`secrets.acl.yaml`). An ACL is
   not credential material, and it has to be readable to filter a listing *without* decrypting
   anything — otherwise the "listing never touches values" promise breaks under DPAPI.

A refusal raises `SecretAccessDeniedException`, which names the scope and key and never any part of
the value. Say *that*: an agent that reports only "connection failed" because its user lacked a
credential produces support tickets instead of understanding.

---

## 4. Resolution

Resolve **as late as possible, as narrowly as possible**:

- SSH resolves at connect time; the value lives for that call and is never returned, logged, or
  surfaced. A `private_key` is fed to the SSH client as an in-memory stream — never written to disk.
- SQL resolves at connection-open onto a **clone** of the config; stored config is never mutated,
  and passwords are not overlaid at plugin init.
- LLM connections resolve inside the credential middleware, immediately before the provider call
  (see [§7](#7-llm-connections)).
- "Test connection" style diagnostics resolve **server-side**. The client sends the connection id,
  never the material.

Synchronous `Resolve()` exists only because backends are local files. Do not add a network backend
behind it without moving callers to the async path.

---

## 5. The client/server boundary

`secret.*` protocol messages are **write-only**: values go browser → server; every reply carries
keys, field names, scopes, and the backend name. No payload type may declare a field that holds
secret material in the server → client direction. When adding a message to
[`agents/protocol.md`](protocol.md), state explicitly that it carries no credentials.

Consequence for UI: a value must not land in the DOM or in tab memory. Inputs are
`type="password"` + `autocomplete="new-password"`; existing values render as a "set / not set"
indicator, never as a masked round-trip of the real string. Credential pickers are populated from
`secret.list` (keys, scopes and field names only). Project scope is disabled when no project is open.

**Every picker shows the scope, and every write states it.** The scope rides on each listed entry
along with a ready-to-paste `reference`, and it is displayed as its own column or badge — never glued
into the key string, which stays machine-parseable. Write payloads have **no default scope**: the
form stays disabled until the user picks one, and the server rejects a request that omits it. The
program does not choose where someone's credential lives.

Listings are filtered **server-side** by the access policy, and each entry carries `canManage` so the
UI renders read-only rows honestly instead of offering buttons that will fail.

**There is exactly one browser-side implementation, and panels borrow it.** `web/src/secrets/` owns
the whole client half: `store.ts` is the only module in `web/` allowed to send `secret.*`, and
[`CredentialField.vue`](../web/src/secrets/CredentialField.vue) (picker + inline
[`SecretEntryEditor.vue`](../web/src/secrets/SecretEntryEditor.vue)) is the control every consumer
embeds. Plugin settings modules are built separately and cannot import it, so the host **hands it
over** through `PluginSettingsMountApi.mountCredentialField` — the plugin supplies an element and
gets back a reference. A plugin that speaks `secret.*` itself is a defect: each copy is another place
to drop the project envelope, to write its own scope default, or to mistake a refusal for success.
That last one is not hypothetical — `secret.*` answers refusals in `secret.result.error` rather than
rejecting, so a caller that only try/catches records a reference to a credential that was never
stored. `store.ts` turns `error` into a throw once, for everyone.

Consequence for settings: **never ship a settings blob to a client unfiltered.** Any structure that
may contain a literal credential is filtered on the way out and merged (not overwritten) on the way
in, so a value the client never saw cannot be erased by a round-trip.

---

## 6. The agent

- No MCP tool is given access to the store. Ever.
- No MCP tool accepts a secret as an argument. A tool that needs a credential accepts the *entry
  name* and resolves it host-side.
- Tools that list configured resources name the credential entry and never look inside it.
- Plugin prompts must instruct the model never to ask the user to paste a password into chat. A
  system-prompt contract must be self-contained, so this rule is **restated in full** in each
  plugin's `meta.yaml` — a pointer is useless to a model.
- Error messages never echo arguments or credentials.

Secrets are entered out of band: the CLI's hidden prompt or the settings panel. The CLI never
accepts a secret value as an argv token (shell history).

---

## 7. LLM connections

LLM connections follow the SQL/SSH model exactly. There is no special case for API keys.

- A connection declares `credential: secret:<scope>:<key>`. **There is no `api_key` field in the
  connection schema, in any DTO, or in any protocol payload.** Absence of the field is the
  enforcement mechanism — a leak must be impossible to write, not merely discouraged.
- A shared cloud key lives in `shared` behind an ACL; a personal key lives in `user`. Both are used
  identically, and neither party can read the other's.
- The client learns only `hasCredential: true|false` and the entry name.
- Writing a key is `secret.set`, the same call the SQL and SSH panels already use.
- Diagnostics (`connection.test`, `models.list`, model swap) take a connection id; the server
  resolves the credential itself.
- The resolved key is handed to the provider client through a narrow accessor at invoke time and is
  not stored on the connection descriptor, the settings object, or the turn context.

In server mode this is what makes a shared admin-owned cloud key possible: users consume the
connection, the key stays on the server, and no user — however privileged in the UI — can read it
back out.

---

## 8. Known open items

Do not treat these as accepted behaviour; they are recorded defects.

1. **No log redaction layer exists.** Promised in the design notes, never built. Until it exists,
   every new log/telemetry call site is responsible for not formatting credential-bearing objects.
2. **Plugin settings blobs cross the boundary unfiltered** — SSH host `password`/`key_passphrase`
   and SQL connection `password` can still be literals and are shipped to clients and round-tripped
   by panels. §5 is the target state.
3. ~~`sql_manage_connection` declares a `password` tool argument~~ — **closed**: the tool is gone.
   Connections are operator configuration, like SSH hosts; `SqlConnectionRegistry` is read-only, so
   there is no longer a tool through which a credential could reach the model's context.
4. **LLM `api_key` is currently a plain field** in the connection schema, the outbound DTO, the
   broadcast, and the committed `*.spla` files. §7 is the target state; this is the largest open
   leak in the repo.
5. **The sudo password is written into the raw pty stream** that is broadcast to terminal viewers.
   Safe only because the pty does not echo; there is no outbound filter and no replay-buffer filter.
6. **Secrets in argv** (`--token`, `--cert-password`) and a constant PFX password remain open
   findings from the dated security reviews under `docs/reviews/`.
7. **Project-scope location is unsettled**: project secrets sit next to the manifest directory while
   the runtime broker zone derives from `WorkspacePath`. These can diverge.
8. ~~Default scope differs by entry point~~ — **resolved**: there is no default anywhere. The CLI
   requires `--user|--project|--shared`, the protocol rejects a missing scope, and both plugin panels
   make the user choose before the save button enables.
