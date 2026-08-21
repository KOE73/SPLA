using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.Runtime;

namespace SPLA.Tests;

public sealed class PendingAskStoreTests
{
    private static ToolFunctionDefinition CreateToolDef(string name)
        => new() { Name = name, Description = "test tool" };

    [Fact]
    public async Task AskPermissionAsync_raises_Asked_event_and_lists_the_ask()
    {
        var store = new PendingAskStore(TimeSpan.Zero);
        var def = CreateToolDef("testTool");
        var chatId = "chat1";
        var arguments = "{\"arg\":\"value\"}";

        var asked = new List<PendingAsk>();
        store.Asked += ask => asked.Add(ask);

        var cts = new CancellationTokenSource();
        var task = store.AskPermissionAsync(chatId, def, arguments, cts.Token);

        // Verify event was raised
        Assert.Single(asked);
        var ask = asked[0];
        Assert.Equal(chatId, ask.ChatId);
        Assert.Equal("testTool", ask.ToolName);
        Assert.Equal(arguments, ask.Arguments);
        Assert.Equal(PendingAskKind.Permission, ask.Kind);

        // Verify it lists while outstanding
        var listed = store.List();
        Assert.Single(listed);
        Assert.Equal(ask.RequestId, listed[0].RequestId);

        cts.Cancel();
        await task;
    }

    [Fact]
    public async Task CompletePermission_with_right_requestId_completes_task_and_raises_Resolved()
    {
        var store = new PendingAskStore(TimeSpan.Zero);
        var def = CreateToolDef("testTool");
        var chatId = "chat1";

        var cts = new CancellationTokenSource();
        var askTask = store.AskPermissionAsync(chatId, def, "{}", cts.Token);
        var asks = store.List();
        var requestId = asks[0].RequestId;

        var resolved = new List<(PendingAsk, AskResolution)>();
        store.Resolved += (ask, resolution) => resolved.Add((ask, resolution));

        var result = store.CompletePermission(requestId, PermissionDecision.AllowOnce);

        Assert.True(result);
        var decision = await askTask;
        Assert.Equal(PermissionDecision.AllowOnce, decision);
        Assert.Single(resolved);
        Assert.Equal(AskResolution.Answered, resolved[0].Item2);
        Assert.Empty(store.List());
    }

    [Fact]
    public async Task CompletePermission_called_twice_with_same_requestId_returns_false_second_time()
    {
        var store = new PendingAskStore(TimeSpan.Zero);
        var def = CreateToolDef("testTool");
        var chatId = "chat1";

        var cts = new CancellationTokenSource();
        var askTask = store.AskPermissionAsync(chatId, def, "{}", cts.Token);
        var asks = store.List();
        var requestId = asks[0].RequestId;

        var resolvedCount = 0;
        store.Resolved += (_, _) => resolvedCount++;

        var first = store.CompletePermission(requestId, PermissionDecision.AllowOnce);
        var second = store.CompletePermission(requestId, PermissionDecision.Deny);

        Assert.True(first);
        Assert.False(second);
        Assert.Equal(1, resolvedCount);  // Only one Resolved event
        var decision = await askTask;
        Assert.Equal(PermissionDecision.AllowOnce, decision);
    }

    [Fact]
    public void CompletePermission_with_unknown_requestId_returns_false()
    {
        var store = new PendingAskStore(TimeSpan.Zero);

        var result = store.CompletePermission("unknown-id", PermissionDecision.Deny);

        Assert.False(result);
    }

    [Fact]
    public async Task Cancelling_CancellationToken_denies_and_raises_Cancelled()
    {
        var store = new PendingAskStore(TimeSpan.Zero);
        var def = CreateToolDef("testTool");
        var chatId = "chat1";

        var cts = new CancellationTokenSource();
        var askTask = store.AskPermissionAsync(chatId, def, "{}", cts.Token);

        var resolved = new List<(PendingAsk, AskResolution)>();
        store.Resolved += (ask, resolution) => resolved.Add((ask, resolution));

        cts.Cancel();
        var decision = await askTask;

        Assert.Equal(PermissionDecision.Deny, decision);
        Assert.Single(resolved);
        Assert.Equal(AskResolution.Cancelled, resolved[0].Item2);
        Assert.Empty(store.List());
    }

    [Fact]
    public async Task Short_timeout_denies_and_raises_TimedOut()
    {
        var store = new PendingAskStore(TimeSpan.FromMilliseconds(150));
        var def = CreateToolDef("testTool");
        var chatId = "chat1";

        var cts = new CancellationTokenSource();
        var askTask = store.AskPermissionAsync(chatId, def, "{}", cts.Token);

        var resolved = new List<(PendingAsk, AskResolution)>();
        store.Resolved += (ask, resolution) => resolved.Add((ask, resolution));

        var decision = await askTask;

        Assert.Equal(PermissionDecision.Deny, decision);
        Assert.Single(resolved);
        Assert.Equal(AskResolution.TimedOut, resolved[0].Item2);
        Assert.Empty(store.List());
    }

