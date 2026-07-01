using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Plugins.Browser;

/// <summary>
/// Plugin-owned settings (browser section in .spla file under plugins.browser.settings).
/// Each Playwright default arg can be toggled off; extra_args are appended verbatim.
/// </summary>
public sealed class BrowserSettings
{
    private static readonly ISerializer   Ser = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
    private static readonly IDeserializer De  = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties().Build();

    // ── Playwright default args (all true = Playwright includes them; false = IgnoreDefaultArgs) ──

    /// <summary>Adds the "Chrome is being controlled by automated test software" banner and
    /// sets navigator.webdriver = true. Disable when testing your own site's UX without the banner.</summary>
    [YamlMember(Alias = "enable_automation")]
    public bool EnableAutomation { get; set; } = true;

    /// <summary>Disables background sync, safe-browsing downloads, extension updates, etc.</summary>
    [YamlMember(Alias = "disable_background_networking")]
    public bool DisableBackgroundNetworking { get; set; } = true;

    /// <summary>Disables all extensions except those explicitly loaded via the launch options.</summary>
    [YamlMember(Alias = "disable_extensions")]
    public bool DisableExtensions { get; set; } = true;

    /// <summary>Suppresses the "Set as default browser?" prompt on first launch.</summary>
    [YamlMember(Alias = "no_default_browser_check")]
    public bool NoDefaultBrowserCheck { get; set; } = true;

    /// <summary>Skips Chrome's first-run wizard (profile import, tips, etc.).</summary>
    [YamlMember(Alias = "no_first_run")]
    public bool NoFirstRun { get; set; } = true;

    /// <summary>Disables Chrome's built-in pop-up blocker (allows new-window pop-ups).</summary>
    [YamlMember(Alias = "disable_popup_blocking")]
    public bool DisablePopupBlocking { get; set; } = true;

    // ── Extra args (appended to launch command, every session) ──

    /// <summary>Additional Chromium command-line flags appended verbatim, e.g. "--mute-audio".</summary>
    [YamlMember(Alias = "extra_args")]
    public List<string> ExtraArgs { get; set; } = new();

    // ── Derived ──────────────────────────────────────────────────────────────────────────────────

    /// <summary>Builds the IgnoreDefaultArgs list from the flags that were turned off.</summary>
    public IReadOnlyList<string> GetIgnoreDefaultArgs()
    {
        var list = new List<string>();
        if (!EnableAutomation)           list.Add("--enable-automation");
        if (!DisableBackgroundNetworking) list.Add("--disable-background-networking");
        if (!DisableExtensions)           list.Add("--disable-extensions");
        if (!NoDefaultBrowserCheck)       list.Add("--no-default-browser-check");
        if (!NoFirstRun)                  list.Add("--no-first-run");
        if (!DisablePopupBlocking)        list.Add("--disable-popup-blocking");
        return list;
    }

    // ── Parsing ───────────────────────────────────────────────────────────────────────────────────

    public static BrowserSettings FromBlob(Dictionary<string, object>? blob)
    {
        if (blob is null || blob.Count == 0) return new();
        return De.Deserialize<BrowserSettings>(Ser.Serialize(blob)) ?? new();
    }
}
