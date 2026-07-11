namespace SPLA.Observability.Collection;

/// <summary>One value on a metric's timeline: the minute's Unix-ms timestamp, the summed value in that
/// minute, and how many measurements landed in it (so counters read <see cref="Sum"/>, histograms read
/// <see cref="Sum"/>/<see cref="Count"/> for an average).</summary>
public sealed record BucketPoint(long EpochMs, double Sum, long Count);

/// <summary>
/// A single metric aggregated into fixed one-minute buckets over a rolling retention window. For a
/// counter each bucket's <see cref="Bucket.Sum"/> is that minute's total delta; for a histogram
/// <c>Sum/Count</c> is the average. Bounded (at most <c>retentionMinutes</c> buckets) and dependency-
/// free — this is the "very easy local" store, not a time-series database.
/// </summary>
public sealed class MetricSeries
{
    internal sealed class Bucket { public double Sum; public long Count; }

    private readonly int _retentionMinutes;
    private readonly SortedDictionary<long, Bucket> _buckets = new();

    public MetricSeries(int retentionMinutes = 24 * 60) => _retentionMinutes = retentionMinutes;

    public void Add(DateTime utc, double value)
    {
        var minute = ToMinute(utc);
        if (!_buckets.TryGetValue(minute, out var bucket))
            _buckets[minute] = bucket = new Bucket();
        bucket.Sum += value;
        bucket.Count++;
        Prune(minute);
    }

    /// <summary>The last <paramref name="minutes"/> buckets ending at <paramref name="nowUtc"/>, gap-filled
    /// with zeros so a chart gets a dense, evenly-spaced series.</summary>
    public IReadOnlyList<BucketPoint> Window(DateTime nowUtc, int minutes)
    {
        var end = ToMinute(nowUtc);
        var start = end - minutes + 1;
        var points = new List<BucketPoint>(minutes);
        for (var m = start; m <= end; m++)
        {
            _buckets.TryGetValue(m, out var bucket);
            points.Add(new BucketPoint(m * 60_000L, bucket?.Sum ?? 0, bucket?.Count ?? 0));
        }
        return points;
    }

    internal IEnumerable<(long Minute, double Sum, long Count)> Raw()
        => _buckets.Select(kv => (kv.Key, kv.Value.Sum, kv.Value.Count));

    internal void Seed(long minute, double sum, long count)
        => _buckets[minute] = new Bucket { Sum = sum, Count = count };

    private void Prune(long newest)
    {
        var cutoff = newest - _retentionMinutes;
        while (_buckets.Count > 0)
        {
            var oldest = _buckets.Keys.First();
            if (oldest >= cutoff) break;
            _buckets.Remove(oldest);
        }
    }

    private static long ToMinute(DateTime utc)
        => (long)Math.Floor((utc.ToUniversalTime() - DateTime.UnixEpoch).TotalMinutes);
}
