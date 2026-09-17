using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPLA.Plugins.Android.Runtime;

/// <summary>
/// One pinned runtime component from <c>runtime-manifest.json</c>: the version it ships, the full
/// download URL (never computed from the version — ADR §3.5, the platform-tools archive name
/// format changed between releases), and the SHA-256 of the archive.
/// </summary>
public sealed record RuntimeComponent(string Version, string Url, string Sha256);

/// <summary>
/// POCO model of <c>runtime-manifest.json</c> — the pinned adb/scrcpy pair (version + URL +
/// SHA-256) that sits next to the plugin dll. It is the source of truth for the "pinned" install
/// channel and for the pinned versions echoed by runtimeStatus (never hardcoded).
///
/// Failure mechanism: <see cref="Load"/> throws <see cref="IOException"/> with a message that
/// names the file and what is wrong with it (missing, unreadable, not valid JSON, or missing the
/// adb/scrcpy entries). A broken manifest is a misconfiguration a human should see, not a value
/// to silently substitute.
/// </summary>
public sealed class RuntimeManifest
{
    /// <summary>Platform the pinned URLs are built for, e.g. "win-x64".</summary>
    public string Platform { get; init; } = "";

    /// <summary>The pinned adb (platform-tools) component. Present after <see cref="Load"/>.</summary>
    public RuntimeComponent? Adb { get; init; }

    /// <summary>The pinned scrcpy component. Present after <see cref="Load"/>.</summary>
    public RuntimeComponent? Scrcpy { get; init; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    /// <summary>
    /// Loads <c>runtime-manifest.json</c> from <paramref name="pluginDir"/>; pass null for the
    /// plugin's own folder (<c>Path.GetDirectoryName(typeof(AndroidPlugin).Assembly.Location)</c>).
    /// </summary>
    public static RuntimeManifest Load(string? pluginDir = null)
    {
        var dir = pluginDir ?? Path.GetDirectoryName(typeof(AndroidPlugin).Assembly.Location)!;
        var manifestPath = Path.Combine(dir, "runtime-manifest.json");

        string json;
        try
        {
            json = File.ReadAllText(manifestPath);
        }
        catch (IOException ex)
        {
            throw new IOException(
                $"Cannot read the Android runtime manifest at '{manifestPath}': {ex.Message}. " +
                "The file is shipped next to the plugin dll — repair or reinstall the plugin.", ex);
        }

        RuntimeManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<RuntimeManifest>(json, JsonOptions)
                ?? throw new IOException($"The Android runtime manifest at '{manifestPath}' is empty.");
        }
        catch (JsonException ex)
        {
            throw new IOException(
                $"The Android runtime manifest at '{manifestPath}' is not valid JSON: {ex.Message}", ex);
        }

        var missing = new List<string>();
        if (manifest.Adb is null) missing.Add("adb");
        if (manifest.Scrcpy is null) missing.Add("scrcpy");
        if (missing.Count > 0)
            throw new IOException(
                $"The Android runtime manifest at '{manifestPath}' is missing the component entries: " +
                $"{string.Join(", ", missing)}.");

        return manifest;
    }

    /// <summary>The pinned entry for "adb" or "scrcpy"; both are present after <see cref="Load"/>.</summary>
    public RuntimeComponent Component(string component) => component switch
    {
        "adb" => Adb!,
        "scrcpy" => Scrcpy!,
        _ => throw new ArgumentException(
            $"Unknown component '{component}' — expected 'adb' or 'scrcpy'.", nameof(component)),
    };
}
