using SplaAtlas.Model;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Tests;

/// <summary>
/// What loading a live project is supposed to tell the caller — including about what it lacks.
/// </summary>
public sealed class ProjectLoadingTests
{
    /// <summary>
    /// Only <c>core</c> declares source roots. The other five must load and say so, not fail.
    /// </summary>
    /// <remarks>
    /// A project with no <c>sources.include</c> cannot be reconciled against code at all. Falling
    /// over would make five of six projects unopenable; assuming a root would report every entity in
    /// them as missing from a tree they never claimed to describe. Both are worse than saying it.
    /// </remarks>
    [Theory]
    [MemberData(nameof(LiveProjects.Names), MemberType = typeof(LiveProjects))]
    public void AProjectWithoutSourceRootsLoadsAndSaysSo(string project)
    {
        var loaded = DiagramProject.Load(LiveProjects.DirectoryOf(project));

        if (project == "core")
        {
            Assert.True(loaded.CanSyncWithCode);
            Assert.Contains("src/core", loaded.Manifest.SourceIncludes);
        }
        else
        {
            Assert.False(loaded.CanSyncWithCode);
            Assert.Empty(loaded.Manifest.SourceIncludes);
        }
    }

    /// <summary>
    /// <c>containers.json</c> exists for two projects. Its absence is ordinary and is reported as a
    /// fact, not raised as an error.
    /// </summary>
    [Theory]
    [MemberData(nameof(LiveProjects.Names), MemberType = typeof(LiveProjects))]
    public void AMissingContainersFileIsAFactNotAFailure(string project)
    {
        var loaded = DiagramProject.Load(LiveProjects.DirectoryOf(project));
        var expected = project is "core" or "spla_system";

        Assert.Equal(expected, loaded.Containers is not null);
        Assert.Equal(!expected, loaded.MissingFiles.Contains(ProjectPaths.Containers));
    }

    [Theory]
    [MemberData(nameof(LiveProjects.Names), MemberType = typeof(LiveProjects))]
    public void EveryProjectDeclaresContractVersionThreeAndMatchesItsDirectory(string project)
    {
        var loaded = DiagramProject.Load(LiveProjects.DirectoryOf(project));

        Assert.Equal(3, loaded.Manifest.ContractVersion);
        Assert.Equal(project, loaded.Manifest.Id);
        Assert.Equal(project, loaded.DirectoryName);
    }

    [Theory]
    [MemberData(nameof(LiveProjects.Names), MemberType = typeof(LiveProjects))]
    public void RussianIsLoadedAndDeclaresItself(string project)
    {
        var loaded = DiagramProject.Load(LiveProjects.DirectoryOf(project));

        Assert.True(loaded.Texts.ContainsKey("ru"));
        Assert.Equal("ru", loaded.Texts["ru"].DeclaredLanguage);
        Assert.NotEmpty(loaded.Texts["ru"].Keys);
    }

    /// <summary>
    /// The two authored entities in <c>spla_system</c> are found and identified as a person's.
    /// </summary>
    /// <remarks>
    /// They are the reason <c>origin</c> exists on an entity: an external API or a database has no
    /// C# to be reconciled against, so extraction must neither overwrite them nor mark them gone.
    /// </remarks>
    [Fact]
    public void TheAuthoredEntitiesAreVisibleAsAuthored()
    {
        var loaded = DiagramProject.Load(LiveProjects.DirectoryOf("spla_system"));
        var authored = loaded.Entities.Entities.Where(e => e.IsAuthored).ToArray();

        Assert.Equal(2, authored.Length);
        Assert.All(authored, e => Assert.Equal(Origin.Authored, e.Origin));
    }

    [Fact]
    public void TheStandHasTheShapeTheBriefDescribes()
    {
        var system = DiagramProject.Load(LiveProjects.DirectoryOf("spla_system"));
        Assert.Equal(69, system.Entities.Entities.Count);
        Assert.Equal(82, system.Relations.Relations.Count);

        var core = DiagramProject.Load(LiveProjects.DirectoryOf("core"));
        Assert.Equal(430, core.Entities.Entities.Count);
        Assert.Equal(286, core.Relations.Relations.Count);

        var full = DiagramProject.Load(LiveProjects.DirectoryOf("full_core"));
        Assert.Equal(1079, full.Entities.Entities.Count);
        Assert.Equal(3058, full.Relations.Relations.Count);
    }

    [Fact]
    public void ADirectoryThatIsNotAProjectIsRefusedClearly()
    {
        var error = Assert.Throws<JsonModelException>(
            () => DiagramProject.Load(Path.Combine(LiveProjects.Root, "no_such_project")));

        Assert.Contains("no such project directory", error.Message, StringComparison.Ordinal);
    }
}
