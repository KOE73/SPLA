namespace SplaAtlas.Model.Json;

/// <summary>
/// The whitespace shape of a JSON file on disk: what its indent looks like, which newline it uses,
/// whether it ends with one, and whether it carries a byte order mark.
///
/// This exists because the registries are hand-edited files under version control, not a machine
/// format. A writer that reformats them turns every <c>sync</c> into a whole-file diff, which buries
/// the finding the run was about and makes the owner's review worthless. Worse, in this repository
/// the line ending is not even stable per file — <c>core.autocrlf=true</c> with no <c>.gitattributes</c>
/// means the same file is LF today and CRLF after the next checkout. So the format is read off the
/// bytes we were handed and reproduced, rather than assumed or configured.
/// </summary>
public sealed record JsonFormat
{
    /// <summary>What the file used to separate lines: "\n" or "\r\n".</summary>
    public required string NewLine { get; init; }

    /// <summary>Indent unit, in <see cref="IndentCharacter"/>s, per nesting level.</summary>
    public required int IndentSize { get; init; }

    /// <summary>Space or tab.</summary>
    public required char IndentCharacter { get; init; }

    /// <summary>Whether the last byte of the file was a newline.</summary>
    public required bool TrailingNewLine { get; init; }

    /// <summary>Whether the file began with a UTF-8 byte order mark.</summary>
    public required bool ByteOrderMark { get; init; }

    /// <summary>
    /// What we emit for a file that did not exist before. Matches the prevailing shape of the live
    /// registries: two spaces, LF, final newline, no BOM.
    /// </summary>
    public static JsonFormat Default { get; } = new()
    {
        NewLine = "\n",
        IndentSize = 2,
        IndentCharacter = ' ',
        TrailingNewLine = true,
        ByteOrderMark = false,
    };

    /// <summary>
    /// Reads the shape off the raw bytes of a file.
    /// </summary>
    /// <remarks>
    /// Everything here is decided from formatting whitespace only, which in JSON can never occur
    /// inside a string literal: the grammar forbids raw control characters there, so a newline or an
    /// indent run outside a string is unambiguously layout. That is what makes detection safe, and
    /// it is the same property the writer leans on.
    /// </remarks>
    public static JsonFormat Detect(ReadOnlySpan<byte> bytes)
    {
        var bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        var body = bom ? bytes[3..] : bytes;

        if (body.IsEmpty)
        {
            return Default with { ByteOrderMark = bom };
        }

        var crlf = 0;
        var lf = 0;
        for (var i = 0; i < body.Length; i++)
        {
            if (body[i] != (byte)'\n')
            {
                continue;
            }

            if (i > 0 && body[i - 1] == (byte)'\r')
            {
                crlf++;
            }
            else
            {
                lf++;
            }
        }

        // A mixed file has to be normalised to something; the majority wins, and ties go to LF
        // because that is what every generated file in the tree uses.
        var newLine = crlf > lf ? "\r\n" : "\n";

        var (indentCharacter, indentSize) = DetectIndent(body);

        return new JsonFormat
        {
            NewLine = newLine,
            IndentSize = indentSize,
            IndentCharacter = indentCharacter,
            TrailingNewLine = body[^1] == (byte)'\n',
            ByteOrderMark = bom,
        };
    }

    /// <summary>
    /// Finds the indent unit as the smallest non-zero indent run in the file.
    /// </summary>
    /// <remarks>
    /// The smallest run rather than the first: a file whose first indented line already sits two
    /// levels deep would otherwise report double the true unit. The smallest non-zero run is the
    /// unit by construction, since every deeper level is a multiple of it.
    /// </remarks>
    private static (char Character, int Size) DetectIndent(ReadOnlySpan<byte> body)
    {
        var character = ' ';
        var smallest = 0;

        for (var i = 0; i < body.Length; i++)
        {
            if (body[i] != (byte)'\n')
            {
                continue;
            }

            var j = i + 1;
            if (j >= body.Length || (body[j] != (byte)' ' && body[j] != (byte)'\t'))
            {
                continue;
            }

            var lead = body[j];
            var run = 0;
            while (j < body.Length && body[j] == lead)
            {
                run++;
                j++;
            }

            if (smallest == 0 || run < smallest)
            {
                smallest = run;
                character = (char)lead;
            }
        }

        return smallest == 0 ? (' ', Default.IndentSize) : (character, smallest);
    }
}
