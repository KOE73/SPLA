---
id: network.lan-discovery
description: LAN host discovery and inventory — sweep a subnet, enrich IPs with MACs and hostnames.
---

# LAN Discovery

Run when the user asks to "scan the network", "discover hosts", "map the LAN", "найди хосты", "что в сети".

## Execution sequence

1. `network.host.info` — local identity, interfaces, gateways; derive the local subnet from this
2. `network.route` — routing table; identify all connected subnets
3. `network.arp` — known MAC neighbors already cached by the OS
4. `network.scan.lan` — discover live hosts in the local subnet (use CIDR from step 1; ask the user only if ambiguous)
5. `network.dns.reverse` (batch) — enrich all discovered IPs with hostnames
6. `network.arp` again (if needed) — resolve MACs for newly discovered hosts

Do NOT pause between steps. Run the full sequence, then summarize.

## Output format

Single markdown table: `IP | Hostname | MAC | OUI Vendor | Notes`

- Sort by IP.
- Mark the local host row.
- Note any IP that has no PTR record.
- Note any MAC with unknown OUI.
