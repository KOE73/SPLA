using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPLA.Plugins.Android.Runtime;

/// <summary>
/// The <c>spla-runtime.json</c> install receipt stored inside each component folder (ADR §3.6):
/// which component, which version, which install channel ('pinned' | 'latest' | 'manual'), the
/// source URL, the archive SHA-256, whether the hash was verified, and the install time. It is
/// the source of truth for the version string the scrcpy client sends to the server (ADR §3.5 —
/// never a constant).
///
/// <see cref="TryRead"/> returns <c>null</c> when the receipt is absent, unreadable, or not valid
/// JSON: a corrupt receipt is reported as "not installed" rather than throwing, so the
/// manual-install fallback (<see cref="TryManualInstall"/>) still applies and the status page
/// cannot crash on a half-written file. <see cref="Write"/> overwrites an existing receipt and
/// creates the folder when it does not exist yet.
///
/// <see cref="InstalledAt"/> is an ISO 8601 string with a UTC offset (e.g.
/// <c>2026-09-10T10:00:00+03:00</c>). It is stored as the string itself, so a Write → TryRead
/// round-trip preserves it verbatim.
/// </summary>
public sealed record RuntimeReceipt(
    string Component,
    string Version,
    string Channel,
    string Url,
    string Sha256,
    bool Verified,
    string InstalledAt)
{
    /// <summary>Name of the receipt file inside a component folder.</summary>
    public const string FileName = "spla-runtime.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
    };

    /// <summary>
    /// Reads <c>&lt;folder&gt;\spla-runtime.json</c>; <c>null</c> when the file is absent, unreadable,
    /// or invalid (see the type documentation for why invalid is reported as absent).
    /// </summary>
    public static RuntimeReceipt? TryRead(string folder)
    {
        try
        {
            return JsonSerializer.Deserialize<RuntimeReceipt>(
                File.ReadAllText(Path.Combine(folder, FileName)), JsonOptions);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Serializes <paramref name="receipt"/> to <c>&lt;folder&gt;\spla-runtime.json</c>, overwriting
    /// an existing receipt; creates the folder when it does not exist yet.
    /// </summary>
    public static void Write(string folder, RuntimeReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        Directory.CreateDirectory(folder);
        File.WriteAllText(
            Path.Combine(folder, FileName),
            JsonSerializer.Serialize(receipt, JsonOptions));
    }

    /// <summary>
    /// The manual-install fallback (ADR §3.6): a component folder WITHOUT a receipt but WITH its
    /// marker file present — <c>adb.exe</c> for adb, <c>scrcpy-server</c> for scrcpy — is "installed
    /// manually" (e.g. the user pointed the setting at an existing platform-tools folder). Nothing
    /// was downloaded and nothing was verified, so the receipt carries version <c>unknown</c>,
    /// channel <c>manual</c>, and empty source fields. <c>null</c> when the marker file is absent
    /// or the component has no known marker. Reused by the runtimeStatus action (plan step 1.5).
    /// </summary>
    internal static RuntimeReceipt? TryManualInstall(string component, string folder)
    {
        var markerPath = MarkerPath(component, folder);
        return markerPath is not null && File.Exists(markerPath)
            ? new RuntimeReceipt(component, "unknown", "manual", "", "", Verified: false, "")
            : null;
    }

    /// <summary>
    /// The marker file whose presence proves a manual install of <paramref name="component"/>;
    /// <c>null</c> for a component without a known marker.
    /// </summary>
    internal static string? MarkerPath(string component, string folder) => component switch
    {
        "adb" => Path.Combine(folder, "adb.exe"),
        "scrcpy" => Path.Combine(folder, "scrcpy-server"),
        _ => null,
    };
}
