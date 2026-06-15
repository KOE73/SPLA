---
id: network.range-audit
description: IPv4 subnet/range audit — full host inventory, scan every IP, reverse DNS, write per-host report. Trigger on: audit subnet, scan range, investigate /24, network segment mapping, all hosts in CIDR.
---

# Network Range Audit

## Tool availability

Before starting, call `agent.info` with each tool name you intend to use to confirm it is registered.
Prefer `network.*` plugin tools when available — they return structured data and handle errors cleanly.
If a specific `network.*` tool is absent, fall back to `RunCommandTool` (cmd/shell) to accomplish the same step.
Adapt the execution sequence below based on what is actually available.

Run this skill whenever the user asks to investigate, scan, inventory, map, audit, discover, enumerate, or document an IPv4 subnet, IP range, LAN, network segment, or hosts in a network. This skill applies even if the user does not say "audit" or "inventory".

Treat requests about finding live devices, identifying what exists in a network, checking an address range, documenting subnet contents, or producing a host inventory as network range audit requests.

## Clarification

Ask one short clarification before scanning only when the task does not specify the desired depth:

`Нужно проверить только наличие хостов ping/DNS, наличие плюс основные TCP-порты, или полное исследование сервисов по открытым портам?`

Do not ask if the user already says one of these modes:

- `presence`: ping all addresses and reverse-resolve every address.
- `basic`: ping all addresses, probe common host-presence ports, reverse-resolve every address.
- `full`: run `basic`, then scan common ports on every potential host and identify services.

Always ask for an explicit bounded IPv4 CIDR or start/end range if the range is missing. Do not infer or broaden a scan target silently.

## Range blocking

Split the target into consecutive blocks of at most 256 IPv4 addresses.

Rules:
- For `/24` ranges, each block is the natural `/24`.
- For larger or arbitrary ranges, use `[blockStart, blockEnd]` windows of 256 addresses.
- Process blocks sequentially so each block can produce its own report and conclusion.
- Use CIDR or explicit `start` and `end` for each block; the scan tool must test every address in the requested block.
- Use `maxHosts` no larger than 256 for each `network.scan.lan` call.
- Use `concurrency: 256` for block discovery unless the user asks for a lower intensity.

## Execution sequence

For each 256-address block:

1. `network.dns.reverse` for every IP in the block — record PTR/DNS names even if the host does not answer.
2. `network.scan.lan` with `cidr` set to the block CIDR (e.g. `172.16.0.0/24`) or `start`+`end` explicitly; `ping:true`, no `ports` — discover ICMP-responsive hosts. **Always pass `cidr` or `start`/`end` — the tool requires one of them.**
3. If mode is `basic` or `full`, run `network.scan.lan` again with the same `cidr`/`start`+`end`, `ports: common`, `ping:false` — find hosts that do not answer ICMP but expose a common TCP service.
4. `network.host.arp` — enrich local-neighbor hosts with MAC/interface data where available.
5. Build the potential-host set as the union of:
   - any IP with a PTR/DNS name,
   - any IP found by ping,
   - any IP found by port-presence probing,
   - any IP found in ARP for this block.
6. If mode is `full`, for every potential host:
   - `network.scan.ports` with omitted `ports` parameter first (common service preset).
   - `network.tcp.probe` on every open TCP port.
   - `network.ssl.check` on 443, 8443, or ports whose banner/protocol suggests TLS.
   - `network.http.head` and `network.http.get` on HTTP-like ports.
   - `network.http.redirects` for HTTP services that redirect.
   - `network.smtp.probe` on 25 or 587 if open.
   - `network.udp.probe` only when the open/expected service gives a concrete reason and payload is known.

Do not stop between steps inside a block. If a tool fails for one host, record the failure in that host row and continue.

## Classification

Classify every address in the block:

