using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using SPLA.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Connections in three layers — <c>shared</c>, <c>user</c>, <c>project</c> — the same three secrets
/// already have, so a person configures their keys once instead of once per repository.
///
/// <para>What is proven here: the merge order and that a later layer replaces an id wholesale; that
/// every entry knows which layer it came from (the editor writes a save back to that file, and a role
/// selects by it); that a save routes each entry to its own file, including removing one from the
/// manifest when it moves out; and that a role's <c>connections:</c> selection narrows what its chats
/// can resolve a model against without being able to add anything.</para>
/// </summary>
public sealed class ConnectionScopeTests
{
    private static SplaConnectionSection Conn(string id, string endpoint, params string[] modelIds) => new()
    {
        Id = id,
        Name = id,
        Provider = "lmstudio",
        Endpoint = endpoint,
        Models = modelIds.Select(m => new SplaModelSection { Id = m, Model = m }).ToList()
    };

    private static string TempDir(string tag) =>
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"spla-{tag}-{Guid.NewGuid():N}")).FullName;

    // ── Merge order ──────────────────────────────────────────────────────────

    [Fact]
    public void All_three_layers_are_available_and_each_entry_knows_its_scope()
    {
        var resolved = SettingsResolver.Resolve(
            new SplaDefaults(),
            new SplaProject { Connections = [Conn("repo", "http://repo", "repo-m")] },
            [Conn("corp", "http://corp", "corp-m")],
            [Conn("mine", "http://mine", "mine-m")]);

        Assert.Equal(
            new Dictionary<string, ConnectionScope>
            {
                ["corp"] = ConnectionScope.Shared,
                ["mine"] = ConnectionScope.User,
                ["repo"] = ConnectionScope.Project
            },
            resolved.Connections.ToDictionary(c => c.Id, c => c.Scope));

        // The flat model list a chat resolves against spans every layer.
        Assert.Equal(["corp-m", "mine-m", "repo-m"], resolved.Models.Select(m => m.Entry.Id).OrderBy(x => x));
    }

    [Fact]
    public void A_later_layer_replaces_an_id_wholesale_project_over_user_over_shared()
    {
        var resolved = SettingsResolver.Resolve(
            new SplaDefaults(),
            new SplaProject { Connections = [Conn("box", "http://from-project", "p")] },
            [Conn("box", "http://from-shared", "s")],
            [Conn("box", "http://from-user", "u")]);

        var box = Assert.Single(resolved.Connections);
        Assert.Equal("http://from-project", box.Endpoint);
        Assert.Equal(ConnectionScope.Project, box.Scope);
        // Wholesale, not field-by-field: the shadowed layers' models do not leak through.
        Assert.Equal(["p"], resolved.Models.Select(m => m.Entry.Id));
    }

    [Fact]
    public void The_user_file_wins_over_the_legacy_connections_block_in_defaults_yaml()
    {
        // defaults.yaml is where user-level connections used to live. It keeps working, at User
        // standing, and connections.yaml — the file the editor writes — is the newer answer.
        var resolved = SettingsResolver.Resolve(
            new SplaDefaults { Connections = [Conn("box", "http://from-defaults-yaml", "old")] },
            new SplaProject(),
            null,
            [Conn("box", "http://from-user-file", "new")]);

        var box = Assert.Single(resolved.Connections);
        Assert.Equal("http://from-user-file", box.Endpoint);
        Assert.Equal(ConnectionScope.User, box.Scope);
    }

    // ── The files themselves ─────────────────────────────────────────────────

    [Fact]
    public void A_user_connection_is_present_in_a_project_that_never_declared_one()
    {
        var home = TempDir("conn-home");
        var projectDir = TempDir("conn-proj");
        var manifest = Path.Combine(projectDir, "test.spla");
        File.WriteAllText(manifest, "version: 1\nname: NoConnections\n");

        ConfigLoader.SaveConnectionLayer(
            ConfigLoader.UserConnectionsPath(home), [Conn("mine", "http://mine/v1", "mine-m")]);

        // The home is this flow's own, so the test cannot see (or write to) the real ~/.spla.
        using var scope = MachineLayerScope.Begin(homeDir: home);
        var resolved = ConfigLoader.LoadAndResolve(manifest);

        var mine = Assert.Single(resolved.Connections, c => c.Id == "mine");
        Assert.Equal(ConnectionScope.User, mine.Scope);
        Assert.Equal("http://mine/v1", mine.Endpoint);
        // No synthesized "default": there IS a connection now, it just did not come from the project.
        Assert.DoesNotContain(resolved.Connections, c => c.Id == "default");
    }

    [Fact]
    public void A_missing_layer_file_is_an_empty_layer_not_a_failure()
        => Assert.Empty(ConfigLoader.LoadConnectionLayer(
            Path.Combine(TempDir("conn-empty"), "connections.yaml"), ConnectionScope.User));

    [Fact]
    public void A_saved_layer_round_trips_through_its_file()
    {
        var path = Path.Combine(TempDir("conn-roundtrip"), "connections.yaml");
        ConfigLoader.SaveConnectionLayer(path, [Conn("mine", "http://mine/v1", "a", "b")]);

        var read = Assert.Single(ConfigLoader.LoadConnectionLayer(path, ConnectionScope.User));
        Assert.Equal("mine", read.Id);
        Assert.Equal("http://mine/v1", read.Endpoint);
        Assert.Equal(["a", "b"], read.Models.Select(m => m.Id));
        Assert.Equal(ConnectionScope.User, read.Scope);
        // The scope is the file, never a key inside it — two answers that can disagree is one too many.
        Assert.DoesNotContain("scope", File.ReadAllText(path), StringComparison.OrdinalIgnoreCase);
    }

    // ── Saving: each entry goes back to its own layer ────────────────────────

    [Fact]
    public void Moving_a_connection_from_project_to_user_writes_the_user_file_and_clears_the_manifest()
    {
        var home = TempDir("conn-save-home");
        var projectDir = TempDir("conn-save-proj");
        var manifest = Path.Combine(projectDir, "test.spla");
        File.WriteAllText(manifest,
            "version: 1\n" +
            "name: SaveScopes\n" +
            "connections:\n" +
            "  - id: box\n" +
            "    provider: lmstudio\n" +
            "    endpoint: http://box/v1\n" +
            "    models:\n" +
            "      - id: box-m\n" +
            "        model: box-m\n");

        using var scope = MachineLayerScope.Begin(homeDir: home);
        using var runtime = new AgentRuntime(ConfigLoader.LoadAndResolve(manifest), NullLoggerFactory.Instance);

        var dto = Assert.Single(SettingsOps.GetConnections(runtime).Connections);
        Assert.Equal("project", dto.Scope);

        dto.Scope = "user";
        var result = SettingsOps.SaveConnections(runtime, [dto]);

        Assert.Null(result.Error);
        Assert.Equal("user", Assert.Single(result.Connections).Scope);
        // Written where the person's own connections live…
        var user = Assert.Single(ConfigLoader.LoadConnectionLayer(
            ConfigLoader.UserConnectionsPath(runtime.Settings.PersonalDir), ConnectionScope.User));
        Assert.Equal("box", user.Id);
        Assert.Equal("http://box/v1", user.Endpoint);
        // …and gone from the manifest, which is what makes the move a move rather than a copy.
        Assert.Null(ConfigLoader.LoadProjectRaw(manifest).Connections);
    }

    [Fact]
    public void A_save_with_nothing_to_say_about_a_layer_leaves_its_file_alone()
    {
        var home = TempDir("conn-quiet-home");
        var projectDir = TempDir("conn-quiet-proj");
        var manifest = Path.Combine(projectDir, "test.spla");
        File.WriteAllText(manifest,
            "version: 1\n" +
            "name: QuietSave\n" +
            "connections:\n" +
            "  - id: box\n" +
            "    provider: lmstudio\n" +
            "    endpoint: http://box/v1\n" +
            "    models:\n" +
            "      - id: box-m\n" +
            "        model: box-m\n");

        using var scope = MachineLayerScope.Begin(homeDir: home);
        using var runtime = new AgentRuntime(ConfigLoader.LoadAndResolve(manifest), NullLoggerFactory.Instance);

        SettingsOps.SaveConnections(runtime, SettingsOps.GetConnections(runtime).Connections);

        // Absent and empty are different statements. Writing "no connections here" unasked plants a
        // file in the person's own home that they never made — and, when the home is a machine's real
        // one, a fixture every later reader then resolves against.
        Assert.False(File.Exists(ConfigLoader.UserConnectionsPath(home)));
        Assert.False(File.Exists(ConfigLoader.SharedConnectionsPath(home)));
    }

    // ── Roles: which connections a role may use ──────────────────────────────

    private static ResolvedSettings ThreeLayerBaseline(SplaProject project) =>
        SettingsResolver.Resolve(
            new SplaDefaults(),
            project,
            [Conn("corp", "http://corp", "corp-m")],
            [Conn("mine", "http://mine", "mine-m")]);

    [Fact]
    public void A_role_that_says_nothing_about_connections_keeps_every_one_of_them()
    {
        var project = new SplaProject { Roles = ["open"], Connections = [Conn("repo", "http://repo", "repo-m")] };
        var resolved = SettingsResolver.ResolveForRole(
            ThreeLayerBaseline(project), project, "open", new SplaRoleSection());

        Assert.Equal(["corp", "mine", "repo"], resolved.Connections.Select(c => c.Id).OrderBy(x => x));
    }

    [Fact]
    public void A_role_narrows_to_the_connections_it_names_by_id()
    {
        var project = new SplaProject { Roles = ["narrow"], Connections = [Conn("repo", "http://repo", "repo-m")] };
        var baseline = ThreeLayerBaseline(project);

        var resolved = SettingsResolver.ResolveForRole(
            baseline, project, "narrow", new SplaRoleSection { Connections = ["repo"] });

        Assert.Equal(["repo"], resolved.Connections.Select(c => c.Id));
        // The narrowing reaches the flat list too — otherwise a chat resolves a model the role was
        // narrowed away from and the selection is decorative.
        Assert.Equal(["repo-m"], resolved.Models.Select(m => m.Entry.Id));
        // The project's own baseline is untouched.
        Assert.Equal(3, baseline.Connections.Count);
    }

    [Fact]
    public void A_scope_name_selects_that_whole_layer()
    {
        var project = new SplaProject { Roles = ["personal"], Connections = [Conn("repo", "http://repo", "repo-m")] };
        var resolved = SettingsResolver.ResolveForRole(
            ThreeLayerBaseline(project), project, "personal", new SplaRoleSection { Connections = ["user"] });

        Assert.Equal(["mine"], resolved.Connections.Select(c => c.Id));
    }

    [Fact]
    public void A_role_cannot_add_a_connection_the_project_never_resolved()
    {
        var project = new SplaProject { Roles = ["greedy"], Connections = [Conn("repo", "http://repo", "repo-m")] };

        var ex = Assert.Throws<InvalidOperationException>(() => SettingsResolver.ResolveForRole(
            ThreeLayerBaseline(project), project, "greedy",
            new SplaRoleSection { Connections = ["some-other-account"] }));

        Assert.Contains("some-other-account", ex.Message);
    }

    [Fact]
    public void A_role_whose_model_is_not_under_any_connection_it_may_use_is_refused()
    {
        var project = new SplaProject { Roles = ["stranded"], Connections = [Conn("repo", "http://repo", "repo-m")] };

        var ex = Assert.Throws<InvalidOperationException>(() => SettingsResolver.ResolveForRole(
            ThreeLayerBaseline(project), project, "stranded",
            new SplaRoleSection { Connections = ["repo"], Model = "mine-m" }));

        Assert.Contains("mine-m", ex.Message);
    }
}
