using System.Collections.Generic;

namespace SPLA.Service.Contracts;

// ──────────────────────────────────────────────────────────────────────────
//  Shared data shapes
// ──────────────────────────────────────────────────────────────────────────

/// <summary>A tool call as seen on the wire — the function name and its raw JSON arguments.</summary>
public sealed class ToolCallDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

/// <summary>
/// One rendered chat message. Mirrors the agent's domain message but only the fields a client needs
/// to display it — role, the markdown/plain content, optional reasoning, and tool calls.
/// </summary>
public sealed class ChatMessageDto
{
    public string MsgId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Reasoning { get; set; }
    public List<ToolCallDto>? ToolCalls { get; set; }
    public string? ToolCallId { get; set; }
    public bool IsEphemeral { get; set; }

    /// <summary>URLs of attached images (e.g. /chat-image/&lt;chatId&gt;/&lt;file&gt; on reopen, or data URLs
    /// for a freshly sent message). Null when the message has no images.</summary>
    public List<string>? Images { get; set; }
}

/// <summary>A chat in the list view.</summary>
public sealed class ChatSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

/// <summary>One selectable option in a clarify request.</summary>
public sealed class ClarifyOptionDto
{
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>A project connection (endpoint/model bundle) a chat can be pointed at.</summary>
public sealed class ConnectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

// ──────────────────────────────────────────────────────────────────────────
//  Client → Server
// ──────────────────────────────────────────────────────────────────────────

/// <summary>First message a client sends; the server replies with <see cref="WelcomePayload"/>.</summary>
public sealed class HelloPayload
{
    public string? ClientName { get; set; }
    public string? ClientVersion { get; set; }
    public string ProtocolVersion { get; set; } = Contracts.ProtocolVersion.Current;
}

public sealed class ChatOpenPayload
{
    public string ChatId { get; set; } = string.Empty;
}

public sealed class ChatNewPayload
{
    public string? Title { get; set; }
}

public sealed class ChatRenamePayload
{
    public string ChatId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public sealed class ChatDeletePayload
{
    public string ChatId { get; set; } = string.Empty;
}

public sealed class ChatSendPayload
{
    public string ChatId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    /// <summary>Optional attached images as data URLs (data:image/png;base64,…) for vision models.</summary>
    public List<string>? Images { get; set; }
}

/// <summary>Which chat a window has focused. Sent as <see cref="MessageTypes.FocusSet"/> and echoed
/// back to all connections as <see cref="MessageTypes.FocusChanged"/> so tear-off windows can follow.</summary>
public sealed class FocusPayload
{
    public string ChatId { get; set; } = string.Empty;
}

/// <summary>Changes a chat's behaviour: its mode and/or which connection it uses. Null fields are left as-is.</summary>
public sealed class ChatSettingsPayload
{
    public string ChatId { get; set; } = string.Empty;
    public string? Mode { get; set; }
    public string? ConnectionId { get; set; }
}

/// <summary>Answer to a <see cref="PermissionRequestPayload"/>; correlated by envelope RequestId.</summary>
public sealed class PermissionDecisionPayload
{
    /// <summary>One of "allowOnce", "allowRemember", "deny".</summary>
    public string Decision { get; set; } = "deny";
}

/// <summary>Answer to a <see cref="ClarifyRequestPayload"/>; correlated by envelope RequestId.</summary>
public sealed class ClarifyChoicePayload
{
    /// <summary>The chosen option label, or null to let the agent decide.</summary>
    public string? Choice { get; set; }
}

/// <summary>One editable LLM connection (full shape, unlike the id+name <see cref="ConnectionDto"/>
/// used for pickers). Carried both ways: server→client for the editor, client→server on save.</summary>
public sealed class ConnectionEditDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Provider { get; set; }
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? Model { get; set; }
}

/// <summary>The full connection list for the editor. <see cref="ConnectionsGet"/> answer and
/// <see cref="ConnectionsSave"/> body; also broadcast as <see cref="MessageTypes.ConnectionsResult"/>.</summary>
public sealed class ConnectionsPayload
{
    public List<ConnectionEditDto> Connections { get; set; } = new();

