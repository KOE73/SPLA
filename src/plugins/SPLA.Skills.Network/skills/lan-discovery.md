---
id: network.lan-discovery
description: LAN host discovery and inventory — sweep a subnet, enrich IPs with MACs and hostnames. Trigger on: discover hosts, what's on the network, who is online, network inventory, find devices.
---

# LAN Discovery

## Memory Keys Used by This Skill

### session scope
- `context:plan` — (object) Step list and current progress
- `context:step` — (number) Currently executing step index

### project scope
- `host:{ip}` — per-host base record: ip, hostname, mac, vendor, ping, class, tags, scanned_at
- `host:{ip}:tcp` — open ports if port scan was performed

Schema for each key is defined in the plugin `default_prompt`.

---

## MANDATORY checkpoint protocol

After discovery (steps 1–5), writing all host KV records can produce a long sequence of tool calls. A checkpoint is required to keep the context clean for the finalize step.

Rules — no exceptions:

- **FORBIDDEN:** starting KV write loop (step 6) without a preceding `checkpoint_save`.
- **FORBIDDEN:** calling `context_rollback` before EVERY `host:{ip}` write for all discovered hosts is confirmed with `ok:` and `context:plan` is updated.
- **REQUIRED:** after `context_rollback`, the FIRST call MUST be `agent_memory_get {key:"context:plan", scope:"session"}` — not a tool, not a scan.
- **REQUIRED:** confirm each `agent_memory_set` returns `ok:` before writing the next host. Never skip a write.

```
checkpoint_save                                           ← REQUIRED before step 6
→ agent_memory_set {key:"host:{ip}", scope:"project"}    ← repeat for EVERY discovered host
→ agent_memory_set {key:"context:plan", scope:"session"} ← mark step 6 done
context_rollback                                         ← REQUIRED after all writes confirmed
→ read context:plan from KV — proceed to step 7
```

---

## Step 0 — Confirm tools and initialize plan

Call `agent_info` with each tool name you intend to use to confirm it is registered.
Prefer lower_snake_case network tools — they return structured data and handle errors cleanly.
Fall back to `system_run_shell` (arp -a, ping) only when a specific tool is absent.

Write the full plan to `context:plan` in session KV:

```json
{
  "total_steps": 7,
  "current_step": 0,
  "steps": [
    "Step 1: network_get_host_info — local identity and subnet",
    "Step 2: network_get_routes — routing table and connected subnets",
    "Step 3: network_get_arp_cache — cached MAC neighbors",
    "Step 4: network_discover_hosts — live host discovery",
    "Step 5: network_reverse_dns (batch) — enrich IPs with hostnames",
    "Step 6: checkpoint_save → write host:{ip} per host to project KV → context_rollback",
    "Step 7: finalize — build report from KV, clear context:, deactivate"
  ]
}
```

---

## Execution sequence

After each step: update `context:plan.current_step` and `context:step` in session KV.

1. `network_get_host_info` — local identity, interfaces, gateways. Derive the local subnet from this result.
2. `network_get_routes` — routing table; identify all connected subnets.
3. `network_get_arp_cache` — MAC neighbors already cached by the OS.
4. `network_discover_hosts` — discover live hosts in the local subnet. Pass `cidr` (from step 1) or `start`+`end`; ask the user only if the subnet is ambiguous.
5. `network_reverse_dns` (batch) — enrich all discovered IPs with hostnames.
6. **Call `checkpoint_save` now.** Write `host:{ip}` in project KV for every discovered host using data from steps 3–5. Confirm each write returns `ok:` before the next. Update `context:plan` to mark step 6 done. **Call `context_rollback` now.** After rollback: first call is `agent_memory_get {key:"context:plan"}`.

If a step fails: note it in one line and continue.

---

## Step 7 — Finalize

Build the final table from KV — not from conversational context:

```
agent_memory_list {scope:"project", filter:"host:*"}
```

Single markdown table: `IP | Hostname | MAC | OUI Vendor | Notes`

- Sort by IP.
- Mark the local host row.
- Note any IP with no PTR record.
- Note any MAC with unknown OUI.

Clear session context keys: `agent_memory_clear {scope:"session", filter:"context:"}`. Call `skill_deactivate`.
