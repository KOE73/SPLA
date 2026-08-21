using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Settings;

/// <summary>
/// Loads and saves YAML configuration files (defaults.yaml, *.spla).
/// </summary>
public static class ConfigLoader
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    /// <summary>
    /// Returns the global defaults directory (~/.spla/).
    /// </summary>
    public static string GetDefaultsDir()
        // A scope, when one is open, answers for this flow alone; otherwise the process-wide
        // SPLA_HOME does, exactly as before. Narrowest answer first — see MachineLayerScope.
        => ResolveDefaultsDir(
            MachineLayerScope.Current?.HomeDir ?? Environment.GetEnvironmentVariable("SPLA_HOME"));

    /// <summary>
    /// The home directory for a given override, or the machine's own when there is none. Pure, and
    /// separated out so the override rule can be proved without moving a process-wide setting to do
    /// it — moving one is what makes a test visible to every test running beside it.
    /// </summary>
    /// <param name="overrideHome">A home to use instead of <c>~/.spla</c>. Empty/whitespace = not set.</param>
    public static string ResolveDefaultsDir(string? overrideHome)
    {
        // The override replaces the machine layer (~/.spla) wholesale — defaults.yaml, machine
        // secrets, token-usage, everything. Lets a second instance run against an isolated home
        // (e.g. a plaintext-secrets dev copy alongside the DPAPI-encrypted production one) without
        // sharing or clobbering the real ~/.spla.
        if (!string.IsNullOrWhiteSpace(overrideHome))
            return Path.GetFullPath(overrideHome.Trim());

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".spla");
    }

    /// <summary>
    /// Returns the full path to defaults.yaml.
    /// </summary>
    public static string GetDefaultsPath() => Path.Combine(GetDefaultsDir(), "defaults.yaml");

    /// <summary>
    /// Loads defaults.yaml. Creates it with hardcoded defaults if it doesn't exist.
    /// </summary>
    public static SplaDefaults LoadDefaults()
    {
        var path = GetDefaultsPath();
        if (File.Exists(path))
        {
            var yaml = File.ReadAllText(path);
            return Deserializer.Deserialize<SplaDefaults>(yaml) ?? new SplaDefaults();
        }

        // Create defaults
        var defaults = new SplaDefaults
        {
            Version = 1,
            Llm = new SplaLlmSection
            {
                Provider = "lmstudio",
                Endpoint = "http://127.0.0.1:1234/v1/",
                ApiKey = "lm-studio",
                Model = "auto",
                Temperature = 0.7
            },
            Agent = new SplaAgentSection { Mode = "Research" },
            Ui = new SplaUiSection { Theme = "Dark" }
        };

        SaveDefaults(defaults);
        return defaults;
    }

    /// <summary>
    /// Saves defaults.yaml.
    /// </summary>
    public static void SaveDefaults(SplaDefaults defaults)
    {
        var dir = GetDefaultsDir();
        Directory.CreateDirectory(dir);
        TryHideDirectory(dir);
        var yaml = Serializer.Serialize(defaults);
        File.WriteAllText(GetDefaultsPath(), yaml);
    }

    /// <summary>
    /// Attempts to set the Hidden attribute on a directory (primarily for Windows Explorer).
    /// </summary>
    public static void TryHideDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                var di = new DirectoryInfo(path);
                if ((di.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    di.Attributes |= FileAttributes.Hidden;
                }
            }
        }
        catch { /* Best effort */ }
    }

    /// <summary>
    /// Returns the full path to recent_projects.txt.
    /// </summary>
    public static string GetRecentProjectsPath() => Path.Combine(GetDefaultsDir(), "recent_projects.txt");

    /// <summary>
    /// Loads the list of recent project file paths.
    /// </summary>
    public static List<string> LoadRecentProjects()
    {
        var path = GetRecentProjectsPath();
        if (!File.Exists(path)) return new List<string>();

        try
        {
            return File.ReadAllLines(path)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line) && File.Exists(line))
                .Distinct()
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Adds a project path to the recent list.
    /// </summary>
    public static void AddRecentProject(string projectFilePath)
    {
        if (string.IsNullOrEmpty(projectFilePath)) return;
        var fullPath = Path.GetFullPath(projectFilePath);
        
        var recent = LoadRecentProjects();
        recent.RemoveAll(x => string.Equals(x, fullPath, StringComparison.OrdinalIgnoreCase));
        recent.Insert(0, fullPath);

        if (recent.Count > 10)
        {
            recent = recent.Take(10).ToList();
        }

        try
        {
            var dir = GetDefaultsDir();
            Directory.CreateDirectory(dir);
            TryHideDirectory(dir);
            File.WriteAllLines(GetRecentProjectsPath(), recent);
        }
        catch { }
    }

    /// <summary>
    /// Drops a project from the recent list. Returns false when it was not there.
    ///
    /// <para>Forgetting is the only way an entry ever leaves other than falling off the end, and it
    /// touches nothing but this list — the project, its manifest and its whole workspace stay exactly
    /// where they are. That distinction has to survive into the UI wording too: a manager offering
    /// "remove" next to a list of projects must never read as "delete".</para>
    /// </summary>
    public static bool RemoveRecentProject(string projectFilePath)
    {
        if (string.IsNullOrEmpty(projectFilePath)) return false;

        var recent = LoadRecentProjects();
        // Compared unrooted as well: an entry can predate the switch to storing full paths, and the
        // person trying to forget it should not have to know that.
        var removed = recent.RemoveAll(x =>
            string.Equals(x, projectFilePath, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Path.GetFullPath(x), Path.GetFullPath(projectFilePath), StringComparison.OrdinalIgnoreCase));
        if (removed == 0) return false;

        try
        {
            var dir = GetDefaultsDir();
            Directory.CreateDirectory(dir);
            TryHideDirectory(dir);
            File.WriteAllLines(GetRecentProjectsPath(), recent);
            return true;
        }
        catch { return false; }
    }

    /// <summary>
    /// Loads a .spla project file.
    /// </summary>
    public static SplaProject LoadProject(string splaFilePath) => LoadProjectRaw(splaFilePath);

    /// <summary>
    /// Loads a .spla project file without resolving relative paths. Use this before editing and saving it.
    /// </summary>
    public static SplaProject LoadProjectRaw(string splaFilePath)
        => ParseProjectYaml(File.ReadAllText(splaFilePath));

    /// <summary>Deserializes .spla content that is already in memory — same shape and conventions as
    /// <see cref="LoadProjectRaw"/>, without touching the filesystem.</summary>
    public static SplaProject ParseProjectYaml(string yaml)
        => Deserializer.Deserialize<SplaProject>(yaml) ?? new SplaProject();

    /// <summary>
    /// Serializes an opaque plugin settings blob (nested mapping) to a YAML string.
    /// Used to hand a plugin its own settings across the assembly-load-context boundary.
    /// </summary>
    public static string? SerializeBlob(Dictionary<string, object>? blob) =>
        blob is null || blob.Count == 0 ? null : Serializer.Serialize(blob);

    /// <summary>
    /// Parses a YAML string produced by a plugin back into an opaque nested mapping for storage.
    /// </summary>
    public static Dictionary<string, object>? DeserializeBlob(string? yaml) =>
        string.IsNullOrWhiteSpace(yaml) ? null : Deserializer.Deserialize<Dictionary<string, object>>(yaml);

    // ── JSON blob transport ───────────────────────────────────────────────────
    // Web settings panels exchange the opaque plugin blob as JSON (native in the browser — no YAML
    // library shipped in every plugin bundle); the host converts to/from the YAML-backed mapping
    // here, in one place.

    /// <summary>Serializes an opaque plugin settings blob to a JSON string for the web client.</summary>
    public static string? BlobToJson(Dictionary<string, object>? blob) =>
        blob is null || blob.Count == 0 ? null : ToJsonNode(blob)!.ToJsonString();

    /// <summary>Parses a JSON string from a web settings panel back into the opaque nested mapping.</summary>
    public static Dictionary<string, object>? BlobFromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        var node = JsonNode.Parse(json);
        return FromJsonNode(node) as Dictionary<string, object>;
    }

    private static JsonNode? ToJsonNode(object? value) => value switch
    {
        null => null,
        string s => JsonValue.Create(s),
        bool b => JsonValue.Create(b),
        int i => JsonValue.Create(i),
        long l => JsonValue.Create(l),
        double d => JsonValue.Create(d),
        // YAML deserialization yields object-keyed mappings for nested levels.
        System.Collections.IDictionary map => new JsonObject(map.Keys.Cast<object>()
            .Select(k => new KeyValuePair<string, JsonNode?>(k.ToString()!, ToJsonNode(map[k])))),
        System.Collections.IEnumerable seq => new JsonArray(seq.Cast<object?>().Select(ToJsonNode).ToArray()),
        _ => JsonValue.Create(value.ToString())
    };

    private static object? FromJsonNode(JsonNode? node) => node switch
    {
        null => null,
        JsonObject obj => obj.ToDictionary(kv => kv.Key, kv => FromJsonNode(kv.Value)!),
        JsonArray arr => arr.Select(FromJsonNode).ToList(),
        JsonValue v => v.GetValueKind() switch
        {
            JsonValueKind.String => v.GetValue<string>(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => v.TryGetValue<long>(out var l) ? l : v.GetValue<double>(),
            _ => v.ToJsonString()
        },
        _ => node.ToJsonString()
    };

    /// <summary>
    /// Saves a .spla project file by rewriting it wholesale. Comments and formatting are lost —
    /// use only for brand-new files (scaffolding); targeted edits go through
    /// <see cref="SaveProjectSections"/>, which preserves the rest of the file.
    /// </summary>
    public static void SaveProject(SplaProject project, string splaFilePath)
    {
        var yaml = Serializer.Serialize(project);
        File.WriteAllText(splaFilePath, yaml);
    }

    /// <summary>
    /// Persists only the named top-level sections of a .spla file (e.g. "ui", "connections"),
    /// splicing them into the existing text so hand-written comments and formatting elsewhere in
    /// the file survive. A null section property removes the key. Falls back to a full
    /// <see cref="SaveProject"/> when the file is missing or its YAML defeats the splicer.
    /// </summary>
    public static void SaveProjectSections(SplaProject project, string splaFilePath, params string[] sectionKeys)
    {
        try
        {
            var text = File.ReadAllText(splaFilePath);
            foreach (var key in sectionKeys)
            {
                var value = GetSectionValue(project, key);
                var sectionText = value == null
                    ? null
                    : Serializer.Serialize(new Dictionary<string, object> { [key] = value });
                text = YamlSectionSplicer.ReplaceSection(text, key, sectionText);
            }
            File.WriteAllText(splaFilePath, text);
        }
        catch (Exception ex) when (ex is IOException or YamlDotNet.Core.YamlException)
        {
            SaveProject(project, splaFilePath);
        }
    }

    private static object? GetSectionValue(SplaProject p, string key) => key switch
    {
        "name" => p.Name,
        "mounts" => p.Mounts,
        "agent" => p.Agent,
        "llm" => p.Llm,
        "connections" => p.Connections,
        "ui" => p.Ui,
        "mcp" => p.Mcp,
        "permissions" => p.Permissions,
        "plugins" => p.Plugins,
        "toolsets" => p.ToolSets,
        "resources" => p.Resources,
        "skills" => p.Skills,
        "docs" => p.Docs,
        "ignore" => p.Ignore,
        _ => throw new ArgumentException($"unknown .spla section '{key}'", nameof(key))
    };

    /// <summary>
    /// Standard ignore patterns written into every new project file.
    /// </summary>
    public static readonly IReadOnlyList<string> DefaultIgnorePatterns =
    [
        "bin/", "obj/", ".git/", ".svn/",
        "node_modules/", ".vs/", ".idea/",
        ".spla/", "*.user", "*.suo"
    ];

    /// <summary>
    /// If the project file looks like a freshly-created empty file (no name, no ignore list),
    /// fills in sensible scaffolding: project name from the filename and standard ignore patterns.
    /// Writes the result back to disk so subsequent opens already have the full config.
    /// No-op when the file already has a name or an explicit ignore list.
    /// </summary>
    public static void ScaffoldIfNew(string splaFilePath)
    {
        SplaProject project;
        try { project = LoadProjectRaw(splaFilePath); }
        catch { return; }

        // Only scaffold genuinely empty / newly-created files.
        if (project.Name != null || (project.Ignore != null && project.Ignore.Count > 0))
            return;

        project.Name = Path.GetFileNameWithoutExtension(splaFilePath);
        project.Ignore = [.. DefaultIgnorePatterns];

        try { SaveProject(project, splaFilePath); }
        catch { /* best-effort */ }
    }

    /// <summary>
    /// The project manifest in <paramref name="directory"/>, or null when there is none.
    ///
    /// <para>Only this directory is searched. There is deliberately no walk up the tree: a command
    /// acts where it was started, and climbing would silently pick a root nobody named — the
    /// difference between "no project here" and "you are inside someone else's project" is exactly
    /// what the person needs to be told, not have guessed for them.</para>
    ///
    /// <para>Two manifests in one directory is an error, not a coin toss. This used to return
    /// <c>files[0]</c>, whose value depended on filesystem enumeration order.</para>
    /// </summary>
    /// <exception cref="InvalidOperationException">More than one <c>*.spla</c> in the directory.</exception>
    public static string? FindProjectFile(string directory)
    {
        var files = Directory.GetFiles(directory, "*.spla");
        if (files.Length > 1)
        {
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            throw new InvalidOperationException(
                $"{directory} holds {files.Length} project files ({string.Join(", ", files.Select(Path.GetFileName))}). " +
                "Name the one you mean on the command line.");
        }
        return files.Length > 0 ? files[0] : null;
    }

    /// <summary>
    /// Full resolve: load defaults + optional project → ResolvedSettings.
    /// </summary>
    public static ResolvedSettings LoadAndResolve(string? splaFilePath = null)
    {
        var defaults = LoadDefaults();
        SplaProject? project = null;

        if (splaFilePath != null && File.Exists(splaFilePath))
        {
            project = LoadProject(splaFilePath);
        }

        var resolved = SettingsResolver.Resolve(defaults, project);
        if (splaFilePath != null && File.Exists(splaFilePath))
            resolved.ProjectFilePath = Path.GetFullPath(splaFilePath);

        // The root, decided in exactly one place: the directory the manifest was found in. Absolute
        // from here on — it used to stay whatever the manifest said (usually "."), which only ever
        // worked because startup chdir'd into it, so anything reading it before that got a path
        // relative to wherever the process happened to start.
        // No manifest ⇒ no project ⇒ no root: the current directory is where we were launched, not a
        // boundary, and callers must consult HasProject before treating it as one.
        var workspace = resolved.ProjectFilePath != null
            ? Path.GetDirectoryName(resolved.ProjectFilePath)
            : null;
        resolved.WorkspacePath = workspace ?? Directory.GetCurrentDirectory();

        // Mounts need the root, so they are resolved here rather than in SettingsResolver — and only
        // when there is a project: with no manifest there is nothing to declare them in and no root to
        // reserve a name in. Refusals throw; a manifest that cannot be honoured must not open half-way.
        if (resolved.ProjectFilePath is { } manifestPath)
            resolved.Mounts = MountResolver.Resolve(project?.Mounts, resolved.WorkspacePath, manifestPath);

        var store = ResolveSecretStore(defaults, workspace);
        resolved.Secrets = store;
        resolved.SecretResolver = new Secrets.SecretResolver(store);

        // Where this person's own state lives. Locally the machine home; on a server, their own area,
        // so two users of one server neither share a fond nor inherit each other's approvals.
        // A deployment that resolves personal directories is one with more than one person in it, and
        // that single fact drives both consequences: whose folders these are, and whether they get to
        // call their own folders vetted.
        var personal = PersonalDirResolver?.Invoke(workspace);
        resolved.IsMultiUserDeployment = personal is not null;
        resolved.PersonalDir = personal ?? GetDefaultsDir();

        // The branches this person added themselves. Same area as their secrets and for the same
        // reason: it is theirs, it is never committed, and the UI has to be able to write it.
        resolved.SkillSourceStore = new FileSkillSourceStore(resolved.PersonalDir);
        // Grants live beside the list, never inside it — the same rule as secrets and their ACL.
        resolved.SkillTrustStore = new FileSkillTrustStore(resolved.PersonalDir);

        return resolved;
    }

    /// <summary>
    /// Maps a workspace to the directory holding that person's own state, or null to use the machine
    /// home. Registered by a deployment that has more than one person in it — the server points it at
    /// <c>{root}/users/{userKey}</c>.
    ///
    /// <para>A hook rather than a parameter for the same reason <see cref="SecretStoreFactory"/> is
    /// one: SPLA.Domain must not learn what a server root is, and every caller of
    /// <see cref="LoadAndResolve"/> would otherwise have to thread through something only one
    /// deployment has.</para>
    /// </summary>
    public static Func<string?, string?>? PersonalDirResolver { get; set; }

    /// <summary>
    /// Pluggable factory for non-default secret backends. SPLA.Domain must not reference Windows
    /// (DPAPI) or other platform packages, so the app entry point registers a factory here before
    /// the first <see cref="LoadAndResolve"/>. Signature: (backend, workspacePath, machineDir) → store,
    /// or null if this backend cannot be provided (wrong OS, package missing) — the caller then falls
    /// back to the plaintext <see cref="Secrets.FileSecretStore"/>.
    /// </summary>
    public static Func<string, string?, string, Secrets.ISecretStore?>? SecretStoreFactory { get; set; }

    /// <summary>The factory to use on this flow: a scope's answer when one is open, otherwise the
    /// registered one. A scope may also say "none", which is a different statement from having no
    /// opinion — see <see cref="MachineLayerScope"/>.</summary>
    private static Func<string, string?, string, Secrets.ISecretStore?>? EffectiveSecretStoreFactory()
    {
        if (MachineLayerScope.Current is not { } scope) return SecretStoreFactory;
        if (scope.SuppressSecretStoreFactory) return null;
        return scope.SecretStoreFactory ?? SecretStoreFactory;
    }

    private static bool _secretBackendWarned;

    private static Secrets.ISecretStore ResolveSecretStore(SplaDefaults defaults, string? workspace)
    {
        var machineDir = GetDefaultsDir();
        var backend = (defaults.Secrets?.Backend ?? "file").Trim().ToLowerInvariant();
        var factory = EffectiveSecretStoreFactory();

        if (backend != "file" && factory != null)
        {
            var store = factory(backend, workspace, machineDir);
            if (store != null) return store;
        }

        if (backend != "file" && !_secretBackendWarned)
        {
            _secretBackendWarned = true;
            Console.WriteLine($"[secrets] backend '{backend}' unavailable (no factory / unsupported platform); using plaintext file store.");
        }

        return new Secrets.FileSecretStore(workspace, machineDir);
    }
}
