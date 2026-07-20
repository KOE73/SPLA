namespace SPLA.Service;

/// <summary>A process-wide, one-shot chat request consumed by the first client that completes the
/// WebSocket handshake. Keeping the request server-side means the startup message uses the same
/// interactive turn path as a message typed in the Web client.</summary>
internal sealed class InitialChatRequest(string message)
{
    private InitialChat? _pending = new(message, CreateTitle(message));

    public static InitialChatRequest? Create(string? message)
        => string.IsNullOrWhiteSpace(message) ? null : new(message.Trim());

    public InitialChat? Take() => Interlocked.Exchange(ref _pending, null);

    private static string CreateTitle(string message)
    {
        var firstLine = message.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?.Trim();
        if (string.IsNullOrEmpty(firstLine)) return "New Chat";
        return firstLine.Length <= 40 ? firstLine : $"{firstLine[..40]}...";
    }
}

internal sealed record InitialChat(string Message, string Title);
