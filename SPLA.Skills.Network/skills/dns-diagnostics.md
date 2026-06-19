---
id: network.dns-diagnostics
description: DNS troubleshooting — resolution, record types, propagation check across public resolvers. Trigger on: DNS not resolving, check DNS records, propagation check, domain lookup issue.
---

# DNS Diagnostics

## Memory Keys Used by This Skill

### session scope
- `context:plan` — (object) Step list and current progress
- `context:step` — (number) Currently executing step index

### project scope
- `host:{ip}:dns` — DNS findings written if the target resolves to a known IP

### State handling
- session: execution state and plan only
- project: write `host:{ip}:dns` if the target is or resolves to an IP. Schema defined in plugin default_prompt.
- If the target is a domain name with no resolvable IP (NXDOMAIN, propagation check only), skip project KV and report inline.

## Step 0 — Confirm tools and choose branch

Call `agent_info` with each tool name you intend to use to confirm it is registered.
Prefer lower_snake_case network tools when available — they return structured data and handle errors cleanly.
If a specific lower_snake_case network tool is absent, fall back to `system_run_shell` (cmd/shell: nslookup, dig) to accomplish the same step.
Choose the branch and write the plan from the tools that are actually available.

## Decision tree

Select the applicable branch based on the user's request, then write the full step list to `context:plan` in session KV before executing any step.
After each numbered step, update both `context:plan.current_step` and `context:step` to that step number.

**Domain does not resolve:**

```json
{
  "total_steps": 4,
  "current_step": 0,
  "steps": [
    "Step 1: network_resolve_host — basic A/AAAA; note NXDOMAIN vs timeout",
    "Step 2: network_query_dns type=SOA — confirm authoritative nameserver",
    "Step 3: network_query_dns type=NS — list nameservers",
    "Step 4: network_reverse_dns — confirm PTR matches forward record; write host:{ip}:dns, deactivate"
  ]
}
```

1. `network_resolve_host` — basic A/AAAA; note if NXDOMAIN vs timeout.
2. `network_query_dns` with `type=SOA` — confirm authoritative nameserver.
3. `network_query_dns` with `type=NS` — list nameservers.
4. `network_reverse_dns` for the expected IP — confirm PTR matches forward record. Write `host:{ip}:dns` in project KV.

**Check a specific record type:**

```json
{
  "total_steps": 2,
  "current_step": 0,
  "steps": [
    "Step 1: network_query_dns for specific record type",
    "Step 2: write host:{ip}:dns if applicable, deactivate"
  ]
}
```

1. `network_query_dns` with the appropriate type (MX, TXT, SRV, CNAME, etc.). Compare TTL; note if record is expired or missing.

**Check propagation after a DNS change:**

```json
{
  "total_steps": 2,
  "current_step": 0,
  "steps": [
    "Step 1: network_check_dns_propagation across 10 public resolvers",
    "Step 2: report results, deactivate"
  ]
}
```

1. `network_check_dns_propagation` — queries 10 public resolvers; shows which have the new value and which are stale.
2. Report: count propagated / total, list stale resolvers with their current value.

## Troubleshooting error codes

- NXDOMAIN: check for typos or confirm domain registration status.
- SERVFAIL: authoritative servers are failing (check DNSSEC, server health).
- Timeout: query a public resolver (8.8.8.8) using `network_query_dns` to rule out local network issues.
- CNAME returned: follow the chain to resolve the final A/AAAA IP.

## Final step — Finalize

Write `host:{ip}:dns` to project KV if target resolved to an IP. Clear session context keys with `agent_memory_clear {scope:"session", filter:"context:"}`. Then call `skill_deactivate`.

## Output

- State the problem clearly in one sentence.
- For propagation: table with columns `Resolver | IP | Current Value | Status`.
- Flag any mismatch between A and PTR records.
- Flag unusually low or zero TTL.
