using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace SPLA.Observability.Collection;

/// <summary>One completed unit of work in the live activity feed (a tool call, a script step),
/// attributed to the acting <see cref="UserKey"/> for the per-user stats slice.</summary>
public sealed record RecentEvent(long EpochMs, string Name, double DurationMs, string? Tool, string? Project, string? UserKey);

/// <summary>The "right now" view: lifetime totals per metric, current gauges (active connections, …),
/// and the most recent activity events. <see cref="Scope"/> is <c>"server"</c> for an admin (everything)
/// or <c>"user"</c> for an ordinary user (only their own activity, no server-wide gauges).</summary>
public sealed record StatsSnapshot(
    DateTime StartedUtc,
    double UptimeSeconds,
    string Scope,
    IReadOnlyDictionary<string, double> Totals,
    IReadOnlyDictionary<string, double> Gauges,
    IReadOnlyList<RecentEvent> Recent);

/// <summary>
/// The batteries-included local telemetry consumer. It taps the process's own <see cref="Meter"/> and
/// <see cref="ActivitySource"/> (the same signals an OTLP exporter would ship out) via in-process
/// <see cref="MeterListener"/>/<see cref="ActivityListener"/> — so it adds <b>nothing</b> to the hot
/// path, it only listens. Measurements roll into bounded per-minute <see cref="MetricSeries"/> for the
/// "over a period" charts and lifetime totals; a ring of <see cref="RecentEvent"/> feeds the live view;
/// hosts push gauges (active connections) via <see cref="SetGauge"/>. Optional JSON persistence lets the
/// period view survive a restart. No external system, no database.
/// </summary>
public sealed class TelemetryCollector : IDisposable
{
    /// <summary>The process-wide collector, if one was started (the stats endpoints read this).</summary>
    public static TelemetryCollector? Current { get; private set; }

    /// <summary>Raised (outside the lock) for every completed activity, carrying its owning user. The
    /// stats live-socket subscribes to fan each event out to admins and to the owning user in real time.</summary>
    public event Action<RecentEvent>? EventRecorded;

    /// <summary>Per-user aggregate: their lifetime totals and their own recent-activity ring.</summary>
    private sealed class UserAggregate
    {
        public readonly Dictionary<string, double> Totals = new();
        public readonly LinkedList<RecentEvent> Recent = new();
    }

    private readonly MeterListener _meterListener = new();
    private readonly ActivityListener _activityListener;
    private readonly Lock _gate = new();
    private readonly Dictionary<string, MetricSeries> _series = new();
    private readonly Dictionary<string, double> _totals = new();
    private readonly Dictionary<string, double> _gauges = new();
    private readonly LinkedList<RecentEvent> _recent = new();
    private readonly Dictionary<string, UserAggregate> _byUser = new();
    private readonly int _recentCapacity;
    private readonly int _userRecentCapacity;
    private readonly string? _persistPath;
    private readonly Timer? _flushTimer;

    public DateTime StartedUtc { get; } = DateTime.UtcNow;

