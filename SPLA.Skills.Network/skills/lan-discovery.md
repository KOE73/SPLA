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

### State handling
- session: execution state and plan only
- project: host records written as soon as a host is identified, updated as enrichment arrives
- Schema for each key is defined in the plugin default_prompt. Do not duplicate it here.

## Step 0 — Confirm tools and initialize plan

Call `agent_info` with each tool name you intend to use to confirm it is registered.
Prefer lower_snake_case network tools when available — they return structured data and handle errors cleanly.
If a specific lower_snake_case network tool is absent, fall back to `system_run_shell` (cmd/shell: arp -a, ping) to accomplish the same step.
Adapt the plan below based on what is actually available.

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
    "Step 6: write host:{ip} per discovered host to project KV (checkpoint before, restore after)",
    "Step 7: finalize — clear context:, deactivate"
  ]
}
```

## Context snapshots

After step 5 the context may be large if the subnet has many hosts. Use a snapshot before writing host KV records:

```
checkpoint_save
→ write host:{ip} scope:project for every discovered host
→ update context:plan / context:step scope:session
context_rollback
```

After restore the orchestrator calls the model again with clean context. KV injection shows updated plan — proceed to the next step.

## Execution sequence

After each numbered step, update both `context:plan.current_step` and `context:step` in session KV.

1. `network_get_host_info` — local identity, interfaces, gateways; derive the local subnet from this.
2. `network_get_routes` — routing table; identify all connected subnets.
3. `network_get_arp_cache` — known MAC neighbors already cached by the OS.
4. `network_discover_hosts` — discover live hosts in the local subnet; pass `cidr` (from step 1) or `start`+`end`; ask the user only if the subnet is ambiguous.
5. `network_reverse_dns` (batch) — enrich all discovered IPs with hostnames.
6. Write `host:{ip}` in project KV for every discovered host using data from steps 3–5. Use `checkpoint_save` before this step and `context_rollback` after all writes are confirmed.

**After each host KV write: confirm `agent_memory_set` returned `ok:` before moving to the next host. Never skip a write.**

If a step fails, note it in one line and continue.

## Step 7 — Finalize

Clear session context keys with `agent_memory_clear {scope:"session", filter:"context:"}`. Then call `skill_deactivate`.

## Output format

Build the final table from `host:{ip}` project KV keys — not from conversational context.
Use `agent_memory_list {scope:"project", filter:"host:*"}` to collect all records.

Single markdown table: `IP | Hostname | MAC | OUI Vendor | Notes`

- Sort by IP.
- Mark the local host row.
- Note any IP that has no PTR record.
- Note any MAC with unknown OUI.
