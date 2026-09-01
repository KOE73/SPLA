using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

public class ChatInboxTests
{
    [Fact]
    public void DrainAll_returns_messages_in_arrival_order()
    {
        var inbox = new ChatInbox();
        var first = new ChatMessage { Role = ChatRole.User, Content = "first" };
        var second = new ChatMessage { Role = ChatRole.User, Content = "second" };

        inbox.Enqueue(first, InboxItemKind.TaskResult);
        inbox.Enqueue(second, InboxItemKind.TaskResult);

        var drained = inbox.DrainAll();

        Assert.Equal(new[] { first, second }, drained);
    }

    [Fact]
    public void DrainAll_on_an_empty_inbox_returns_empty_not_null()
    {
        var inbox = new ChatInbox();

        Assert.Empty(inbox.DrainAll());
    }

    [Fact]
    public void A_message_is_delivered_once()
    {
        var inbox = new ChatInbox();
        inbox.Enqueue(new ChatMessage { Role = ChatRole.User, Content = "once" }, InboxItemKind.TaskResult);

        inbox.DrainAll();
        var second = inbox.DrainAll();

        Assert.Empty(second);
    }

    [Fact]
    public async Task Enqueue_is_safe_from_a_different_thread_than_DrainAll()
    {
        var inbox = new ChatInbox();

        var producer = Task.Run(() =>
        {
            for (var i = 0; i < 100; i++)
                inbox.Enqueue(new ChatMessage { Role = ChatRole.User, Content = i.ToString() }, InboxItemKind.TaskResult);
        });

        await producer;
        var drained = inbox.DrainAll();

        Assert.Equal(100, drained.Count);
    }

    [Fact]
    public void DrainAllWithKinds_returns_messages_with_kinds_in_arrival_order()
    {
        var inbox = new ChatInbox();
        var first = new ChatMessage { Role = ChatRole.User, Content = "first" };
        var second = new ChatMessage { Role = ChatRole.User, Content = "second" };

        inbox.Enqueue(first, InboxItemKind.Human);
        inbox.Enqueue(second, InboxItemKind.TaskResult);

        var drained = inbox.DrainAllWithKinds();

        Assert.Equal(2, drained.Count);
        Assert.Equal((first, InboxItemKind.Human), drained[0]);
        Assert.Equal((second, InboxItemKind.TaskResult), drained[1]);
    }

    [Fact]
    public void DrainAllWithKinds_on_an_empty_inbox_returns_empty_not_null()
    {
        var inbox = new ChatInbox();

        Assert.Empty(inbox.DrainAllWithKinds());
    }

    [Fact]
    public void DrainAllWithKinds_empties_the_queue()
    {
        var inbox = new ChatInbox();
        inbox.Enqueue(new ChatMessage { Role = ChatRole.User, Content = "once" }, InboxItemKind.Human);

        inbox.DrainAllWithKinds();
        var second = inbox.DrainAllWithKinds();

        Assert.Empty(second);
    }

    [Fact]
    public async Task DrainAllWithKinds_is_safe_from_a_different_thread_than_Enqueue()
    {
        var inbox = new ChatInbox();

        var producer = Task.Run(() =>
        {
            for (var i = 0; i < 100; i++)
                inbox.Enqueue(new ChatMessage { Role = ChatRole.User, Content = i.ToString() },
                    i % 2 == 0 ? InboxItemKind.Human : InboxItemKind.TaskResult);
        });

        await producer;
        var drained = inbox.DrainAllWithKinds();

        Assert.Equal(100, drained.Count);
        for (var i = 0; i < 100; i++)
            Assert.Equal(i % 2 == 0 ? InboxItemKind.Human : InboxItemKind.TaskResult, drained[i].Kind);
    }
}
