using System.Text.RegularExpressions;

namespace SPLA.Service;

/// <summary>
/// Sidecar storage for chat image attachments. Images are written as files under
/// <c>&lt;workspace&gt;/.spla/chat-images/&lt;chatId&gt;/</c>; the chat YAML keeps only filenames, so the
/// (potentially large) binary never bloats the chat file or its frequent re-saves. Both the writer
/// (<see cref="ChatRuntime"/>) and the reader (the host's <c>/chat-image</c> endpoint) resolve paths
/// through here so the convention lives in one place.
/// </summary>
public static class ChatImages
{
    // Only simple names — guards the serving endpoint against path traversal.
    private static readonly Regex SafeName = new("^[A-Za-z0-9._-]+$", RegexOptions.Compiled);

    public static string Dir(string workspace, string chatId)
        => Path.Combine(workspace, ".spla", "chat-images", chatId);

    /// <summary>The public URL the client uses to fetch a stored image.</summary>
    public static string Url(string chatId, string fileName) => $"/chat-image/{chatId}/{fileName}";

    /// <summary>Writes a data URL (data:image/png;base64,…) to a fresh file and returns its filename.
    /// Returns null if the input is not a recognisable image data URL.</summary>
    public static string? WriteDataUrl(string workspace, string chatId, string dataUrl)
    {
        var m = Regex.Match(dataUrl, @"^data:image/(?<ext>[a-zA-Z0-9.+-]+);base64,(?<data>.+)$", RegexOptions.Singleline);
        if (!m.Success) return null;

        var ext = m.Groups["ext"].Value.ToLowerInvariant() switch
        {
            "jpeg" => "jpg",
            "svg+xml" => "svg",
            var e => Regex.Replace(e, "[^a-z0-9]", "")
        };
        byte[] bytes;
        try { bytes = Convert.FromBase64String(m.Groups["data"].Value); }
        catch { return null; }

        var dir = Dir(workspace, chatId);
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid():N}.{(string.IsNullOrEmpty(ext) ? "bin" : ext)}";
        File.WriteAllBytes(Path.Combine(dir, fileName), bytes);
        return fileName;
    }

    /// <summary>Resolves a stored image to its full path, or null if the name is unsafe / missing.</summary>
    public static string? Resolve(string workspace, string chatId, string fileName)
    {
        if (!SafeName.IsMatch(chatId) || !SafeName.IsMatch(fileName)) return null;
        var full = Path.Combine(Dir(workspace, chatId), fileName);
        return File.Exists(full) ? full : null;
    }

    public static string ContentType(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".svg" => "image/svg+xml",
        ".bmp" => "image/bmp",
        _ => "application/octet-stream"
    };
}
