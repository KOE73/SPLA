using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using SPLA.Service;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The role editor's read/write half (<c>roles.get</c> / <c>roles.save</c>).
///
/// <para>What is proven here is the one thing a role editor can get wrong in a way nothing else
/// catches: a role has TWO halves in two places — a body in <c>roles/&lt;name&gt;.yaml</c> and a name
/// in the manifest's <c>roles:</c> list — and only the second one makes it act
/// (<c>SettingsResolver.ResolveForRole</c> refuses a name the manifest never listed). So the editor
/// must show a body nobody named, must be able to name it, and must keep the two halves in step on
/// every save, including the live manifest that <c>role_list</c> reads.</para>
/// </summary>
public sealed class RolesEditorTests
{
    private static string TempDir(string tag) =>
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"spla-{tag}-{Guid.NewGuid():N}")).FullName;

    /// <summary>A project with a manifest naming <paramref name="declared"/>, and a body file for each
    /// name in <paramref name="bodies"/> — the two halves, set independently on purpose.</summary>
    private static string Project(string tag, string[] declared, params (string Name, string Yaml)[] bodies)
    {
        var dir = TempDir(tag);
        var manifest = Path.Combine(dir, "test.spla");
        File.WriteAllText(manifest,
            "version: 1\nname: RolesTest\n" +
            (declared.Length > 0 ? "roles:\n" + string.Concat(declared.Select(n => $"  - {n}\n")) : ""));

        var rolesDir = Directory.CreateDirectory(Path.Combine(dir, "roles")).FullName;
        foreach (var (name, yaml) in bodies)
            File.WriteAllText(Path.Combine(rolesDir, name + ".yaml"), yaml);
        return manifest;
    }

    private static AgentRuntime Open(string manifest)
        => new(ConfigLoader.LoadAndResolve(manifest), NullLoggerFactory.Instance);

    // ── Reading: both halves, and the difference between them ────────────────

    [Fact]
    public void A_body_nobody_named_is_listed_and_marked_inactive()
    {
        var manifest = Project("roles-read",
            ["architect"],
            ("architect", "description: Designs first.\nmode: Edit\n"),
            ("stray", "description: A body nobody named.\n"));

        using var runtime = Open(manifest);
        var payload = SettingsOps.GetRoles(runtime);

        Assert.Equal(["architect", "stray"], payload.Roles.Select(r => r.Name));
        Assert.True(payload.Roles.Single(r => r.Name == "architect").Active);
        // The whole point: the file exists, is editable, and does not act.
        Assert.False(payload.Roles.Single(r => r.Name == "stray").Active);
    }

    [Fact]
    public void A_name_with_no_body_is_still_listed_so_the_break_is_visible()
    {
        var manifest = Project("roles-missing-body", ["ghost"]);

        using var runtime = Open(manifest);
        var role = Assert.Single(SettingsOps.GetRoles(runtime).Roles);

        Assert.Equal("ghost", role.Name);
        Assert.True(role.Active);
        Assert.Null(role.Mode);
    }

    [Fact]
    public void Unset_fields_come_back_null_rather_than_as_the_projects_values()
    {
        // "Inherit" is the meaning of an absent key (ResolveForRole falls back to the baseline), so the
        // editor must be handed nothing — filling the blanks in with the project's numbers would turn
        // the next save into a role that pins every one of them.
        var manifest = Project("roles-inherit", ["plain"], ("plain", "description: Says nothing else.\n"));

        using var runtime = Open(manifest);
        var role = Assert.Single(SettingsOps.GetRoles(runtime).Roles);

        Assert.Null(role.Mode);
        Assert.Null(role.LoopGuard);
        Assert.Null(role.ShellTimeoutSeconds);
        Assert.Null(role.Capabilities);
        Assert.Null(role.Connections);
    }

    // ── Saving: the two halves stay in step ──────────────────────────────────

    [Fact]
    public void Activating_a_role_writes_the_manifest_and_the_live_one_together()
    {
        var manifest = Project("roles-activate", [], ("stray", "description: Not yet named.\n"));

        using var runtime = Open(manifest);
        var payload = SettingsOps.GetRoles(runtime);
        payload.Roles.Single().Active = true;

        var result = SettingsOps.SaveRoles(runtime, payload.Roles);

        Assert.Null(result.Error);
        Assert.Equal(["stray"], ConfigLoader.LoadProjectRaw(manifest).Roles);
        // The live manifest is what role_list and ResolveForRole consult; leaving it stale would keep a
        // freshly declared role refusing to resolve until the next start.
        Assert.Equal(["stray"], runtime.Settings.Manifest!.Roles);
    }

    [Fact]
    public void Renaming_a_role_moves_its_file_instead_of_leaving_two()
    {
        var manifest = Project("roles-rename", ["stray"], ("stray", "description: A body nobody named.\n"));
        var dir = Path.GetDirectoryName(manifest)!;

        using var runtime = Open(manifest);
        var payload = SettingsOps.GetRoles(runtime);
        payload.Roles.Single().Name = "reviewer";

        SettingsOps.SaveRoles(runtime, payload.Roles);

        Assert.Equal(["reviewer"], ConfigLoader.ListRoleFiles(dir));
        Assert.Equal(["reviewer"], ConfigLoader.LoadProjectRaw(manifest).Roles);
        Assert.Equal("A body nobody named.", ConfigLoader.LoadRole(dir, "reviewer")!.Description);
    }

    [Fact]
    public void A_role_dropped_from_the_list_loses_its_file()
    {
        var manifest = Project("roles-delete",
            ["a", "b"], ("a", "description: Stays.\n"), ("b", "description: Goes.\n"));
        var dir = Path.GetDirectoryName(manifest)!;

        using var runtime = Open(manifest);
        var payload = SettingsOps.GetRoles(runtime);

        SettingsOps.SaveRoles(runtime, payload.Roles.Where(r => r.Name == "a").ToList());

        Assert.Equal(["a"], ConfigLoader.ListRoleFiles(dir));
        Assert.Equal(["a"], ConfigLoader.LoadProjectRaw(manifest).Roles);
    }

    [Fact]
    public void A_name_that_would_write_outside_the_roles_directory_is_refused_whole()
    {
        var manifest = Project("roles-badname", ["good"], ("good", "description: Fine.\n"));
        var dir = Path.GetDirectoryName(manifest)!;

        using var runtime = Open(manifest);
        var payload = SettingsOps.GetRoles(runtime);
        payload.Roles.Add(new SPLA.Service.Contracts.RoleEditDto { Name = "../escape", Active = true });

        var result = SettingsOps.SaveRoles(runtime, payload.Roles);

        Assert.NotNull(result.Error);
        // Refused whole: the good role is not half-saved beside the bad one.
        Assert.Equal(["good"], ConfigLoader.ListRoleFiles(dir));
        Assert.False(File.Exists(Path.Combine(dir, "escape.yaml")));
    }

    [Fact]
    public void A_round_trip_through_the_editor_changes_nothing_it_was_not_asked_to_change()
    {
        var manifest = Project("roles-roundtrip", ["architect"],
            ("architect",
             "description: Designs first.\n" +
             "mode: Edit\n" +
             "custom_prompt: You think in interfaces.\n" +
             "capabilities:\n  - core.files\n" +
             "connections:\n  - user\n" +
             "toolsets:\n  sql: agent_demand\n" +
             "peer_depth_ceiling: 3\n"));
        var dir = Path.GetDirectoryName(manifest)!;

        using var runtime = Open(manifest);
        SettingsOps.SaveRoles(runtime, SettingsOps.GetRoles(runtime).Roles);

        var role = ConfigLoader.LoadRole(dir, "architect")!;
        Assert.Equal("Designs first.", role.Description);
        Assert.Equal("Edit", role.Mode);
        Assert.Equal("You think in interfaces.", role.CustomPrompt);
        Assert.Equal(["core.files"], role.Capabilities);
        Assert.Equal(["user"], role.Connections);
        Assert.Equal("agent_demand", role.ToolSets!["sql"]);
        Assert.Equal(3, role.PeerDepthCeiling);
    }

    [Fact]
    public void An_empty_capability_list_is_kept_apart_from_no_list_at_all()
    {
        // Null = inherit the project's capabilities; empty = this role deliberately has none. Collapsing
        // the two would silently hand a role everything the project has the moment someone unticks the
        // last box.
        var manifest = Project("roles-empty-caps", ["narrow"], ("narrow", "description: Nothing.\n"));
        var dir = Path.GetDirectoryName(manifest)!;

        using var runtime = Open(manifest);
        var payload = SettingsOps.GetRoles(runtime);
        payload.Roles.Single().Capabilities = [];

        SettingsOps.SaveRoles(runtime, payload.Roles);

        Assert.Empty(ConfigLoader.LoadRole(dir, "narrow")!.Capabilities!);
        Assert.NotNull(SettingsOps.GetRoles(runtime).Roles.Single().Capabilities);
    }

    [Fact]
    public void With_no_project_a_save_is_refused_rather_than_written_somewhere()
    {
        using var runtime = new AgentRuntime(new ResolvedSettings(), NullLoggerFactory.Instance);

        var result = SettingsOps.SaveRoles(runtime, [new SPLA.Service.Contracts.RoleEditDto { Name = "x", Active = true }]);

        Assert.NotNull(result.Error);
        Assert.False(result.CanPersist);
    }
}
