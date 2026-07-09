using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SPLA.Plugins.Browser;

/// <summary>One real Edge/Chrome profile found on this machine, read from its "Local State" file.
/// <see cref="UserDataDir"/> is the browser's root user-data directory (what Playwright's
/// <c>userDataDir</c> launch argument expects); <see cref="ProfileDirectory"/> is the specific
/// sub-profile folder name (e.g. "Default", "Profile 1") selected via Chromium's
/// <c>--profile-directory</c> flag — pointing <c>userDataDir</c> straight at the sub-folder does
/// not work the way you'd expect, Chromium always wants the root plus that flag.</summary>
internal sealed record DetectedBrowserProfile(
    string Channel, string UserDataDir, string ProfileDirectory, string DisplayName, string? Account);

/// <summary>
/// Best-effort discovery of the user's existing Edge/Chrome profiles, for <c>browser_start</c> to
/// offer as an option (alongside a project-local profile and a fresh temporary one) instead of
/// silently picking one. Read-only: only inspects each browser's "Local State" JSON for profile
/// names; never touches profile contents (cookies, history, passwords).
/// </summary>
internal static class BrowserProfileDiscovery
{
    public static IReadOnlyList<DetectedBrowserProfile> Discover()
    {
        var results = new List<DetectedBrowserProfile>();
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrEmpty(localAppData))
        {
            TryAdd(results, "msedge", Path.Combine(localAppData, "Microsoft", "Edge", "User Data"));
            TryAdd(results, "chrome", Path.Combine(localAppData, "Google", "Chrome", "User Data"));
        }
        return results;
    }

    private static void TryAdd(List<DetectedBrowserProfile> results, string channel, string userDataDir)
    {
        var localStatePath = Path.Combine(userDataDir, "Local State");
        if (!File.Exists(localStatePath)) return;

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(localStatePath));
            if (!doc.RootElement.TryGetProperty("profile", out var profile)) return;
            if (!profile.TryGetProperty("info_cache", out var cache) || cache.ValueKind != JsonValueKind.Object) return;

            foreach (var entry in cache.EnumerateObject())
            {
                var folder = entry.Name; // "Default", "Profile 1", …
                var profileDir = Path.Combine(userDataDir, folder);
                if (!Directory.Exists(profileDir)) continue;

                var name = entry.Value.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String
                    ? n.GetString() : null;
                var account = entry.Value.TryGetProperty("user_name", out var u) && u.ValueKind == JsonValueKind.String
                    ? u.GetString() : null;

                results.Add(new DetectedBrowserProfile(
                    channel, userDataDir, folder, string.IsNullOrWhiteSpace(name) ? folder : name!,
                    string.IsNullOrWhiteSpace(account) ? null : account));
            }
        }
        catch
        {
            // A malformed or momentarily-locked Local State just means this browser reports no profiles.
        }
    }
}
