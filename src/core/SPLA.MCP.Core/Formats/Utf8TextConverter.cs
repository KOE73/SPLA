using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Formats;
using SPLA.Domain.Resources;

namespace SPLA.MCP.Core.Formats;

/// <summary>
/// Bytes to text, when the bytes really are UTF-8 — and a loud, named refusal when they are not.
///
/// <para><b>The refusal is the point.</b> Identity proves that a lookup happens; it proves nothing
/// about what a conversion does when it cannot be done. This converter is in the day-one set because
/// it is the first one that can fail on its input, and a registry whose members never fail has not
/// been shown to carry a failure back to the caller at all.</para>
///
/// <para>Registered twice — once for <c>text/*</c> and once for <c>application/octet-stream</c> — the
/// two sources whose bytes are plausibly text. The registry keys on the (source, target) pair, so one
/// implementation type on two pairs is ordinary, not a clash.</para>
/// </summary>
public sealed class Utf8TextConverter : IFormatConverter
{
    public Utf8TextConverter(string sourceType) => SourceType = sourceType;

    public string SourceType { get; }

    public string TargetType => ContentTypes.Text;

    public string Summary =>
        "bytes to UTF-8 text — decoded as-is; refuses when the bytes are not valid UTF-8 rather than " +
        "handing back replacement characters";

    public Task<ResourceContent> ConvertAsync(
        ResourceContent source,
        IReadOnlyDictionary<string, object?>? options,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var bytes = source.Bytes ?? Array.Empty<byte>();

        // Never lossy-decode. Encoding.UTF8.GetString would happily turn a PNG into a page of U+FFFD
        // and report success, which is how binary reaches a model dressed as text.
        if (!ContentTypes.LooksLikeUtf8Text(bytes))
            throw new InvalidOperationException(
                $"Cannot read these {bytes.Length} bytes as UTF-8 text: they are binary (declared " +
                $"'{source.ContentType}'). Decoding anyway would produce replacement characters, not content — " +
                "keep the bytes as a handle, or convert them with a converter that understands the format.");

        // A BOM is a marker, not content: it survives the round-trip check either way, and carrying it
        // into the text would put an invisible U+FEFF at the front of every decoded file.
        var body = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF
            ? bytes[3..]
            : bytes;

        return Task.FromResult(new ResourceContent(body, ContentTypes.Text));
    }
}
