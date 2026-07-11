# SPLA Server — Authentication Modes

`SPLA.Server` (and `spla serve`) can run in three authentication modes. All three converge on the same
`spla.auth` cookie, so everything downstream — the WebSocket upgrade, per-user storage areas, sharing —
behaves identically regardless of how the user signed in. OIDC will later slot in as a fourth mode that
fills the same cookie.

| Mode | Flag | Identity source | Use case |
|------|------|-----------------|----------|
| **None** | `--auth none` / `--no-auth` | single local user | loopback / embedded / trusted LAN with `--token` |
| **Negotiate** | `--auth negotiate` (default) | Windows domain (NTLM/Kerberos) | domain deployment |
| **Local** | `--auth local` | username/password store + admin panel | home / small workgroup, no domain |

## Local mode (username / password)

```
SPLA.Server --auth local --root C:\SPLA\ServerData --https
```

- **User store:** a JSON file (`users.json`) in the server root (or next to the exe when no `--root`
  is set). Passwords are stored only as PBKDF2 hashes (`Microsoft.AspNetCore.Identity.PasswordHasher`),
  never in plaintext.
- **First run:** when the store is empty, an `admin` account is seeded with a **random password printed
  once to the console and the log**. Sign in and change it immediately. There is no hardcoded default
  credential.
- **Self-registration:** enabled by default at `/register`; new accounts get the ordinary `user` role.
  Pass `--no-register` to require an admin to create every account instead.
- **Per-user areas:** each local user gets their own `{root}/users/<key>/` area (same isolation as the
  domain deployment). Set `--root` — without it all users share one project.
- **Groups:** a user's groups flow into the connection identity, ready for group-based sharing.

### Pages

| Path | Who | What |
|------|-----|------|
| `/login` | anyone | sign in |
| `/register` | anyone (if enabled) | create an account |
| `/account` | any signed-in user | change own password |
| `/admin` | `admin` role only | full user administration |

### Admin panel (`/admin`)

A self-contained page (no external assets) backed by a small JSON API under `/admin/api`, all gated by
the `admin` role. It supports: list/filter users, create, edit (display name, roles, groups, password),
enable/disable, and delete. A **last-admin guard** prevents disabling, deleting, or demoting the only
remaining admin, so a deployment can never lock itself out.

Denial semantics are client-appropriate: browser navigations to `/admin` as a non-admin land on a 403
access-denied page; the panel's API calls receive `401` (not signed in) or `403` (not an admin) status
codes rather than HTML redirects.

## Security notes

- **HTTPS:** use `--https` for any non-loopback deployment — the cookie is the credential. A self-signed
  certificate is generated next to the exe on first run (clients trust it once). Set a non-default PFX
  password with `--cert-password` for anything beyond a home box.
- **Origin gate:** the `/ws` upgrade is protected against cross-site WebSocket hijacking (the `Origin`
  header must match the server host) whenever auth is enabled.
- **Execution isolation:** local/workgroup auth authenticates *who* connects; it does not yet sandbox
  *what* their agent can do on the server. Until per-user sandboxing lands, run the server only where
  every authenticated user is trusted to run commands on the host. See the security review under
  `docs/reviews/` for the roadmap.
