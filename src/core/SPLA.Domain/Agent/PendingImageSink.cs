namespace SPLA.Domain.Agent;

/// <summary>
/// Per-chat queue of images a tool wants the model to actually see. Tool results are plain text
/// (<see cref="SPLA.MCP.Core.Interfaces.IMcpTool.ExecuteAsync"/> returns a string), but vision
/// models only receive images via <see cref="Models.ChatMessage.Images"/> on a user-role message.
/// A tool that produces an image (e.g. a browser screenshot) pushes its data URL here; the
/// conversation loop drains the queue right after the tool result is recorded and injects a
/// synthetic user message carrying the images, so the model sees them on its very next turn.
/// Same ambient-resolution pattern as <see cref="IBlobStore"/> — per chat, transient, resolved via
/// <see cref="AgentSessionScope"/>.
/// </summary>
public interface IPendingImageSink
{
    /// <summary>Queues an image (as a data URL, e.g. <c>data:image/png;base64,...</c>) to be shown
    /// to the model on the next turn.</summary>
    void Push(string dataUrl);

    /// <summary>Returns and clears all currently queued images. Empty when nothing is pending.</summary>
    IReadOnlyList<string> DrainAll();
}

/// <summary>In-memory <see cref="IPendingImageSink"/>. Thread-safe for concurrent tool calls
/// (e.g. parallel script-driven tool invocations).</summary>
public sealed class PendingImageSink : IPendingImageSink
{
    private readonly List<string> _items = new();
    private readonly object _lock = new();

    public void Push(string dataUrl)
    {
        if (string.IsNullOrWhiteSpace(dataUrl)) return;
        lock (_lock) _items.Add(dataUrl);
    }

    public IReadOnlyList<string> DrainAll()
    {
        lock (_lock)
        {
            if (_items.Count == 0) return Array.Empty<string>();
            var copy = _items.ToArray();
            _items.Clear();
            return copy;
        }
    }
}
