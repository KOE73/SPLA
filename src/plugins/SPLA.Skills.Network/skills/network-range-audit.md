---
id: network.range-audit
description: IPv4 subnet/range bulk audit — fast host discovery using one tool call per /24 block, then per-host service audit in full mode. Best for presence/basic sweeps and for full audit when the range is known to have moderate density. Trigger on: audit subnet, scan range, investigate /24, network segment mapping, all hosts in CIDR, fast scan.
---

# Network Range Audit (Bulk)

Bulk strategy: the unit of context management is one 256-address block. Discovery tools sweep an entire /24 in a single call; context is reset between blocks and between hosts (full mode). Fast, but context per block grows proportionally to discovered hosts.

When the user does not specify a strategy, offer a choice:
> **Bulk** (this skill) — one tool call per /24, fast. Recommended for presence/basic sweeps and ranges ≤ /16.
> **Loop** (`network.range-audit-loop`) — one IP at a time, slower but constant context size. Better for full-mode audits of dense ranges or when the model struggles with large bulk results.

---

## Memory Keys

### session scope
- `context:plan` — full execution plan with block progress
- `context:step` — current block and step identifier

### project scope
- `host:{ip}` — base record (ping, dns, class, mac, tags, scanned_at)
- `host:{ip}:tcp` — open ports and banners (full mode)
- `host:{ip}:tls:{port}` — TLS certificate details per port (full mode, e.g. `host:{ip}:tls:443`)
- `host:{ip}:http:{port}` — HTTP response metadata per port (full mode, e.g. `host:{ip}:http:80`)
- `host:{ip}:smtp:{port}` — SMTP handshake details per port (full mode, e.g. `host:{ip}:smtp:25`)

Schema defined in plugin `default_prompt`. Always use project scope for host data.

---

## MANDATORY checkpoint protocol

- **FORBIDDEN:** starting any block's discovery steps without a preceding `checkpoint_save`.
- **FORBIDDEN:** calling `context_rollback` before ALL host KV writes for the current block/host are confirmed with `ok:`.
- **FORBIDDEN:** proceeding to the next block or host without `context_rollback` (exception: last item of the last block — go directly to final report).
- **FORBIDDEN:** calling `context_rollback` mid-scan while tool calls for the current item are still running.
- **REQUIRED:** after every `context_rollback` the FIRST call MUST be `agent_memory_get {key:"context:plan", scope:"session"}` — never assume conversational memory survived.
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

Write plan to `context:plan` (session KV):

```json
{
  "total_blocks": "<N>",
  "current_block": 0,
  "mode": "<presence|basic|full>",
  "current_step": "tool-confirmation",
  "blocks": [
    {
      "start": "<start_ip>",
      "end": "<end_ip>",
      "cidr": "<cidr_or_null>",
      "done": false,
      "service_queue": [],
      "current_host": null
    }
  ]
}
```

Read the plan back from KV and verify blocks, mode, and CIDR match the user's target before scanning.

---

## Range blocking

- Max 256 addresses per block. For `/24`: one block per subnet. For larger ranges: consecutive 256-address windows.
- Process blocks sequentially.
- `max_hosts` ≤ 256 per `network_discover_hosts` call. `concurrency: 256` unless user requests lower.

---

## Execution — per block

```
checkpoint_save                                          ← REQUIRED before any discovery
→ network_reverse_dns for every IP in the block
→ network_discover_hosts {cidr or start+end, ping:true}          ← ICMP sweep
→ (basic/full) network_discover_hosts {same range, ports:"common", ping:false}  ← TCP presence
→ network_get_arp_cache                                  ← MAC enrichment
→ Build host set: union of DNS / ping / TCP / ARP evidence
  Write host:{ip} to project KV for every IP in the host set
  (class:"empty" only at block-summary time for IPs with no evidence)
→ agent_memory_set {key:"context:plan", scope:"session"}         ← advance current_block, mark done
context_rollback                                         ← REQUIRED; skip only on last block
```

After `context_rollback`: FIRST call is `agent_memory_get {key:"context:plan"}`.

---

## Execution — full mode per-host service audit

After discovery, set `context:plan.blocks[i].service_queue` to a compact array of live IP strings — `["172.16.0.5", "172.16.0.12"]`, no objects, no done-flags. **FORBIDDEN:** pre-filling with all 256 addresses. Set `current_host` to `service_queue[0]`. For each host:

```
checkpoint_save                                          ← REQUIRED before this host
→ network_scan_tcp_ports {host: ip}
→ network_probe_tcp on every open port
→ network_check_tls on TLS ports
→ network_http_head + network_http_get on HTTP ports
→ network_check_http_redirects if redirects present
→ network_probe_smtp on port 25/587 if open
→ agent_memory_set {key:"host:{ip}:tcp",          scope:"project"}
→ agent_memory_set {key:"host:{ip}:tls:{port}",  scope:"project"}   ← one call per TLS port
→ agent_memory_set {key:"host:{ip}:http:{port}", scope:"project"}   ← one call per HTTP port
→ agent_memory_set {key:"host:{ip}:smtp:{port}", scope:"project"}   ← one call per SMTP port (only if probed)
→ agent_memory_set {key:"context:plan", scope:"session",
    value: {…plan, blocks[i]: {…block, service_queue: [remaining after pop], current_host: service_queue[1]}}}
context_rollback                                         ← REQUIRED; skip when service_queue has 1 element (last host)
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
1. Scope: CIDR, mode, strategy=bulk, scan date.
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
- Prefer `common` for broad ranges. Timeouts: 500 ms discovery, 3000 ms probes.
