namespace SPLA.Domain.Agent;

/// <summary>How a blob's payload is materialised. All variants are held fully in memory
/// (materialisation-only by design — no streaming); the kind tells consumers how to interpret it.</summary>
public enum BlobKind
{
    Text,
    Bytes
}

/// <summary>
/// A materialised chunk of tool output that is too large (or too uninteresting) to put into the
/// model's context. Held entirely in memory. Either <see cref="Text"/> or <see cref="Bytes"/> is
/// set according to <see cref="Kind"/>.
/// </summary>
public sealed record BlobPayload(BlobKind Kind, string? Text, byte[]? Bytes, string? ContentType = null)
{
    /// <summary>Size of the payload in bytes (UTF-8 for text).</summary>
    public long Size => Kind == BlobKind.Bytes
        ? (Bytes?.LongLength ?? 0)
        : System.Text.Encoding.UTF8.GetByteCount(Text ?? string.Empty);

    public static BlobPayload OfText(string text, string? contentType = null)
        => new(BlobKind.Text, text, null, contentType);

    public static BlobPayload OfBytes(byte[] bytes, string? contentType = null)
        => new(BlobKind.Bytes, null, bytes, contentType);
}

/// <summary>Lightweight metadata about a stored blob, for listing/inspection without pulling the payload.</summary>
public sealed record BlobEntry(string Handle, string? Name, BlobKind Kind, long Size, DateTimeOffset CreatedAt);

/// <summary>
/// Per-chat store for bulk tool output that should bypass the model's context — the "data channel".
/// A tool that produces large data calls <see cref="Put"/> and returns only a short summary plus the
/// returned handle; a downstream tool consumes the data by passing that handle back in.
/// <para>
/// Same lifecycle and ambient-resolution pattern as <see cref="IKeyValueStore"/> (resolved per chat
/// via <see cref="AgentSessionScope"/>). Unlike KV this holds large opaque payloads, is addressed
/// only by handle, and is NOT auto-injected into the prompt. The default implementation is in-memory
/// and transient (cleared with the chat); the interface exists so a disk/overflow backend can be
/// layered later without touching tools.
/// </para>
/// </summary>
public interface IBlobStore
{
    /// <summary>
    /// Stores <paramref name="payload"/> and returns its handle (form <c>blob:&lt;id&gt;</c>).
    /// When <paramref name="name"/> is given the handle is stable (<c>blob:&lt;name&gt;</c>) and an
    /// existing blob with that name is overwritten; otherwise a fresh id is generated.
    /// </summary>
    string Put(BlobPayload payload, string? name = null);

    /// <summary>Returns the payload for <paramref name="handle"/>, or null if unknown.
    /// Accepts the handle with or without the <c>blob:</c> prefix.</summary>
    BlobPayload? Get(string handle);

    /// <summary>Removes the blob. Returns true if it existed.</summary>
    bool Delete(string handle);

    /// <summary>Metadata for every stored blob, ordered by creation time.</summary>
    IReadOnlyList<BlobEntry> List();

    /// <summary>Raised after any mutation (put, delete, clear). Lets a debug view react to live updates.</summary>
    event EventHandler? Changed;
}
