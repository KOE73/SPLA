using SplaAtlas.Model;

namespace SplaAtlas.Tests;

/// <summary>
/// The acceptance test for the model layer: every file of every live project, read and written back,
/// byte for byte.
/// </summary>
/// <remarks>
/// This is the whole reason the codec is a view over the parsed tree rather than a set of records.
/// One assertion catches four different ways of quietly corrupting a registry — losing a provenance
/// stamp, reordering keys, dropping an optional field, and turning an <c>authored</c> record into a
/// <c>code</c> one — and it catches them on the real files, with their real leftovers, rather than
/// on a fixture written to agree with the parser.
/// </remarks>
public sealed class RoundTripTests
{
    [Theory]
    [MemberData(nameof(LiveProjects.Names), MemberType = typeof(LiveProjects))]
    public void EveryFileOfEveryProjectSurvivesAReadAndWrite(string project)
    {
        var directory = LiveProjects.DirectoryOf(project);
        var checkedFiles = 0;

        foreach (var path in Directory.EnumerateFiles(directory, "*.json").OrderBy(p => p, StringComparer.Ordinal))
        {
            var original = File.ReadAllBytes(path);
            var fileName = Path.GetFileName(path);

            var rewritten = fileName switch
            {
                ProjectPaths.Manifest => ProjectManifest.Read(path).Serialize(),
                ProjectPaths.Entities => EntityCatalog.Read(path).Serialize(),
                ProjectPaths.Relations => RelationCatalog.Read(path).Serialize(),
                ProjectPaths.RelationTypes => RelationTypeCatalog.Read(path).Serialize(),
                ProjectPaths.Containers => ContainerCatalog.Read(path).Serialize(),
                _ when fileName.StartsWith("text.", StringComparison.Ordinal) =>
                    TextCatalog.Read(path).Serialize(),
                _ => null,
            };

            if (rewritten is null)
            {
                continue;
            }

            LiveProjects.AssertSameBytes(original, rewritten, $"{project}/{fileName}");
            checkedFiles++;
        }

        // Guards against the test passing because it silently matched nothing.
        Assert.True(checkedFiles >= 5, $"{project}: expected at least 5 model files, checked {checkedFiles}.");
    }

    /// <summary>
    /// The same round-trip through <see cref="DiagramProject.Load"/>, which is how everything
    /// downstream will open a project.
    /// </summary>
    [Theory]
    [MemberData(nameof(LiveProjects.Names), MemberType = typeof(LiveProjects))]
    public void LoadingAWholeProjectChangesNothing(string project)
    {
        var directory = LiveProjects.DirectoryOf(project);
        var loaded = DiagramProject.Load(directory);

        Check(ProjectPaths.Manifest, loaded.Manifest.Serialize());
        Check(ProjectPaths.Entities, loaded.Entities.Serialize());
        Check(ProjectPaths.Relations, loaded.Relations.Serialize());
        Check(ProjectPaths.RelationTypes, loaded.RelationTypes.Serialize());

        if (loaded.Containers is { } containers)
        {
            Check(ProjectPaths.Containers, containers.Serialize());
        }

        foreach (var (language, catalog) in loaded.Texts)
        {
            Check(ProjectPaths.TextCatalog(language), catalog.Serialize());
        }

        void Check(string fileName, byte[] rewritten) =>
            LiveProjects.AssertSameBytes(
                File.ReadAllBytes(Path.Combine(directory, fileName)), rewritten, $"{project}/{fileName}");
    }

