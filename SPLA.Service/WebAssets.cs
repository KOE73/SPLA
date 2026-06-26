using System.Reflection;

namespace SPLA.Service;

/// <summary>
/// Serves the browser client's static files (html/css/js + marked/mermaid) straight from embedded
/// resources. The files live under <c>WebClient/</c> and are split by concern (index, styles,
/// themes, renderers, app) so the client is structured and maintainable rather than one blob — and
/// so the agent can edit one element's renderer in isolation.
/// </summary>
public static class WebAssets
{
    private const string Prefix = "SPLA.Service.WebClient.";
    private static readonly Assembly Asm = typeof(WebAssets).Assembly;

    /// <summary>Loads an asset by request path (e.g. "/app.js", "/lib/marked.min.js"); "/" → index.html.</summary>
    public static (byte[] Bytes, string ContentType)? Get(string path)
    {
        if (string.IsNullOrEmpty(path) || path == "/") path = "/index.html";

        // WebClient/app.js → SPLA.Service.WebClient.app.js ; WebClient/lib/x.js → ...WebClient.lib.x.js
        var resource = Prefix + path.TrimStart('/').Replace('/', '.');
        using var stream = Asm.GetManifestResourceStream(resource);
        if (stream == null) return null;

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return (ms.ToArray(), ContentType(path));
    }

    private static string ContentType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".html" => "text/html; charset=utf-8",
        ".css" => "text/css; charset=utf-8",
        ".js" => "text/javascript; charset=utf-8",
        ".json" => "application/json; charset=utf-8",
        ".svg" => "image/svg+xml",
        ".png" => "image/png",
        _ => "application/octet-stream"
    };
}
