using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;

namespace SPLA.Tests;

public class ToolProgressScopeTests
{
    [Fact]
    public void Report_without_open_scope_is_a_noop()
    {
        // Must not throw when nothing is listening.
        ToolProgressScope.Report(1, 10);
        ToolProgressScope.Report(new ToolProgress { Message = "x" });
    }

    [Fact]
    public void Open_scope_receives_ticks_then_stops_after_dispose()
    {
        var seen = new List<ToolProgress>();
        using (ToolProgressScope.Begin(seen.Add))
        {
            ToolProgressScope.Report(1, 4);
            ToolProgressScope.Report(2, 4);
        }
        ToolProgressScope.Report(3, 4); // outside scope — ignored

        Assert.Equal(2, seen.Count);
        Assert.Equal(1, seen[0].Current);
        Assert.Equal(0.25, seen[0].Fraction);
    }

    [Fact]
    public async Task Sink_flows_across_async_and_parallel_boundaries()
    {
        // This is the whole point: a tool reporting deep inside Parallel.ForEachAsync must reach the
        // sink opened by the caller, with no parameter threading.
        var ticks = new ConcurrentBag<long>();
        using (ToolProgressScope.Begin(p => { if (p.Current is long c) ticks.Add(c); }))
        {
            await SimulateToolAsync(itemCount: 50);
        }

        Assert.Equal(50, ticks.Count);
        Assert.Contains(50L, ticks);
    }

    [Fact]
    public void Fraction_is_null_when_total_unknown_or_zero()
    {
        Assert.Null(new ToolProgress { Current = 5 }.Fraction);
        Assert.Null(new ToolProgress { Current = 5, Total = 0 }.Fraction);
        Assert.Equal(0.5, new ToolProgress { Current = 1, Total = 2 }.Fraction);
    }

    private static async Task SimulateToolAsync(int itemCount)
    {
        long done = 0;
        await Parallel.ForEachAsync(Enumerable.Range(0, itemCount), async (i, ct) =>
        {
            await Task.Yield();
            var n = System.Threading.Interlocked.Increment(ref done);
            ToolProgressScope.Report(n, itemCount);
        });
    }
}
