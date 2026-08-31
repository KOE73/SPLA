using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>
/// One project directory under <c>docs/diagrams/projects/</c>, loaded.
/// </summary>
/// <remarks>
/// <para>
/// Six file kinds, of which two are optional in practice: <c>containers.json</c> exists for two of
/// the six live projects, and text catalogs follow whatever <c>project.json</c> declares under
/// <c>languages</c>. An absent optional file is a fact, not a failure — the loader records it so the
/// report can say so plainly instead of the run falling over.
/// </para>
/// <para>
/// <c>views/</c> is not loaded and not modelled. Nothing in this assembly reads a view file and
/// nothing writes one; the ban on touching a person's layout is therefore a property of the code,
/// not a rule someone has to remember.
/// </para>
/// </remarks>
public sealed class DiagramProject
{
    private DiagramProject(
        string directory,
        ProjectManifest manifest,
        EntityCatalog entities,
        RelationCatalog relations,
        RelationTypeCatalog relationTypes,
        ContainerCatalog? containers,
        IReadOnlyDictionary<string, TextCatalog> texts,
        IReadOnlyList<string> missingFiles)
    {
        Directory = directory;
        Manifest = manifest;
        Entities = entities;
        Relations = relations;
        RelationTypes = relationTypes;
        Containers = containers;
        Texts = texts;
        MissingFiles = missingFiles;
    }

    /// <summary>Absolute path of the project directory.</summary>
    public string Directory { get; }

    /// <summary>Directory name, which the contract requires to match <c>project.json</c>'s id.</summary>
    public string DirectoryName => Path.GetFileName(Directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

    public ProjectManifest Manifest { get; }

    public EntityCatalog Entities { get; }

    public RelationCatalog Relations { get; }

    public RelationTypeCatalog RelationTypes { get; }

    /// <summary>Null when the project has no <c>containers.json</c>, which is ordinary.</summary>
    public ContainerCatalog? Containers { get; }

    /// <summary>Text catalogs by language.</summary>
    public IReadOnlyDictionary<string, TextCatalog> Texts { get; }

    /// <summary>Optional files the directory does not have, by file name.</summary>
    public IReadOnlyList<string> MissingFiles { get; }

    /// <summary>
    /// Whether the manifest names source roots. False for five of the six live projects.
    /// </summary>
    /// <remarks>
    /// A project without them cannot be reconciled against code, and a sync run has to say that in
    /// so many words rather than fail or reconcile against an assumed root — either would report
    /// every entity in the project as missing from a tree it never claimed to describe.
    /// </remarks>
    public bool CanSyncWithCode => Manifest.HasSourceIncludes;

    /// <summary>Loads every file of a project directory.</summary>
    /// <exception cref="JsonModelException">
    /// A required file is absent or unreadable. Only the manifest and the three registries are
    /// required; <c>containers.json</c> and the text catalogs are not.
    /// </exception>
    public static DiagramProject Load(string directory)
    {
        if (!System.IO.Directory.Exists(directory))
        {
            throw new JsonModelException($"{directory}: no such project directory.");
        }

        var missing = new List<string>();

        var manifest = ProjectManifest.Read(Require(directory, ProjectPaths.Manifest));
        var entities = EntityCatalog.Read(Require(directory, ProjectPaths.Entities));
        var relations = RelationCatalog.Read(Require(directory, ProjectPaths.Relations));
        var relationTypes = RelationTypeCatalog.Read(Require(directory, ProjectPaths.RelationTypes));

        var containersPath = Path.Combine(directory, ProjectPaths.Containers);
        ContainerCatalog? containers = null;
        if (File.Exists(containersPath))
        {
            containers = ContainerCatalog.Read(containersPath);
        }
        else
        {
            missing.Add(ProjectPaths.Containers);
        }

        var texts = new Dictionary<string, TextCatalog>(StringComparer.Ordinal);
        foreach (var language in DeclaredLanguages(manifest, directory))
        {
            var path = Path.Combine(directory, ProjectPaths.TextCatalog(language));
            if (File.Exists(path))
            {
                texts[language] = TextCatalog.Read(path);
            }
            else
            {
                missing.Add(ProjectPaths.TextCatalog(language));
            }
        }

        return new DiagramProject(directory, manifest, entities, relations, relationTypes, containers, texts, missing);
    }

    /// <summary>Enumerates the project directories under a <c>projects/</c> root.</summary>
    public static IReadOnlyList<string> Discover(string projectsRoot) =>
        System.IO.Directory.Exists(projectsRoot)
            ? [.. System.IO.Directory
                .EnumerateDirectories(projectsRoot)
                .Where(d => File.Exists(Path.Combine(d, ProjectPaths.Manifest)))
                .OrderBy(d => d, StringComparer.Ordinal)]
            : [];

    /// <summary>Path of a file within this project.</summary>
    public string PathOf(string fileName) => Path.Combine(Directory, fileName);

    /// <summary>
    /// Writes back the three registries the utility owns, and nothing else.
    /// </summary>
    /// <remarks>
    /// Untouched documents serialise to the bytes they were read from, so a run that found nothing
    /// leaves an empty diff without any special case for it.
    /// </remarks>
    public void SaveRegistries()
    {
        Entities.Write(PathOf(ProjectPaths.Entities));
        Relations.Write(PathOf(ProjectPaths.Relations));
        RelationTypes.Write(PathOf(ProjectPaths.RelationTypes));
    }

    /// <summary>
    /// Languages to load: whatever the manifest declares, or every <c>text.*.json</c> on disk when it
    /// declares none.
    /// </summary>
    private static IReadOnlyList<string> DeclaredLanguages(ProjectManifest manifest, string directory)
    {
        if (manifest.Languages.Count > 0)
        {
            return manifest.Languages;
        }

        return [.. System.IO.Directory
            .EnumerateFiles(directory, "text.*.json")
            .Select(TextCatalog.LanguageFromFileName)
            .Where(l => l.Length > 0)
            .OrderBy(l => l, StringComparer.Ordinal)];
    }

    private static string Require(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        if (!File.Exists(path))
        {
            throw new JsonModelException($"{path}: required file is missing.");
        }

        return path;
    }
}
