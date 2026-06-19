---
id: network.range-audit-loop
description: IPv4 subnet/range audit using a per-IP loop — probes each address individually with a context reset per IP. Constant context growth regardless of range density. Best for full-mode audits, dense ranges, large /16+ targets, or when bulk results overwhelm the model. Trigger on: audit subnet per host, loop scan range, per-IP scan, deep range audit, scan each host individually.
---

# Network Range Audit (Per-IP Loop)

Loop strategy: the unit of context management is one IP address. Every IP is probed individually; context is reset after each IP. Context size stays constant throughout the entire range, regardless of how many hosts are found or how large the range is. Slower than bulk, but predictable and reliable for full-mode audits.

When the user does not specify a strategy, offer a choice:
> **Loop** (this skill) — one IP at a time, constant context size. Recommended for full-mode audits and dense ranges.
> **Bulk** (`network.range-audit`) — one tool call per /24, fast. Recommended for presence/basic sweeps.

---

## Memory Keys

### session scope
- `context:plan` — full execution plan with per-IP progress
- `context:step` — current block and IP identifier

### project scope
- `host:{ip}` — base record (ping, dns, class, mac, tags, scanned_at)
- `host:{ip}:tcp` — open ports and banners (full mode)
- `host:{ip}:tls:{port}` — TLS certificate details per port (full mode, e.g. `host:{ip}:tls:443`)
- `host:{ip}:http:{port}` — HTTP response metadata per port (full mode, e.g. `host:{ip}:http:80`)
- `host:{ip}:smtp:{port}` — SMTP handshake details per port (full mode, e.g. `host:{ip}:smtp:25`)

Schema defined in plugin `default_prompt`. Always use project scope for host data.

---

## MANDATORY checkpoint protocol

Every IP is one iteration of the checkpoint loop. These rules apply without exception:

- **FORBIDDEN:** probing any IP without a preceding `checkpoint_save` for that IP.
- **FORBIDDEN:** calling `context_rollback` before the `host:{ip}` KV write for the current IP is confirmed with `ok:` and `context:plan` is updated.
- **FORBIDDEN:** moving to the next IP without `context_rollback` (exception: last IP of the last block — go directly to service audit or final report).
- **FORBIDDEN:** calling `context_rollback` while tool calls for the current IP are still running.
- **REQUIRED:** after every `context_rollback` the FIRST call MUST be `agent_memory_get {key:"context:plan", scope:"session"}` — read position from KV, never from conversational memory.
- **REQUIRED:** every host record MUST be in project KV before `context_rollback`. If it is not in KV it does not exist.

---

## Clarification

Ask before scanning if not already specified:

> Scan depth: `presence` (ping + DNS) / `basic` (ping + common TCP ports) / `full` (all open ports, banner grab, TLS/HTTP)?

Always ask for an explicit bounded IPv4 CIDR or start/end range — never infer or broaden the target silently.
For public internet ranges: ask the user to confirm authorization before scanning.

---

## Step 0 — Confirm tools, create plan

Call `agent_info` for each tool you intend to use. Fall back to `system_run_shell` only when a specific tool is absent.

Write plan to `context:plan` (session KV). The plan is a **cursor**, not a checklist — never store the full IP list:

```json
{
  "total_blocks": "<N>",
  "current_block": 0,
  "mode": "<presence|basic|full>",
  "phase": "discovery",
  "block": {
    "cidr": "<cidr_or_null>",
    "start": "<first_ip>",
    "end": "<last_ip>",
    "current_ip": "<first_ip>"
  },
  "service_queue": []
}
```

**FORBIDDEN:** putting a list of IP objects (`[{"ip":..., "done":false}, ...]`) into the plan. The plan must not grow with the size of the range. The next IP is always `current_ip + 1`; done when `current_ip > block.end`.

`service_queue` — populated after Phase 1 completes: a compact array of live IP strings only, no flags: `["172.16.0.5", "172.16.0.12"]`. After each host's service audit, remove the first element and save the shorter array.

Read the plan back from KV and verify it matches the user's target before starting.

---

## Phase 1 — Discovery loop (all modes)

For each IP: probe it, write `host:{ip}` to KV, advance `current_ip` in the plan cursor, rollback.

