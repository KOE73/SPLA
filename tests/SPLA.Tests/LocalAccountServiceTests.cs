using System.IO;
using System.Linq;
using SPLA.Service.Auth;
using Xunit;

namespace SPLA.Tests;

public class LocalAccountServiceTests
{
    private static (LocalAccountService svc, string path) NewService(bool allowRegister = true)
    {
        var path = Path.Combine(Path.GetTempPath(), "spla-users-" + Path.GetRandomFileName() + ".json");
        return (new LocalAccountService(new JsonUserStore(path), allowRegister), path);
    }

    [Fact]
    public void SeedsAdmin_WhenStoreEmpty()
    {
        var (svc, _) = NewService();
        var users = svc.ListUsers();
        Assert.Single(users);
        Assert.Equal("admin", users[0].Username);
        Assert.True(users[0].IsAdmin);
    }

    [Fact]
    public void Register_ThenValidate_RoundTrips()
    {
        var (svc, _) = NewService();
        var reg = svc.Register("alice", "sekret1", "Alice");
        Assert.True(reg.Ok);

        Assert.NotNull(svc.Validate("alice", "sekret1"));
        Assert.Null(svc.Validate("alice", "wrong"));
        Assert.Null(svc.Validate("nobody", "sekret1"));
    }

    [Fact]
    public void Password_IsNotStoredInPlaintext()
    {
        var (svc, path) = NewService();
        svc.Register("bob", "hunter2xyz", "Bob");
        var raw = File.ReadAllText(path);
        Assert.DoesNotContain("hunter2xyz", raw);
    }

    [Fact]
    public void Register_RejectsDuplicateUsername_CaseInsensitive()
    {
        var (svc, _) = NewService();
        Assert.True(svc.Register("carol", "passw0rd", "Carol").Ok);
        var dup = svc.Register("CAROL", "passw0rd", "Carol2");
        Assert.False(dup.Ok);
    }

    [Fact]
    public void Register_EnforcesMinimumLengths()
    {
        var (svc, _) = NewService();
        Assert.False(svc.Register("ab", "passw0rd", null).Ok);   // username too short
        Assert.False(svc.Register("dave", "123", null).Ok);      // password too short
    }

    [Fact]
    public void DisabledUser_CannotValidate()
    {
        var (svc, _) = NewService();
        var reg = svc.Register("erin", "passw0rd", "Erin");
        svc.SetEnabled(reg.User!.UserKey, false);
        Assert.Null(svc.Validate("erin", "passw0rd"));
    }

    [Fact]
    public void CannotDisableOrDeleteOrDemote_LastAdmin()
    {
        var (svc, _) = NewService();
        var admin = svc.ListUsers().Single(u => u.IsAdmin);

        Assert.False(svc.SetEnabled(admin.UserKey, false).Ok);
        Assert.False(svc.Delete(admin.UserKey).Ok);
        Assert.False(svc.SetRoles(admin.UserKey, ["user"]).Ok);
    }

    [Fact]
    public void SecondAdmin_AllowsDemotingTheFirst()
    {
        var (svc, _) = NewService();
        var first = svc.ListUsers().Single(u => u.IsAdmin);
        svc.Register("frank", "passw0rd", "Frank", roles: ["admin", "user"]);

        // With a second admin present, the guard no longer blocks demoting the first.
        Assert.True(svc.SetRoles(first.UserKey, ["user"]).Ok);
    }

    [Fact]
    public void SetPassword_ChangesCredential()
    {
        var (svc, _) = NewService();
        var reg = svc.Register("grace", "passw0rd", "Grace");
        Assert.True(svc.SetPassword(reg.User!.UserKey, "newpass1").Ok);
        Assert.Null(svc.Validate("grace", "passw0rd"));
        Assert.NotNull(svc.Validate("grace", "newpass1"));
    }

    [Fact]
    public void Groups_ArePersistedAndNormalized()
    {
        var (svc, _) = NewService();
        var reg = svc.Register("heidi", "passw0rd", "Heidi", groups: ["team-a", " team-a ", "team-b"]);
        Assert.True(reg.Ok);
        var user = svc.FindByKey(reg.User!.UserKey)!;
        Assert.Equal(["team-a", "team-b"], user.Groups.OrderBy(g => g));
    }

    [Fact]
    public void Store_PersistsAcrossReload()
    {
        var (svc, path) = NewService();
        svc.Register("ivan", "passw0rd", "Ivan");

        var reopened = new LocalAccountService(new JsonUserStore(path), true);
        Assert.NotNull(reopened.Validate("ivan", "passw0rd"));
    }
}
