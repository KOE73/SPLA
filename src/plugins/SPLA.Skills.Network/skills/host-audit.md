---
id: network.host-audit
description: Deep autonomous investigation of a single host — DNS, ping, port scan, banner grab, TLS, HTTP. Trigger on: investigate host, audit server, check open ports, what is running on, scan host.
---

# Network Host Audit

## Memory Keys Used by This Skill

### session scope
- `context:current_ip` — IP address currently being investigated (write at the start, clear at finalize)
- `context:plan` — `{ prior_record_existed: bool, done: bool }`

### project scope
- `host:{ip}` — base record: ping, mac, vendor, class, tags, scanned_at. Always read before writing to merge.
- `host:{ip}:dns` — DNS resolution results
- `host:{ip}:tcp` — open ports and banners
- `host:{ip}:tls:{port}` — TLS/SSL certificate and handshake details (one key per port)
- `host:{ip}:http:{port}` — HTTP response metadata (one key per port)

Schema for each key is defined in the plugin `default_prompt`.

---

## Transaction model

One host = one mark transaction. All probing steps run inside a single `mark_set` / `mark_rollback` window so the agent retains full conversational context across all findings. KV writes happen **after** all probing is complete.

```
mark_set "host-audit-start"
→ [all probe steps — agent keeps full context throughout]
→ [all KV writes — after reasoning is complete]
→ agent_memory_set context:plan  ← mark done
mark_rollback "host-audit-start"
```

**FORBIDDEN:** writing host KV keys before all probing steps are complete.
**FORBIDDEN:** calling `mark_rollback` before all host KV writes are confirmed with `ok:` and `context:plan` is updated.
**REQUIRED:** every write to `host:{ip}` must be preceded by `agent_memory_get {key:"host:{ip}", scope:"project"}`. Merge new fields into the existing object — do not replace the whole record. Start from `{}` if no record exists yet.

---

## Step 0 — Confirm tools and initialize

Call `agent_info` for each tool you intend to use. Fall back to `system_run_shell` only when a specific tool is absent.

**Write `context:current_ip`** — `agent_memory_set {key:"context:current_ip", scope:"session", value:"{ip}"}`.

Check for a prior host record — `agent_memory_get {key:"host:{ip}", scope:"project"}` — and note whether one existed.

Write plan — `agent_memory_set {key:"context:plan", scope:"session"}`:

```json
{ "prior_record_existed": false, "done": false }
```

---

## Probe sequence

**Set the mark** — substitute the actual target IP into the resume string:
```
mark_set "host-audit-start" resume:"Resumed after rollback for host 1.2.3.4. Check context:plan — if done:true, read agent_memory_list {filter:\"host:1.2.3.4*\", scope:\"project\"} and produce the final report, then move to next host. If done:false, KV writes did NOT complete — do NOT produce a report, restart probing from Step 1."
```
Replace `1.2.3.4` with the actual target IP. **FORBIDDEN:** using `{ip}` literally in the resume string.

Then run all steps without rollback until the KV write phase.

**Step 1 — DNS**
`network_resolve_host` — resolve A/AAAA/PTR.

**Step 2 — Ping**
`network_ping_host` — reachability, RTT, TTL. TTL hint: 64=Linux, 128=Windows, 255=network gear.
Set preliminary `class:"icmp-only"` — will be recalculated in the KV write phase.

**Step 3 — Port scan**
`network_scan_tcp_ports` (omit `ports` — uses built-in 49-port preset).

**Step 4 — Banner grab**
`network_probe_tcp` on every open port — banner, service, version. Skip if zero open ports.

**Step 5 — TLS**
`network_check_tls` on every port that returned a TLS banner or is 443/8443. One call per port.
**FORBIDDEN:** calling without a filter port match. If no TLS candidates, skip entirely.

**Step 6 — HTTP**
`network_http_get` + `network_http_head` on every HTTP port. Extract Server, X-Powered-By, cookies, redirects.
One call per port.

**Step 7 — ARP**
`network_get_arp_cache` with `filter:{ip}`.
**FORBIDDEN:** retrying without a filter or with a broader filter on a miss. If not in cache — record `mac:null, vendor:null` and continue.

**Step 8 — Reverse DNS**
`network_reverse_dns` — PTR record.

If a step fails: note it in one line and continue.

---

## KV write phase

After all probing is complete, reason over all findings together, then write.

**Compute final `class`:**

| ping  | ports        | dns/ptr | prior record | class       |
|-------|--------------|---------|--------------|-------------|
| true  | non-empty    | any     | any          | `active`    |
| true  | empty        | any     | any          | `icmp-only` |
| false | non-empty    | any     | any          | `tcp-only`  |
| false | empty        | true    | any          | `dns-only`  |
| false | empty        | false   | existed      | `gone`      |
| false | empty        | false   | none         | `empty`     |

**FORBIDDEN:** setting `class:"gone"` if `context:plan.prior_record_existed` is false.

**Write order** — confirm each write returns `ok:` before the next:

1. `agent_memory_get host:{ip}` → merge → `agent_memory_set host:{ip}` — `ip, hostname, ping, ttl, mac, vendor, class, tags, scanned_at`
2. `agent_memory_set host:{ip}:dns` — `hostname, ptr, a, aaaa, resolver, ttl, scanned_at`
3. `agent_memory_set host:{ip}:tcp` — `ports:[{port, protocol, banner, service, version}], scanned_at`
4. For each TLS port: `agent_memory_set host:{ip}:tls:{port}`
5. For each HTTP port: `agent_memory_set host:{ip}:http:{port}`
6. `agent_memory_set {key:"context:plan", scope:"session", value:{..., done:true}}`

**FORBIDDEN:** calling `mark_rollback` before step 6 above returns `ok:` and `context:plan.done` is `true`.

**Roll back to mark:**
```
mark_rollback "host-audit-start"
```

---

## Output

After rollback: `agent_memory_list {filter:"host:{ip}*", scope:"project"}` — read all records and synthesize the report.

Final report: one compact markdown table per service group.

`Port | Service | Version/Banner | Security Notes | Action Items`

- Skip obvious facts — every row must carry information an admin cannot already infer.
- Action items = concrete tasks: "patch OpenSSH to 9.x", "enforce MQTT auth", "disable SMBv1".
- The user is a sysadmin/network engineer — do NOT explain what SSH, MQTT, SMB are.
- Write exact versions, build strings, OS from TTL/banner, config flags, anomalies, attack surface.

---

## Finalize

`agent_memory_clear {scope:"session", filter:"context:"}`. Call `skill_deactivate`.
