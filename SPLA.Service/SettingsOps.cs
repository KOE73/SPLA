using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Read/write operations for project settings, exposed to clients over the protocol. Kept separate
/// from <see cref="ClientConnection"/> so each settings area (connections today; modes/permissions/
/// plugins later) is a small, self-contained unit rather than swelling the connection dispatch.
/// <para>
/// Edits mutate the live <see cref="ResolvedSettings"/> in place (so running chats pick them up) and,
/// when the service is on a real <c>.spla</c> project, persist into that file's <c>connections:</c>
/// section. With no project file, edits live only for the session.
/// </para>
/// </summary>
public static class SettingsOps
{
    public static ConnectionsPayload GetConnections(AgentRuntime runtime) => new()
    {
        CanPersist = runtime.Settings.ProjectFilePath != null,
        Connections = runtime.Settings.Connections.Select(c => new ConnectionEditDto
        {
            Id = c.Id,
            Name = c.Name,
            Provider = c.Provider,
            Endpoint = c.Endpoint,
            ApiKey = c.ApiKey,
            Model = c.Model
        }).ToList()
    };

    /// <summary>Replaces the connection list: persists to the .spla project (when present) and mutates
    /// the live settings so chats see the new set immediately. Returns the canonical list to broadcast.</summary>
    public static ConnectionsPayload SaveConnections(AgentRuntime runtime, IEnumerable<ConnectionEditDto> incoming)
    {
        var sections = incoming
            .Select(ToSection)
            .Where(c => !string.IsNullOrWhiteSpace(c.Id))
            .GroupBy(c => c.Id, StringComparer.OrdinalIgnoreCase)   // last write wins per id
            .Select(g => g.Last())
            .ToList();

        // Persist into the project file's connections: section, leaving everything else untouched.
        var path = runtime.Settings.ProjectFilePath;
        if (path != null)
        {
            var project = ConfigLoader.LoadProjectRaw(path);
            project.Connections = sections.Count > 0 ? sections : null;
            ConfigLoader.SaveProject(project, path);
        }

        // Mutate the live settings in place so running chats resolve against the new list.
        runtime.Settings.Connections.Clear();
        runtime.Settings.Connections.AddRange(sections);

        return GetConnections(runtime);
    }

    // ── Agent settings: default mode + permission overrides ──────────────────

    private static readonly List<string> KnownThemes   = ["dark", "emerald", "cream", "light"];
    private static readonly List<string> KnownDensities = ["nano", "mini", "norm", "max"];

    public static AgentSettingsPayload GetAgent(AgentRuntime runtime) => new()
    {
        CanPersist = runtime.Settings.ProjectFilePath != null,
        Mode = runtime.Settings.Mode.ToString(),
        Modes = Enum.GetNames<AgentMode>().ToList(),
        PermRead = runtime.Settings.PermRead,
        PermWrite = runtime.Settings.PermWrite,
        PermShell = runtime.Settings.PermShell,
        PermInternet = runtime.Settings.PermInternet,
        Theme = runtime.Settings.Theme,
        Density = runtime.Settings.Density,
        Themes = KnownThemes,
        Densities = KnownDensities
    };

    /// <summary>Persists agent mode + permission overrides to the .spla project (when present) and
    /// updates the live settings. Note: the system prompt is built once at startup, so a default-mode
    /// change takes effect for new chats/turns, not retroactively — per-chat mode still governs each chat.</summary>
    public static AgentSettingsPayload SaveAgent(AgentRuntime runtime, AgentSettingsPayload dto)
    {
        var read = Blank(dto.PermRead); var write = Blank(dto.PermWrite);
        var shell = Blank(dto.PermShell); var net = Blank(dto.PermInternet);

        if (Enum.TryParse<AgentMode>(dto.Mode, true, out var mode)) runtime.Settings.Mode = mode;
        runtime.Settings.PermRead = read; runtime.Settings.PermWrite = write;
        runtime.Settings.PermShell = shell; runtime.Settings.PermInternet = net;

        var theme   = Blank(dto.Theme)   ?? runtime.Settings.Theme;
        var density = Blank(dto.Density) ?? runtime.Settings.Density;
        runtime.Settings.Theme   = theme;
        runtime.Settings.Density = density;

        var path = runtime.Settings.ProjectFilePath;
        if (path != null)
        {
            var project = ConfigLoader.LoadProjectRaw(path);
            (project.Agent ??= new SplaAgentSection()).Mode = Blank(dto.Mode);
            var anyPerm = read != null || write != null || shell != null || net != null;
            project.Permissions = anyPerm
                ? new SplaPermissionsSection { Read = read, Write = write, Shell = shell, Internet = net }
                : null;
            (project.Ui ??= new()).Theme   = theme;
            project.Ui.Density             = density;
            ConfigLoader.SaveProject(project, path);
        }

        // Appearance is a cross-cutting concern: announce it on its own channel so every window applies
        // it, not just the settings editor. The host turns this into an appearance.changed broadcast.
        runtime.Events.Publish(new AppearanceChanged(theme, density));

        return GetAgent(runtime);
    }

