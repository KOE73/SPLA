using SPLA.MCP.Core.ToolSets;
using SPLA.Runtime;
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
            AdminKey = c.AdminKey,
            SwapModel = c.SwapModel,
            Models = c.Models.Select(m => new ModelEditDto
            {
                Id = m.Id,
                Name = m.Name,
                Model = m.Model,
                ContextLength = m.ContextLength
            }).ToList()
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

        // Model ids are referenced by chats without naming a connection, so a duplicate across two
        // connections has no defined meaning. Resolution throws on one — refusing the save here is
        // what keeps a bad edit from writing a project file that no longer loads.
        var clash = sections
            .SelectMany(c => c.Models.Select(m => (Conn: c.Id, m.Id)))
            .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);
        if (clash != null)
        {
            var result = GetConnections(runtime);
            result.Error = $"Model id '{clash.Key}' is used by more than one connection " +
                           $"({string.Join(", ", clash.Select(x => x.Conn).Distinct())}). Ids must be unique across the project.";
            return result;
        }

        // Persist into the project file's connections: section, leaving everything else untouched.
        var path = runtime.Settings.ProjectFilePath;
        if (path != null)
        {
            var project = ConfigLoader.LoadProjectRaw(path);
            project.Connections = sections.Count > 0 ? sections : null;
            ConfigLoader.SaveProjectSections(project, path, "connections");
        }

        // Mutate the live settings in place so running chats resolve against the new list. The flat
        // model projection is rebuilt from the same objects — chats resolve through it, so leaving it
        // stale would keep them pointed at the pre-save tree.
        runtime.Settings.Connections.Clear();
        runtime.Settings.Connections.AddRange(sections);
        runtime.Settings.Models = sections
            .SelectMany(c => c.Models.Select(m => new ResolvedModelEntry { Connection = c, Entry = m }))
            .ToList();

        return GetConnections(runtime);
    }

    // ── Token usage: session/project/machine totals ───────────────────────────

    public static UsageResultPayload GetUsage(AgentRuntime runtime) => new()
    {
        Session = ToScope(runtime.TokenUsageProject.Session),
        Project = ToScope(runtime.TokenUsageProject.Total),
        Machine = ToScope(runtime.TokenUsageGlobal.Total)
    };

    private static TokenUsageScopePayload ToScope(SPLA.Domain.Models.TokenUsageTotals t) => new()
    {
        PromptTokens = t.PromptTokens,
        CompletionTokens = t.CompletionTokens,
        Turns = t.Turns,
        TotalTokens = t.TotalTokens
    };

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
        CustomPrompt = runtime.Settings.CustomPrompt,
        LoopGuard = runtime.Settings.LoopGuard,
        LoopGuardRepeats = runtime.Settings.LoopGuardRepeats,
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
        runtime.Settings.CustomPrompt = Blank(dto.CustomPrompt);
        var loopGuard = dto.LoopGuard ?? false;
        var loopRepeats = Math.Clamp(dto.LoopGuardRepeats ?? runtime.Settings.LoopGuardRepeats, 2, 20);
        runtime.Settings.LoopGuard = loopGuard;
        runtime.Settings.LoopGuardRepeats = loopRepeats;

        var path = runtime.Settings.ProjectFilePath;
        if (path != null)
        {
            var project = ConfigLoader.LoadProjectRaw(path);
            (project.Agent ??= new SplaAgentSection()).Mode = Blank(dto.Mode);
            project.Agent.CustomPrompt = Blank(dto.CustomPrompt);
            // Write only non-default values so untouched projects keep a clean file.
            project.Agent.LoopGuard = loopGuard ? true : null;
            project.Agent.LoopGuardRepeats = loopRepeats != 3 ? loopRepeats : null;
            var anyPerm = read != null || write != null || shell != null || net != null;
            project.Permissions = anyPerm
                ? new SplaPermissionsSection { Read = read, Write = write, Shell = shell, Internet = net }
                : null;
            ConfigLoader.SaveProjectSections(project, path, "agent", "permissions");
        }

        return GetAgent(runtime);
    }

    /// <summary>Persists just the UI appearance (theme/density) to the .spla project and mutates the
    /// live settings, then publishes <see cref="AppearanceChanged"/> so every window applies it. Kept
    /// separate from agent settings: appearance is a low-stakes, instantly-reversible preference that
    /// auto-applies on change with no Save step — unlike the transactional mode/permission edits.</summary>
    public static void SaveAppearance(AgentRuntime runtime, string? theme, string? density)
    {
        theme   = Blank(theme)   ?? runtime.Settings.Theme;
        density = Blank(density) ?? runtime.Settings.Density;
        runtime.Settings.Theme   = theme;
        runtime.Settings.Density = density;

        var path = runtime.Settings.ProjectFilePath;
        if (path != null)
        {
            var project = ConfigLoader.LoadProjectRaw(path);
            (project.Ui ??= new()).Theme = theme;
            project.Ui.Density           = density;
            ConfigLoader.SaveProjectSections(project, path, "ui");
        }

        runtime.Events.Publish(new AppearanceChanged(theme, density));
    }

    // ── Plugins: enable/disable + custom prompt + opaque settings blob ───────

    public static PluginsPayload GetPlugins(AgentRuntime runtime)
    {
        var payload = new PluginsPayload
        {
            CanPersist = runtime.Settings.ProjectFilePath != null,
            // Enable/disable applies LIVE: disable gates exposure (tools/prompt vanish next turn),
            // enable lazily loads a plugin that was skipped at startup (see SavePlugins).
            RestartToApply = false
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
                // The level as written, not as resolved: a set with no entry must show as "follows the
                // enable flag", otherwise saving the panel back would freeze a derived level into the file.
                Level = runtime.Settings.ToolSets.GetValueOrDefault(d.Meta.Id),
                State = d.EffectiveState.ToString(),
                StateReason = string.IsNullOrWhiteSpace(d.EffectiveStateReason) ? null : d.EffectiveStateReason,
                CustomPrompt = section?.CustomPrompt,
                SettingsJson = ConfigLoader.BlobToJson(section?.Settings),
                WebSettingsUrl = string.IsNullOrWhiteSpace(d.Meta.WebSettingsEntry)
                    ? null
                    : $"/plugin-assets/{Uri.EscapeDataString(d.Meta.Id)}/{d.Meta.WebSettingsEntry.Replace('\\', '/')}"
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
            try { blob = ConfigLoader.BlobFromJson(dto.SettingsJson); }
            catch { blob = existing?.Settings; }   // bad JSON → keep what was there

            var merged = new SplaPluginSection
            {
                Enabled = dto.Enabled,
                CustomPrompt = Blank(dto.CustomPrompt),
                Settings = blob,
                Tools = existing?.Tools
            };

            if (project != null) { (project.Plugins ??= new())[dto.Id] = merged; }
            runtime.Settings.Plugins[dto.Id] = merged;

            // The level lives in its own section: it is a property of the SET, and the plugin section
            // is about the supplier. An empty level clears the entry so the set goes back to following
            // the enable flag rather than being pinned to whatever it happened to resolve to.
            if (string.IsNullOrWhiteSpace(dto.Level))
            {
                runtime.Settings.ToolSets.Remove(dto.Id);
                project?.ToolSets?.Remove(dto.Id);
            }
            else if (ToolSetRegistry.TryParseLevel(dto.Level, out var level))
            {
                var written = ToolSetRegistry.Format(level);
                runtime.Settings.ToolSets[dto.Id] = written;
                if (project != null) (project.ToolSets ??= new())[dto.Id] = written;
            }
        }

        if (project != null && path != null) ConfigLoader.SaveProjectSections(project, path, "plugins", "toolsets");

        // Live ENABLE: a plugin that was disabled at startup never got its assembly loaded — load it
        // now and expose its tools immediately (disable needs nothing: exposure is gated per call).
        foreach (var dto in incoming)
            if (dto.Enabled != false && !string.IsNullOrWhiteSpace(dto.Id))
                foreach (var tool in runtime.PluginManager.EnsureLoaded(dto.Id))
                    runtime.McpHost.RegisterTool(tool);

        // A plugin toggle moves skills too: the plugin's own bundled skills appear or vanish, and a
        // skill elsewhere that required one of its tools becomes available or blocked. Rebuild here
        // rather than leaving the skill list resolved against the tool surface as it was at startup.
        runtime.RefreshSkillCapabilities();

        return GetPlugins(runtime);
    }

    // ── Skills: per-skill switches over the source registry ──────────────────

    public static SkillsPayload GetSkills(AgentRuntime runtime)
    {
        var payload = new SkillsPayload { CanPersist = runtime.Settings.ProjectFilePath != null };

        foreach (var source in runtime.SkillLibrary.Sources)
            payload.Sources.Add(new SkillSourceDto
            {
                Id = source.Id,
                Label = source.Label,
                Trust = source.Trust.ToString(),
                Path = (source as SPLA.Library.Sources.DirectorySkillSource)?.RootPath
            });

        // Holdings, not Catalog: an unavailable skill must stay visible WITH its reason, otherwise
        // the panel silently loses the one thing the user needs in order to fix it.
        foreach (var skill in runtime.SkillLibrary.Holdings())
            payload.Skills.Add(new CapabilityDto
            {
                Id = skill.Id,
                Kind = "skill",
                Name = skill.Id,
                Description = skill.Description,
                Enabled = skill.IsEnabled,
                Preloaded = skill.IsPreloaded,
                State = skill.State.ToString(),
                StateReason = string.IsNullOrWhiteSpace(skill.StateReason) ? null : skill.StateReason,
                Source = skill.SourceId,
                SourceLabel = skill.SourceLabel,
                MissingTools = skill.MissingTools.ToList(),
                MissingFeatures = skill.MissingFeatures.ToList(),
                MissingPlugins = skill.MissingPlugins.ToList()
            });

        return payload;
    }

    /// <summary>Persists per-skill switches to <c>skills.items</c> and applies them live — skills are
    /// read on demand, so unlike plugin assemblies nothing needs a restart. The <c>sources</c> half of
    /// the section is left untouched: this editor switches skills on and off, it does not repoint
    /// where they come from.</summary>
    public static SkillsPayload SaveSkills(AgentRuntime runtime, IEnumerable<CapabilityDto> incoming)
    {
        foreach (var dto in incoming)
        {
            if (string.IsNullOrWhiteSpace(dto.Id)) continue;
            runtime.Settings.Skills[dto.Id] = new SplaSkillSection
            {
                Enabled = dto.Enabled,
                Preloaded = dto.Preloaded ? true : null
            };
        }

        var path = runtime.Settings.ProjectFilePath;
        if (path != null)
        {
            var project = ConfigLoader.LoadProjectRaw(path);
            var section = project.Skills ??= new SplaSkillsSection();
            section.Items ??= new Dictionary<string, SplaSkillSection>();
            foreach (var kvp in runtime.Settings.Skills) section.Items[kvp.Key] = kvp.Value;
            ConfigLoader.SaveProjectSections(project, path, "skills");
        }

        runtime.SkillLibrary.ApplySettings(runtime.Settings.Skills);
        return GetSkills(runtime);
    }

    // ── Built-in capabilities: the agent.capabilities set ────────────────────

    public static FeaturesPayload GetFeatures(AgentRuntime runtime)
    {
        var payload = new FeaturesPayload { CanPersist = runtime.Settings.ProjectFilePath != null };

        // Resolved from the LIVE setting rather than runtime.HasFeature, which is frozen at startup:
        // after a save the panel must show what was saved, while RestartToApply explains that the
        // tools themselves follow on the next start.
        var enabledIds = SPLA.MCP.Core.Agent.AgentFeatureCatalog
            .Resolve(runtime.Settings.Capabilities)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var id in SPLA.MCP.Core.Agent.AgentFeatureCatalog.Order)
        {
            var enabled = enabledIds.Contains(id);
            payload.Features.Add(new CapabilityDto
            {
                Id = id,
                Kind = "builtin",
                Name = id,
                Enabled = enabled,
                State = enabled ? "Enabled" : "DisabledByUser",
                Requires = SPLA.MCP.Core.Agent.AgentFeatureCatalog.RequiresOf(id).ToList()
            });
        }

        return payload;
    }

    /// <summary>Persists the enabled built-in set to <c>agent.capabilities</c>. Dependencies are
    /// resolved through the catalog before writing, so the stored list is always self-consistent —
    /// enabling core.checkpoints without core.memory would otherwise produce a file that silently
    /// means something else than it says. Takes effect on the next start: feature tools register once.</summary>
    public static FeaturesPayload SaveFeatures(AgentRuntime runtime, IEnumerable<CapabilityDto> incoming)
    {
        var selected = incoming.Where(f => f.Enabled && !string.IsNullOrWhiteSpace(f.Id))
                               .Select(f => f.Id).ToList();
        var resolved = SPLA.MCP.Core.Agent.AgentFeatureCatalog.Resolve(selected).ToList();

        // The full catalog means "no restriction" — store null rather than an exhaustive list, so a
        // capability added in a future version is enabled by default instead of silently missing.
        var isFullSet = resolved.Count == SPLA.MCP.Core.Agent.AgentFeatureCatalog.Order.Count;
        runtime.Settings.Capabilities = isFullSet ? null : resolved;

        var path = runtime.Settings.ProjectFilePath;
        if (path != null)
        {
            var project = ConfigLoader.LoadProjectRaw(path);
            (project.Agent ??= new SplaAgentSection()).Capabilities = isFullSet ? null : resolved;
            ConfigLoader.SaveProjectSections(project, path, "agent");
        }

        return GetFeatures(runtime);
    }

    private static SplaConnectionSection ToSection(ConnectionEditDto d)
    {
        var id = string.IsNullOrWhiteSpace(d.Id) ? Slug(d.Name ?? "") : d.Id.Trim();
        return new SplaConnectionSection
        {
            Id = id,
            Name = string.IsNullOrWhiteSpace(d.Name) ? null : d.Name.Trim(),
            Provider = Blank(d.Provider),
            Endpoint = Blank(d.Endpoint),
            ApiKey = Blank(d.ApiKey),
            AdminKey = Blank(d.AdminKey),
            SwapModel = d.SwapModel,
            Models = d.Models
                .Select(m => ToModelSection(m, id))
                .Where(m => !string.IsNullOrWhiteSpace(m.Id))
                .GroupBy(m => m.Id, StringComparer.OrdinalIgnoreCase)   // last write wins per id
                .Select(g => g.Last())
                .ToList()
        };
    }

    /// <summary>Maps one model row. A blank id is derived from the entry's own name or wire model,
    /// prefixed with the owning connection — the readable default for the common case where two
    /// connections carry the same model and a bare "opus" would collide across them.</summary>
    private static SplaModelSection ToModelSection(ModelEditDto d, string connectionId)
    {
        var raw = string.IsNullOrWhiteSpace(d.Id) ? Slug($"{connectionId}-{d.Name ?? d.Model ?? ""}") : d.Id.Trim();
        return new SplaModelSection
        {
            Id = raw,
            Name = string.IsNullOrWhiteSpace(d.Name) ? null : d.Name.Trim(),
            Model = Blank(d.Model),
            ContextLength = d.ContextLength is > 0 ? d.ContextLength : null
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
