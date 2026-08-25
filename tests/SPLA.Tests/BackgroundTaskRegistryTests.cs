using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using System.Linq;
using System.Threading;
using Xunit;

namespace SPLA.Tests;

public class BackgroundTaskRegistryTests
{
    [Fact]
    public void TryStart_returns_a_running_record_with_a_bg_prefixed_id()
    {
        var registry = new BackgroundTaskRegistry();

        var (record, refusal) = registry.TryStart("system_run_shell", "{\"command\":\"dotnet build\"}");

        Assert.Null(refusal);
        Assert.NotNull(record);
        Assert.StartsWith("bg_", record!.Id);
        Assert.Equal(BackgroundTaskState.Running, record.State);
        Assert.Equal("system_run_shell", record.ToolName);
    }

    [Fact]
    public void Ids_are_distinct_across_calls()
    {
        var registry = new BackgroundTaskRegistry();

        var (a, _) = registry.TryStart("t", "{}");
        var (b, _) = registry.TryStart("t", "{}");

        Assert.NotEqual(a!.Id, b!.Id);
    }

    [Fact]
    public void Long_arguments_are_summarized_not_carried_whole()
    {
        var registry = new BackgroundTaskRegistry();
        var longArgs = "{\"data\":\"" + new string('x', 500) + "\"}";

        var (record, _) = registry.TryStart("t", longArgs);

        Assert.True(record!.ArgumentsSummary.Length < longArgs.Length);
        Assert.EndsWith("…", record.ArgumentsSummary);
    }

    [Fact]
    public void The_ninth_live_task_is_refused_not_queued()
    {
        var registry = new BackgroundTaskRegistry();
        for (var i = 0; i < BackgroundTaskRegistry.MaxLiveTasks; i++)
        {
            var (record, refusal) = registry.TryStart("t", "{}");
            Assert.NotNull(record);
            Assert.Null(refusal);
        }

        var (ninth, ninthRefusal) = registry.TryStart("t", "{}");

        Assert.Null(ninth);
        Assert.NotNull(ninthRefusal);
    }

    [Fact]
    public void A_finished_task_frees_a_slot_for_a_new_one()
    {
        var registry = new BackgroundTaskRegistry();
        var started = Enumerable.Range(0, BackgroundTaskRegistry.MaxLiveTasks)
            .Select(_ => registry.TryStart("t", "{}").record!).ToList();

        registry.Finish(started[0].Id, BackgroundTaskState.Completed, ToolResult.Text("done"));
        var (next, refusal) = registry.TryStart("t", "{}");

        Assert.NotNull(next);
        Assert.Null(refusal);
    }

    [Fact]
    public void Finish_records_state_and_result_and_a_finish_time()
    {
        var registry = new BackgroundTaskRegistry();
        var (record, _) = registry.TryStart("t", "{}");

        registry.Finish(record!.Id, BackgroundTaskState.Failed, ToolResult.Fail("boom"));

        Assert.Equal(BackgroundTaskState.Failed, record.State);
        Assert.Equal("boom", record.Result!.TextContent);
        Assert.NotNull(record.FinishedAt);
    }

    [Fact]
    public void Finish_on_an_unknown_id_does_not_throw()
    {
        var registry = new BackgroundTaskRegistry();
        registry.Finish("bg_nonexistent", BackgroundTaskState.Completed, ToolResult.Text("x"));
        // No assertion beyond "did not throw" — this is the shape a race between cancellation and
        // completion produces, and it must be a no-op, not a crash.
    }

    [Fact]
    public void TryGet_finds_a_known_task_and_fails_cleanly_for_an_unknown_one()
    {
        var registry = new BackgroundTaskRegistry();
        var (record, _) = registry.TryStart("t", "{}");

        Assert.True(registry.TryGet(record!.Id, out var found));
        Assert.Same(record, found);
        Assert.False(registry.TryGet("bg_nope", out _));
    }

    [Fact]
    public void All_lists_oldest_first_running_and_finished_alike()
    {
        var registry = new BackgroundTaskRegistry();
        var (a, _) = registry.TryStart("t", "{}");
        var (b, _) = registry.TryStart("t", "{}");
        registry.Finish(a!.Id, BackgroundTaskState.Completed, ToolResult.Text("done"));

        var all = registry.All;

        Assert.Equal(new[] { a.Id, b!.Id }, all.Select(t => t.Id));
    }

    [Fact]
    public void Cancel_signals_the_tasks_own_token_and_returns_true_once()
    {
        var registry = new BackgroundTaskRegistry();
        var (record, _) = registry.TryStart("t", "{}");

        var cancelled = registry.Cancel(record!.Id);

        Assert.True(cancelled);
        Assert.True(record.Cts.IsCancellationRequested);
    }

    [Fact]
    public void Cancel_on_an_unknown_or_finished_task_returns_false()
    {
        var registry = new BackgroundTaskRegistry();
        var (record, _) = registry.TryStart("t", "{}");
        registry.Finish(record!.Id, BackgroundTaskState.Completed, ToolResult.Text("done"));

        Assert.False(registry.Cancel("bg_nope"));
        Assert.False(registry.Cancel(record.Id));
    }

    [Fact]
    public void Forget_drops_a_finished_task_from_All()
    {
        var registry = new BackgroundTaskRegistry();
        var (record, _) = registry.TryStart("t", "{}");
        registry.Finish(record!.Id, BackgroundTaskState.Completed, ToolResult.Text("done"));

        registry.Forget(record.Id);

        Assert.DoesNotContain(registry.All, t => t.Id == record.Id);
    }

    [Fact]
    public void Disposing_the_chat_lifetime_token_cancels_every_live_tasks_own_token()
    {
        // This is the property BackgroundTaskRegistry exists to give ChatRuntime.Dispose: one cancel
        // reaching every live task without walking a list of ids by hand.
        using var chatLifetime = new CancellationTokenSource();
        var registry = new BackgroundTaskRegistry(chatLifetime.Token);
        var (a, _) = registry.TryStart("t", "{}");
        var (b, _) = registry.TryStart("t", "{}");

        chatLifetime.Cancel();

        Assert.True(a!.Cts.IsCancellationRequested);
        Assert.True(b!.Cts.IsCancellationRequested);
    }

    [Fact]
    public void LiveCount_counts_only_running_tasks()
    {
        var registry = new BackgroundTaskRegistry();
        var (a, _) = registry.TryStart("t", "{}");
        registry.TryStart("t", "{}");
        registry.Finish(a!.Id, BackgroundTaskState.Completed, ToolResult.Text("done"));

        Assert.Equal(1, registry.LiveCount);
    }
}