    [Fact]
    public async Task AskClarifyAsync_with_CompleteClarify_returns_chosen_string()
    {
        var store = new PendingAskStore(TimeSpan.Zero);
        var request = new ClarifyRequest
        {
            Question = "Which option?",
            Options = new List<ClarifyOption>
            {
                new() { Label = "Option A" },
                new() { Label = "Option B" }
            }
        };
        var chatId = "chat1";

        var cts = new CancellationTokenSource();
        var clarifyTask = store.AskClarifyAsync(chatId, request, cts.Token);
        var asks = store.List();
        var requestId = asks[0].RequestId;

        var result = store.CompleteClarify(requestId, "Option B");

        Assert.True(result);
        var choice = await clarifyTask;
        Assert.Equal("Option B", choice);
        Assert.Empty(store.List());
    }

    [Fact]
    public async Task CompleteClarify_with_null_choice_returns_null()
    {
        var store = new PendingAskStore(TimeSpan.Zero);
        var request = new ClarifyRequest
        {
            Question = "Which?",
            Options = new List<ClarifyOption>
            {
                new() { Label = "A" }
            }
        };
        var chatId = "chat1";

        var cts = new CancellationTokenSource();
        var clarifyTask = store.AskClarifyAsync(chatId, request, cts.Token);
        var asks = store.List();
        var requestId = asks[0].RequestId;

        var result = store.CompleteClarify(requestId, null);

        Assert.True(result);
        var choice = await clarifyTask;
        Assert.Null(choice);
    }

    [Fact]
    public async Task List_filters_by_chatId()
    {
        var store = new PendingAskStore(TimeSpan.Zero);
        var def = CreateToolDef("testTool");

        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();

        // Ask on chat1
        var task1 = store.AskPermissionAsync("chat1", def, "{}", cts1.Token);

        // Ask on chat2
        var task2 = store.AskPermissionAsync("chat2", def, "{}", cts2.Token);

        var allAsks = store.List();
        Assert.Equal(2, allAsks.Count);

        var chat1Asks = store.List("chat1");
        Assert.Single(chat1Asks);
        Assert.Equal("chat1", chat1Asks[0].ChatId);

        var chat2Asks = store.List("chat2");
        Assert.Single(chat2Asks);
        Assert.Equal("chat2", chat2Asks[0].ChatId);

        cts1.Cancel();
        cts2.Cancel();
        await task1;
        await task2;
    }

    [Fact]
    public async Task AbandonAll_completes_all_outstanding_asks()
    {
        var store = new PendingAskStore(TimeSpan.Zero);
        var def = CreateToolDef("testTool");

        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();

        var task1 = store.AskPermissionAsync("chat1", def, "{}", cts1.Token);
        var task2 = store.AskPermissionAsync("chat2", def, "{}", cts2.Token);

        var resolved = new List<AskResolution>();
        store.Resolved += (_, resolution) => resolved.Add(resolution);

        store.AbandonAll(AskResolution.Cancelled);

        var decision1 = await task1;
        var decision2 = await task2;

        Assert.Equal(PermissionDecision.Deny, decision1);
        Assert.Equal(PermissionDecision.Deny, decision2);
        Assert.Equal(2, resolved.Count);
        Assert.All(resolved, r => Assert.Equal(AskResolution.Cancelled, r));
        Assert.Empty(store.List());
    }

    [Fact]
    public void TurnRegistry_TryCancel_unknown_chat_returns_false()
    {
        var registry = new TurnRegistry();
        var result = registry.TryCancel("unknown-chat");

        Assert.False(result);
    }

    [Fact]
    public void TurnRegistry_TryCancel_after_Register_cancels_and_returns_true()
    {
        var registry = new TurnRegistry();
        var cts = new CancellationTokenSource();

        registry.Register("chat1", cts);
        var result = registry.TryCancel("chat1");

        Assert.True(result);
        Assert.True(cts.Token.IsCancellationRequested);
    }

    [Fact]
    public void TurnRegistry_Remove_with_same_CancellationTokenSource_clears_it()
    {
        var registry = new TurnRegistry();
        var cts = new CancellationTokenSource();

        registry.Register("chat1", cts);
        registry.Remove("chat1", cts);

        var result = registry.TryCancel("chat1");
        Assert.False(result);
    }

    [Fact]
    public void TurnRegistry_Remove_with_different_CancellationTokenSource_does_not_clear()
    {
        var registry = new TurnRegistry();
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();

        registry.Register("chat1", cts1);
        registry.Remove("chat1", cts2);  // Different source

        var result = registry.TryCancel("chat1");
        Assert.True(result);  // cts1 is still there
        Assert.True(cts1.Token.IsCancellationRequested);
    }
}
