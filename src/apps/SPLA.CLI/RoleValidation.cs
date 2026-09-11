using Spectre.Console;
using SPLA.Domain.Settings;

namespace SPLA.CLI;

/// <summary>Shared "--role &lt;name&gt;" check for the local-runtime CLI paths (<c>chat run</c>,
/// <c>chat open</c>) that create a chat directly against <see cref="ResolvedSettings"/> rather than
/// over the wire. A remote instance (<c>RemoteChatRun</c>) does not use this — the server performs the
/// same check itself against the same source (<c>ChatHandlers.New</c>), and its refusal arrives as an
/// ordinary <c>error</c> frame that the wire client already turns into an exception.</summary>
internal static class RoleValidation
{
    /// <summary>Resolves <paramref name="requested"/> against the project's declared <c>roles:</c>
    /// list, case-insensitively — the same source <c>agent_spawn</c>'s <c>GetAvailableRoles</c> and
    /// <c>ChatHandlers.New</c> both use. Returns the canonical name on success; on an unknown name,
    /// prints a red line listing what is available and returns null so the caller can exit(2).</summary>
    public static string? Resolve(ResolvedSettings settings, string requested)
    {
        var available = settings.Manifest?.Roles
            ?.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToList() ?? [];
        var match = available.FirstOrDefault(n => string.Equals(n, requested, StringComparison.OrdinalIgnoreCase));
        if (match != null) return match;

        var list = available.Count > 0 ? string.Join(", ", available) : "(none)";
        AnsiConsole.MarkupLine($"[red]Unknown role:[/] {requested.EscapeMarkup()}. [grey]Available roles:[/] {list.EscapeMarkup()}");
        return null;
    }
}
