# SPLA Project TODO & Roadmaps

## Network/Admin Tools Roadmap

### Phase 1 (Completed)
Минимальный набор диагностических инструментов (максимально узкие и безопасные C# реализации):
- [x] `network.ping` (Ping host using native .NET APIs)
- [x] `network.nslookup` (Resolve DNS using native .NET APIs)
- [x] `network.http_get` (Make HTTP GET requests, with context-safe body truncation)
- [x] `network.http_head` (Make HTTP HEAD requests to fetch headers)
- [x] `network.port_check` (Test TCP port connection with timeout)
- [x] `network.traceroute` (Execute traceroute using C# Ping with TTL incrementing)

### Phase 2 (Future)
Расширенная диагностика:
- [ ] `tcp_connect`
- [ ] `tls_info`
- [ ] `ssl_certificate`
- [ ] `reverse_dns`
- [ ] `subnet_scan`

### Phase 3 (Future)
Полноценные сетевые проверки:
- [ ] `nmap_wrapper`
- [ ] `service_check`
- [ ] `docker_logs`
- [ ] `docker_ps`
