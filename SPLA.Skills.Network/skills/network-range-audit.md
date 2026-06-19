---
id: network.range-audit
description: IPv4 subnet/range audit — full host inventory, scan every IP, reverse DNS, write per-host report. Trigger on: audit subnet, scan range, investigate /24, network segment mapping, all hosts in CIDR.
---

# Network Range Audit

## Memory Keys Used by This Skill

### session scope
- `context:plan` — (object) High-level block progress, stored in session scope
- `context:step` — (string) Currently executing block and step

### project scope
- `host:{ip}` — (object, one key per discovered host) Per-host inventory record using the shared network schema (see plugin default_prompt). Written as each host is confirmed; readable by other skills.

### State handling:
- write execution state, progress, and block indexes to KV memory in session scope;
- write per-host inventory records to KV memory in project scope.

Per-host KV schema is defined in the plugin `default_prompt`. Write to `host:{ip}` (base) and sub-keys `host:{ip}:tcp`, `host:{ip}:tls`, `host:{ip}:http`, `host:{ip}:smtp`, `host:{ip}:dns` as evidence accumulates. Always use project scope.

## Execution protocol — mandatory checkpointing

### Context snapshots — two levels

Without checkpoints the context grows unbounded across blocks and hosts, and model performance degrades.

**Level 1 — per-block** (presence / basic / full modes):

```
checkpoint_save                          ← before block discovery steps
→ steps 1–5: reverse DNS, ping discovery, TCP presence, ARP, host-set write
→ agent_memory_set {key:"host:{ip}", scope:"project"}      ← write base records for all found hosts
→ agent_memory_set {key:"context:plan", scope:"session"}   ← advance current_block
context_rollback                      ← clear block reasoning, move to next block
```

**Level 2 — per-host** (full mode only, step 6):

```
checkpoint_save                          ← before this host's service audit
→ network_scan_tcp_ports, network_probe_tcp, network_check_tls,
  network_http_get/head, network_probe_smtp …
→ agent_memory_set {key:"host:{ip}:tcp/tls/http/smtp", scope:"project"}
→ agent_memory_set {key:"context:plan", scope:"session"}   ← mark this host done
context_rollback                      ← clear host reasoning, move to next host
```

After every `context_rollback` the orchestrator truncates back to the anchor (the message that was current when `checkpoint_save` was called) and calls the model again. The working memory injection re-reads `context:plan`, so the model knows which block/host is next without any conversational memory.

**Rules (both levels):**
- Always write all KV records **before** calling `context_rollback` — KV is the only state that survives.
- After restore, re-read `context:plan` from session KV to determine next target.
- Skip restore only for the **last host in the last block** — proceed directly to the block summary / final report.
- Never call `context_rollback` mid-scan (while tools for the current host are still running).

### Step 0 — Confirm tools, then create the plan

Before writing the plan, call `agent_info` with each tool name you intend to use to confirm it is registered.
Prefer lower_snake_case network tools when available — they return structured data and handle errors cleanly.
If a specific lower_snake_case network tool is absent, fall back to `system_run_shell` (cmd/shell) to accomplish the same step.
Build the plan from the tools that are actually available.

Write the complete block and step plan to `context:plan` in session-scoped KV:

```json
{
  "total_blocks": "<N>",
  "current_block": 0,
  "current_step": "tool-confirmation",
  "mode": "<presence|basic|full>",
  "blocks": [
    {
      "start": "<start>",
      "end": "<end>",
      "done": false,
      "steps": [
        { "id": "dns-reverse",    "done": false },
        { "id": "icmp-discovery", "done": false },
        { "id": "tcp-presence",   "done": false },
        { "id": "arp-enrichment", "done": false },
        { "id": "host-set",       "done": false },
        {
          "id": "service-audit",
          "done": false,
          "hosts": [
            { "ip": "<ip>", "done": false }
          ],
          "current_host": "<ip or null>"
        },
        { "id": "block-summary",  "done": false }
      ]
    }
  ]
}
```

