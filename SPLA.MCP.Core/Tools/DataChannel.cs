using SPLA.Domain.Agent;
using System.Text;

namespace SPLA.MCP.Core.Tools;

/// <summary>Where a data-producing tool sends its payload.</summary>
public enum OutputTarget
{
    /// <summary>Default. Inline the data into the model's context (legacy behaviour).</summary>
    Context,
    /// <summary>Store the data in the chat's blob store; the model gets only a summary + handle.</summary>
    Blob,
    /// <summary>Both: store in the blob store AND inline into context.</summary>
    Both
}

/// <summary>
/// The single place the "data channel" convention lives. Tools don't each implement routing — a
/// data-producing tool computes its payload and a one-line summary, then calls <see cref="Route"/>;
/// a data-consuming tool calls <see cref="ResolveText"/>/<see cref="ResolveBytes"/> on any input that
/// might be a <c>blob:</c> handle. Target is selected by the standard <c>output</c> parameter every
/// such tool exposes via <see cref="SchemaParts"/>.
/// </summary>
public static class DataChannel
{
    public const string HandlePrefix = BlobStore.HandlePrefix;

    public static OutputTarget ParseTarget(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "blob" => OutputTarget.Blob,
        "both" => OutputTarget.Both,
        _ => OutputTarget.Context
    };

    /// <summary>True if <paramref name="value"/> looks like a blob handle (<c>blob:…</c>).</summary>
    public static bool IsHandle(string? value)
        => value != null && value.StartsWith(HandlePrefix, StringComparison.Ordinal);

    /// <summary>The ambient chat's blob store, or null when no chat run is active (e.g. unit test, CLI).</summary>
    private static IBlobStore? Store => AgentSessionScope.Current?.Blobs;

    /// <summary>
    /// Routes produced <paramref name="payload"/> per <paramref name="target"/> and returns the string
    /// the model should see. <paramref name="summary"/> is a short human/model-facing description of
    /// what was produced (counts, size). <paramref name="name"/> optionally names the stored blob.
    /// Falls back to inlining when blob storage is requested but no store is available.
    /// </summary>
    public static string Route(OutputTarget target, BlobPayload payload, string summary, string? name = null)
    {
        if (target == OutputTarget.Context)
            return Inline(payload, summary);

        var store = Store;
        if (store is null)
            return Inline(payload, summary) + "\n(note: no active chat — blob storage unavailable, data inlined)";

        var handle = store.Put(payload, name);
        var meta = $"{summary}\nstored: {handle}  ({payload.Kind}, {payload.Size} bytes). " +
                   $"Pass this handle to a consuming tool to use the data without loading it into context.";

        return target == OutputTarget.Both && payload.Kind == BlobKind.Text
            ? meta + "\n\n" + payload.Text
            : meta;
    }

    /// <summary>
    /// Resolves a text input that may be either a literal value or a <c>blob:</c> handle. Returns true
    /// on success; on a handle that can't be found/decoded, returns false with the reason in
    /// <paramref name="error"/>.
    /// </summary>
    public static bool ResolveText(string? value, out string text, out string? error)
    {
        text = string.Empty;
        error = null;
        if (value is null) { error = "value is null"; return false; }
        if (!IsHandle(value)) { text = value; return true; }

        var payload = Store?.Get(value);
        if (payload is null) { error = $"blob handle not found: {value}"; return false; }

        text = payload.Kind == BlobKind.Text
            ? payload.Text ?? string.Empty
            : Encoding.UTF8.GetString(payload.Bytes ?? Array.Empty<byte>());
        return true;
    }

    /// <summary>Resolves a byte input that may be either inline (treated as UTF-8 text) or a <c>blob:</c> handle.</summary>
    public static bool ResolveBytes(string? value, out byte[] bytes, out string? error)
    {
        bytes = Array.Empty<byte>();
        error = null;
        if (value is null) { error = "value is null"; return false; }
        if (!IsHandle(value)) { bytes = Encoding.UTF8.GetBytes(value); return true; }

        var payload = Store?.Get(value);
        if (payload is null) { error = $"blob handle not found: {value}"; return false; }

        bytes = payload.Kind == BlobKind.Bytes
            ? payload.Bytes ?? Array.Empty<byte>()
            : Encoding.UTF8.GetBytes(payload.Text ?? string.Empty);
        return true;
    }

    private static string Inline(BlobPayload payload, string summary)
    {
        if (payload.Kind == BlobKind.Bytes)
            return $"{summary}\n(binary payload, {payload.Size} bytes — request output=blob to capture it)";
        // For pure inlining the data IS the result; the summary is implied by the content.
        return payload.Text ?? string.Empty;
    }
}

/// <summary>Reusable JSON-schema fragments so the standard data-channel parameters are declared once,
/// not copy-pasted into every tool. Add these to a producing tool's <c>properties</c>.</summary>
public static class SchemaParts
{
    /// <summary>The <c>output</c> parameter: context | blob | both.</summary>
    public static object Output => new
    {
        type = new[] { "string", "null" },
        @enum = new[] { "context", "blob", "both" },
        description = "Where the result data goes: 'context' (default) inlines it into your reply; " +
                      "'blob' stores it in working-memory and returns a blob:<handle> instead — use this for " +
                      "bulk data you'll route to another tool (e.g. write to a file) without reading it; " +
                      "'both' does both."
    };

    /// <summary>The optional <c>output_name</c> parameter for naming the stored blob.</summary>
    public static object OutputName => new
    {
        type = new[] { "string", "null" },
        description = "Optional name for the stored blob (handle becomes blob:<name>). Omit for an auto id. Only used when output is 'blob' or 'both'."
    };
}
