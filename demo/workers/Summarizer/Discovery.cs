using SPLA.Domain.Interfaces;
using SPLA.Domain.Settings;

namespace SPLA.Demo.Summarizer;

/// <summary>One prompt variant. <see cref="Name"/> is what lands in the output file name, so it is
/// how two attempts are told apart a week later.</summary>
public sealed record PromptVariant(string Name, string Text, string? Path);

/// <summary>One model to run against. Either a model entry from the project's <c>connections:</c>
/// (<see cref="LmKey"/> null), or a downloaded LM Studio model that has to be loaded first — in which
/// case <see cref="EntryId"/> names the lmstudio entry whose wire name gets swapped for the run.</summary>
public sealed record ModelTarget(string Label, string EntryId, string? LmKey, string? WireModel)
{
    /// <summary>What to show in the console.</summary>
    public string Display => LmKey is null ? $"{EntryId} · {WireModel ?? "auto"}" : $"LM Studio · {LmKey}";
}

/// <summary>Finding things: the project file, the source document, the prompt variants, the model
/// targets. Pure lookup — nothing here talks to a model or writes a file.</summary>
public static class Discovery
{
    /// <summary>Walks up from <paramref name="startDir"/> looking for a <c>*.spla</c>. Unlike the
    /// loader's own finder this climbs, because the документ lives in a sub-folder of the project and
    /// the manifest sits at its root.</summary>
    public static string? FindProjectFileUpwards(string startDir)
    {
        var dir = new DirectoryInfo(Path.GetFullPath(startDir));
        while (dir != null)
        {
            var hit = dir.GetFiles("*.spla").OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
            if (hit != null) return hit.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    /// <summary>Picks the one source document out of <paramref name="folder"/>: the first configured
    /// extension that yields exactly one candidate, ignoring anything that looks like a summary
    /// (ours or somebody else's).</summary>
    /// <param name="error">Why nothing was picked, when the result is null.</param>
    public static string? PickSource(string folder, SummarizeConfig cfg, out string? error)
    {
        error = null;
        if (!Directory.Exists(folder)) { error = $"папки нет: {folder}"; return null; }

        foreach (var ext in cfg.SourceExtensions)
        {
            var candidates = Directory
                .GetFiles(folder, "*." + ext.TrimStart('.'))
                .Where(f => !IsExcluded(Path.GetFileName(f), cfg))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (candidates.Count == 1) return candidates[0];
            if (candidates.Count > 1)
            {
                error = $"в папке несколько файлов *.{ext}, непонятно какой исходник — укажите --source:\n" +
                        string.Join('\n', candidates.Select(c => "  " + Path.GetFileName(c)));
                return null;
            }
        }

        error = $"в папке нет исходника ({string.Join(", ", cfg.SourceExtensions.Select(e => "*." + e))}): {folder}";
        return null;
    }

    public static bool IsExcluded(string fileName, SummarizeConfig cfg) =>
        cfg.ExcludeContaining.Any(x =>
            !string.IsNullOrEmpty(x) && fileName.Contains(x, StringComparison.OrdinalIgnoreCase));

    /// <summary>Resolves the prompt variants for this run: what the command line asked for, else what
    /// the section asked for, else every <c>*.md</c> in the prompts folder.</summary>
    public static List<PromptVariant> LoadPrompts(
        Options opts, SummarizeConfig cfg, string promptsDir, out string? error)
    {
        error = null;
        var result = new List<PromptVariant>();

        for (var i = 0; i < opts.PromptTexts.Count; i++)
            result.Add(new PromptVariant($"text{i + 1}", opts.PromptTexts[i], null));

        var names = opts.Prompts.Count > 0 ? opts.Prompts : cfg.Prompts;
        foreach (var name in names)
        {
            var path = ResolvePromptPath(name, promptsDir);
            if (path == null)
            {
                error = $"вариант промпта не найден: {name} (искал в {promptsDir})";
                return result;
            }
            result.Add(new PromptVariant(Path.GetFileNameWithoutExtension(path), File.ReadAllText(path), path));
        }

        // Nothing named anywhere: the folder itself is the list.
        if (result.Count == 0)
        {
            if (!Directory.Exists(promptsDir))
            {
                error = $"папки с промптами нет: {promptsDir} (или задайте --prompt-text)";
                return result;
            }
            foreach (var path in Directory.GetFiles(promptsDir, "*.md")
                         .Where(f => !IsNotAVariant(Path.GetFileName(f)))
                         .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                result.Add(new PromptVariant(Path.GetFileNameWithoutExtension(path), File.ReadAllText(path), path));

            if (result.Count == 0)
                error = $"в папке с промптами нет ни одного *.md: {promptsDir}";
        }

        return result;
    }

    /// <summary>Files in the prompts folder that are about the folder rather than variants in it: its
    /// own README, drafts parked under a leading underscore. Named explicitly they still work — this
    /// only governs the "run everything in here" scan.</summary>
    private static bool IsNotAVariant(string fileName) =>
        fileName.StartsWith('_') || fileName.StartsWith('.') ||
        Path.GetFileNameWithoutExtension(fileName).Equals("README", StringComparison.OrdinalIgnoreCase);

    private static string? ResolvePromptPath(string name, string promptsDir)
    {
        if (File.Exists(name)) return Path.GetFullPath(name);

        foreach (var candidate in new[]
                 {
                     Path.Combine(promptsDir, name),
                     Path.Combine(promptsDir, name + ".md")
                 })
            if (File.Exists(candidate)) return Path.GetFullPath(candidate);

        return null;
    }

    /// <summary>Resolves the model targets. <c>--lm all</c> asks LM Studio what it has downloaded, which
    /// is the only part of discovery that needs the network.</summary>
    public static async Task<List<ModelTarget>> ResolveTargetsAsync(
        Options opts, SummarizeConfig cfg, ResolvedSettings settings,
        IModelManagementService management, CancellationToken ct)
    {
        var targets = new List<ModelTarget>();

        // ── Model entries from connections: ──
        var wantedEntries = opts.ModelsAll
            ? settings.Models.Select(m => m.Id).ToList()
            : opts.Models.Count > 0 ? opts.Models : new List<string>();

        foreach (var id in wantedEntries)
        {
            var entry = settings.FindModel(id);
            if (entry == null)
            {
                Console.WriteLine($"[!] нет записи модели '{id}' в connections: — пропускаю");
                continue;
            }
            var wire = await ResolveAutoAsync(entry, management, ct);
            targets.Add(new ModelTarget(Label(entry.Id, wire), entry.Id, null, wire));
        }

        // ── LM Studio keys, loaded one by one ──
        var lmEntry = settings.Models.FirstOrDefault(m =>
            string.Equals(m.Provider, "lmstudio", StringComparison.OrdinalIgnoreCase));

        var lmKeys = new List<string>(opts.LmModels);
        if (opts.LmAll)
        {
            if (lmEntry == null)
                Console.WriteLine("[!] --lm all: в проекте нет соединения lmstudio — пропускаю");
            else
            {
                var models = await management.GetModelDetailsAsync(
                    lmEntry.Endpoint ?? "", lmEntry.ApiKey ?? "lm-studio", ct);
                lmKeys.AddRange(models
                    .Where(m => !string.Equals(m.Type, "embedding", StringComparison.OrdinalIgnoreCase))
                    .Select(m => m.Id));
            }
        }

        foreach (var key in lmKeys.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (lmEntry == null)
            {
                Console.WriteLine($"[!] --lm {key}: в проекте нет соединения lmstudio — пропускаю");
                continue;
            }
            targets.Add(new ModelTarget($"lm {Sanitize(key)}", lmEntry.Id, key, key));
        }

        // ── Nothing asked for: the section's standing choice, else the project's first entry ──
        if (targets.Count == 0)
        {
            foreach (var id in cfg.Models)
                if (settings.FindModel(id) is { } e)
                {
                    var wire = await ResolveAutoAsync(e, management, ct);
                    targets.Add(new ModelTarget(Label(e.Id, wire), e.Id, null, wire));
                }

            foreach (var key in cfg.LmstudioModels)
                if (lmEntry != null)
                    targets.Add(new ModelTarget($"lm {Sanitize(key)}", lmEntry.Id, key, key));

            if (targets.Count == 0 && settings.Models.FirstOrDefault() is { } first)
            {
                var wire = await ResolveAutoAsync(first, management, ct);
                targets.Add(new ModelTarget(Label(first.Id, wire), first.Id, null, wire));
            }
        }

        return targets;
    }

    /// <summary>
    /// Turns <c>auto</c> into the name of the model that will actually answer.
    ///
    /// <para><c>auto</c> means "whatever the runtime has in memory", which is the right thing to
    /// request and the wrong thing to write on a result: a file named <c>… auto.md</c> does not say
    /// which model made it, and that is the one thing a folder of attempts exists to record. Asking
    /// the provider costs one call at planning time; when it cannot answer, <c>auto</c> stands.</para>
    /// </summary>
    private static async Task<string?> ResolveAutoAsync(
        ResolvedModelEntry entry, IModelManagementService management, CancellationToken ct)
    {
        var wire = entry.Model;
        if (!string.IsNullOrWhiteSpace(wire) && wire != "auto") return wire;

        try
        {
            var models = await management.GetModelDetailsAsync(
                entry.Endpoint ?? "", entry.ApiKey ?? "lm-studio", ct);
            var llms = models
                .Where(m => !string.Equals(m.Type, "embedding", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // The loaded instance is the one a chat request lands on; a single listed model is
            // unambiguous even when the provider does not report load state at all.
            var chosen = llms.FirstOrDefault(m => m.IsLoaded) ?? (llms.Count == 1 ? llms[0] : null);
            if (chosen != null) return chosen.Id;
        }
        catch { /* provider offline or no management surface — auto stays auto */ }

        return wire;
    }

    /// <summary>The model's label for a file name: the entry id (which connection) plus the wire name
    /// (which model). Both halves are needed — two connections often serve the same model.</summary>
    private static string Label(string entryId, string? wireModel) =>
        string.IsNullOrWhiteSpace(wireModel) || wireModel == "auto"
            ? Sanitize(entryId)
            : $"{Sanitize(entryId)} {Sanitize(wireModel)}";

    /// <summary>Makes a model key safe for a file name. Publisher slashes become dashes rather than
    /// disappearing: <c>google/gemma-4-e4b</c> and <c>unsloth/gemma-4-e4b</c> are different runs.</summary>
    public static string Sanitize(string value)
    {
        var chars = value.Replace('/', '-').Replace('\\', '-').Replace(':', '-').Replace('@', '-');
        foreach (var bad in Path.GetInvalidFileNameChars())
            chars = chars.Replace(bad, '-');
        while (chars.Contains("--")) chars = chars.Replace("--", "-");
        return chars.Trim(' ', '-', '.');
    }
}
