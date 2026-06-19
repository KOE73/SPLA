---
id: network.host-audit
description: Deep autonomous investigation of a single host — DNS, ping, port scan, banner grab, TLS, HTTP. Trigger on: investigate host, audit server, check open ports, what is running on, scan host.
---

# Network Host Audit

## Memory Keys Used by This Skill

### session scope
- `context:plan` — (object) Step list and current progress
- `context:step` — (number) Currently executing step index

### project scope
- `host:{ip}` — base record: ping, mac, vendor, class, tags, scanned_at
- `host:{ip}:dns` — DNS resolution results (step 1 + 8)
- `host:{ip}:tcp` — open ports and banners (steps 3–4)
- `host:{ip}:tls` — TLS/SSL details (step 5)
- `host:{ip}:http` — HTTP response metadata (step 6)

### State handling
- session: execution state and plan only
- project: all host findings written incrementally as each step completes
- Schema for each key is defined in the plugin default_prompt

## Context snapshots

Steps 3–4 (port scan + banner grab) can produce large output that fills the context before TLS/HTTP analysis begins. Use a snapshot between the scan phase and the analysis phase:

```
checkpoint_save                         ← after step 2 (ping confirmed), before port scan
→ step 3: network_scan_tcp_ports
→ step 4: network_probe_tcp on every port
→ agent_memory_set {key:"host:{ip}:tcp", scope:"project"} ← write banners before restore
→ agent_memory_set {key:"context:plan", scope:"session"}  ← mark steps 3-4 done
context_rollback                     ← clean context; KV has tcp data
→ step 5: network_check_tls     (reads host:{ip}:tcp from KV)
→ step 6: network_http_get/head (reads host:{ip}:tcp from KV)
→ step 7: network_get_arp_cache
→ step 8: network_reverse_dns
→ step 9: finalize
```

**Rules:**
- Write `host:{ip}:tcp` to project KV and update `context:plan` to session KV **before** calling `context_rollback`.
- Steps 5–8 derive their port list from `host:{ip}:tcp` in KV — not from conversational context.
- If the port scan finds no open ports: skip the restore and continue directly to step 7.
- If called from a parent loop (e.g. `agent_spawn_batch`): the parent manages cross-host snapshots. Still use the intra-host snapshot above.

## Step 0 — Confirm tools and initialize plan

Call `agent_info` with each tool name you intend to use to confirm it is registered.
Prefer lower_snake_case network tools when available — they return structured data and handle errors cleanly.
If a specific lower_snake_case network tool is absent, fall back to `system_run_shell` (cmd/shell) to accomplish the same step.
Adapt the plan below based on what is actually available.

Write the full plan to `context:plan` in session KV:

```json
{
  "total_steps": 9,
  "current_step": 0,
  "checkpoint_after": 4,
  "steps": [
    "Step 1: network_resolve_host — resolve A/AAAA/PTR → host:{ip}:dns",
    "Step 2: network_ping_host — reachability, RTT, TTL → host:{ip}",
    "Step 3: network_scan_tcp_ports — common service preset → host:{ip}:tcp",
    "Step 4: network_probe_tcp on every open port — banners → host:{ip}:tcp  [checkpoint_set before step 3, restore after step 4]",
    "Step 5: network_check_tls on TLS ports → host:{ip}:tls",
    "Step 6: network_http_get + network_http_head on HTTP ports → host:{ip}:http",
    "Step 7: network_get_arp_cache — resolve MAC and OUI → host:{ip}",
    "Step 8: network_reverse_dns — PTR record → host:{ip}:dns",
    "Step 9: finalize — clear context:, deactivate"
  ]
}
```

## Execution sequence

After each numbered step, update both `context:plan.current_step` and `context:step` in session KV to that step number. Write findings to project KV immediately after each step — do not batch.

1. `network_resolve_host` — resolve target; check A/AAAA/PTR. Write `host:{ip}:dns`.
2. `network_ping_host` — reachability, RTT, TTL (64=Linux, 128=Windows, 255=network gear). Write/update `host:{ip}` with ping result and class.
3. Call `checkpoint_save`. Then `network_scan_tcp_ports` — omit `ports` parameter (built-in 49-port preset). Write initial `host:{ip}:tcp` with port list.
4. `network_probe_tcp` on **every open port** — grab banner, identify service and version; do not skip a port. Update `host:{ip}:tcp` with banners and service guesses. Update `context:plan` to mark steps 3-4 done. Call `context_rollback`.
5. `network_check_tls` — on every port from `host:{ip}:tcp` in KV that returned a TLS banner or is 443/8443. Write `host:{ip}:tls`.
6. `network_http_get` + `network_http_head` — on every HTTP port from `host:{ip}:tcp` in KV; extract Server, X-Powered-By, cookies, redirects. Write `host:{ip}:http`.
7. `network_get_arp_cache` — resolve MAC of target; note OUI vendor. Update `host:{ip}` with mac and vendor.
8. `network_reverse_dns` — PTR record. Update `host:{ip}:dns` with ptr.

If a step fails (host unreachable, port closed), note it in one line and continue.

## Step 9 — Finalize

Ensure all project KV writes are confirmed. Clear session context keys with `agent_memory_clear {scope:"session", filter:"context:"}`. Then call `skill_deactivate`.

## Output format

Final report: one compact markdown table per service.

Columns: `Port | Service | Version/Banner | Security Notes | Action Items`

Rules:
- Skip obvious facts; every sentence must carry information density an admin cannot already infer.
- Action items = concrete admin tasks: "patch OpenSSH", "enforce MQTT auth", "disable SMBv1".
- The user is a sysadmin/network engineer — do NOT explain what SSH, MQTT, SMB are.
- Write exact software versions, build strings, OS from TTL/banner, config flags, anomalies, attack surface.