    /// <summary>
    /// The same again, but through the typed API rather than straight from parse to serialise.
    /// </summary>
    /// <remarks>
    /// Reading is supposed to be free of side effects, and the way it stops being free is subtle: a
    /// list view that materialises its array to answer <c>Count</c> writes <c>"members": []</c> into
    /// the five entities that have no such key, and the file changes because somebody asked it a
    /// question. This walks every accessor the model offers, then demands the bytes back unchanged.
    /// </remarks>
    [Theory]
    [MemberData(nameof(LiveProjects.Names), MemberType = typeof(LiveProjects))]
    public void ReadingThroughTheTypedApiHasNoSideEffects(string project)
    {
        var directory = LiveProjects.DirectoryOf(project);
        var loaded = DiagramProject.Load(directory);

        _ = loaded.Manifest.Id;
        _ = loaded.Manifest.Title;
        _ = loaded.Manifest.Subtitle;
        _ = loaded.Manifest.ContractVersion;
        _ = loaded.Manifest.DefaultView;
        _ = loaded.Manifest.DefaultAxis;
        _ = loaded.Manifest.Languages;
        _ = loaded.Manifest.Views;
        _ = loaded.Manifest.Styles;
        _ = loaded.Manifest.SourceIncludes;
        _ = loaded.CanSyncWithCode;

        foreach (var entity in loaded.Entities.Entities)
        {
            _ = entity.Id;
            _ = entity.Name;
            _ = entity.Kind;
            _ = entity.Origin;
            _ = entity.IsAuthored;
            _ = entity.Status;
            _ = entity.Namespace;
            _ = entity.CodeRef;
            _ = entity.FirstSeen;
            _ = entity.Members.Count;

            foreach (var member in entity.Members)
            {
                _ = member.Name;
                _ = member.MemberKind;
                _ = member.Type;
                _ = member.TypeRef;
                _ = member.Value;
                _ = member.Signature;
            }
        }

        foreach (var relation in loaded.Relations.Relations)
        {
            _ = relation.Id;
            _ = relation.From;
            _ = relation.To;
            _ = relation.Type;
            _ = relation.Origin;
            _ = relation.IsAuthored;
            _ = relation.Status;
            _ = relation.LegacyRelationField;
            _ = relation.HasLegacyPoints;
            _ = relation.Evidence.Count;

            foreach (var evidence in relation.Evidence)
            {
                _ = evidence.CodeRef;
                _ = evidence.Symbol;
                _ = evidence.Line;
            }
        }

        foreach (var type in loaded.RelationTypes.RelationTypes)
        {
            _ = type.Id;
            _ = type.Origin;
            _ = type.StyleId;
            _ = type.TextKey;
        }

        if (loaded.Containers is { } containers)
        {
            _ = containers.Overrides;
            foreach (var container in containers.Containers)
            {
                _ = container.Id;
                _ = container.Parent;
                _ = container.HasExplicitNullParent;
                _ = container.Theme;
                _ = container.Axis;
                _ = container.Match?.Names;
                _ = container.Match?.NameRegexes;
                _ = container.Match?.Paths;
                _ = container.Match?.IsEmpty;
            }
        }

        foreach (var (_, catalog) in loaded.Texts)
        {
            _ = catalog.DeclaredLanguage;
            foreach (var key in catalog.Keys)
            {
                var entry = catalog[key];
                Assert.NotNull(entry);
                foreach (var field in entry.FieldNames)
                {
                    var value = entry.Get(field);
                    _ = value?.Value;
                    _ = value?.At;
                    _ = value?.Origin;
                    _ = value?.From;
                    _ = value?.FromHash;
                    _ = value?.HasProvenance;
                }
            }
        }

        Check(ProjectPaths.Manifest, loaded.Manifest.Serialize());
        Check(ProjectPaths.Entities, loaded.Entities.Serialize());
        Check(ProjectPaths.Relations, loaded.Relations.Serialize());
        Check(ProjectPaths.RelationTypes, loaded.RelationTypes.Serialize());
        if (loaded.Containers is { } c)
        {
            Check(ProjectPaths.Containers, c.Serialize());
        }

        foreach (var (language, catalog) in loaded.Texts)
        {
            Check(ProjectPaths.TextCatalog(language), catalog.Serialize());
        }

        void Check(string fileName, byte[] rewritten) =>
            LiveProjects.AssertSameBytes(
                File.ReadAllBytes(Path.Combine(directory, fileName)), rewritten, $"{project}/{fileName}");
    }

    /// <summary>
    /// A written file matches a serialised one, so nothing is lost between the buffer and the disk.
    /// </summary>
    [Fact]
    public void WritingToDiskProducesTheSameBytesAsSerialising()
    {
        var source = Path.Combine(LiveProjects.DirectoryOf("spla_system"), ProjectPaths.Entities);
        var original = File.ReadAllBytes(source);

        var temp = Path.Combine(Path.GetTempPath(), $"spla-atlas-{Guid.NewGuid():N}.json");
        try
        {
            EntityCatalog.Read(source).Write(temp);
            LiveProjects.AssertSameBytes(original, File.ReadAllBytes(temp), "spla_system/entities.json via disk");
        }
        finally
        {
            File.Delete(temp);
        }
    }

    /// <summary>
    /// The suite is pointed at the six real projects, not at whatever happens to be lying around.
    /// </summary>
    [Fact]
    public void TheStandIsTheSixLiveProjects()
    {
        var found = DiagramProject.Discover(LiveProjects.Root)
            .Select(d => Path.GetFileName(d) ?? string.Empty)
            .ToArray();

        Assert.Equal<string>(
            ["core", "features", "full_core", "llm_pipeline", "plugins", "spla_system"],
            found);
    }
}