    /// <summary>False when there is no .spla project to persist into (edits then live only in-memory).</summary>
    public bool CanPersist { get; set; }
}

/// <summary>Asks the server for a debug snapshot. <see cref="Kind"/> selects which inspector view.</summary>
public sealed class DebugRequestPayload
{
    /// <summary>One of <see cref="DebugKinds"/>.</summary>
    public string Kind { get; set; } = string.Empty;
}

public static class DebugKinds
{
    public const string KvSession = "kv.session";
    public const string KvProject = "kv.project";
    public const string Blobs = "blobs";
    public const string LastContext = "context.last";
    public const string Prompt = "prompt";
}

// ──────────────────────────────────────────────────────────────────────────
//  Server → Client
// ──────────────────────────────────────────────────────────────────────────

public sealed class WelcomePayload
{
    public string ProtocolVersion { get; set; } = Contracts.ProtocolVersion.Current;
    public string ActorId { get; set; } = string.Empty;
    public string[] Capabilities { get; set; } = System.Array.Empty<string>();
    public string ServerName { get; set; } = "SPLA";
    public string? ProjectName { get; set; }
    public string? WorkspacePath { get; set; }

    /// <summary>Project connections a chat can point at, and the available agent modes — so a client
    /// can offer per-chat model/mode pickers without hardcoding them.</summary>
    public List<ConnectionDto> Connections { get; set; } = new();
    public string[] Modes { get; set; } = System.Array.Empty<string>();
    public string DefaultMode { get; set; } = string.Empty;
}

public sealed class ChatListResultPayload
{
    public List<ChatSummaryDto> Chats { get; set; } = new();
}

/// <summary>Full state of a chat the client just opened (or created): its existing messages + settings.</summary>
public sealed class ChatOpenedPayload
{
    public string ChatId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<ChatMessageDto> Messages { get; set; } = new();
    public string Mode { get; set; } = string.Empty;
    public string? ConnectionId { get; set; }
}

public sealed class DeltaPayload
{
    public int MsgIndex { get; set; }
    public string Text { get; set; } = string.Empty;
}

public sealed class ReasoningPayload
{
    public int MsgIndex { get; set; }
    public string Text { get; set; } = string.Empty;
}

public sealed class AssistantMessagePayload
{
    public int MsgIndex { get; set; }
    public ChatMessageDto Message { get; set; } = new();
}

public sealed class ToolStartedPayload
{
    public ToolCallDto ToolCall { get; set; } = new();
}

public sealed class ToolProgressPayload
{
    public string ToolName { get; set; } = string.Empty;
    public long Current { get; set; }
    public long Total { get; set; }
    public double? Fraction { get; set; }
    public string? Message { get; set; }
    public List<ToolProgressDetailDto>? Details { get; set; }
}

public sealed class ToolProgressDetailDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public sealed class ToolResultPayload
{
    public string ToolCallId { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}

public sealed class NoticePayload
{
    public string Text { get; set; } = string.Empty;
}

public sealed class TokenUsagePayload
{
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
}

/// <summary>Sent when a chat's turn finishes (the agent loop returned), so the client can re-enable input.</summary>
public sealed class TurnCompletePayload
{
    public bool Cancelled { get; set; }
    public string? Error { get; set; }
}

public sealed class PermissionRequestPayload
{
    public string ToolName { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

public sealed class ClarifyRequestPayload
{
    public string Question { get; set; } = string.Empty;
    public List<ClarifyOptionDto> Options { get; set; } = new();
}

/// <summary>
/// A debug snapshot the server produced in response to <see cref="DebugRequestPayload"/>. The shape
/// of <see cref="Entries"/>/<see cref="Text"/> depends on <see cref="Kind"/> — KV kinds fill Entries,
/// the prompt kind fills Text/Segments. Keeps debug a read-only protocol concern, available to any
/// client, not just the native debug windows.
/// </summary>
public sealed class DebugSnapshotPayload
{
    public string Kind { get; set; } = string.Empty;
    public List<DebugKvEntryDto>? Entries { get; set; }
    public List<DebugSegmentDto>? Segments { get; set; }
    public string? Text { get; set; }
}

public sealed class DebugKvEntryDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public sealed class DebugSegmentDto
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public sealed class ErrorPayload
{
    public string Message { get; set; } = string.Empty;
    public string? Detail { get; set; }
}
