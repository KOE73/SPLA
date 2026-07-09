using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

/// <summary>
/// Launches the browser. The interesting part is profile selection: when the caller doesn't pin a
/// choice, this tool does NOT silently default to a real personal profile (that would hand the
/// agent access to its saved logins/cookies without the user ever agreeing) — it asks, via
/// <see cref="ClarifyScope"/>, offering exactly three kinds of option: a project-local persistent
/// profile, any real Edge/Chrome profiles found on the machine, and a fresh temporary one. In
/// autonomous/headless runs (no clarify handler attached) it falls back to the safe default: new.
/// </summary>
public sealed class BrowserStartTool : IMcpTool
{
    private readonly SPLA.Domain.Project.IProject _project;
    private readonly BrowserSettings _settings;

    public BrowserStartTool(SPLA.Domain.Project.IProject project, BrowserSettings settings)
    {
        _project = project;
        _settings = settings;
    }

    public string Name => "browser_start";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Launches a real browser you can then navigate and interact with. Call once per chat; " +
                          "reuse the running browser for the rest of the task. If 'channel' is omitted, tries the " +
                          "system Microsoft Edge first and falls back to a managed Chromium (installed automatically " +
                          "on first use) if Edge isn't available. If 'profile' is omitted, asks the user to choose " +
                          "between a project-local persistent profile, a detected real browser profile, or a fresh " +
                          "temporary one (call browser_list_profiles to see the choices yourself first).",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    channel = new { type = "string", @enum = new[] { "msedge", "chrome", "chromium" }, description = "Browser to launch. Omit for auto (Edge, falling back to managed Chromium). Ignored (overridden) when 'profile' selects a real browser profile — it forces the matching channel." },
                    profile = new
                    {
                        type = "string",
                        description = "Profile to use: \"new\" (fresh, temporary, default if nothing else is decided), " +
                                      "\"project\" (persistent profile stored in this project, reused across runs), " +
                                      "a real profile's display name or account email from browser_list_profiles " +
                                      "(uses that real Edge/Chrome profile — the agent gets access to its saved " +
                                      "logins/cookies), or a raw local folder path for a custom persistent profile. " +
                                      "Omit to be asked."
                    },
                    headless = new { type = "boolean", description = "Run without a visible window. Default: false." },
                    viewport_width = new { type = "integer", description = "Viewport width in pixels. Omit for the browser default." },
                    viewport_height = new { type = "integer", description = "Viewport height in pixels. Omit for the browser default." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string? channel, profile;
        bool headless;
        int? vw, vh;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            channel = ToolJson.GetStringTrimmed(root, "channel");
            profile = ToolJson.GetStringTrimmed(root, "profile");
            headless = ToolJson.GetBoolean(root, "headless", false);
            vw = ToolJson.GetInt32(root, "viewport_width");
            vh = ToolJson.GetInt32(root, "viewport_height");
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        try
        {
            var mgr = BrowserToolBase.GetOrCreateSession();

            var resolved = profile is null
                ? await ResolveByAskingAsync()
                : ResolveExplicit(profile);

            var effectiveChannel = resolved.Channel ?? channel;
            var note = resolved.Note is null ? "" : $" {resolved.Note}";

            // Merge profile-level extra args (e.g. --profile-directory) with settings-level extra args.
            var mergedArgs = MergeArgs(resolved.ExtraArgs, _settings.ExtraArgs);
            var ignoreArgs = _settings.GetIgnoreDefaultArgs();

            return await mgr.StartAsync(effectiveChannel, headless, resolved.ProfilePath, vw, vh, mergedArgs, ignoreArgs) + note;
        }
        catch (InvalidOperationException ex) { return $"Error: {ex.Message}"; }
        catch (Exception ex) { return BrowserToolBase.Fail("browser_start", ex); }
    }

    private sealed record ResolvedProfile(string? ProfilePath, string? Channel, string[]? ExtraArgs, string? Note);

    /// <summary>Explicit 'profile' argument: "new", "project", a detected profile's name/email, or a raw path.</summary>
    private ResolvedProfile ResolveExplicit(string profile)
    {
        if (profile.Equals("new", StringComparison.OrdinalIgnoreCase))
            return new ResolvedProfile(null, null, null, null);

        if (profile.Equals("project", StringComparison.OrdinalIgnoreCase))
            return new ResolvedProfile(BrowserProfilePaths.ProjectProfileDir(_project), null, null, null);

        var detected = BrowserProfileDiscovery.Discover().FirstOrDefault(p =>
            p.DisplayName.Equals(profile, StringComparison.OrdinalIgnoreCase) ||
            (p.Account != null && p.Account.Equals(profile, StringComparison.OrdinalIgnoreCase)));
        if (detected != null) return FromDetected(detected);

        // Anything else is treated as a raw folder path — the pre-MVP behaviour, kept for scripting.
        return new ResolvedProfile(profile, null, null, null);
    }

    /// <summary>No 'profile' argument: build the 3-way choice and ask. Falls back to "new" when no
    /// clarify handler is attached (autonomous/headless) — never silently picks a real profile.</summary>
    private async Task<ResolvedProfile> ResolveByAskingAsync()
    {
        var detected = BrowserProfileDiscovery.Discover();
        var options = new List<ClarifyOption>();
        var byLabel = new Dictionary<string, ResolvedProfile>();

        var projectHasState = BrowserProfilePaths.ProjectProfileHasState(_project);
        var projectOption = new ClarifyOption
        {
            Label = BrowserProfilePaths.ProjectProfileLabel,
            Description = projectHasState
                ? "Reuses logins/cookies saved by earlier browser_start calls in this project."
                : "Fresh now, but persists in this project — reused (and accumulates state) on future runs."
        };
        options.Add(projectOption);
        byLabel[projectOption.Label] = new ResolvedProfile(BrowserProfilePaths.ProjectProfileDir(_project), null, null, null);

        foreach (var p in detected)
        {
            var label = p.Account != null ? $"{p.Channel} — {p.DisplayName} ({p.Account})" : $"{p.Channel} — {p.DisplayName}";
            var option = new ClarifyOption
            {
                Label = label,
                Description = "Your real, already-logged-in profile — the agent will have access to its saved logins, cookies, and history."
            };
            options.Add(option);
            byLabel[label] = FromDetected(p);
        }

        var newOption = new ClarifyOption { Label = "New, temporary", Description = "Empty profile, no saved logins or history. Discarded when the browser closes." };
        options.Add(newOption);
        byLabel[newOption.Label] = new ResolvedProfile(null, null, null, null);

        var chosen = await ClarifyScope.AskAsync(new ClarifyRequest
        {
            Question = "Which browser profile should I use?",
            Options = options
        });

        if (chosen != null && byLabel.TryGetValue(chosen, out var pick))
            return pick;

        // No clarify handler (autonomous/headless), or an unrecognised answer: safest default.
        return new ResolvedProfile(null, null, null, "(no profile choice available — used a new, temporary profile)");
    }

    private static ResolvedProfile FromDetected(DetectedBrowserProfile p)
        => new(p.UserDataDir, p.Channel, new[] { $"--profile-directory={p.ProfileDirectory}" }, null);

    private static IReadOnlyList<string>? MergeArgs(string[]? profileArgs, List<string> settingsArgs)
    {
        var all = new List<string>();
        if (profileArgs != null) all.AddRange(profileArgs);
        if (settingsArgs.Count > 0) all.AddRange(settingsArgs);
        return all.Count > 0 ? all : null;
    }
}
