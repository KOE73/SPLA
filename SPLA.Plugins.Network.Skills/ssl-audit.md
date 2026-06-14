---
id: network.ssl-audit
description: TLS/SSL certificate and protocol audit — chain, expiry, cipher suite, vulnerabilities.
---

# SSL/TLS Audit

Run when the user asks to "check SSL", "check the certificate", "is the cert valid", "проверь сертификат", "TLS проблема".

## Execution sequence

1. `network.ssl.check` on the target host:port (default 443 if not specified)
2. If HTTP redirects are relevant: `network.http.redirects` — verify HTTPS redirect chain is clean
3. `network.http.head` — check HSTS header (`Strict-Transport-Security`)

## What to report

**Certificate:**
- Subject CN / SAN list
- Issuer and chain depth
- Expiry date and days remaining (flag if < 30 days)
- Self-signed or publicly trusted
- Key type and size (flag RSA < 2048, EC < 256)

**Protocol:**
- TLS versions negotiated (flag TLS 1.0 / 1.1 as deprecated)
- Cipher suite (flag RC4, DES, 3DES, export ciphers, NULL)
- Flag if TLS 1.3 is NOT supported

**Security headers:**
- HSTS present and `max-age` value (flag if missing or max-age < 180 days)
- Flag missing `includeSubDomains` on HSTS if subdomains are in SAN

**Action items:** concrete tasks — "renew certificate (expires in N days)", "disable TLS 1.0", "add HSTS header", etc.