    public TelemetryCollector(string sourceName = "SPLA", string? persistPath = null, int recentCapacity = 200, int userRecentCapacity = 50)
    {
        _persistPath = persistPath;
        _recentCapacity = recentCapacity;
        _userRecentCapacity = userRecentCapacity;
        if (persistPath != null) TryLoad(persistPath);

        _meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == sourceName) listener.EnableMeasurementEvents(instrument);
        };
        _meterListener.SetMeasurementEventCallback<long>((instrument, value, _, _) => Record(instrument.Name, value));
        _meterListener.SetMeasurementEventCallback<double>((instrument, value, _, _) => Record(instrument.Name, value));
        _meterListener.SetMeasurementEventCallback<int>((instrument, value, _, _) => Record(instrument.Name, value));
        _meterListener.Start();

        _activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = OnActivityStopped
        };
        ActivitySource.AddActivityListener(_activityListener);

        if (persistPath != null)
            _flushTimer = new Timer(_ => TrySave(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

        Current = this;
    }

    private void Record(string instrument, double value)
    {
        // The measurement callback runs synchronously on the emitting async flow, so the turn's ambient
        // user (pushed by the host) is readable here — attribute the measurement without any tag on the
        // Add() call site.
        var userKey = SplaTelemetry.CurrentUserKey;
        lock (_gate)
        {
            if (!_series.TryGetValue(instrument, out var series))
                _series[instrument] = series = new MetricSeries();
            series.Add(DateTime.UtcNow, value);
            _totals[instrument] = (_totals.TryGetValue(instrument, out var total) ? total : 0) + value;

            if (userKey != null)
            {
                var agg = UserAgg(userKey);
                agg.Totals[instrument] = (agg.Totals.TryGetValue(instrument, out var t) ? t : 0) + value;
            }
        }
    }

    private void OnActivityStopped(Activity activity)
    {
        var userKey = activity.GetTagItem("spla.user") as string;
        var evt = new RecentEvent(
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            activity.DisplayName,
            activity.Duration.TotalMilliseconds,
            activity.GetTagItem("spla.tool.name") as string,
            activity.GetTagItem("spla.project_id") as string,
            userKey);
        lock (_gate)
        {
            _recent.AddFirst(evt);
            while (_recent.Count > _recentCapacity) _recent.RemoveLast();

            if (userKey != null)
            {
                var recent = UserAgg(userKey).Recent;
                recent.AddFirst(evt);
                while (recent.Count > _userRecentCapacity) recent.RemoveLast();
            }
        }
        EventRecorded?.Invoke(evt);
    }

    private UserAggregate UserAgg(string userKey)
    {
        if (!_byUser.TryGetValue(userKey, out var agg))
            _byUser[userKey] = agg = new UserAggregate();
        return agg;
    }

    /// <summary>Sets a current-value gauge (e.g. active connections). Hosts refresh these periodically.</summary>
    public void SetGauge(string name, double value)
    {
        lock (_gate) _gauges[name] = value;
    }

    /// <summary>Server-wide snapshot (admin scope) — everything.</summary>
    public StatsSnapshot Snapshot()
    {
        lock (_gate)
            return new StatsSnapshot(
                StartedUtc,
                (DateTime.UtcNow - StartedUtc).TotalSeconds,
                "server",
                new Dictionary<string, double>(_totals),
                new Dictionary<string, double>(_gauges),
                _recent.ToList());
    }

    /// <summary>Scope-aware snapshot: an admin gets the server-wide view; an ordinary user gets only
    /// their own totals and recent activity (no server-wide gauges).</summary>
    public StatsSnapshot SnapshotFor(string? userKey, bool isAdmin)
    {
        if (isAdmin) return Snapshot();
        lock (_gate)
        {
            var agg = userKey != null && _byUser.TryGetValue(userKey, out var a) ? a : null;
            return new StatsSnapshot(
                StartedUtc,
                (DateTime.UtcNow - StartedUtc).TotalSeconds,
                "user",
                agg != null ? new Dictionary<string, double>(agg.Totals) : new Dictionary<string, double>(),
                new Dictionary<string, double>(),   // no server-wide gauges for a plain user
                agg != null ? agg.Recent.ToList() : []);
        }
    }

    /// <summary>The last <paramref name="minutes"/> minute-buckets for a metric (dense, zero-filled).</summary>
    public IReadOnlyList<BucketPoint> Series(string instrument, int minutes)
    {
        lock (_gate)
            return _series.TryGetValue(instrument, out var series)
                ? series.Window(DateTime.UtcNow, minutes)
                : [];
    }

    public IReadOnlyList<string> Instruments()
    {
        lock (_gate) return _series.Keys.OrderBy(k => k).ToList();
    }

    // ── JSON persistence (optional) ───────────────────────────────────────────

    private sealed record PersistModel(
        Dictionary<string, double> Totals,
        Dictionary<string, List<long[]>> Series);   // series[name] = [[minute, sumScaled?, count], …] via [minute, sumBits, count]

    private void TrySave()
    {
        try { if (_persistPath != null) Save(_persistPath); } catch { /* best-effort */ }
    }

    private void Save(string path)
    {
        PersistModel model;
        lock (_gate)
        {
            var series = _series.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.Raw().Select(b => new[] { b.Minute, BitConverter.DoubleToInt64Bits(b.Sum), b.Count }).ToList());
            model = new PersistModel(new Dictionary<string, double>(_totals), series);
        }

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(model));
        File.Move(tmp, path, overwrite: true);
    }

    private void TryLoad(string path)
    {
        try
        {
            if (!File.Exists(path)) return;
            var model = JsonSerializer.Deserialize<PersistModel>(File.ReadAllText(path));
            if (model == null) return;
            lock (_gate)
            {
                foreach (var (k, v) in model.Totals) _totals[k] = v;
                foreach (var (name, rows) in model.Series)
                {
                    var series = new MetricSeries();
                    foreach (var row in rows.Where(r => r.Length == 3))
                        series.Seed(row[0], BitConverter.Int64BitsToDouble(row[1]), row[2]);
                    _series[name] = series;
                }
            }
        }
        catch { /* a corrupt stats file must never block startup */ }
    }

    public void Dispose()
    {
        TrySave();
        _flushTimer?.Dispose();
        _meterListener.Dispose();
        _activityListener.Dispose();
        if (Current == this) Current = null;
    }
}
