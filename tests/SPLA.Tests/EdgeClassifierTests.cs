using System.Linq;
using SPLA.Domain.Models;
using SPLA.Domain.Security;
using SPLA.MCP.Core.Security;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// A call is a movement, and the single scope axis could not say which. These tests pin the three
/// cases that motivated the change — a leak, a database mistaken for a file, and a transfer with no
/// context at either end — plus the ordinary ones the generic rule has to keep getting right.
/// </summary>
public sealed class EdgeClassifierTests
{
    /// <summary>Anything under <c>/project</c> is inside; everything else is not. Stands in for the
    /// real boundary, which answers the same question about real paths.</summary>
    private static readonly EdgeClassifier Classifier = new(path =>
        path is null ? Zone.Unknown
        : path.Replace('\\', '/').StartsWith("/project/") ? Zone.Project
        : Zone.Machine);

    private static ToolFunctionDefinition Tool(
        string name, ToolScope scope, ToolEffect effect) => new() { Name = name, Scope = scope, Effect = effect };

    [Fact]
    public void Reading_a_project_file_moves_it_into_the_context()
    {
        var edge = Classifier.Classify(
            Tool("system_read_file", ToolScope.Project, ToolEffect.Read),
            """{"path":"/project/src/app.cs"}""");

        Assert.Equal(Zone.Project, edge.Source);
        Assert.Equal(Zone.Context, edge.Sink);
        Assert.Equal(ZoneEffect.Read, edge.Effect);
    }

    /// <summary>The declaration says "project write" whatever the path is. Where it lands is what
    /// decides the zone, which is the whole reason the old model could not see a write to C:\ .</summary>
    [Fact]
    public void A_write_outside_the_root_is_a_different_movement_from_the_same_tool()
    {
        var inside = Classifier.Classify(
            Tool("system_write_file", ToolScope.Project, ToolEffect.Write), """{"path":"/project/notes.md"}""");
        var outside = Classifier.Classify(
            Tool("system_write_file", ToolScope.Project, ToolEffect.Write), """{"path":"/etc/passwd"}""");

        Assert.Equal(Zone.Project, inside.Sink);
        Assert.Equal(Zone.Machine, outside.Sink);
    }

    /// <summary>
    /// The case that started all of this. Declared Scope=Internet/Write, which reads as "posting
    /// something to the web" and hides the half that matters: a file off the disk is what is being
    /// posted. In the old model this passed as an ordinary internet write.
    /// </summary>
    [Fact]
    public void Uploading_a_file_to_a_web_form_is_a_movement_from_the_disk_not_from_nowhere()
    {
        var edge = Classifier.Classify(
            new ToolFunctionDefinition { Name = "browser_upload", Scope = ToolScope.Internet, Effect = ToolEffect.Write },
            """{"files":["/home/me/taxes.pdf"]}""");

        Assert.Equal(Zone.Machine, edge.Source);
        Assert.Equal(Zone.Web, edge.Sink);
        Assert.Equal(ZoneEffect.Write, edge.Effect);
    }

    /// <summary>SQL tools are declared Local — as if a database on another machine were a file on
    /// this one. There was no cell for an island, so they went in the nearest one.</summary>
    [Fact]
    public void A_database_is_an_island_and_a_named_one()
    {
        var prod = Classifier.Classify(
            Tool("sql_query", ToolScope.Local, ToolEffect.Read), """{"connection":"prod","sql":"select 1"}""");
        var test = Classifier.Classify(
            Tool("sql_query", ToolScope.Local, ToolEffect.Read), """{"connection":"test","sql":"select 1"}""");

        Assert.Equal(new Zone("sql", "prod"), prod.Source);
        Assert.Equal(Zone.Context, prod.Sink);

        // The reason instances matter: a permission for one must not cover the other.
        Assert.NotEqual(prod.Key, test.Key);
    }

    [Fact]
    public void Writing_to_a_database_reverses_the_movement()
    {
        var edge = Classifier.Classify(
            Tool("sql_execute", ToolScope.Local, ToolEffect.Write), """{"connection":"prod","sql":"delete from orders"}""");

        Assert.Equal(Zone.Context, edge.Source);
        Assert.Equal(new Zone("sql", "prod"), edge.Sink);
    }