    // ── Plugins: enable/disable + custom prompt + opaque settings blob ───────

    public static PluginsPayload GetPlugins(AgentRuntime runtime)
    {
        var payload = new PluginsPayload
        {
            CanPersist = runtime.Settings.ProjectFilePath != null,
            RestartToApply = true   // plugins load once at startup; enable/disable applies next launch
        };

        foreach (var d in runtime.PluginManager.GetPlugins())
        {
            runtime.Settings.Plugins.TryGetValue(d.Meta.Id, out var section);
            payload.Plugins.Add(new PluginEditDto
            {
                Id = d.Meta.Id,
                Name = d.Meta.Metadata.TryGetValue("name", out var n) && !string.IsNullOrWhiteSpace(n) ? n : d.Meta.Id,
                Type = d.Meta.Type,
                Version = d.Meta.Version,
                // Prefer the live settings section (reflects edits made this session); the descriptor's
                // UserEnabled is fixed at startup and would otherwise mask an unsaved/just-saved toggle.
                Enabled = section?.Enabled ?? d.UserEnabled,
                State = d.EffectiveState.ToString(),
                StateReason = string.IsNullOrWhiteSpace(d.EffectiveStateReason) ? null : d.EffectiveStateReason,
                CustomPrompt = section?.CustomPrompt,
                SettingsYaml = ConfigLoader.SerializeBlob(section?.Settings)
            });
        }
        return payload;
    }

    /// <summary>Persists plugin enable flags, custom prompts and opaque settings blobs to the .spla
    /// project and mutates the live settings. Per-tool toggles (<c>tools:</c>) are preserved untouched.
    /// Enable/disable only takes effect on the next service start (plugins are loaded once).</summary>
    public static PluginsPayload SavePlugins(AgentRuntime runtime, IEnumerable<PluginEditDto> incoming)
    {
        var path = runtime.Settings.ProjectFilePath;
        SplaProject? project = path != null ? ConfigLoader.LoadProjectRaw(path) : null;

        foreach (var dto in incoming)
        {
            if (string.IsNullOrWhiteSpace(dto.Id)) continue;

            // Preserve anything the web editor doesn't touch (per-tool toggles) from the existing section.
            var existing = (project?.Plugins?.GetValueOrDefault(dto.Id))
                           ?? (runtime.Settings.Plugins.TryGetValue(dto.Id, out var s) ? s : null);

            Dictionary<string, object>? blob;
            try { blob = ConfigLoader.DeserializeBlob(dto.SettingsYaml); }
            catch { blob = existing?.Settings; }   // bad YAML → keep what was there

            var merged = new SplaPluginSection
            {
                Enabled = dto.Enabled,
                CustomPrompt = Blank(dto.CustomPrompt),
                Settings = blob,
                Tools = existing?.Tools
            };

            if (project != null) { (project.Plugins ??= new())[dto.Id] = merged; }
            runtime.Settings.Plugins[dto.Id] = merged;
        }

        if (project != null && path != null) ConfigLoader.SaveProject(project, path);
        return GetPlugins(runtime);
    }

    private static SplaConnectionSection ToSection(ConnectionEditDto d)
    {
        var id = string.IsNullOrWhiteSpace(d.Id) ? Slug(d.Name ?? d.Model ?? "") : d.Id.Trim();
        return new SplaConnectionSection
        {
            Id = id,
            Name = string.IsNullOrWhiteSpace(d.Name) ? null : d.Name.Trim(),
            Provider = Blank(d.Provider),
            Endpoint = Blank(d.Endpoint),
            ApiKey = Blank(d.ApiKey),
            Model = Blank(d.Model)
        };
    }

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string Slug(string s)
    {
        var chars = s.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var slug = new string(chars).Trim('-');
        return string.IsNullOrEmpty(slug) ? "conn-" + Guid.NewGuid().ToString("N")[..6] : slug;
    }
}
