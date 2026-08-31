using System.Text;
using SplaAtlas.Model;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Tests;

/// <summary>
/// Formatting fidelity: the shape a file arrived in is the shape it goes back out in.
/// </summary>
/// <remarks>
/// This matters more here than in most codecs. The registries are hand-edited files under review,
/// so a writer with its own formatting opinion turns every run into a whole-file diff and buries the
/// one line the run was about. And the shape is genuinely not uniform: the live projects mix LF and
/// CRLF, and mix files that end with a newline with files that do not — in a repository where
/// <c>core.autocrlf</c> is on and no <c>.gitattributes</c> pins anything, so the same file can be LF
/// today and CRLF after a checkout. Detection off the input bytes is the only stable answer.
/// </remarks>
public sealed class JsonFormatTests
{
    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);

    [Theory]
    [InlineData("{\n  \"a\": 1\n}", "\n", 2, false)]
    [InlineData("{\r\n  \"a\": 1\r\n}\r\n", "\r\n", 2, true)]
    [InlineData("{\n    \"a\": 1\n}\n", "\n", 4, true)]
    public void FormatIsReadOffTheBytes(string source, string newLine, int indent, bool trailing)
    {
        var format = JsonFormat.Detect(Bytes(source));

        Assert.Equal(newLine, format.NewLine);
        Assert.Equal(indent, format.IndentSize);
        Assert.Equal(' ', format.IndentCharacter);
        Assert.Equal(trailing, format.TrailingNewLine);
        Assert.False(format.ByteOrderMark);
    }

    [Fact]
    public void TabIndentIsRecognised()
    {
        var format = JsonFormat.Detect(Bytes("{\n\t\"a\": 1\n}"));

        Assert.Equal('\t', format.IndentCharacter);
        Assert.Equal(1, format.IndentSize);
    }

    /// <summary>
    /// The indent unit is the smallest run in the file, not the first one seen.
    /// </summary>
    /// <remarks>
    /// A document whose first indented line already sits two levels deep would otherwise report
    /// double the true unit and re-indent the whole file on the next write.
    /// </remarks>
    [Fact]
    public void IndentIsTheSmallestRunNotTheFirst()
    {
        var format = JsonFormat.Detect(Bytes("{\n  \"a\": {\n    \"b\": 1\n  }\n}"));

        Assert.Equal(2, format.IndentSize);
    }

    [Theory]
    [InlineData("{\n  \"a\": 1\n}")]
    [InlineData("{\r\n  \"a\": 1\r\n}")]
    [InlineData("{\r\n  \"a\": 1\r\n}\r\n")]
    [InlineData("{\n    \"a\": [\n        1,\n        2\n    ]\n}\n")]
    [InlineData("{\n\t\"a\": 1\n}\n")]
    public void ADocumentComesBackExactlyAsItWent(string source)
    {
        var bytes = Bytes(source);
        var parsed = JsonFile.Parse(bytes, "test");

        Assert.Equal(bytes, JsonFile.Serialize(parsed.Root, parsed.Format));
    }

    [Fact]
    public void AByteOrderMarkIsPreserved()
    {
        var bytes = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Bytes("{\n  \"a\": 1\n}\n")).ToArray();
        var parsed = JsonFile.Parse(bytes, "test");

        Assert.True(parsed.Format.ByteOrderMark);
        Assert.Equal(bytes, JsonFile.Serialize(parsed.Root, parsed.Format));
    }

    /// <summary>
    /// Cyrillic stays raw UTF-8, and only the characters JSON requires escaping get escaped.
    /// </summary>
    /// <remarks>
    /// The default encoder escapes non-ASCII plus a handful of ASCII it considers unsafe in HTML.
    /// With it, the first save would rewrite every Russian description in the tree into
    /// <c>\uXXXX</c> — a diff of thousands of lines saying nothing.
    /// </remarks>
    [Fact]
    public void ProseIsNotEscaped()
    {
        var source = Bytes("{\n  \"v\": \"Стадия Output: <a> & \\\"b\\\" + 'c'\"\n}\n");
        var parsed = JsonFile.Parse(source, "test");
        var written = Encoding.UTF8.GetString(JsonFile.Serialize(parsed.Root, parsed.Format));

        Assert.Contains("Стадия Output", written, StringComparison.Ordinal);
        Assert.Contains("<a> &", written, StringComparison.Ordinal);
        Assert.Contains("'c'", written, StringComparison.Ordinal);
        Assert.DoesNotContain("\\u", written, StringComparison.Ordinal);
        Assert.Equal(source, JsonFile.Serialize(parsed.Root, parsed.Format));
    }

    /// <summary>
    /// Characters outside the basic multilingual plane stay raw.
    /// </summary>
    /// <remarks>
    /// One emoji in <c>features/text.ru.json</c> is what caught this. Every stock encoder escapes a
    /// surrogate pair — <c>UnsafeRelaxedJsonEscaping</c> included, despite leaving Cyrillic alone —
    /// so the codec carries its own. Pinned here rather than left to the live file, because the day
    /// somebody removes that emoji the rule would silently stop being tested.
    /// </remarks>
    [Fact]
    public void AnEmojiIsNotTurnedIntoSurrogateEscapes()
    {
        var source = Bytes("{\n  \"v\": \"иконку \U0001F4C4 и стрелку ↓\"\n}\n");
        var parsed = JsonFile.Parse(source, "test");
        var written = JsonFile.Serialize(parsed.Root, parsed.Format);

        Assert.Equal(source, written);
        Assert.DoesNotContain("\\uD83D", Encoding.UTF8.GetString(written), StringComparison.Ordinal);
    }

    /// <summary>
    /// Escaping matches what the other writer of these files does.
    /// </summary>
    /// <remarks>
    /// The editor in <c>tools/spla-diagram</c> saves the same registries through
    /// <c>JSON.stringify</c>. Two writers with different escaping policies would produce a
    /// whole-file diff every time authorship of a file changed hands, so this pins the policy to
    /// that one: the quote, the backslash and the C0 controls, and nothing else.
    /// </remarks>
    [Fact]
    public void OnlyWhatJsonRequiresIsEscaped()
    {
        var parsed = JsonFile.Parse(Bytes("{\"a\":\"q\\\"q\",\"b\":\"s\\\\s\",\"c\":\"t\\tt\",\"d\":\"z\\u0001z\"}"), "test");
        var written = Encoding.UTF8.GetString(JsonFile.Serialize(parsed.Root, parsed.Format));

        Assert.Contains("q\\\"q", written, StringComparison.Ordinal);
        Assert.Contains("s\\\\s", written, StringComparison.Ordinal);
        Assert.Contains("t\\tt", written, StringComparison.Ordinal);
        Assert.Contains("z\\u0001z", written, StringComparison.Ordinal);
    }

    /// <summary>
    /// A newline inside a string is content and must not be touched by the newline pass.
    /// </summary>
    /// <remarks>
    /// The writer normalises line endings over the whole serialised text, which is safe only because
    /// JSON forbids raw control characters inside string literals — an embedded newline is always
    /// written as an escape, never as a byte. This pins that assumption.
    /// </remarks>
    [Fact]
    public void AnEscapedNewlineInsideAStringIsNotConvertedToCrlf()
    {
        var source = Bytes("{\r\n  \"doc\": \"first\\nsecond\"\r\n}\r\n");
        var parsed = JsonFile.Parse(source, "test");
        var written = JsonFile.Serialize(parsed.Root, parsed.Format);

        Assert.Equal(source, written);
        Assert.DoesNotContain("first\\r\\nsecond", Encoding.UTF8.GetString(written), StringComparison.Ordinal);
    }

    [Fact]
    public void NotJsonIsRejectedWithTheFileNameInTheMessage()
    {
        var error = Assert.Throws<JsonModelException>(
            () => EntityCatalog.Parse(Bytes("{ oops"), "core/entities.json"));

        Assert.Contains("core/entities.json", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARootThatIsNotAnObjectIsRejected()
    {
        var error = Assert.Throws<JsonModelException>(() => EntityCatalog.Parse(Bytes("[]"), "test"));

        Assert.Contains("expected a JSON object", error.Message, StringComparison.Ordinal);
    }
}
