# SPLA Project TODO & Roadmaps

## Network/Admin Tools Roadmap

### Phase 1 (Completed)
Минимальный набор диагностических инструментов (максимально узкие и безопасные C# реализации):
- [x] `network_ping_host` (Ping host using native .NET APIs)
- [x] `network_resolve_host` (Resolve DNS using native .NET APIs)
- [x] `network_http_get` (Make HTTP GET requests, with context-safe body truncation)
- [x] `network_http_head` (Make HTTP HEAD requests to fetch headers)
- [x] `network_check_tcp_port` (Test TCP port connection with timeout)
- [x] `network_trace_route` (Execute traceroute using C# Ping with TTL incrementing)

### Phase 2 (Future)
Расширенная диагностика:
- [ ] `network_check_tcp_port`
- [ ] `network_check_tls`
- [ ] `network_check_tls_certificate`
- [ ] `network_reverse_dns`
- [ ] `network_discover_hosts`

### Phase 3 (Future)
Полноценные сетевые проверки:
- [ ] `network_run_nmap`
- [ ] `network_check_service`
- [ ] `docker_get_logs`
- [ ] `docker_list_containers`
