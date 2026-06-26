using System.Text.Json;

namespace SPLA.Service.Contracts;

/// <summary>
/// Version of the wire protocol. Bumped when the envelope or a payload shape changes in a way
/// older clients cannot tolerate. A client compares this against what the server reports in
/// <see cref="WelcomePayload.ProtocolVersion"/> and may refuse to proceed on a mismatch.
/// </summary>
public static class ProtocolVersion
{
    public const string Current = "1";
}

/// <summary>
/// Every message — both directions — is one of these on the wire. <see cref="Type"/> selects the
/// payload shape; <see cref="Payload"/> carries it as raw JSON so a client only deserializes the
/// shapes it knows. <see cref="Auth"/> rides on every message (not just the handshake) so identity
/// and, later, capability checks can be enforced per-message without a protocol change. For now the
/// server ignores it beyond an optional connect token.
/// </summary>
public sealed class ProtocolEnvelope
{
    /// <summary>One of the <see cref="MessageTypes"/> constants.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Identity/authorization envelope. Reserved: the server is allow-all at this stage.</summary>
    public AuthInfo? Auth { get; set; }

    /// <summary>Which chat this message concerns, when applicable.</summary>
    public string? ChatId { get; set; }

    /// <summary>
    /// Correlation id for request/response pairs that need one — permission and clarify round-trips
    /// carry the same RequestId on the outgoing request and the incoming answer.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>The typed body for <see cref="Type"/>, as raw JSON.</summary>
    public JsonElement? Payload { get; set; }
}

/// <summary>
/// Identity/authorization carried on every message. At this stage only <see cref="Token"/> may be
/// checked, and only when the server is bound to a non-loopback address. <see cref="ActorId"/> and
/// capability negotiation are reserved for later stages — the fields exist now so the protocol does
/// not have to change when auth grows from "allow-all on localhost" to shared-secret to per-actor.
/// </summary>
public sealed class AuthInfo
{
    /// <summary>Shared connect secret. Ignored on loopback; required when bound to a network address (future).</summary>
    public string? Token { get; set; }

    /// <summary>Who the client claims to be. Server-assigned in <see cref="WelcomePayload"/>; reserved.</summary>
    public string? ActorId { get; set; }
}

/// <summary>The set of <see cref="ProtocolEnvelope.Type"/> values, grouped by direction.</summary>
public static class MessageTypes
{
    // ── Client → Server ──────────────────────────────────────────────────
    public const string Hello = "hello";
    public const string ChatList = "chat.list";
    public const string ChatOpen = "chat.open";
    public const string ChatNew = "chat.new";
    public const string ChatRename = "chat.rename";
    public const string ChatDelete = "chat.delete";
    public const string ChatSend = "chat.send";
    public const string ChatSettings = "chat.settings";
    /// <summary>A window tells the service which chat it has focused; the service echoes
    /// <see cref="FocusChanged"/> to every connection so auxiliary windows (e.g. a tear-off debug
    /// panel) can follow the active chat across separate sockets.</summary>
    public const string FocusSet = "focus.set";
    public const string Cancel = "cancel";
    public const string PermissionDecision = "permission.decision";
    public const string ClarifyChoice = "clarify.choice";
    public const string DebugRequest = "debug.request";

    // ── Server → Client ──────────────────────────────────────────────────
    public const string Welcome = "welcome";
    public const string ChatListResult = "chat.list.result";
    public const string ChatOpened = "chat.opened";
    public const string LlmTurnStart = "llm.turn.start";
    public const string Delta = "delta";
    public const string Reasoning = "reasoning";
    public const string AssistantMessage = "assistant.message";
    public const string ToolStarted = "tool.started";
    public const string ToolProgress = "tool.progress";
    public const string ToolResult = "tool.result";
    public const string Notice = "notice";
    public const string TokenUsage = "token.usage";
    public const string TurnComplete = "turn.complete";
    public const string PermissionRequest = "permission.request";
    public const string ClarifyRequest = "clarify.request";
    public const string DebugSnapshot = "debug.snapshot";
    /// <summary>Broadcast to all connections when a window changes the focused chat (see <see cref="FocusSet"/>).</summary>
    public const string FocusChanged = "focus.changed";
    public const string Error = "error";
}

/// <summary>
/// Capability tokens the server grants a client in <see cref="WelcomePayload.Capabilities"/>. Today
/// the server grants the full set, but clients are expected to look at what they were granted rather
/// than assume omnipotence — so that when groups/roles arrive, a restricted grant just works.
/// </summary>
public static class Capabilities
{
    public const string Chat = "chat";
    public const string Debug = "debug";
    public const string Manage = "manage";

    public static readonly string[] Full = { Chat, Debug, Manage };
}
