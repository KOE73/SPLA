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
- `host:{ip}:smtp` — SMTP probe findings per port

### State handling
- session: execution state and plan only
- project: written at finalize step if target resolves to an IP. Schema defined in plugin default_prompt.
- If the target is a domain name only (no resolved IP), skip host:{ip} writes and report inline.

## Step 0 — Confirm tools and initialize plan

Call `agent_info` with each tool name you intend to use to confirm it is registered.
Prefer lower_snake_case network tools when available — they return structured data and handle errors cleanly.
If a specific lower_snake_case network tool is absent, fall back to `system_run_shell` (cmd/shell: telnet, openssl s_client) to accomplish the same step.
Adapt the plan below based on what is actually available.

Write the full plan to `context:plan` in session KV:

```json
{
  "total_steps": 9,
  "current_step": 0,
  "checkpoint_after": 4,
  "steps": [
    "Step 1: network_ping_host — basic reachability → host:{ip}",
    "Step 2: network_scan_tcp_ports with ports=25,465,587,2525",
    "Step 3: network_probe_smtp on each open SMTP port → host:{ip}:smtp",
    "Step 4: network_check_tls on port 465 if open → host:{ip}:tls  [checkpoint_set before step 2, restore after step 4]",
    "Step 5: network_query_dns type=MX — verify MX records",
    "Step 6: network_query_dns type=TXT — check SPF",
    "Step 7: network_query_dns name=_dmarc.{domain} type=TXT — check DMARC",
    "Step 8: network_query_dns type=TXT for DKIM selector if known",
    "Step 9: finalize — write project KV, clear context:, deactivate"
  ]
}
```

## Context snapshot

SMTP probe output (EHLO responses, STARTTLS negotiation) and TLS details can be verbose. Use a snapshot between the SMTP phase and the DNS phase:

```
checkpoint_save                              ← before step 2 (port scan)
→ step 2: network_scan_tcp_ports
→ step 3: network_probe_smtp
→ step 4: network_check_tls
→ agent_memory_set {key:"host:{ip}:smtp", scope:"project"}
→ agent_memory_set {key:"host:{ip}:tls", scope:"project"}
→ agent_memory_set {key:"context:plan", scope:"session"}       ← mark steps 2-4 done
context_rollback                          ← clean context; KV has smtp/tls data
→ step 5–8: DNS record queries
→ step 9: finalize
```

## Execution sequence

After each numbered step, update both `context:plan.current_step` and `context:step` in session KV.

1. `network_ping_host` — basic reachability. Write `host:{ip}` in project KV with ping result.
2. Call `checkpoint_save`. Then `network_scan_tcp_ports` with `ports=25,465,587,2525` — find which SMTP ports are open.
3. `network_probe_smtp` on each open port — EHLO handshake; captures banner, STARTTLS, AUTH methods, SIZE, extensions. Write `host:{ip}:smtp` in project KV.
4. `network_check_tls` on port 465 if open — implicit TLS; check cert validity. Write `host:{ip}:tls` in project KV. Update `context:plan` to mark steps 2-4 done. Call `context_rollback`.
5. `network_query_dns` with `type=MX` for the domain — verify MX records point to this host.
6. `network_query_dns` with `type=TXT` for the domain — check SPF record.
7. `network_query_dns` with `name=_dmarc.{domain}` and `type=TXT` — check DMARC policy.
8. `network_query_dns` with `type=TXT` for DKIM selector if known — check DKIM public key.

If a step fails or a port is not open, note it in one line and continue.

## Step 9 — Finalize

Confirm all project KV writes are complete. Clear session context keys with `agent_memory_clear {scope:"session", filter:"context:"}`. Then call `skill_deactivate`.

## What to report

| Port | Banner | STARTTLS | AUTH Methods | Size Limit | Notes |
|------|--------|----------|--------------|------------|-------|

- Flag open relay risk if AUTH is not required.
- Flag missing STARTTLS on port 25/587.
- Flag SPF missing or too permissive (`+all`).
- Flag missing DMARC or policy=none.
- Flag certificate issues on port 465.
- Action items: "enforce AUTH", "add SPF", "set DMARC p=quarantine", etc.
