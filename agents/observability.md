# SPLA Observability

SPLA uses an OpenTelemetry-ready observability model.

Application code emits telemetry through standard .NET abstractions:

- `Microsoft.Extensions.Logging.ILogger<T>` for structured logs;
- `System.Diagnostics.ActivitySource` for traces;
- `System.Diagnostics.Metrics.Meter` for metrics.

Business code must not decide final telemetry destinations. Destinations are configured by application startup.

## Current implementation

The shared observability entry point is `SPLA.Observability`.

Core types:

- `SplaTelemetry`
- `SplaTelemetryContext`
- `SplaFileLoggerProvider`

Current destinations:

- global runtime logs: `%LocalAppData%/SPLA/logs/`;
- project operational logs: `<workspace>/.spla/logs/`.

Current file naming:

```text
spla-YYYYMMDD.log
spla-YYYYMMDD.001.log
```

Files roll when they reach approximately 10 MB.

## Correlation fields

Every request/tool flow should propagate these fields when available:

- `TraceId`
- `ConversationId`
- `RequestId`
- `MessageId`
- `ToolCallId`
- `ProjectId`
- `WorkspacePath`

In code, use `SplaTelemetry.PushContext(...)` to set request context around an operation.

## Signal usage

Use traces for operation flow:

- app startup;
- LLM request;
- tool execution;
- plugin loading;
- indexing;
- graph rendering.

Use logs for factual events and diagnostics:

- plugin discovered/loaded/failed;
- tool registered/executed/failed;
- panel opened/failed;
- permission decisions;
- unhandled exceptions.

Use metrics for counters and timings:

- tool call count;
- tool error count;
- tool duration;
- LLM request duration;
- token/cost counters;
- indexing duration and object/relation counts.

## Rules

Use `ILogger<T>` in application code.

Do not write logs through `File.AppendAllText` or console output from business logic.

Do not hardcode final telemetry destinations in plugin/tool/business code.

When adding a new subsystem, add logging at lifecycle boundaries first:

- start;
- finish;
- fail;
- important counts;
- selected identifiers.

Avoid logging full sensitive payloads by default. Prefer IDs, counts, durations, paths that are already local operational data, and short diagnostic messages.

## Future routing

The current file provider is intentionally small and local-first.

Future destinations can be added without changing business code:

- Serilog sinks;
- OpenTelemetry Collector;
- Seq;
- Grafana Loki / Tempo / Prometheus;
- SQLite analysis store;
- autonomous SPLA telemetry analyzer tool.
