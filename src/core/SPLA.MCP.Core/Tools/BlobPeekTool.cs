using SPLA.Domain.Agent;
using SPLA.Domain.Resources;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// <c>blob_peek</c> — looks at a small window of a stored blob without pulling the whole thing into
/// context: a slice of text, or a hex dump for binary.
/// <para>
/// This exists because its absence was doing damage. A blob the model could not read but wanted to
/// understand left it one tool that accepted a handle "to look at" — <see cref="ImageViewTool"/> —
/// and it reached for that. A bounded, honest way to inspect bytes removes the temptation instead of
/// forbidding the mistake.
/// </para>
/// </summary>
public sealed class BlobPeekTool : IMcpTool
{
    private const int DefaultLength = 256;
    private const int MaxLength = 2048;

    public string Name => "blob_peek";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Shows a small window of a stored blob (by blob: handle) — a slice of the text, or a " +
                          "hex dump when the blob is binary. Use it to identify or sanity-check data you are " +
                          "routing between tools; it is deliberately bounded and never returns the whole payload.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    handle = new { type = "string", description = "The blob: handle to inspect." },
                    offset = new { type = new[] { "integer", "null" }, description = "Byte (or character, for text) offset to start at. Default 0." },
                    length = new { type = new[] { "integer", "null" }, description = $"How much to show. Default {DefaultLength}, capped at {MaxLength}." }
                },
                required = new[] { "handle" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string? handle;
        int offset, length;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            handle = ToolJson.GetStringTrimmed(doc.RootElement, "handle");
            offset = Math.Max(0, ToolJson.GetInt32(doc.RootElement, "offset", 0));
            length = Math.Clamp(ToolJson.GetInt32(doc.RootElement, "length", DefaultLength), 1, MaxLength);
        }
        catch (JsonException) { return Task.FromResult(ToolResult.Fail("error: invalid_json", "invalid json")); }

        if (handle is null) return Task.FromResult(ToolResult.Fail("error: 'handle' is required", "missing handle"));

        var session = AgentSessionScope.Current;
        if (session is null) return Task.FromResult(ToolResult.Refuse("error: no active chat session", "no chat session"));

        var payload = session.Blobs.Get(handle);
        if (payload is null) return Task.FromResult(ToolResult.Fail($"error: no blob found for handle '{handle}'", "unknown handle"));

        var type = string.IsNullOrWhiteSpace(payload.ContentType) ? ContentTypes.Unknown : payload.ContentType;
        var head = $"{handle} — {(payload.Kind == BlobKind.Text ? "text" : "binary")} {type}, {payload.Size} bytes total";

        if (payload.Kind == BlobKind.Text)
        {
            var text = payload.Text ?? string.Empty;
            if (offset >= text.Length) return Task.FromResult(ToolResult.Text($"{head}\n(offset {offset} is past the end)"));
            var slice = text.Substring(offset, Math.Min(length, text.Length - offset));
            var shown = offset + slice.Length;
            return Task.FromResult(ToolResult.Text(
                $"{head}\nchars {offset}..{shown}:\n{slice}" +
                (shown < text.Length ? $"\n… ({text.Length - shown} more characters)" : "")));
        }

        var bytes = payload.Bytes ?? Array.Empty<byte>();
        if (offset >= bytes.Length) return Task.FromResult(ToolResult.Text($"{head}\n(offset {offset} is past the end)"));
        var count = Math.Min(length, bytes.Length - offset);
        var end = offset + count;

        var sb = new StringBuilder();
        sb.Append(head).Append("\nbytes ").Append(offset).Append("..").Append(end).Append(":\n");
        for (var i = offset; i < end; i += 16)
        {
            var lineEnd = Math.Min(i + 16, end);
            sb.Append(i.ToString("x8")).Append("  ");
            for (var j = i; j < i + 16; j++)
                sb.Append(j < lineEnd ? bytes[j].ToString("x2") : "  ").Append(' ');
            sb.Append(' ');
            for (var j = i; j < lineEnd; j++)
                sb.Append(bytes[j] >= 0x20 && bytes[j] < 0x7F ? (char)bytes[j] : '.');
            sb.Append('\n');
        }
        if (end < bytes.Length) sb.Append("… (").Append(bytes.Length - end).Append(" more bytes)\n");

        return Task.FromResult(ToolResult.Text(sb.ToString()));
    }
}
