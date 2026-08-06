using System;
using System.Collections.Generic;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Works out what a blob actually holds. A blob handle is an address, not a type — the type belongs
/// on the payload (<see cref="SPLA.Domain.Agent.BlobPayload.ContentType"/>), and this is the single
/// place that fills it in when a producing tool did not.
/// <para>
/// The order matters: a producer that knows the type wins, then the bytes themselves (a signature
/// cannot lie about what was actually downloaded), then the file name. Guessing at read time rather
/// than baking a guess into the handle means a wrong guess stays correctable.
/// </para>
/// </summary>
public static class BlobContentType
{
    public const string Unknown = "application/octet-stream";

    private static readonly Dictionary<string, string> ByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".bmp"] = "image/bmp",
        [".tif"] = "image/tiff",
        [".tiff"] = "image/tiff",
        [".ico"] = "image/vnd.microsoft.icon",
        [".svg"] = "image/svg+xml",
        [".pdf"] = "application/pdf",
        [".zip"] = "application/zip",
        [".gz"] = "application/gzip",
        [".tar"] = "application/x-tar",
        [".json"] = "application/json",
        [".xml"] = "application/xml",
        [".yaml"] = "application/yaml",
        [".yml"] = "application/yaml",
        [".csv"] = "text/csv",
        [".html"] = "text/html",
        [".htm"] = "text/html",
        [".md"] = "text/markdown",
        [".txt"] = "text/plain",
        [".log"] = "text/plain",
        [".sql"] = "text/plain",
        [".conf"] = "text/plain",
        [".ini"] = "text/plain",
        [".sh"] = "text/plain",
    };

    /// <summary>Content type from a file (or archive entry) name, or null when the extension says nothing.</summary>
    public static string? FromFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        var dot = fileName.LastIndexOf('.');
        if (dot < 0 || dot == fileName.Length - 1) return null;
        return ByExtension.TryGetValue(fileName[dot..], out var type) ? type : null;
    }

    /// <summary>Content type read off the payload's leading bytes, or null when no signature matches.</summary>
    public static string? Sniff(byte[]? bytes)
    {
        if (bytes is null || bytes.Length < 4) return null;

        if (Starts(bytes, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)) return "image/png";
        if (Starts(bytes, 0xFF, 0xD8, 0xFF)) return "image/jpeg";
        if (Starts(bytes, 0x47, 0x49, 0x46, 0x38)) return "image/gif";                 // GIF8
        if (Starts(bytes, 0x42, 0x4D)) return "image/bmp";                             // BM
        if (Starts(bytes, 0x49, 0x49, 0x2A, 0x00) || Starts(bytes, 0x4D, 0x4D, 0x00, 0x2A)) return "image/tiff";
        if (Starts(bytes, 0x00, 0x00, 0x01, 0x00)) return "image/vnd.microsoft.icon";
        if (bytes.Length >= 12 && Starts(bytes, 0x52, 0x49, 0x46, 0x46) &&
            bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50) return "image/webp";
        if (Starts(bytes, 0x25, 0x50, 0x44, 0x46)) return "application/pdf";           // %PDF
        if (Starts(bytes, 0x1F, 0x8B)) return "application/gzip";
        if (Starts(bytes, 0x50, 0x4B, 0x03, 0x04)) return "application/zip";           // PK..

        return null;
    }

    /// <summary>
    /// The type to record for a payload: <paramref name="declared"/> if the producer knew it, else the
    /// byte signature, else the file name, else <see cref="Unknown"/> for bytes / <c>text/plain</c> for text.
    /// </summary>
    public static string Resolve(string? declared, byte[]? bytes, string? fileName, bool isText)
    {
        if (!string.IsNullOrWhiteSpace(declared)) return declared!;
        return Sniff(bytes) ?? FromFileName(fileName) ?? (isText ? "text/plain" : Unknown);
    }

    /// <summary>True for a content type a vision model can actually be shown (SVG excluded — not raster).</summary>
    public static bool IsViewableImage(string? contentType)
        => contentType is not null
           && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
           && !contentType.StartsWith("image/svg", StringComparison.OrdinalIgnoreCase);

    private static bool Starts(byte[] bytes, params byte[] signature)
    {
        if (bytes.Length < signature.Length) return false;
        for (var i = 0; i < signature.Length; i++)
            if (bytes[i] != signature[i]) return false;
        return true;
    }
}
