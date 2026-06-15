---
id: network.lan-discovery
description: LAN host discovery and inventory — sweep a subnet, enrich IPs with MACs and hostnames. Trigger on: discover hosts, what's on the network, who is online, network inventory, find devices.
---

# LAN Discovery

## Tool availability

Before starting, call `agent.info` with each tool name you intend to use to confirm it is registered.
Prefer `network.*` plugin tools when available — they return structured data and handle errors cleanly.
If a specific `network.*` tool is absent, fall back to `RunCommandTool` (cmd/shell: arp -a, ping) to accomplish the same step.
Adapt the execution sequence below based on what is actually available.

Run when the user asks to "scan the network", "discover hosts", "map the LAN", "найди хосты", "что в сети".

## Execution sequence

1. `network.host.info` — local identity, interfaces, gateways; derive the local subnet from this
2. `network.host.route` — routing table; identify all connected subnets
3. `network.host.arp` — known MAC neighbors already cached by the OS
4. `network.scan.lan` — discover live hosts in the local subnet; always pass `cidr` (from step 1) or `start`+`end`; ask the user only if subnet is ambiguous
5. `network.dns.reverse` (batch) — enrich all discovered IPs with hostnames
6. `network.host.arp` again (if needed) — resolve MACs for newly discovered hosts

Do NOT pause between steps. Run the full sequence, then summarize.

## Output format

Single markdown table: `IP | Hostname | MAC | OUI Vendor | Notes`

- Sort by IP.
- Mark the local host row.
- Note any IP that has no PTR record.
- Note any MAC with unknown OUI.
