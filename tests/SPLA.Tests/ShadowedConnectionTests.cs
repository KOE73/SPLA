using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using SPLA.Service;

namespace SPLA.Tests;

/// <summary>
/// Two layers may declare the same connection id — a personal <c>default</c> and a project one, which
/// is what copying a project directory around produces. Resolution collapses them (the project wins,
/// ADR_20260904 §2.3) and that is correct; what was not correct was handing the collapsed list to the
/// settings editor. The shadowed entry became invisible, and since saving the panel rewrites each
/// layer file wholesale, the next save deleted it from the person's own connections.yaml.
/// </summary>
public sealed class ShadowedConnectionTests
{
    private static SplaConnectionSection Connection(string id, string endpoint, string modelId) => new()
    {
        Id = id,
        Name = id,
        Provider = "lmstudio",
        Endpoint = endpoint,
        Models = [new SplaModelSection { Id = modelId, Model = modelId }]
    };

    /// <summary>The same project the bug was reported against: `default` in both the user file and the
    /// manifest, plus one connection that exists only in the user layer.</summary>
    private static ResolvedSettings Resolve() => SettingsResolver.Resolve(
        null,
        new SplaProject
        {
            Connections = [Connection("default", "http://project", "project-model")]
        },
        null,
        [
            Connection("llama", "http://llama", "llama-model"),
            Connection("default", "http://user", "user-model")
        ]);

    [Fact]
    public void Resolution_still_collapses_the_shadowed_id_for_turns()
    {
        var resolved = Resolve();

        var live = Assert.Single(resolved.Connections.Where(c => c.Id == "default"));
        Assert.Equal("http://project", live.Endpoint);
        Assert.Equal(ConnectionScope.Project, live.Scope);

        // The flat model list a chat resolves against carries the winner's models only.
        Assert.Contains(resolved.Models, m => m.Id == "project-model");
        Assert.DoesNotContain(resolved.Models, m => m.Id == "user-model");
    }

    [Fact]
    public void Both_declarations_survive_for_the_editor()
    {
        var declared = Resolve().DeclaredConnections;

        var defaults = declared.Where(c => c.Id == "default").ToList();
        Assert.Equal(2, defaults.Count);
        Assert.Contains(defaults, c => c.Scope == ConnectionScope.User && c.Endpoint == "http://user");
        Assert.Contains(defaults, c => c.Scope == ConnectionScope.Project && c.Endpoint == "http://project");
    }

    [Fact]
    public void The_editor_is_shown_the_shadowed_entry_and_told_which_one_lost()
    {
        using var runtime = new AgentRuntime(Resolve(), NullLoggerFactory.Instance);

        var shown = SettingsOps.GetConnections(runtime).Connections;

        var user = Assert.Single(shown.Where(c => c.Id == "default" && c.Scope == "user"));
        var project = Assert.Single(shown.Where(c => c.Id == "default" && c.Scope == "project"));
        Assert.True(user.Shadowed);
        Assert.False(project.Shadowed);
    }

    /// <summary>The bug itself: open the panel in a project that shadows a personal connection, press
    /// Save, and the personal one must still be there.</summary>
    [Fact]
    public void Saving_the_panel_does_not_delete_the_shadowed_user_entry()
    {
        using var runtime = new AgentRuntime(Resolve(), NullLoggerFactory.Instance);

        SettingsOps.SaveConnections(runtime, SettingsOps.GetConnections(runtime).Connections);

        var userLayer = runtime.Settings.DeclaredConnections
            .Where(c => c.Scope == ConnectionScope.User)
            .Select(c => c.Id)
            .ToList();
        Assert.Contains("default", userLayer);
        Assert.Contains("llama", userLayer);
    }

    /// <summary>Saving must not turn the shadowed pair into two live connections of one id: what a turn
    /// resolves against is the merge, not the flat list of every layer's declarations.</summary>
    [Fact]
    public void Saving_keeps_the_live_tree_merged()
    {
        using var runtime = new AgentRuntime(Resolve(), NullLoggerFactory.Instance);

        SettingsOps.SaveConnections(runtime, SettingsOps.GetConnections(runtime).Connections);

        var live = Assert.Single(runtime.Settings.Connections.Where(c => c.Id == "default"));
        Assert.Equal("http://project", live.Endpoint);
        Assert.Single(runtime.Settings.Models, m => m.Id == "project-model");
        Assert.DoesNotContain(runtime.Settings.Models, m => m.Id == "user-model");
    }

    /// <summary>The same model id in two layers is not a clash: only one of the connections survives
    /// the merge, so the ids never meet in the flat list. Refusing this save would refuse exactly the
    /// configuration this fix exists to keep.</summary>
    [Fact]
    public void A_model_id_repeated_across_layers_is_not_refused()
    {
        var settings = SettingsResolver.Resolve(
            null,
            new SplaProject { Connections = [Connection("default", "http://project", "same-model")] },
            null,
            [Connection("default", "http://user", "same-model")]);
        using var runtime = new AgentRuntime(settings, NullLoggerFactory.Instance);

        var saved = SettingsOps.SaveConnections(runtime, SettingsOps.GetConnections(runtime).Connections);

        Assert.Null(saved.Error);
    }
}
