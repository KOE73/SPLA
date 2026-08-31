using System.Text.Encodings.Web;

namespace SplaAtlas.Model.Json;

/// <summary>
/// Escapes exactly what JSON requires — the quote, the backslash, and the C0 controls — and nothing
/// else.
/// </summary>
/// <remarks>
/// <para>
/// None of the stock encoders will do. <see cref="JavaScriptEncoder.Default"/> escapes all non-ASCII,
/// which would rewrite every Russian description in the tree as <c>\uXXXX</c> on the first save.
/// <c>UnsafeRelaxedJsonEscaping</c> keeps Cyrillic raw but still escapes anything outside the basic
/// multilingual plane — one emoji in <c>features/text.ru.json</c> is enough to make the round-trip
/// lose. <see cref="JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRange[])"/> escapes the
/// HTML-sensitive set unconditionally, whatever ranges it is handed.
/// </para>
/// <para>
/// The target is not a matter of taste. These files have a second writer — the TypeScript editor in
/// <c>tools/spla-diagram</c>, which saves them through <c>JSON.stringify</c> — and two writers that
/// disagree about escaping would produce a whole-file diff every time authorship changed hands. This
/// encoder is what <c>JSON.stringify</c> does, so both writers agree.
/// </para>
/// </remarks>
internal sealed class MinimalJsonEncoder : JavaScriptEncoder
{
    public static readonly MinimalJsonEncoder Shared = new();

    private MinimalJsonEncoder()
    {
    }

    /// <summary>A surrogate pair as two <c>\uXXXX</c> escapes, which is the widest thing we ever emit.</summary>
    public override int MaxOutputCharactersPerInputCharacter => 12;

    public override bool WillEncode(int unicodeScalar) =>
        unicodeScalar is '"' or '\\' || unicodeScalar < 0x20;

    public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
    {
        for (var i = 0; i < textLength; i++)
        {
            if (WillEncode(text[i]))
            {
                return i;
            }
        }

        return -1;
    }

    public override unsafe bool TryEncodeUnicodeScalar(
        int unicodeScalar,
        char* buffer,
        int bufferLength,
        out int numberOfCharactersWritten)
    {
        // The short forms JSON.stringify uses, so a control character in a doc string comes back as
        // a two-character escape rather than as a six-digit one.
        var shortForm = unicodeScalar switch
        {
            '"' => '"',
            '\\' => '\\',
            '\b' => 'b',
            '\f' => 'f',
            '\n' => 'n',
            '\r' => 'r',
            '\t' => 't',
            _ => '\0',
        };

        if (shortForm != '\0')
        {
            if (bufferLength < 2)
            {
                numberOfCharactersWritten = 0;
                return false;
            }

            buffer[0] = '\\';
            buffer[1] = shortForm;
            numberOfCharactersWritten = 2;
            return true;
        }

        if (unicodeScalar > 0xFFFF)
        {
            // Never reached while WillEncode says no to these; written out so the contract of the
            // method holds if the runtime ever asks.
            var value = unicodeScalar - 0x10000;
            return TryWriteHexEscapes(
                [(char)(0xD800 + (value >> 10)), (char)(0xDC00 + (value & 0x3FF))],
                buffer,
                bufferLength,
                out numberOfCharactersWritten);
        }

        return TryWriteHexEscapes([(char)unicodeScalar], buffer, bufferLength, out numberOfCharactersWritten);
    }

    private static unsafe bool TryWriteHexEscapes(
        ReadOnlySpan<char> units,
        char* buffer,
        int bufferLength,
        out int numberOfCharactersWritten)
    {
        const string Hex = "0123456789ABCDEF";
        var needed = units.Length * 6;
        if (bufferLength < needed)
        {
            numberOfCharactersWritten = 0;
            return false;
        }

        var at = 0;
        foreach (var unit in units)
        {
            buffer[at++] = '\\';
            buffer[at++] = 'u';
            buffer[at++] = Hex[(unit >> 12) & 0xF];
            buffer[at++] = Hex[(unit >> 8) & 0xF];
            buffer[at++] = Hex[(unit >> 4) & 0xF];
            buffer[at++] = Hex[unit & 0xF];
        }

        numberOfCharactersWritten = needed;
        return true;
    }
}
