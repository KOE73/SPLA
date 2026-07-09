using System.Reflection;

namespace SPLA.Service;

/// <summary>
/// Serves the browser client's static files — the built output of <c>web/</c> (Vue 3 + TS + Vite).
/// <para>
/// In development the files are read straight from <c>web/dist/</c> (found by walking up from the
/// binary directory), so a rebuild (<c>npm run build</c> in <c>web/</c>, or the .csproj's pre-build
/// target) is visible on the next browser reload without rebuilding the .NET host. In distribution
/// (no source directory present) every file falls back to the embedded resource baked into the
/// assembly at .NET build time.
/// </para>
/// </summary>
public static class WebAssets
{
    private const string Prefix = "SPLA.Service.WebClient.";
    private static readonly Assembly Asm = typeof(WebAssets).Assembly;

    // Resolved once at startup. Null in production (no source tree found).
    private static readonly string? DevDir = FindDevWebClientDir();

    /// <summary>Loads an asset by request path (e.g. "/app.js", "/lib/marked.min.js"); "/" → index.html.</summary>
    public static (byte[] Bytes, string ContentType)? Get(string path)
    {
        if (string.IsNullOrEmpty(path) || path == "/") path = "/index.html";
        var rel = path.TrimStart('/');

        // Dev mode: read from source directory so changes are visible on reload without rebuild.
        if (DevDir != null)
        {
            var filePath = Path.Combine(DevDir, rel.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(filePath))
                return (File.ReadAllBytes(filePath), MimeType(path));
        }

        // Fallback: embedded resource (production / no source tree).
        // WebClient/app.js → SPLA.Service.WebClient.app.js ; lib/x.js → ...WebClient.lib.x.js
        var resource = Prefix + rel.Replace('/', '.');
        using var stream = Asm.GetManifestResourceStream(resource);
        if (stream == null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return (ms.ToArray(), MimeType(path));
    }

    /// <summary>Loads a file straight from a plugin's own directory on disk (its prebuilt web settings
    /// module — see <c>web_settings_entry</c> in meta.yaml). Plugins live outside the assembly, so
    /// there is no embedded-resource fallback here, only a path-traversal guard.</summary>
    public static (byte[] Bytes, string ContentType)? GetFromDirectory(string baseDir, string relPath)
    {
        var baseFull = Path.GetFullPath(baseDir);
        var filePath = Path.GetFullPath(Path.Combine(baseFull, relPath.Replace('/', Path.DirectorySeparatorChar)));
        if (!filePath.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase) || !File.Exists(filePath))
            return null;
        return (File.ReadAllBytes(filePath), MimeType(filePath));
    }

    private static string MimeType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".html" => "text/html; charset=utf-8",
        ".css"  => "text/css; charset=utf-8",
        ".js"   => "text/javascript; charset=utf-8",
        ".json" => "application/json; charset=utf-8",
        ".svg"  => "image/svg+xml",
        ".png"  => "image/png",
        _       => "application/octet-stream"
    };

    // Walk up from the binary directory to find the repo's web/dist/ folder.
    // Works both when the CLI runs from SPLA.CLI/bin/…/ and when SPLA.Service itself is the host.
    private static string? FindDevWebClientDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            var dist = Path.Combine(dir.FullName, "web", "dist");
            if (Directory.Exists(dist)) return dist;
        }
        return null;
    }
}
