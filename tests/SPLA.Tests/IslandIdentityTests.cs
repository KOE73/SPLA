using SPLA.Domain.Security;
using SPLA.Plugins.Sql;
using SPLA.Plugins.Ssh;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// A grant is a promise about a thing, so identity has to be the thing and not its label. These
/// tests fix the two halves of that: a rename carries nothing, and a repointing keeps nothing.
/// </summary>
public sealed class IslandIdentityTests
{
    private static SqlConnectionConfig Prod() => new()
    {
        Provider = "mssql",
        Server = "db01.corp.local",
        Database = "Sales",
        User = "reader",
        Credential = "sql/prod"
    };

    [Fact]
    public void Renaming_a_connection_does_not_change_who_it_is()
    {
        var before = Prod().Identity("prod");
        var after = Prod().Identity("production");

        Assert.Equal(before.Key, after.Key);
        Assert.NotEqual(before.DisplayName, after.DisplayName);
    }

    /// <summary>
    /// The trick worth defending against: rename <c>test</c> to <c>prod</c> and <c>prod</c> to
    /// <c>test</c>, and a grant keyed by name would hand the production database everything the test
    /// one was allowed. Keyed by substance, each entry keeps its own identity and the swap achieves
    /// nothing.
    /// </summary>
    [Fact]
    public void Swapping_two_names_swaps_no_permissions()
    {
        var prod = Prod();
        var test = new SqlConnectionConfig
        {
            Provider = "mssql",
            Server = "db02.corp.local",
            Database = "Sales",
            User = "reader",
            Credential = "sql/test"
        };

        var prodBefore = prod.Identity("prod");
        var testBefore = test.Identity("test");

        // The operator swaps the labels; the entries themselves are untouched.
        var prodAfter = prod.Identity("test");
        var testAfter = test.Identity("prod");

        Assert.Equal(prodBefore.Key, prodAfter.Key);
        Assert.Equal(testBefore.Key, testAfter.Key);
        Assert.NotEqual(prodBefore.Key, testBefore.Key);
    }

    [Theory]
    [InlineData("server")]
    [InlineData("database")]
    [InlineData("port")]
    [InlineData("user")]
    [InlineData("credential")]
    public void Repointing_any_substantive_field_makes_it_a_different_island(string field)
    {
        var before = Prod().Identity("prod");

        var moved = Prod();
        switch (field)
        {
            case "server": moved.Server = "db99.corp.local"; break;
            case "database": moved.Database = "Payroll"; break;
            case "port": moved.Port = 14330; break;
            case "user": moved.User = "writer"; break;
            case "credential": moved.Credential = "sql/other"; break;
        }

        Assert.NotEqual(before.Key, moved.Identity("prod").Key);
    }

    /// <summary>Rewording the blurb the model reads is not a change of island — charging the operator
    /// a re-approval for editing a comment is how a permission system gets switched off.</summary>
    [Fact]
    public void Editing_the_description_costs_nothing()
    {
        var before = Prod().Identity("prod");

        var reworded = Prod();
        reworded.Description = "Sales, read-only replica. Ask before running anything heavy.";

        Assert.Equal(before.Key, reworded.Identity("prod").Key);
    }

    /// <summary>A host's own read-only flag is a property of the node, enforced separately. Folding
    /// it into identity would mean opening a host up silently invalidated every grant about it.</summary>
    [Fact]
    public void Opening_an_ssh_host_for_writing_does_not_change_who_it_is()
    {
        var host = new SshHostConfig { Host = "web-01", User = "deploy", Credential = "ssh/web" };
        var before = host.Identity("web-01");

        host.AllowWrite = true;

        Assert.Equal(before.Key, host.Identity("web-01").Key);
    }

    [Fact]
    public void Two_kinds_never_collide_even_on_identical_substance()
    {
        var sql = new SqlConnectionConfig { Provider = "mssql", Host = "box", User = "u" }.Identity("x");
        var ssh = new SshHostConfig { Host = "box", User = "u" }.Identity("x");

        Assert.NotEqual(sql.Key, ssh.Key);
    }

    /// <summary>The fingerprint is written to the grant file in the clear, so nothing secret may go
    /// into it — a credential contributes which entry is named, never what it holds.</summary>
    [Fact]
    public void A_secret_value_never_reaches_the_fingerprint()
    {
        Assert.Equal("entry:sql/prod", Substance.CredentialShape("sql/prod", null));
        Assert.Equal("secret:DB_PASS", Substance.CredentialShape(null, "secret:DB_PASS"));
        Assert.Equal("env:DB_PASS", Substance.CredentialShape(null, "env:DB_PASS"));
        Assert.Equal("none", Substance.CredentialShape(null, null));

        // A literal left in the field contributes only that it is one.
        Assert.Equal("literal", Substance.CredentialShape(null, "hunter2"));
    }

    /// <summary>Hostnames are officially case-insensitive, and this deliberately does not care: of
    /// the two ways to be wrong, conflating entries lends one the other's permissions, while treating
    /// a re-cased host as new costs one confirmation.</summary>
    [Fact]
    public void Case_changes_are_treated_as_a_new_island_on_purpose()
    {
        var lower = new SshHostConfig { Host = "web-01", User = "deploy" }.Identity("web");
        var upper = new SshHostConfig { Host = "WEB-01", User = "deploy" }.Identity("web");

        Assert.NotEqual(lower.Key, upper.Key);
    }

    [Fact]
    public void The_fingerprint_is_stable_across_calls()
    {
        Assert.Equal(Prod().Identity("prod").Key, Prod().Identity("prod").Key);
        Assert.Equal(16, Prod().Identity("prod").Fingerprint.Length);
    }
}