- `empty`: no DNS, no ping, no open host-presence port, no ARP entry.
- `dns-only`: PTR/DNS exists, but no ping/port/ARP evidence.
- `icmp-only`: ping answers, but no DNS or open probed port.
- `tcp-only`: no ping, but a host-presence TCP port is open.
- `active`: multiple evidence types or full service audit data exists.

For `full` mode, identify likely device/service type from exact evidence:

- Use DNS names, banners, HTTP headers/titles, TLS certificate CN/SAN/issuer, SMTP EHLO, SSH/FTP banners, common management ports, and TTL hints.
- Do not invent a device type when evidence is weak. Use `unknown` plus the exact signals.
- Keep version strings exact when available.

## Per-block report

Create one markdown report file per block when file tools are available. This file is a required output artifact, not an optional summary. Do not treat an inline chat summary as completion when a file can be written.

`network-range-{blockStart}-{blockEnd}.md`

Each block report must contain:

1. Scope: block start/end, mode, timeout/concurrency, ports policy, scan time.
2. Summary: total addresses, DNS-only, ICMP-responsive, TCP-only, active, empty, errors.
3. Host table sorted by IP:

`IP | DNS/PTR | Class | Ping | Open Ports | Service Guess | Evidence | MAC | Notes`

The host table must include every IP address in the block, including `empty` hosts. Do not omit silent hosts from the file.

4. Full-mode service details table when services were audited:

`IP | Port | Protocol | Version/Banner | TLS/HTTP/SMTP Details | Security Notes | Action Items`

5. Block conclusion:
   - visible infrastructure,
   - likely unused ranges,
   - unexpected exposed services,
   - hosts with DNS but no response,
   - hosts with ports but no DNS,
   - follow-up checks worth doing.

After finishing a block, write or update its markdown report file before moving to the next block. Then emit only a brief block status in chat: block range, report file path, counts, and notable findings. Do not print the full block report inline when the file was written. If file tools are not available, emit the same block report inline and clearly label the block.

## Final report

After all blocks are processed, create a final markdown report file when file tools are available. This file is mandatory whenever any block report file was written.

`network-range-summary.md`

The final report must contain:

1. Overall scope and selected mode.
2. One table per range block:

`Block | Addresses | DNS-only | ICMP | TCP-only | Active | Empty | Notable Findings | Block Report`

3. Cross-range findings:
   - naming patterns,
   - likely server/network-equipment/client zones,
   - duplicated hostnames or suspicious PTR mismatches,
   - high-value exposed services,
   - dead DNS records,
   - recommended next scans.

4. Explicit limitations:
   - ICMP may be filtered.
   - Common-port probing is not exhaustive.
   - Full mode identifies only what banners/protocol probes reveal.
   - UDP service detection is partial unless protocol-specific payloads were used.

The final assistant response must list the written report file paths. If no files were written, explicitly state that file tools were unavailable and the report was provided inline.

## Safety and scope

- Scan only user-specified private or owned/administered ranges.
- For public internet ranges, ask the user to confirm authorization before scanning.
- Never use `ports: all` unless the user explicitly asks for exhaustive port scanning.
- Prefer `common` ports for broad ranges; reserve `1-1024` or custom port lists for explicit requests.
- Keep timeouts conservative by default: 500 ms for discovery, 3000 ms for banner/service probes.

## Useful missing tools

The current network plugin can run this skill with existing tools, but these tools would make it much more reliable:

- `network.scan.range_audit`: one orchestration tool that accepts CIDR/start/end, mode, blockSize, concurrency, portsPolicy and returns structured JSON per block.
- `network.report.write`: writes markdown/CSV/JSON report artifacts from structured scan results.
- `network.service.identify`: service fingerprint helper that normalizes port scan, banner, HTTP, TLS, SMTP and DNS signals into `service`, `product`, `version`, `confidence`.
- `network.netbios.probe` or `network.smb.probe`: Windows hostnames, SMB dialect/signing, domain/workgroup where authorized.
- `network.snmp.probe`: optional SNMP sysName/sysDescr for managed network equipment when the user provides community/credentials.
