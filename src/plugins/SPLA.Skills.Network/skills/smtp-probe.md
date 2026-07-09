---
id: network.smtp-probe
description: Mail server diagnostic — SMTP EHLO handshake, STARTTLS, AUTH methods, relay check. Trigger on: check mail server, SMTP probe, email not working, test relay, MX check.
---

# SMTP Probe

## Memory Keys Used by This Skill

### session scope
- `context:plan` — (object) Step list and current progress
- `context:step` — (number) Currently executing step index

### project scope
- `host:{ip}` — base record updated with ping result and class (if target is an IP)
- `host:{ip}:smtp:{port}` — SMTP probe findings per port (e.g. `host:{ip}:smtp:25`, `host:{ip}:smtp:587`)
- `host:{ip}:tls:{port}` — TLS details if port 465 was checked (e.g. `host:{ip}:tls:465`)

Schema for each key is defined in the plugin `default_prompt`.
If the target is a domain name only (no resolved IP), skip `host:{ip}` writes and report inline.

---

## MANDATORY checkpoint protocol

SMTP probe output (EHLO responses, STARTTLS negotiation) and TLS details are verbose and fill the context quickly. A checkpoint between the SMTP/TLS phase and the DNS phase is required.

Rules — no exceptions:

- **FORBIDDEN:** calling `network_scan_tcp_ports` (step 2) without a preceding `checkpoint_save`.
- **FORBIDDEN:** calling `context_rollback` before all `host:{ip}:smtp:{port}` and `host:{ip}:tls:{port}` writes and `context:plan` are confirmed with `ok:`. Write one key per port — do not merge ports into a single key.
- **REQUIRED:** steps 5–8 (DNS queries) MUST run after `context_rollback` with a clean context. They do not depend on SMTP/TLS conversational output — they read from KV if needed.
- **REQUIRED:** after `context_rollback`, the FIRST call MUST be `agent_memory_get {key:"context:plan", scope:"session"}`.

```
checkpoint_save                                          ← REQUIRED before step 2
→ step 2: network_scan_tcp_ports {ports:"25,465,587,2525"}
→ step 3: network_probe_smtp on each open SMTP port
→ step 4: network_check_tls on port 465 if open
→ agent_memory_set {key:"host:{ip}:smtp:{port}", scope:"project"}   ← one call per probed SMTP port
→ agent_memory_set {key:"host:{ip}:tls:465",    scope:"project"}   ← only if port 465 was checked
→ agent_memory_set {key:"context:plan",   scope:"session"}   ← mark steps 2-4 done
context_rollback                                         ← REQUIRED after all three writes confirmed
→ read context:plan from KV — proceed to step 5
```

---

## Step 0 — Confirm tools and initialize plan

Call `agent_info` with each tool name you intend to use to confirm it is registered.
Prefer lower_snake_case network tools — they return structured data and handle errors cleanly.
Fall back to `system_run_shell` (telnet, openssl s_client) only when a specific tool is absent.

Write the full plan to `context:plan` in session KV:

```json
{
  "total_steps": 9,
  "current_step": 0,
  "checkpoint_after_step": 4,
  "steps": [
    "Step 1: network_ping_host — basic reachability → host:{ip}",
    "Step 2: network_scan_tcp_ports ports=25,465,587,2525  [checkpoint_save BEFORE this step]",
    "Step 3: network_probe_smtp on each open SMTP port → host:{ip}:smtp:{port} (one key per port)",
    "Step 4: network_check_tls on port 465 if open → host:{ip}:tls:465  [context_rollback AFTER KV writes]",
    "Step 5: network_query_dns type=MX — verify MX records",
    "Step 6: network_query_dns type=TXT — check SPF",
    "Step 7: network_query_dns name=_dmarc.{domain} type=TXT — check DMARC",
    "Step 8: network_query_dns type=TXT for DKIM selector if known",
    "Step 9: finalize — write project KV, clear context:, deactivate"
  ]
}
```

---

## Execution sequence

After each step: update `context:plan.current_step` and `context:step` in session KV.

1. `network_ping_host` — basic reachability. Write `host:{ip}` in project KV with ping result.
2. **Call `checkpoint_save` now.** Then `network_scan_tcp_ports` with `ports=25,465,587,2525`.
3. `network_probe_smtp` on each open port — EHLO handshake; captures banner, STARTTLS, AUTH methods, SIZE, extensions. Write `host:{ip}:smtp:{port}` for each probed port (e.g. `host:{ip}:smtp:25`, `host:{ip}:smtp:587`).
4. `network_check_tls` on port 465 if open — implicit TLS; check cert validity. Write `host:{ip}:tls:465`. Update `context:plan` to mark steps 2–4 done. **Call `context_rollback` now.** (If port 465 is not open: still call `context_rollback` after confirming all SMTP KV writes.)
5. `network_query_dns` with `type=MX` for the domain — verify MX records point to this host.
6. `network_query_dns` with `type=TXT` for the domain — check SPF record.
7. `network_query_dns` with `name=_dmarc.{domain}` and `type=TXT` — check DMARC policy.
8. `network_query_dns` with `type=TXT` for DKIM selector if known — check DKIM public key.

If a step fails or a port is not open: note it in one line and continue.

---

## Step 9 — Finalize

Confirm all project KV writes are complete. Clear session context keys: `agent_memory_clear {scope:"session", filter:"context:"}`. Call `skill_deactivate`.

---

## What to report

| Port | Banner | STARTTLS | AUTH Methods | Size Limit | Notes |
|------|--------|----------|--------------|------------|-------|

- Flag open relay risk if AUTH is not required.
- Flag missing STARTTLS on port 25/587.
- Flag SPF missing or too permissive (`+all`).
- Flag missing DMARC or `policy=none`.
- Flag certificate issues on port 465.
- Action items: "enforce AUTH", "add SPF", "set DMARC p=quarantine", etc.