    /// <summary>Both ends are real places and the context is at neither. The single-axis model had no
    /// way to say this at all.</summary>
    [Fact]
    public void A_transfer_has_the_context_at_neither_end()
    {
        var up = Classifier.Classify(
            Tool("sftp_upload", ToolScope.Local, ToolEffect.Write),
            """{"host":"web-01","local_path":"/project/staging/conf.tar"}""");

        Assert.Equal(Zone.Project, up.Source);
        Assert.Equal(new Zone("ssh", "web-01"), up.Sink);
    }

    /// <summary>
    /// Configuring a server and posting the payroll to it are the same destination and different
    /// acts. This pair is why a verdict cannot be a function of the sink alone — and why
    /// <c>allow_write</c>, which is exactly such a function, could not be folded into this.
    /// </summary>
    [Fact]
    public void The_same_destination_from_two_sources_is_two_movements()
    {
        var config = Classifier.Classify(
            Tool("sftp_upload", ToolScope.Local, ToolEffect.Write),
            """{"host":"web-01","local_path":"/project/deploy/nginx.conf"}""");

        var payroll = Classifier.Classify(
            Tool("sftp_upload", ToolScope.Local, ToolEffect.Write),
            """{"host":"web-01","local_path":"/home/me/payroll.csv"}""");

        Assert.Equal(config.Sink, payroll.Sink);
        Assert.NotEqual(config.Source, payroll.Source);
        Assert.NotEqual(config.Key, payroll.Key);
    }

    /// <summary>A shell reaches everything. Saying so plainly is what makes it obvious that no rule
    /// here bounds it — only an isolator does.</summary>
    [Fact]
    public void A_shell_admits_to_reaching_everywhere()
    {
        var edge = Classifier.Classify(
            Tool("system_run_shell", ToolScope.Shell, ToolEffect.Execute), """{"command":"ls"}""");

        Assert.Equal(Zone.Any, edge.Source);
        Assert.Equal(Zone.Any, edge.Sink);
        Assert.Equal(ZoneEffect.Execute, edge.Effect);
    }

    [Fact]
    public void A_call_that_names_no_path_is_unknown_rather_than_assumed()
    {
        var edge = Classifier.Classify(Tool("system_read_file", ToolScope.Project, ToolEffect.Read), "{}");

        Assert.Equal(Zone.Unknown, edge.Source);
        Assert.False(edge.Source.IsKnown);
    }

    [Fact]
    public void Malformed_arguments_still_classify_on_the_declaration()
    {
        var edge = Classifier.Classify(Tool("web_fetch", ToolScope.Internet, ToolEffect.Read), "{not json");

        Assert.Equal(Zone.Web, edge.Source);
        Assert.Equal(Zone.Context, edge.Sink);
    }

    /// <summary>The agent's own memory is not traffic between perimeters, and counting it would bury
    /// the handful of rows that matter.</summary>
    [Fact]
    public void The_agents_own_state_is_not_recorded_as_traffic()
    {
        var ledger = new EdgeLedger();

        ledger.Record(
            Classifier.Classify(Tool("agent_memory_set", ToolScope.Agent, ToolEffect.Write), "{}"), "agent_memory_set");
        ledger.Record(
            Classifier.Classify(Tool("system_read_file", ToolScope.Project, ToolEffect.Read),
                """{"path":"/project/a.cs"}"""), "system_read_file");

        var only = Assert.Single(ledger.List());
        Assert.Equal(Zone.Project, only.Edge.Source);
    }

    [Fact]
    public void The_ledger_counts_repeats_of_one_movement_rather_than_listing_them()
    {
        var ledger = new EdgeLedger();
        var edge = Classifier.Classify(
            Tool("system_read_file", ToolScope.Project, ToolEffect.Read), """{"path":"/project/a.cs"}""");

        ledger.Record(edge, "system_read_file");
        ledger.Record(edge, "system_read_file");
        ledger.Record(edge, "system_read_file");

        Assert.Equal(3, ledger.List().Single().Calls);
    }
}