In full mode, populate `service-audit.hosts` after the `host-set` step (once the potential-host list is known). Set `current_host` to the IP being audited. After each per-host restore, update `hosts[x].done = true` and advance `current_host` before writing `context:plan`.

Initialize `context:step` with the first block and first executable step.

After writing the plan: read it back from KV, confirm it looks correct, then start Block 1.
This read-back verification is required: the tool choices, blocks, and mode must match the user's bounded target before scanning.

### Between every step

Before executing each step:
1. Read `context:plan` and `context:step` from session KV.
2. Identify the next unfinished step in `context:plan`.
3. Execute that step.
4. For each host discovered or enriched: write/update `host:{ip}` in project KV immediately — do not batch host data.
5. Update `context:plan.current_block`, the step/block status, and `context:step` in session KV.
6. Continue from KV state only; do not keep raw scan results only in conversational context.

Re-read KV state before every step to maintain position across context resets.

**At the end of each block (before moving to the next):**
Verify all host KV writes and the block index update are confirmed, then call `context_rollback`. Do not call it mid-block or before host KV writes are complete. Skip the restore on the final block.

### Working data

- Per-host data belongs in `host:{ip}` project KV keys — written incrementally as evidence accumulates.
- When a step produces data for multiple hosts (e.g. bulk reverse DNS), write each host's record separately before moving to the next step.
- Do not create intermediate plan files, per-step files, or per-block report files during execution.
- **Never summarize or report results inline without first writing them to KV.** The final report is built from KV — if data is not in KV it does not exist. If a step discovered hosts, the next tool call must be `agent_memory_set {key:"host:{ip}", ...}`, not a scan or a text response.

## Clarification

Ask one short clarification before scanning only when the task does not specify the desired depth:

`Scan depth: presence only (ping/DNS), basic (ping + common TCP ports), or full service audit (all open ports, banner grab, TLS/HTTP)?`

Do not ask if the user already specifies one of these modes:

- `presence`: ping all addresses and reverse-resolve every address.
- `basic`: ping all addresses, probe common host-presence ports, reverse-resolve every address.
- `full`: run `basic`, then scan common ports on every potential host and identify services.

Always ask for an explicit bounded IPv4 CIDR or start/end range if the range is missing. Do not infer or broaden a scan target silently.

## Range blocking

Split the target into consecutive blocks of at most 256 IPv4 addresses.

Rules:
- For `/24` ranges, each block is the natural `/24`.
- For larger or arbitrary ranges, use `[blockStart, blockEnd]` windows of 256 addresses.
- Process blocks sequentially so each block can produce its own KV summary and conclusion.
- Use CIDR or explicit `start` and `end` for each block; the scan tool must test every address in the requested block.
- Use `max_hosts` no larger than 256 for each `network_discover_hosts` call.
- Use `concurrency: 256` for block discovery unless the user asks for a lower intensity.

## Execution sequence

For each 256-address block:

1. `network_reverse_dns` for every IP in the block — record PTR/DNS names even if the host does not answer.
2. `network_discover_hosts` with `cidr` set to the block CIDR (e.g. `172.16.0.0/24`) or `start`+`end` explicitly; `ping:true`, no `ports` — discover ICMP-responsive hosts. **Always pass `cidr` or `start`/`end` — the tool requires one of them.**
3. If mode is `basic` or `full`, run `network_discover_hosts` again with the same `cidr`/`start`+`end`, `ports: common`, `ping:false` — find hosts that do not answer ICMP but expose a common TCP service.
4. `network_get_arp_cache` — enrich local-neighbor hosts with MAC/interface data where available.
5. Build the potential-host set as the union of:
   - any IP with a PTR/DNS name,
   - any IP found by ping,
   - any IP found by port-presence probing,
   - any IP found in ARP for this block.
   For every IP in the potential-host set: create or update `host:{ip}` in project KV with all evidence collected so far. Use `class: "empty"` for IPs with no evidence at all only at block-summary time.
