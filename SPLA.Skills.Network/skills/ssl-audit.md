---
id: network.ssl-audit
description: TLS/SSL certificate and protocol audit — chain, expiry, cipher suite, vulnerabilities. Trigger on: check certificate, SSL error, cert expiry, TLS issue, HTTPS audit.
---

# SSL/TLS Audit

## Memory Keys Used by This Skill

### session scope
- `context:plan` — (object) Step list and current progress
- `context:step` — (number) Currently executing step index

### project scope
- `host:{ip}:tls:{port}` — TLS/SSL audit findings (cert chain, protocol, cipher, HSTS) — one key per audited port

### State handling
- session: execution state and plan only
- project: written at finalize step if target resolves to an IP. Schema defined in plugin default_prompt.
- If the target is a domain name only (no resolved IP), skip `host:{ip}:tls:{port}` and report inline.

## Step 0 — Confirm tools and initialize plan

Call `agent_info` with each tool name you intend to use to confirm it is registered.
Prefer lower_snake_case network tools when available — they return structured data and handle errors cleanly.
If a specific lower_snake_case network tool is absent, fall back to `system_run_shell` (cmd/shell: openssl s_client) to accomplish the same step.
Adapt the plan below based on what is actually available.

Write the full plan to `context:plan` in session KV:

```json
{
  "total_steps": 4,
  "current_step": 0,
  "steps": [
    "Step 1: network_check_tls on target host:port → host:{ip}:tls:{port}",
    "Step 2: network_check_http_redirects — verify HTTPS redirect chain",
    "Step 3: network_http_head — check HSTS header → update host:{ip}:tls:{port} with hsts field",
    "Step 4: finalize — write project KV, clear context:, deactivate"
  ]
}
```

## Execution sequence

After each numbered step, update both `context:plan.current_step` and `context:step` in session KV.

1. `network_check_tls` on the target host:port (default 443 if not specified). Write `host:{ip}:tls:{port}` in project KV (e.g. `host:10.0.0.1:tls:443`).
2. If HTTP redirects are relevant: `network_check_http_redirects` — verify the HTTPS redirect chain is clean.
3. `network_http_head` — check HSTS header (`Strict-Transport-Security`). Update `host:{ip}:tls:{port}` with hsts field.

If a step fails or is not applicable, note it in one line and continue.

## Step 4 — Finalize

Confirm all project KV writes are complete. Clear session context keys with `agent_memory_clear {scope:"session", filter:"context:"}`. Then call `skill_deactivate`.

## What to report

**Certificate:**
- Subject CN / SAN list
- Issuer and chain depth
- Expiry date and days remaining (flag if < 30 days)
- Self-signed or publicly trusted
- Key type and size (flag RSA < 2048, EC < 256)

**Protocol:**
- TLS versions negotiated (flag TLS 1.0 / 1.1 as deprecated)
- Cipher suite (flag RC4, DES, 3DES, export ciphers, NULL)
- Flag if TLS 1.3 is NOT supported

**Security headers:**
- HSTS present and `max-age` value (flag if missing or max-age < 180 days)
- Flag missing `includeSubDomains` on HSTS if subdomains are in SAN

**Action items:** concrete tasks — "renew certificate (expires in N days)", "disable TLS 1.0", "add HSTS header", etc.
