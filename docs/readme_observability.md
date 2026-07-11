# SPLA Server — Observability

SPLA emits telemetry through the standard .NET primitives (`ActivitySource` + `Meter`, both named
`SPLA`) from `SPLA.Observability`. Nothing in the agent's hot path knows who consumes it — consumers
attach from the outside. There are two, chosen independently:

## 1. Local stats dashboard (batteries-included, default on)

A built-in, dependency-free consumer: an in-process `MeterListener` + `ActivityListener` taps the same
signals and rolls them into bounded per-minute buckets (for the "over time" charts) plus lifetime
totals and a live activity feed. No external system, no database.

- **Page:** `GET /stats` — KPIs (active connections, tool calls/errors, prompt/completion tokens), an
  over-time sparkline per metric with a selectable window (hour / 6h / 24h), and a recent-activity feed.
- **Access:** admin-only where local auth is in force (`spla.admin` policy); open on a loopback/no-auth
  box. It's the observability plane — viewing, not host configuration.
- **Persistence:** with a server `--root`, the per-period buckets are written to `stats.json` there and
  reloaded on restart, so the "over time" view survives a bounce. Without a root it's in-memory only.
- **Cost:** in-process listeners on signals that already fire; nothing is added to the agent loop.

This exists because a full observability stack isn't always available or wanted — you can always open
`/stats` and see what the server is doing with zero setup.

## 2. OTLP export to a real backend (opt-in)

For Prometheus+Grafana, Tempo, Jaeger, Datadog, or the .NET Aspire dashboard, point OTLP at it:

```
SPLA.Server --auth local --root C:\SPLA\Data --otlp-endpoint http://collector-host:4317
```

The OpenTelemetry SDK exports the same `SPLA` traces and metrics over OTLP. The backend is selected
purely by the endpoint — no code change; OTLP *is* the pluggable-backend interface, so SPLA does not
carry a bespoke metrics abstraction. Off unless `--otlp-endpoint` is set.

**Where this is configured — control plane, not admin.** Setting the OTLP endpoint decides where
telemetry *leaves the host* (an egress/exfiltration decision), so it is a server-launch setting
(`--otlp-endpoint` / `ServiceOptions.OtlpEndpoint`), never something a signed-in admin can point at an
arbitrary external collector. *Viewing* stats is admin-plane; *routing telemetry off the box* is
control-plane.

## Signals emitted

| Signal | Kind | Meaning |
|--------|------|---------|
| `spla.tool.calls` | counter | tool executions |
| `spla.tool.errors` | counter | tool failures (also sets the trace span status to Error) |
| `spla.tool.duration.ms` | histogram | tool execution time |
| `spla.tokens.prompt` / `spla.tokens.completion` | counter | LLM tokens (recorded once per turn in the agent loop) |
| `mcp.tool.execute` | trace (span) | one tool call, tagged with tool name and project |
| gauge `connections.active` | gauge | currently connected clients (local dashboard only) |

Both consumers read these same signals — the local dashboard in-process, the OTLP exporter over the
wire. Adding a new metric means adding one `Meter` instrument; both consumers pick it up automatically.