6. If mode is `full`, for every potential host — **one host at a time with per-host checkpoint/restore**:

   ```
   checkpoint_save                              ← snapshot before this host
   → network_scan_tcp_ports
   → network_probe_tcp on every open port
   → network_check_tls on TLS ports
   → network_http_head + network_http_get on HTTP ports
   → network_check_http_redirects if redirects present
   → network_probe_smtp on port 25/587 if open
   → agent_memory_set {key:"host:{ip}:tcp",  scope:"project"}    ← write ALL sub-keys before restore
   → agent_memory_set {key:"host:{ip}:tls",  scope:"project"}
   → agent_memory_set {key:"host:{ip}:http", scope:"project"}
   → agent_memory_set {key:"context:plan",   scope:"session"}    ← mark this host done in plan
   context_rollback                          ← clear host reasoning, move to next host
   ```

   **Rules for per-host loop:**
   - Write every sub-key (`host:{ip}:tcp`, `:tls`, `:http`, `:smtp`) to project KV **before** calling `context_rollback`.
   - After restore, re-read `context:plan` from session KV to find the next unaudited host — do not rely on conversational memory.
   - Skip `context_rollback` only for the **last host in the last block** — proceed directly to the block summary.
   - `network_probe_udp` only when a concrete service reason exists and payload is known.

   If a tool fails for one host, write the failure into `host:{ip}` and restore to move on.

If a tool fails for one host, record the failure in that host row and continue.

## Classification

Classify every address in the block:

- `empty`: no DNS, no ping, no open host-presence port, no ARP entry.
- `dns-only`: PTR/DNS exists, but no ping/port/ARP evidence.
- `icmp-only`: ping answers, but no DNS or open probed port.
- `tcp-only`: no ping, but a host-presence TCP port is open.
- `active`: multiple evidence types or full service audit data exists.

For `full` mode, identify likely device/service type from exact evidence:

- Use DNS names, banners, HTTP headers/titles, TLS certificate CN/SAN/issuer, SMTP EHLO, SSH/FTP banners, common management ports, and TTL hints.
- Do not invent a device type when evidence is weak. Use `unknown` plus the exact signals.
- Keep version strings exact when available.

## Final report

After all blocks are processed, create the final markdown report file from KV results when file tools are available. Files are for the final collection and conclusions only.

Collect all host records: `agent_memory_list {scope:"project", filter:"host:*"}` — get full inventory. For sub-key data use `filter:"host:*:tcp"` etc.

`network-range-summary.md`

The final report must contain:

1. Overall scope and selected mode.
2. One table per range block:

`Block | Addresses | DNS-only | ICMP | TCP-only | Active | Empty | Errors | Notable Findings`

3. Host inventory table sorted by IP. It must include every IP address in scope, including `empty` hosts:

`IP | DNS/PTR | Class | Ping | Open Ports | Service Guess | Evidence | MAC | Notes`

4. Full-mode service details table when services were audited:

`IP | Port | Protocol | Version/Banner | TLS/HTTP/SMTP Details | Security Notes | Action Items`

5. Cross-range findings:
   - naming patterns,
   - likely server/network-equipment/client zones,
   - duplicated hostnames or suspicious PTR mismatches,
   - high-value exposed services,
   - dead DNS records,
   - recommended next scans.

6. Explicit limitations:
   - ICMP may be filtered.
   - Common-port probing is not exhaustive.
   - Full mode identifies only what banners/protocol probes reveal.
   - UDP service detection is partial unless protocol-specific payloads were used.

Write the final summary to `task:range-audit:summary` in KV. The final assistant response must list the written final report path. If no file was written, explicitly state that file tools were unavailable and the final report was provided inline.

## Final step — Finalize

Clear session context keys with `agent_memory_clear {scope:"session", filter:"context:"}`. Then call `skill_deactivate`.

## Safety and scope

- Scan only user-specified private or owned/administered ranges.
- For public internet ranges, ask the user to confirm authorization before scanning.
- Never use `ports: all` unless the user explicitly asks for exhaustive port scanning.
- Prefer `common` ports for broad ranges; reserve `1-1024` or custom port lists for explicit requests.
- Keep timeouts conservative by default: 500 ms for discovery, 3000 ms for banner/service probes.
