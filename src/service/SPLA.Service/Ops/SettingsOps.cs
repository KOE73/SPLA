using SPLA.MCP.Core.ToolSets;
using SPLA.Runtime;
using SPLA.Domain.Models;
using SPLA.Domain.Resources;
using SPLA.Domain.Secrets;
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
            // References travel; key material does not. A project that still carries a pasted literal
            // says so through the flag — enough for the editor to prompt for a move into the store,
            // without the value making the trip to a browser to get there.
            ApiKey = Reference(c.ApiKey),
            AdminKey = Reference(c.AdminKey),
            ApiKeyIsLiteral = IsLiteral(c.ApiKey),
            AdminKeyIsLiteral = IsLiteral(c.AdminKey),
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
        // What is on disk now, to fall back on per credential: the editor is never handed a literal,
        // so a blank field means "unchanged", not "cleared". Without this, opening the panel and
        // pressing Save on a project with pasted keys would wipe every one of them.
        var stored = runtime.Settings.Connections
            .GroupBy(c => c.Id, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var sections = incoming
            .Select(d => ToSection(d, stored))
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
        SaveToolCalls = runtime.Settings.SaveToolCalls,
        SaveAttempts = runtime.Settings.SaveAttempts,
        UnifiedResources = runtime.Settings.UnifiedResources,
        ResourceSchemes = ResourceRegistry.For(runtime.Settings).Cards().Select(c => new ResourceSchemeDto
        {
            Scheme = c.Scheme,
            Summary = c.Summary,
            Verbs = c.Verbs.Select(VerbWord).ToList(),
            Enabled = c.Enabled
        }).ToList(),
        Theme = runtime.Settings.Theme,
        Density = runtime.Settings.Density,
        Themes = KnownThemes,
        Densities = KnownDensities
    };

    /// <summary>Lower-case wire word for a verb — kept identical to
    /// <c>SPLA.Agent.Composition.ResourceSchemesContributor.VerbWord</c> so the settings panel and the
    /// system prompt never disagree on what to call the same thing.</summary>
    private static string VerbWord(ResourceVerb verb) => verb switch
    {
        ResourceVerb.Read => "read",
        ResourceVerb.Exists => "exists",
        ResourceVerb.List => "list",
        ResourceVerb.Write => "write",
        ResourceVerb.Delete => "delete",
        ResourceVerb.MakeDir => "mkdir",
        _ => verb.ToString().ToLowerInvariant()
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
        var saveToolCalls = dto.SaveToolCalls ?? false;
        runtime.Settings.SaveToolCalls = saveToolCalls;
        var saveAttempts = dto.SaveAttempts ?? false;
        runtime.Settings.SaveAttempts = saveAttempts;
        var unifiedResources = dto.UnifiedResources ?? false;
        runtime.Settings.UnifiedResources = unifiedResources;

        // Per-scheme switches. Only what the panel actually sent is touched — a scheme this project
        // never mentioned stays absent (enabled), rather than every known scheme getting written the
        // instant anyone saves the Agent tab.
        var registry = ResourceRegistry.For(runtime.Settings);
        if (dto.ResourceSchemeSwitches != null)
            foreach (var s in dto.ResourceSchemeSwitches)
            {
                if (string.IsNullOrWhiteSpace(s.Scheme)) continue;
                if (s.Enabled) runtime.Settings.ResourceSchemes.Remove(s.Scheme);
                else runtime.Settings.ResourceSchemes[s.Scheme] = false;
            }
        // Live: takes effect immediately, the same way a plugin's enable/disable does, without
        // waiting for the next turn to re-read settings.
        registry.ApplySwitches(runtime.Settings.ResourceSchemes);

        var path = runtime.Settings.ProjectFilePath;
        if (path != null)
        {
            var project = ConfigLoader.LoadProjectRaw(path);
            (project.Agent ??= new SplaAgentSection()).Mode = Blank(dto.Mode);
            project.Agent.CustomPrompt = Blank(dto.CustomPrompt);
            // Write only non-default values so untouched projects keep a clean file.
            project.Agent.LoopGuard = loopGuard ? true : null;
            project.Agent.LoopGuardRepeats = loopRepeats != 3 ? loopRepeats : null;
            project.Agent.SaveToolCalls = saveToolCalls ? true : null;
            project.Agent.SaveAttempts = saveAttempts ? true : null;
            project.Agent.UnifiedResources = unifiedResources ? true : null;
            var anyPerm = read != null || write != null || shell != null || net != null;
            project.Permissions = anyPerm
                ? new SplaPermissionsSection { Read = read, Write = write, Shell = shell, Internet = net }
                : null;
            project.Resources = runtime.Settings.ResourceSchemes.Count > 0
                ? new Dictionary<string, bool>(runtime.Settings.ResourceSchemes, StringComparer.OrdinalIgnoreCase)
                : null;
            ConfigLoader.SaveProjectSections(project, path, "agent", "permissions", "resources");
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

    // ── MCP over HTTP: whether POST /mcp is offered, and a fixed port for it ─

    public static McpSettingsPayload GetMcp(AgentRuntime runtime) => new()
    {
        CanPersist = runtime.Settings.ProjectFilePath != null,
        Enabled = runtime.Settings.McpEnabled,
        Port = runtime.Settings.McpPort
    };

    /// <summary>Persists to the .spla project (when present) and mutates the live
    /// <see cref="ResolvedSettings"/> so the next read (and the next <c>spla serve</c> start) sees it.
    /// Does NOT touch a currently running listener — Kestrel binds its port once, at startup, the same
    /// way a plugin enable flag needs a restart to load an assembly.</summary>
    public static McpSettingsPayload SaveMcp(AgentRuntime runtime, McpSettingsPayload dto)
    {
        runtime.Settings.McpEnabled = dto.Enabled;
        runtime.Settings.McpPort = dto.Port is > 0 ? dto.Port : null;

        var path = runtime.Settings.ProjectFilePath;
        if (path != null)
        {
            var project = ConfigLoader.LoadProjectRaw(path);
            // Write only when it departs from the default, so a project that never touched this stays
            // free of an "mcp:" section — same convention SaveAgent follows for its own flags.
            project.Mcp = runtime.Settings.McpEnabled || runtime.Settings.McpPort is > 0
                ? new SplaMcpSection
                {
                    Enabled = runtime.Settings.McpEnabled ? true : null,
                    Port = runtime.Settings.McpPort
                }
                : null;
            ConfigLoader.SaveProjectSections(project, path, "mcp");
        }

        return GetMcp(runtime);
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

        // Both halves of the fond as one list. A person needs to see everything they have, not the
        // half they happen to own — the prescribed rows are simply marked as not editable in place.
        var declared = runtime.Settings.EffectiveSkillSources()
            .GroupBy(s => s.Id ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.OrdinalIgnoreCase);
        var grants = runtime.Settings.SkillTrustStore;

        foreach (var source in runtime.SkillLibrary.Sources)
        {
            var path = (source as SPLA.Library.Sources.DirectorySkillSource)?.RootPath;
            declared.TryGetValue(source.Id, out var entry);

            payload.Sources.Add(new SkillSourceDto
            {
                Id = source.Id,
                Label = source.Label,
                Trust = source.Trust.ToString(),
                Level = source.Level.ToString(),
                Path = path,
                Origin = (entry?.Origin ?? SourceOrigin.Machine).ToString(),
                Enabled = true,   // a switched-off branch never becomes a source; see below
                Editable = entry?.Origin == SourceOrigin.Granted,
                TrustGranted = path is not null && grants?.IsGranted(path) == true
            });
        }

        // Switched-off branches are not sources, so the loop above cannot show them — and a branch
        // that vanishes from the panel the moment it is turned off is one nobody can turn back on.
        foreach (var (id, entry) in declared)
        {
            if (entry.Enabled != false || payload.Sources.Any(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
                continue;

            payload.Sources.Add(new SkillSourceDto
            {
                Id = id,
                Label = entry.Label ?? id,
                Trust = "Untrusted",
                Level = "OnShelf",
                Path = entry.Path,
                Origin = entry.Origin.ToString(),
                Enabled = false,
                Editable = entry.Origin == SourceOrigin.Granted
            });
        }

        // Holdings, not Catalog: an unavailable skill must stay visible WITH its reason, otherwise
        // the panel silently loses the one thing the user needs in order to fix it.
        foreach (var skill in runtime.SkillLibrary.Holdings())
            payload.Skills.Add(new CapabilityDto
            {
                Id = skill.Id,
                Address = skill.Address,
                Kind = "skill",
                // What the model would be shown: bare while the name is unique, qualified once it is
                // not, so the panel and the prompt call the same book the same thing.
                Name = skill.DisplayId,
                Description = skill.Description,
                Enabled = skill.IsEnabled,
                Tags = skill.Tags.ToList(),
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
    /// where they come from.
    ///
    /// <para>Written under the full address, never the bare id. A row in this panel is one edition of
    /// one book, and the person clicking it is looking at its branch; a bare key would be a statement
    /// about every branch at once, which is a legitimate thing to write by hand and a terrible thing
    /// to produce by clicking.</para></summary>
    public static SkillsPayload SaveSkills(AgentRuntime runtime, IEnumerable<CapabilityDto> incoming)
    {
        foreach (var dto in incoming)
        {
            var key = !string.IsNullOrWhiteSpace(dto.Address) ? dto.Address : dto.Id;
            if (string.IsNullOrWhiteSpace(key)) continue;
            runtime.Settings.Skills[key] = new SplaSkillSection { Enabled = dto.Enabled };
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

    // ── Skill sources: the branches this person owns ─────────────────────────

    /// <summary>The editable half of the fond. Prescribed entries are not returned: they are
    /// read-only by construction and already travel with <see cref="GetSkills"/>, and offering them
    /// here would invite a client to save them somewhere they must not be saved.</summary>
    public static SkillSourcesPayload GetSkillSources(AgentRuntime runtime)
    {
        var payload = new SkillSourcesPayload();
        foreach (var entry in runtime.Settings.SkillSourceStore?.Load() ?? [])
            payload.Sources.Add(new SkillSourceEditDto
            {
                Id = entry.Id ?? string.Empty,
                Path = entry.Path,
                Label = entry.Label,
                Level = entry.Level,
                Enabled = entry.Enabled ?? true
            });

        return payload;
    }

    /// <summary>
    /// Replaces this person's branch list and rebuilds the fond live.
    ///
    /// <para>Written to their own store — never to the project file. That is the whole point of the
    /// second store: "I added a folder from my D: drive" is one person's decision, and committing it
    /// would deliver that decision to everyone who clones the repository.</para>
    ///
    /// <para>Trust is NOT settable here. A new branch arrives untrusted and is vouched for by
    /// <see cref="SetSkillSourceTrust"/>, which is a separate act with a separate confirmation.</para>
    /// </summary>
    public static SkillsPayload SaveSkillSources(AgentRuntime runtime, IEnumerable<SkillSourceEditDto> incoming)
    {
        var entries = new List<SplaSkillSourceSection>();
        foreach (var dto in incoming)
        {
            var id = dto.Id?.Trim();
            if (string.IsNullOrWhiteSpace(id)) continue;

            entries.Add(new SplaSkillSourceSection
            {
                Id = id,
                // A row that only switches an inherited branch off carries no path, and must not
                // invent a type either — the overlay inherits both from the entry underneath.
                Type = string.IsNullOrWhiteSpace(dto.Path) ? null : "directory",
                Path = string.IsNullOrWhiteSpace(dto.Path) ? null : dto.Path.Trim(),
                Label = string.IsNullOrWhiteSpace(dto.Label) ? null : dto.Label.Trim(),
                Level = string.IsNullOrWhiteSpace(dto.Level) ? null : dto.Level.Trim(),
                Enabled = dto.Enabled ? null : false,
                Origin = SourceOrigin.Granted
            });
        }

        runtime.Settings.SkillSourceStore?.Save(entries);
        runtime.RebuildSkillSources();
        return GetSkills(runtime);
    }

    /// <summary>
    /// Records or withdraws approval of a branch's contents.
    ///
    /// <para>Addressed by the branch's id here, because that is what the person clicked, but STORED
    /// against the resolved folder — approval is about contents, and contents live at a location.
    /// Rename the folder and the grant does not follow it.</para>
    /// </summary>
    public static SkillsPayload SetSkillSourceTrust(AgentRuntime runtime, string sourceId, bool trusted)
    {
        var source = runtime.SkillLibrary.Sources
            .FirstOrDefault(s => s.Id.Equals(sourceId, StringComparison.OrdinalIgnoreCase));

        if (source is SPLA.Library.Sources.DirectorySkillSource dir &&
            runtime.Settings.SkillTrustStore is { } grants)
        {
            if (trusted) grants.Grant(dir.RootPath);
            else grants.Revoke(dir.RootPath);

            runtime.RebuildSkillSources();
        }

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
                Description = SPLA.MCP.Core.Agent.AgentFeatureCatalog.DescriptionOf(id),
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

    private static SplaConnectionSection ToSection(
        ConnectionEditDto d, IReadOnlyDictionary<string, SplaConnectionSection> stored)
    {
        var id = string.IsNullOrWhiteSpace(d.Id) ? Slug(d.Name ?? "") : d.Id.Trim();
        var previous = stored.GetValueOrDefault(id);
        return new SplaConnectionSection
        {
            Id = id,
            Name = string.IsNullOrWhiteSpace(d.Name) ? null : d.Name.Trim(),
            Provider = Blank(d.Provider),
            Endpoint = Blank(d.Endpoint),
            ApiKey = Credential(d.ApiKey, d.ApiKeyIsLiteral, previous?.ApiKey),
            AdminKey = Credential(d.AdminKey, d.AdminKeyIsLiteral, previous?.AdminKey),
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

    /// <summary>The reference to publish for a stored credential: the pointer itself, or nothing at
    /// all when the project holds a literal. Sending a literal to a client would defeat the point of
    /// storing references at all — the editor gets a flag instead.</summary>
    private static string? Reference(string? stored)
        => SecretRef.IsReference(stored) ? stored : null;

    /// <summary>True when something is stored and it is key material rather than a pointer.</summary>
    private static bool IsLiteral(string? stored)
        => !string.IsNullOrWhiteSpace(stored) && !SecretRef.IsReference(stored);

    /// <summary>
    /// Decides what one credential field becomes on save, given what the editor sent and what is on
    /// disk. Three cases, and the middle one is the reason this is not a plain assignment:
    /// <list type="bullet">
    ///   <item>a reference (or any non-empty value) — the new value, as sent;</item>
    ///   <item>blank with <paramref name="keepLiteral"/> — the untouched literal the editor was never
    ///   shown; it stays exactly as it was, so merely opening the panel cannot erase a key;</item>
    ///   <item>blank without it — an explicit clear, which is how the user removes a credential.</item>
    /// </list>
    /// </summary>
    private static string? Credential(string? incoming, bool keepLiteral, string? stored)
    {
        var value = Blank(incoming);
        if (value != null) return value;
        return keepLiteral && IsLiteral(stored) ? stored : null;
    }

    private static string Slug(string s)
    {
        var chars = s.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var slug = new string(chars).Trim('-');
        return string.IsNullOrEmpty(slug) ? "conn-" + Guid.NewGuid().ToString("N")[..6] : slug;
    }
}
