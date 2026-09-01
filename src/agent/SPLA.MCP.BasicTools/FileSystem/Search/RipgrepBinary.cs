using System;
using System.IO;

namespace SPLA.MCP.BasicTools.FileSystem.Search;

/// <summary>
/// Resolves the ripgrep executable by an <em>explicit path</em>, never by bare PATH lookup.
/// <para>
/// A bare <c>FileName = "rg"</c> is a hijack vector: whoever can place an <c>rg</c> earlier in PATH
/// gets executed inside the agent — and this happens under a tool declared
/// <see cref="SPLA.Domain.Models.ToolEffect.Read"/>, which makes the surprise worse. So PATH is the
/// last resort and is <b>off unless explicitly enabled</b>; the normal answer is the binary we ship.
/// See <c>docs/adr/ADR_20260831_mcp_search-and-listing-tools.md</c> §2.3a.
/// </para>
/// </summary>
internal static class RipgrepBinary
{
    /// <summary>Explicit override — an absolute path to the binary to use.</summary>
    public const string PathVariable = "SPLA_RIPGREP_PATH";

    /// <summary>Opt-in to the legacy PATH lookup. Anything other than "0"/"false"/empty enables it.</summary>
    public const string AllowPathVariable = "SPLA_RIPGREP_ALLOW_PATH";

    private static readonly Lazy<string?> Resolved = new(Resolve, isThreadSafe: true);

    /// <summary>Absolute path to the binary, or <c>null</c> when no trusted copy was found.
    /// When PATH lookup is explicitly enabled this may be the bare file name instead.</summary>
    public static string? ExecutablePath => Resolved.Value;

    public static bool IsAvailable => Resolved.Value is not null;

    private static string? Resolve()
    {
        var exe = OperatingSystem.IsWindows() ? "rg.exe" : "rg";

        // 1. Explicit path wins — this is the setting an operator reaches for.
        var configured = Environment.GetEnvironmentVariable(PathVariable);
        if (!string.IsNullOrWhiteSpace(configured))
            return File.Exists(configured) ? configured : null;

        // 2. Shipped alongside the SPLA binary — the intended normal case.
        var shipped = Path.Combine(AppContext.BaseDirectory, "tools", exe);
        if (File.Exists(shipped)) return shipped;

        // 3. Global SPLA tools directory.
        try
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(home))
            {
                var global = Path.Combine(home, ".spla", "tools", exe);
                if (File.Exists(global)) return global;
            }
        }
        catch { /* no profile directory — fall through */ }

        // 4. PATH, only if someone deliberately turned it back on.
        return AllowPathLookup() ? exe : null;
    }

    private static bool AllowPathLookup()
    {
        var raw = Environment.GetEnvironmentVariable(AllowPathVariable);
        if (string.IsNullOrWhiteSpace(raw)) return false;
        raw = raw.Trim();
        return !raw.Equals("0", StringComparison.Ordinal)
            && !raw.Equals("false", StringComparison.OrdinalIgnoreCase);
    }
}