```
checkpoint_save                                               ← REQUIRED before this IP
→ network_ping_host {host: current_ip, count: 1, timeout: 500}
→ network_reverse_dns {ip: current_ip}
→ (basic/full) network_scan_tcp_ports {host: current_ip, ports: "common", timeout: 500}
→ network_get_arp_cache                                       ← check if current_ip appears
→ Determine class: empty / dns-only / icmp-only / tcp-only / active
→ agent_memory_set {key:"host:{current_ip}", scope:"project"} ← base record, REQUIRED before rollback
→ agent_memory_set {key:"context:plan", scope:"session",
    value: {…plan, block: {…block, current_ip: "<next_ip>"}}} ← advance cursor; set phase:"done" when current_ip passes block.end
context_rollback                                              ← REQUIRED; skip only when current_ip == block.end (last IP)
```

After `context_rollback`: FIRST call is `agent_memory_get {key:"context:plan"}` — read `block.current_ip` and continue.

When `block.current_ip` passes `block.end`: Phase 1 is complete. Move to Phase 2 (full mode) or Final report.

If `network_ping_host` or `network_scan_tcp_ports` is unavailable: use `system_run_shell` (ping, nmap) as fallback.

---

## Phase 2 — Full mode service audit

After Phase 1 is complete for all blocks, collect live hosts:

```
agent_memory_list {scope:"project", filter:"host:*"}
```

Filter to IPs where `class != "empty"`. Write the compact IP list to `context:plan.service_queue` as a plain array of strings: `["172.16.0.5", "172.16.0.12"]` — no objects, no done-flags. Save updated plan to KV. The first element is always the current host; after each audit it is removed.

For each live host:

```
checkpoint_save                                               ← REQUIRED before this host
→ network_scan_tcp_ports {host: ip}                          ← full preset, not just common
→ network_probe_tcp on every open port
→ network_check_tls on TLS ports
→ network_http_head + network_http_get on HTTP ports
→ network_check_http_redirects if redirects present
→ network_probe_smtp on port 25/587 if open
→ agent_memory_set {key:"host:{ip}:tcp",          scope:"project"}   ← ALL sub-keys before rollback
→ agent_memory_set {key:"host:{ip}:tls:{port}",  scope:"project"}   ← one call per TLS port
→ agent_memory_set {key:"host:{ip}:http:{port}", scope:"project"}   ← one call per HTTP port
→ agent_memory_set {key:"host:{ip}:smtp:{port}", scope:"project"}   ← one call per SMTP port (only if probed)
→ agent_memory_set {key:"context:plan", scope:"session",
    value: {…plan, service_queue: [remaining IPs after removing first]}} ← pop current host from queue
context_rollback                                              ← REQUIRED; skip when service_queue will be empty after pop
```

If a tool fails for a host: write the failure into `host:{ip}` and call `context_rollback` to move on.

---

## Classification

- `empty`: no DNS, no ping, no open port, no ARP.
- `dns-only`: PTR/DNS exists, no other evidence.
- `icmp-only`: ping responds, no DNS or open port.
- `tcp-only`: no ping, but a host-presence TCP port is open.
- `active`: multiple evidence types or full service audit data.

Full mode: identify device/service type from DNS names, banners, HTTP headers/titles, TLS CN/SAN/issuer, SMTP EHLO, SSH/FTP banners, management ports, TTL hints. Use `unknown` when evidence is weak. Keep version strings exact.

---

## Final report

Collect all host records:
```
agent_memory_list {scope:"project", filter:"host:*"}
agent_memory_list {scope:"project", filter:"host:*:tcp"}
agent_memory_list {scope:"project", filter:"host:*:tls:*"}
agent_memory_list {scope:"project", filter:"host:*:http:*"}
```

Write to file `network-range-summary.md` when file tools are available.

Sections:
1. Scope: CIDR, mode, strategy=loop, scan date.
2. Block summary: `Block | Addresses | DNS-only | ICMP | TCP-only | Active | Empty | Errors | Notable Findings`
3. Host inventory (every IP, sorted, including empty): `IP | DNS/PTR | Class | Ping | Open Ports | Service Guess | Evidence | MAC | Notes`
4. Service details (full mode): `IP | Port | Protocol | Version/Banner | TLS/HTTP/SMTP Details | Security Notes | Action Items`
5. Cross-range findings: naming patterns, dead DNS, high-value services, suspicious PTR mismatches, recommended next scans.
6. Limitations: ICMP may be filtered; common-port probing is not exhaustive; UDP detection is partial.

Write final summary to `task:range-audit:summary` in session KV. State the file path in the final response.

---

## Finalize

`agent_memory_clear {scope:"session", filter:"context:"}`. Call `skill_deactivate`.

---

## Safety

- Scan only user-specified private or owned/administered ranges.
- Never use `ports: all` without explicit user request.
- Prefer `common` for discovery phase. Timeouts: 500 ms discovery, 3000 ms probes.
