using SPLA.Runtime;
using Xunit;

namespace SPLA.Tests;

/// <summary>ADR_20260910-2 wave 0: the chat event stream itself, independent of ChatRuntime — order,
/// unsubscribe, and the "publish to nobody must be cheap and safe" promise (§4.7).</summary>
public class ChatFeedTests
{
    private static ChatTurnStarted Ev(string chatId, string? text = null) => new(text) { ChatId = chatId };

    [Fact]
    public void Publish_with_no_subscribers_does_nothing_and_does_not_throw()
    {
        var feed = new ChatFeed();
        var ex = Record.Exception(() => feed.Publish(Ev("c1")));
        Assert.Null(ex);
    }

    [Fact]
    public void A_subscriber_receives_events_in_the_order_they_were_published()
    {
        var feed = new ChatFeed();
        var received = new List<ChatEvent>();
        using var sub = feed.Subscribe(e => received.Add(e));

        feed.Publish(Ev("c1", "first"));
        feed.Publish(Ev("c1", "second"));
        feed.Publish(Ev("c1", "third"));

        Assert.Equal(3, received.Count);
        Assert.Equal("first", Assert.IsType<ChatTurnStarted>(received[0]).Text);
        Assert.Equal("second", Assert.IsType<ChatTurnStarted>(received[1]).Text);
        Assert.Equal("third", Assert.IsType<ChatTurnStarted>(received[2]).Text);
    }

    [Fact]
    public void Disposing_the_subscription_stops_further_delivery()
    {
        var feed = new ChatFeed();
        var received = new List<ChatEvent>();
        var sub = feed.Subscribe(e => received.Add(e));

        feed.Publish(Ev("c1", "seen"));
        sub.Dispose();
        feed.Publish(Ev("c1", "not seen"));

        var only = Assert.Single(received);
        Assert.Equal("seen", Assert.IsType<ChatTurnStarted>(only).Text);
    }

    [Fact]
    public void Disposing_twice_is_harmless()
    {
        var feed = new ChatFeed();
        var sub = feed.Subscribe(_ => { });
        sub.Dispose();
        var ex = Record.Exception(() => sub.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public void Two_independent_subscribers_each_see_every_event()
    {
        var feed = new ChatFeed();
        var a = new List<ChatEvent>();
        var b = new List<ChatEvent>();
        using var subA = feed.Subscribe(e => a.Add(e));
        using var subB = feed.Subscribe(e => b.Add(e));

        feed.Publish(Ev("c1"));
        feed.Publish(Ev("c1"));

        Assert.Equal(2, a.Count);
        Assert.Equal(2, b.Count);
    }

    [Fact]
    public void A_throwing_subscriber_does_not_stop_delivery_to_the_others()
    {
        var feed = new ChatFeed();
        var received = new List<ChatEvent>();
        using var bad = feed.Subscribe(_ => throw new InvalidOperationException("boom"));
        using var good = feed.Subscribe(e => received.Add(e));

        var ex = Record.Exception(() => feed.Publish(Ev("c1")));

        Assert.Null(ex);
        Assert.Single(received);
    }
}
