using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SPLA.Observability;

namespace SPLA.Service.Observability;

/// <summary>
/// Wires the OpenTelemetry SDK to export the process's <c>SPLA</c> meter and traces over OTLP to an
/// external backend (Grafana/Tempo, Jaeger, Prometheus-via-collector, Datadog, .NET Aspire dashboard,
/// …). Off unless an endpoint is configured — the instrumentation itself (the same
/// <see cref="System.Diagnostics.ActivitySource"/>/<see cref="System.Diagnostics.Metrics.Meter"/> the
/// local collector taps) is unchanged; this only adds a wire exporter as a second consumer.
/// <para>Configuring the export destination is an egress decision (data leaves the host), so it belongs
/// to the control plane — passed in via <see cref="ServiceOptions.OtlpEndpoint"/>, not toggled by a
/// signed-in admin.</para>
/// </summary>
internal static class OtlpExport
{
    public static void MaybeWire(WebApplicationBuilder builder, ServiceOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.OtlpEndpoint)) return;
        if (!Uri.TryCreate(options.OtlpEndpoint, UriKind.Absolute, out var endpoint)) return;

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(SplaTelemetry.ServiceName))
            .WithTracing(tracing => tracing
                .AddSource(SplaTelemetry.ServiceName)
                .AddOtlpExporter(o => o.Endpoint = endpoint))
            .WithMetrics(metrics => metrics
                .AddMeter(SplaTelemetry.ServiceName)
                .AddOtlpExporter(o => o.Endpoint = endpoint));
    }
}
