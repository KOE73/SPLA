using System.Diagnostics;

namespace SPLA.Plugins.Android.Video;

internal sealed class LatestFrame
{
    private readonly object sync = new();
    private YuvFrame? current;
    private long sequence;
    private TaskCompletionSource<YuvFrame> next = NewWaiter();
    private Exception? failure;
    private int[]? signature;
    private long unchangedSince, lastEvidence;
    private (int Width, int Height) signatureSize;
    private static TaskCompletionSource<YuvFrame> NewWaiter() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    public YuvFrame? Current { get { lock (sync) return current; } }
    public void Publish(YuvFrame frame)
    {
        lock (sync)
        {
            var sample = Signature(frame);
            var now = Stopwatch.GetTimestamp();
            if (signature is null || signatureSize != (frame.Width, frame.Height) ||
                signature.Where((value, index) => Math.Abs(value - sample[index]) > 2).Any())
            {
                unchangedSince = now;
                signature = sample;
                signatureSize = (frame.Width, frame.Height);
            }
            lastEvidence = now;
            current = frame with { Sequence = ++sequence };
            var ready = next; next = NewWaiter(); ready.TrySetResult(current);
        }
    }
    public void InvalidateStability()
    {
        lock (sync) { signature = null; unchangedSince = lastEvidence = 0; }
    }
    public void Fail(Exception error)
    {
        lock (sync) { failure = error; next.TrySetException(error); _ = next.Task.Exception; }
    }
    public Task<YuvFrame> WaitNextAsync(long afterSequence, TimeSpan timeout, CancellationToken ct)
    {
        lock (sync)
        {
            if (failure is not null) return Task.FromException<YuvFrame>(failure);
            if (current is { } frame && frame.Sequence > afterSequence) return Task.FromResult(frame);
            return next.Task.WaitAsync(timeout, ct);
        }
    }
    public async Task<(YuvFrame Frame, bool Settled, long ElapsedMs)> WaitStableAsync(int settleMs, int timeoutMs, CancellationToken ct,
        Func<CancellationToken, Task>? refresh = null)
    {
        var watch = Stopwatch.StartNew();
        var frame = Current ?? await WaitNextAsync(-1, TimeSpan.FromSeconds(5), ct);
        if (settleMs == 0) return (frame, true, watch.ElapsedMilliseconds);
        while (watch.ElapsedMilliseconds < timeoutMs)
        {
            lock (sync)
            {
                if (failure is not null) throw new IOException("Video stream failed.", failure);
                // Only distinct decoded frames count as evidence. Reuse evidence accumulated by
                // the running stream; asking for a screenshot must not restart the settle timer.
                if (signature is not null && Stopwatch.GetElapsedTime(unchangedSince, lastEvidence).TotalMilliseconds >= settleMs)
                    return (current!, true, watch.ElapsedMilliseconds);
            }
            var remaining = Math.Max(1, timeoutMs - watch.ElapsedMilliseconds);
            try { frame = await WaitNextAsync(frame.Sequence, TimeSpan.FromMilliseconds(refresh is null ? remaining : Math.Min(remaining, 350)), ct); }
            catch (TimeoutException)
            {
                if (refresh is null || watch.ElapsedMilliseconds >= timeoutMs) break;
                await refresh(ct); refresh = null;
            }
        }
        return (Current ?? frame, false, watch.ElapsedMilliseconds);
    }
    internal static int[] Signature(YuvFrame frame)
    {
        var values = new int[1024];
        for (var gy = 0; gy < 32; gy++)
        for (var gx = 0; gx < 32; gx++)
        {
            var x0 = gx * frame.Width / 32; var x1 = Math.Max(x0 + 1, (gx + 1) * frame.Width / 32);
            var y0 = gy * frame.Height / 32; var y1 = Math.Max(y0 + 1, (gy + 1) * frame.Height / 32);
            long sum = 0;
            for (var y = y0; y < y1; y++) for (var x = x0; x < x1; x++) sum += frame.Y[y * frame.StrideY + x];
            values[gy * 32 + gx] = (int)(sum / ((x1 - x0) * (y1 - y0)));
        }
        return values;
    }
}
