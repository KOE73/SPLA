using System.Text.Json.Nodes;

namespace SPLA.Mcp.Client;

/// <summary>Frame construction and the handful of protocol constants worth naming. Nothing here
/// decides anything — it exists so that the shape of a frame is written once.</summary>
internal static class JsonRpc
{
    /// <summary>The protocol revision we negotiate. The server answers with the one it will actually
    /// speak, which may differ; we log the difference rather than refuse, because refusing on a
    /// version mismatch would break against every server that is merely newer than us.</summary>
    public const string ProtocolVersion = "2025-06-18";

    /// <summary>JSON-RPC's "there is no such method here". The answer to every request a server sends
    /// us in wave one — elicitation, sampling, roots. Not an invented error string: a server is
    /// entitled to a protocol-shaped refusal so it can decide for itself whether to continue without
    /// the answer or fail the call. See ADR_20260826_service_mcp-client §2.</summary>
    public const int MethodNotFound = -32601;

    public static JsonObject Request(long id, string method, JsonObject? parameters = null)
    {
        var frame = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["method"] = method
        };
        if (parameters is not null) frame["params"] = parameters;
        return frame;
    }

    public static JsonObject Notification(string method, JsonObject? parameters = null)
    {
        var frame = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["method"] = method
        };
        if (parameters is not null) frame["params"] = parameters;
        return frame;
    }

    public static JsonObject ErrorReply(JsonNode? id, int code, string message) => new()
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id?.DeepClone(),
        ["error"] = new JsonObject { ["code"] = code, ["message"] = message }
    };

    /// <summary>True for a frame that is a request from the server — it has a method (so it is not a
    /// reply) and an id (so it is not a notification) and therefore owes an answer.</summary>
    public static bool IsServerRequest(JsonNode frame) =>
        frame["method"] is not null && frame["id"] is not null;

    /// <summary>True for a notification: a method and no id, so it must NOT be answered. Replying to
    /// one is a protocol error, not merely noise.</summary>
    public static bool IsNotification(JsonNode frame) =>
        frame["method"] is not null && frame["id"] is null;
}

/// <summary>The server answered, and the answer was an error. Carried as an exception rather than a
/// return value because it aborts the one call that asked; turning it into something a model reads
/// happens above, where a <c>ToolResult</c> exists.</summary>
public sealed class McpServerException(int code, string message)
    : Exception($"MCP server returned error {code}: {message}")
{
    public int Code { get; } = code;
    public string ServerMessage { get; } = message;
}
