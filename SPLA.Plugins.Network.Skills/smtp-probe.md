---
id: network.smtp-probe
description: Mail server diagnostic — SMTP EHLO handshake, STARTTLS, AUTH methods, relay check.
---

# SMTP Probe

Run when the user asks to "check mail", "check SMTP", "test email delivery", "проверь почту", "SMTP не работает".

## Execution sequence

1. `network.diag.ping` — basic reachability
2. `network.scan.ports` with ports=25,465,587,2525 — find which SMTP ports are open
3. `network.smtp.probe` on each open port — EHLO handshake; captures banner, STARTTLS, AUTH methods, SIZE, extensions
4. `network.ssl.check` on port 465 (if open) — implicit TLS; check cert validity
5. `network.dns.query` with type=MX for the domain — verify MX records point to this host
6. `network.dns.query` with type=TXT for the domain — check SPF record
7. `network.dns.query` with name=_dmarc.{domain} type=TXT — check DMARC policy
8. `network.dns.query` with type=TXT for DKIM selector if known — check DKIM public key

## What to report

| Port | Banner | STARTTLS | AUTH Methods | Size Limit | Notes |
|------|--------|----------|--------------|------------|-------|

- Flag open relay risk if AUTH is not required.
- Flag missing STARTTLS on port 25/587.
- Flag SPF missing or too permissive (`+all`).
- Flag missing DMARC or policy=none.
- Flag certificate issues on port 465.
- Action items: "enforce AUTH", "add SPF", "set DMARC p=quarantine", etc.
