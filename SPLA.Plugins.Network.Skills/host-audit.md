---
id: network.host-audit
description: Deep autonomous investigation of a single host — DNS, ping, port scan, banner grab, TLS, HTTP.
---

# Network Host Audit

When the user says "investigate", "audit", "check", "scan", "probe", "analyze", "изучи", "проверь", "что там" — start immediately without asking for permission.

## Execution sequence

Run these steps in order without pausing between them:

1. `network.dns.nslookup` — resolve target; check A/AAAA/PTR
2. `network.diag.ping` — reachability, RTT, TTL (64=Linux, 128=Windows, 255=network gear)
3. `network.scan.ports` — omit ports parameter (built-in 49-port preset); add extras only if context implies non-standard ports
4. `network.tcp.probe` on **every open port** — grab banner, identify service+version; never skip a port
5. `network.ssl.check` — on every port that returned TLS or is 443/8443
6. `network.http.get` + `network.http.head` — on every HTTP port; extract Server, X-Powered-By, cookies, redirects
7. `network.arp` — resolve MAC of target; note OUI vendor
8. `network.dns.reverse` — PTR record

If a step fails (unreachable, closed), note it in one line and proceed.

## Output format

Final report: one compact markdown table per service.

Columns: `Port | Service | Version/Banner | Security Notes | Action Items`

Rules:
- Skip obvious facts; every sentence must carry information density an admin cannot already infer.
- Action items = concrete admin tasks: "patch OpenSSH", "enforce MQTT auth", "disable SMBv1".
- The user is a sysadmin/network engineer — do NOT explain what SSH, MQTT, SMB are.
- Write exact software versions, build strings, OS from TTL/banner, config flags, anomalies, attack surface.
