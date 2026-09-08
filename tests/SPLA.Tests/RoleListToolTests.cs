using SPLA.Domain.Settings;
using SPLA.MCP.Core.Tools;

namespace SPLA.Tests;

/// <summary>
/// The role directory (<see cref="RoleListTool"/>): a chat can only address a role it knows exists.
/// The load-bearing test here is the last one — the listing names roles, it does not publish them,
/// because a role body carries capabilities/islands/trusted domains alongside its character. See the
/// tool's own type doc and <see cref="SplaRoleSection.Description"/>.
/// </summary>
public sealed class RoleListToolTests
{
    private static string TempProjectDir() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-rolelist-{Guid.NewGuid():N}")).FullName;

    private static void WriteRole(string projectDir, string name, string yaml)
    {
        var rolesDir = Directory.CreateDirectory(Path.Combine(projectDir, "roles")).FullName;
        File.WriteAllText(Path.Combine(rolesDir, name + ".yaml"), yaml);
    }

    private static string Run(string projectDir, params string[] declaredRoles)
    {
        var manifestPath = Path.Combine(projectDir, "test.spla");
        File.WriteAllText(manifestPath, "version: 1\nname: RoleListTest\n");

        var settings = new ResolvedSettings
        {
            ProjectFilePath = manifestPath,
            WorkspacePath = projectDir,
            Manifest = new SplaProject { Roles = [.. declaredRoles] }
        };

        var result = new RoleListTool(settings).ExecuteAsync("{}").GetAwaiter().GetResult();
        return result.TextContent;
    }

    [Fact]
    public void A_role_is_listed_with_its_mode_and_its_description_from_yaml()
    {
        var dir = TempProjectDir();
        try
        {
            WriteRole(dir, "architect", """
                mode: Research
                description: Owns the shape of the system and reviews interfaces.
                custom_prompt: |
                  SECRET ROLE PROMPT BODY
                """);

            var text = Run(dir, "architect");

            Assert.Contains("architect", text);
            Assert.Contains("Research", text);
            Assert.Contains("Owns the shape of the system and reviews interfaces.", text);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    /// <summary>A name the manifest declares is addressable whether or not its file loads — and a
    /// caller about to be refused on that name needs to see it, not have it silently dropped.</summary>
    [Fact]
    public void A_declared_role_whose_body_is_missing_is_still_listed_marked()
    {
        var dir = TempProjectDir();
        try
        {
            WriteRole(dir, "tester", "mode: Edit\n");

            var text = Run(dir, "tester", "ghost");

            Assert.Contains("tester", text);
            Assert.Contains("ghost", text);
            Assert.Contains("body unreadable", text);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Fact]
    public void No_declared_roles_says_so_instead_of_failing()
    {
        var dir = TempProjectDir();
        try
        {
            Assert.Contains("(none)", Run(dir));
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    /// <summary>The whole point of the tool's shape: the role's own prompt never crosses to a chat
    /// that merely asked who exists. There is no flag that changes this.</summary>
    [Fact]
    public void The_roles_custom_prompt_is_never_disclosed()
    {
        var dir = TempProjectDir();
        try
        {
            WriteRole(dir, "architect", """
                mode: Research
                description: Owns the shape of the system.
                custom_prompt: SECRET ROLE PROMPT BODY
                capabilities: [core.files]
                trusted_domains: [internal.example.com]
                """);

            var text = Run(dir, "architect");

            Assert.DoesNotContain("SECRET ROLE PROMPT BODY", text);
            Assert.DoesNotContain("internal.example.com", text);
            Assert.DoesNotContain("core.files", text);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }
}
