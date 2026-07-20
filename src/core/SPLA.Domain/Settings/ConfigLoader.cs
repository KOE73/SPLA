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
    {
        // SPLA_HOME overrides the machine layer (~/.spla) wholesale — defaults.yaml, machine
        // secrets, token-usage, everything. Lets a second instance run against an isolated home
        // (e.g. a plaintext-secrets dev copy alongside the DPAPI-encrypted production one) without
        // sharing or clobbering the real ~/.spla. Empty/whitespace = not set.
        var overrideHome = Environment.GetEnvironmentVariable("SPLA_HOME");
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
    /// Loads a .spla project file.
    /// </summary>
    public static SplaProject LoadProject(string splaFilePath)
    {
        var project = LoadProjectRaw(splaFilePath);

        // Resolve workspace path relative to the .spla file location
        var splaDir = Path.GetDirectoryName(Path.GetFullPath(splaFilePath)) ?? ".";
        var workspace = project.Workspace ?? ".";
        project.Workspace = Path.GetFullPath(Path.Combine(splaDir, workspace));

        return project;
    }

    /// <summary>
    /// Loads a .spla project file without resolving relative paths. Use this before editing and saving it.
    /// </summary>
    public static SplaProject LoadProjectRaw(string splaFilePath)
    {
        var yaml = File.ReadAllText(splaFilePath);
        return Deserializer.Deserialize<SplaProject>(yaml) ?? new SplaProject();
    }

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
        "workspace" => p.Workspace,
        "agent" => p.Agent,
        "llm" => p.Llm,
        "connections" => p.Connections,
        "ui" => p.Ui,
        "permissions" => p.Permissions,
        "plugins" => p.Plugins,
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
        project.Workspace ??= ".";
        project.Ignore = [.. DefaultIgnorePatterns];

        try { SaveProject(project, splaFilePath); }
        catch { /* best-effort */ }
    }

    /// <summary>
    /// Searches for a *.spla file in the given directory.
    /// Returns the first one found, or null.
    /// </summary>
    public static string? FindProjectFile(string directory)
    {
        var files = Directory.GetFiles(directory, "*.spla");
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

        // Global secrets service: project scope under the workspace, machine scope under ~/.spla.
        var workspace = resolved.ProjectFilePath != null
            ? Path.GetDirectoryName(resolved.ProjectFilePath)
            : null;
        var store = ResolveSecretStore(defaults, workspace);
        resolved.Secrets = store;
        resolved.SecretResolver = new Secrets.SecretResolver(store);

        return resolved;
    }

    /// <summary>
    /// Pluggable factory for non-default secret backends. SPLA.Domain must not reference Windows
    /// (DPAPI) or other platform packages, so the app entry point registers a factory here before
    /// the first <see cref="LoadAndResolve"/>. Signature: (backend, workspacePath, machineDir) → store,
    /// or null if this backend cannot be provided (wrong OS, package missing) — the caller then falls
    /// back to the plaintext <see cref="Secrets.FileSecretStore"/>.
    /// </summary>
    public static Func<string, string?, string, Secrets.ISecretStore?>? SecretStoreFactory { get; set; }

    private static bool _secretBackendWarned;

    private static Secrets.ISecretStore ResolveSecretStore(SplaDefaults defaults, string? workspace)
    {
        var machineDir = GetDefaultsDir();
        var backend = (defaults.Secrets?.Backend ?? "file").Trim().ToLowerInvariant();

        if (backend != "file" && SecretStoreFactory != null)
        {
            var store = SecretStoreFactory(backend, workspace, machineDir);
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
