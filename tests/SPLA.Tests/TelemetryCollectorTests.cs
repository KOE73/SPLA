using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using SPLA.Observability;
using SPLA.Observability.Collection;
using Xunit;

namespace SPLA.Tests;

public class TelemetryCollectorTests
{
    [Fact]
    public void CapturesCounter_IntoTotalsAndSeries()
    {
        // The instrument exists BEFORE the collector — mirrors the real app, where SplaTelemetry's
        // static counters are created at type-load, long before any collector starts.
        using var meter = new Meter("spla-test-counter");
        var calls = meter.CreateCounter<long>("spla.tool.calls");

        using var collector = new TelemetryCollector("spla-test-counter");
        calls.Add(3);
        calls.Add(2);

        var snap = collector.Snapshot();
        Assert.Equal(5, snap.Totals["spla.tool.calls"]);

        var series = collector.Series("spla.tool.calls", 5);
        Assert.Equal(5, series.Sum(p => p.Sum));
    }

    [Fact]
    public void CapturesHistogram_AsSumAndCount()
    {
        using var meter = new Meter("spla-test-hist");
        var dur = meter.CreateHistogram<double>("spla.tool.duration.ms");

        using var collector = new TelemetryCollector("spla-test-hist");
        dur.Record(100);
        dur.Record(300);

        var series = collector.Series("spla.tool.duration.ms", 5);
        Assert.Equal(400, series.Sum(p => p.Sum));
        Assert.Equal(2, series.Sum(p => p.Count));   // average = 200
    }

    [Fact]
    public void Gauge_SetAndRead()
    {
        using var collector = new TelemetryCollector("spla-test-gauge");
        collector.SetGauge("connections.active", 4);
        Assert.Equal(4, collector.Snapshot().Gauges["connections.active"]);
    }

    [Fact]
    public void CapturesActivity_IntoRecentFeed()
    {
        using var source = new ActivitySource("spla-test-activity");
        using var collector = new TelemetryCollector("spla-test-activity");

        using (var activity = source.StartActivity("mcp.tool.execute"))
        {
            Assert.NotNull(activity);   // the collector's listener makes the source sampled
            activity!.SetTag("spla.tool.name", "system_read_file");
        }

        var recent = collector.Snapshot().Recent;
        Assert.Contains(recent, e => e.Name == "mcp.tool.execute" && e.Tool == "system_read_file");
    }

    [Fact]
    public void AttributesMeasurement_ToAmbientUser_ForPerUserSlice()
    {
        using var meter = new Meter("spla-test-peruser");
        var calls = meter.CreateCounter<long>("spla.tool.calls");
        using var collector = new TelemetryCollector("spla-test-peruser");

        using (SplaTelemetry.PushContext(new SplaTelemetryContext(UserKey: "u1")))
            calls.Add(2);
        calls.Add(5);   // no ambient user → global only

        // Admin (server scope) sees everything.
        var admin = collector.SnapshotFor(null, isAdmin: true);
        Assert.Equal("server", admin.Scope);
        Assert.Equal(7, admin.Totals["spla.tool.calls"]);

        // u1 (user scope) sees only their own 2.
        var u1 = collector.SnapshotFor("u1", isAdmin: false);
        Assert.Equal("user", u1.Scope);
        Assert.Equal(2, u1.Totals["spla.tool.calls"]);

        // A different user sees nothing of u1's.
        var u2 = collector.SnapshotFor("u2", isAdmin: false);
        Assert.False(u2.Totals.ContainsKey("spla.tool.calls"));
    }

    [Fact]
    public void RaisesEvent_WithOwningUser_AndFiltersRecentPerUser()
    {
        using var source = new ActivitySource("spla-test-evt");
        using var collector = new TelemetryCollector("spla-test-evt");

        RecentEvent? captured = null;
        collector.EventRecorded += e => captured = e;

        using (var activity = source.StartActivity("mcp.tool.execute"))
        {
            activity!.SetTag("spla.user", "owner-1");   // Enrich does this from the ambient context in the real app
            activity.SetTag("spla.tool.name", "system_read_file");
        }

        Assert.Equal("owner-1", captured?.UserKey);

        // The owner sees it in their recent feed; another user does not.
        Assert.Contains(collector.SnapshotFor("owner-1", isAdmin: false).Recent, e => e.Tool == "system_read_file");
        Assert.Empty(collector.SnapshotFor("someone-else", isAdmin: false).Recent);
        // Admin sees it server-wide.
        Assert.Contains(collector.SnapshotFor(null, isAdmin: true).Recent, e => e.Tool == "system_read_file");
    }

    [Fact]
    public void EnrichPropagatesUserKey_FromContextToActivityTag()
    {
        // Confirms the real path: SplaTelemetry.StartActivity stamps the ambient user onto the span.
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == "SPLA",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        using (SplaTelemetry.PushContext(new SplaTelemetryContext(UserKey: "ctx-user")))
        using (var activity = SplaTelemetry.StartActivity("mcp.tool.execute"))
            Assert.Equal("ctx-user", activity?.GetTagItem("spla.user"));
    }

    [Fact]
    public void Persistence_RoundTripsTotalsAcrossReload()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "spla-stats-" + System.IO.Path.GetRandomFileName() + ".json");
        using (var meter = new Meter("spla-test-persist"))
        {
            var calls = meter.CreateCounter<long>("spla.tool.calls");
            using var collector = new TelemetryCollector("spla-test-persist", persistPath: path);
            calls.Add(7);
            collector.Dispose();   // flushes

            using var reopened = new TelemetryCollector("spla-test-persist-other", persistPath: path);
            Assert.Equal(7, reopened.Snapshot().Totals["spla.tool.calls"]);
        }
    }
}
