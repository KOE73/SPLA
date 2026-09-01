using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SplaAtlas.Model.Json;

/// <summary>
/// Reads a JSON file into a mutable tree and writes that tree back in the shape it arrived in.
///
/// The registries outlive any one run of this utility, so the codec is built to be lossless rather
/// than tidy: a document nobody touched must come back byte for byte. That rules out mapping the
/// files onto plain records — a record drops every field it was not told about, silently, and the
/// fields it would drop here are exactly the ones that matter (<c>firstSeen</c> on entities,
/// <c>evidence</c> and the leftover <c>relation</c>/<c>points</c> keys on relations, and every
/// provenance stamp in a text catalog). The typed model in this assembly is therefore a view over
/// the parsed tree, not a copy of it.
/// </summary>
public static class JsonFile
{
    private static readonly JsonNodeOptions NodeOptions = new() { PropertyNameCaseInsensitive = false };

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false,
    };

    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Indented = true,
        // The live catalogs hold Russian prose — and one emoji — as raw UTF-8, and escape nothing
        // but the quote. No stock encoder reproduces that; see MinimalJsonEncoder.
        Encoder = MinimalJsonEncoder.Shared,
    };

    /// <summary>A parsed document together with the formatting it was written in.</summary>
    public sealed record Parsed(JsonObject Root, JsonFormat Format);

    /// <summary>Parses raw file bytes, remembering their formatting.</summary>
    /// <exception cref="JsonModelException">The bytes are not a JSON object.</exception>
    public static Parsed Parse(ReadOnlySpan<byte> bytes, string origin)
    {
        var format = JsonFormat.Detect(bytes);
        var body = format.ByteOrderMark ? bytes[3..] : bytes;

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(body, NodeOptions, DocumentOptions);
        }
        catch (JsonException ex)
        {
            throw new JsonModelException($"{origin}: not valid JSON — {ex.Message}", ex);
        }

        if (node is not JsonObject root)
        {
            var actual = node is null ? "null" : node.GetType().Name;
            throw new JsonModelException($"{origin}: expected a JSON object at the root, found {actual}.");
        }

        return new Parsed(root, format);
    }

    /// <summary>Reads and parses a file from disk.</summary>
    public static Parsed Read(string path)
    {
        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new JsonModelException($"{path}: cannot be read — {ex.Message}", ex);
        }

        return Parse(bytes, path);
    }

    /// <summary>Serialises a tree back to bytes in the given formatting.</summary>
    public static byte[] Serialize(JsonNode root, JsonFormat format)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer, WriterOptions with
               {
                   IndentCharacter = format.IndentCharacter,
                   IndentSize = format.IndentSize,
               }))
        {
            root.WriteTo(writer);
        }

        var text = Encoding.UTF8.GetString(buffer.ToArray());

        // Utf8JsonWriter picks its own newline, and which one it picks has moved between runtimes.
        // Rather than depend on that, normalise and then apply the newline the file arrived with.
        // This is safe without parsing: JSON forbids raw control characters inside string literals,
        // so every bare LF in the output is layout and never content.
        text = text.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (format.NewLine != "\n")
        {
            text = text.Replace("\n", format.NewLine, StringComparison.Ordinal);
        }

        if (format.TrailingNewLine)
        {
            text += format.NewLine;
        }

        var payload = Encoding.UTF8.GetBytes(text);
        if (!format.ByteOrderMark)
        {
            return payload;
        }

        var withBom = new byte[payload.Length + 3];
        withBom[0] = 0xEF;
        withBom[1] = 0xBB;
        withBom[2] = 0xBF;
        payload.CopyTo(withBom, 3);
        return withBom;
    }

    /// <summary>Writes a tree to disk in the given formatting.</summary>
    public static void Write(string path, JsonNode root, JsonFormat format)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(path, Serialize(root, format));
    }
}
