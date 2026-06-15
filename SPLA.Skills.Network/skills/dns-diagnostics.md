---
id: network.dns-diagnostics
description: DNS troubleshooting — resolution, record types, propagation check across public resolvers. Trigger on: DNS not resolving, check DNS records, propagation check, domain lookup issue.
---

# DNS Diagnostics

## Tool availability

Before starting, call `agent.info` with each tool name you intend to use to confirm it is registered.
Prefer `network.*` plugin tools when available — they return structured data and handle errors cleanly.
If a specific `network.*` tool is absent, fall back to `RunCommandTool` (cmd/shell: nslookup, dig) to accomplish the same step.
Adapt the execution sequence below based on what is actually available.

Run when the user asks to:
- "check DNS", "get DNS records", "where does the domain point", "проверь DNS", "какой IP у домена"
- "check propagation", "did DNS update", "обновились ли записи"
- "check MX records", "verify SPF/TXT", "where is mail hosted", "настройки почты"
- "why doesn't the domain resolve", "site not found", "не резолвится", "домен не открывается"

## Decision tree

**Domain doesn't resolve at all:**
1. `network.dns.nslookup` — basic A/AAAA; note if NXDOMAIN vs timeout
2. `network.dns.query` with type=SOA — confirm authoritative nameserver
3. `network.dns.query` with type=NS — list nameservers
4. `network.dns.reverse` for the expected IP — confirm PTR matches forward record

**Check specific record type:**
1. `network.dns.query` with appropriate type (MX, TXT, SRV, CNAME, etc.)
2. Compare TTL; note if record is expired or missing

**Check propagation after a DNS change:**
1. `network.dns.propagation` — queries 10 public resolvers; shows which have the new value and which are still stale
2. Report: number propagated / total, list of stale resolvers with their current value

**Troubleshooting specific errors:**
- If NXDOMAIN: Check for typos or confirm domain registration status.
- If SERVFAIL: The authoritative servers are failing (check DNSSEC, server health).
- If Timeout: Query a public resolver (like 8.8.8.8) using `network.dns.query` to rule out local network issues.
- If CNAME is returned: Always follow the chain to resolve the final A/AAAA IP.

## Output

- State the problem clearly in one sentence.
- For propagation: table with columns `Resolver | IP | Current Value | Status`.
- Flag any mismatch between A and PTR records.
- Flag unusually low or zero TTL.
