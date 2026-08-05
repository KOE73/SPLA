using SPLA.Domain.Settings;
using SPLA.Library.Sources;

namespace SPLA.Tests;

/// <summary>
/// The half of the fond a person owns. Prescribed entries come from the settings layers and travel
/// with the project; granted ones live in the person's own area and are never committed — which is
/// the whole reason "add a folder" could not be done from the panel before.
/// </summary>
public class SkillGrantedSourceTests : IDisposable
{
    private readonly string _temp = Path.Combine(
        Path.GetTempPath(), "spla_granted_" + Path.GetRandomFileName());

    public SkillGrantedSourceTests() => Directory.CreateDirectory(_temp);

    public void Dispose()
    {
        if (Directory.Exists(_temp)) Directory.Delete(_temp, recursive: true);
    }

    private FileSkillSourceStore Store() => new(_temp);

    private static SplaSkillSourceSection Entry(string id, string? path = null, bool? enabled = null) =>
        new() { Id = id, Type = "directory", Path = path ?? id, Enabled = enabled };

    [Fact]
    public void An_absent_store_is_an_empty_list_not_an_error()
    {
        Assert.Empty(Store().Load());
    }

    [Fact]
    public void Entries_round_trip_and_come_back_stamped_as_granted()
    {
        var store = Store();
        store.Save([Entry("ops", @"D:\shared\ops-skills")]);

        var loaded = store.Load().Single();

        Assert.Equal("ops", loaded.Id);
        Assert.Equal(@"D:\shared\ops-skills", loaded.Path);
        // Stamped on the way in, never read from the file — the same rule as every other layer.
        Assert.Equal(SourceOrigin.Granted, loaded.Origin);
    }

    [Fact]
    public void An_empty_save_removes_the_file_rather_than_leaving_a_stub()
    {
        var store = Store();
        store.Save([Entry("ops")]);
        Assert.True(File.Exists(store.FilePath));

        store.Save([]);
        Assert.False(File.Exists(store.FilePath));
    }

    [Fact]
    public void A_corrupt_personal_file_does_not_take_the_fond_down()
    {
        var store = Store();
        File.WriteAllText(store.FilePath, "sources: [ this is not: valid yaml ][");

        Assert.Empty(store.Load());
    }

    [Fact]
    public void Granted_entries_come_last_so_they_override_prescribed_ones()
    {
        var settings = SettingsResolver.Resolve(
            new SplaDefaults(),
            new SplaProject { Skills = new SplaSkillsSection { Sources = [Entry("ops", "from-project")] } });

        var store = Store();
        store.Save([Entry("ops", "from-me")]);
        settings.SkillSourceStore = store;

        var effective = settings.EffectiveSkillSources();

        Assert.Equal(["from-project", "from-me"], effective.Select(s => s.Path));
        Assert.Equal([SourceOrigin.Project, SourceOrigin.Granted], effective.Select(s => s.Origin));

        // Folded: one branch, pointed where the person said, standing as theirs rather than the
        // project's — which is also what lifts it past the project ceiling.
        var built = SkillSourceRegistry.Build(effective, new SkillSourceContext(_temp, _temp, null),
            inheritDefaults: false).Single();
        Assert.Equal("ops", built.Id);
        Assert.Equal(SkillTrust.Trusted, built.Trust);
    }

    [Fact]
    public void The_panel_switches_off_an_inherited_branch_without_touching_the_project_file()
    {
        var project = new SplaProject { Skills = new SplaSkillsSection { Sources = [Entry("ops", "from-project")] } };
        var settings = SettingsResolver.Resolve(new SplaDefaults(), project);

        var store = Store();
        store.Save([new SplaSkillSourceSection { Id = "ops", Enabled = false }]);
        settings.SkillSourceStore = store;

        var built = SkillSourceRegistry.Build(
            settings.EffectiveSkillSources(), new SkillSourceContext(_temp, _temp, null),
            inheritDefaults: false);

        Assert.Empty(built);
        // The prescribed entry is untouched — the override lives in the person's own store, so a
        // private decision never arrives in the repository for everybody else.
        Assert.Single(project.Skills!.Sources!);
        Assert.Null(project.Skills.Sources![0].Enabled);
    }
}
