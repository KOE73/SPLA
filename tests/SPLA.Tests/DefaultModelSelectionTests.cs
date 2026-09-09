using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using SPLA.Service;

namespace SPLA.Tests;

public sealed class DefaultModelSelectionTests
{
    private static SplaModelSection Model(
        string id,
        string? name = null,
        string? wireModel = null,
        bool isDefault = false) => new()
        {
            Id = id,
            Name = name,
            Model = wireModel ?? id,
            Default = isDefault
        };

    private static SplaConnectionSection Connection(
        string id,
        string? name = null,
        params SplaModelSection[] models) => new()
        {
            Id = id,
            Name = name ?? id,
            Provider = "lmstudio",
            Endpoint = $"http://{id}",
            Models = [.. models]
        };

    [Fact]
    public void No_marked_model_keeps_the_historical_first_entry_fallback()
    {
        var resolved = SettingsResolver.Resolve(null, new SplaProject
        {
            Connections = [Connection("project", models: [Model("first"), Model("second")])]
        });

        Assert.Null(resolved.DefaultModelId);
        Assert.Equal("first", resolved.ToLLMSettings().ModelName);
    }

    [Fact]
    public void User_default_is_selected_independently_of_connection_order()
    {
        var resolved = SettingsResolver.Resolve(
            null,
            new SplaProject(),
            [Connection("shared", models: [Model("shared-first")])],
            [Connection("user", models: [Model("user-first"), Model("chosen", isDefault: true)])]);

        Assert.Equal("chosen", resolved.DefaultModelId);
        Assert.Equal("chosen", resolved.ToLLMSettings().ModelName);
    }

    [Fact]
    public void Project_default_replaces_the_user_default_wholesale()
    {
        var resolved = SettingsResolver.Resolve(
            null,
            new SplaProject
            {
                Connections = [Connection("project", models: [Model("project-choice", isDefault: true)])]
            },
            null,
            [Connection("user", models: [Model("user-choice", isDefault: true)])]);

        Assert.Equal("project-choice", resolved.DefaultModelId);
        Assert.Equal("project-choice", resolved.ToLLMSettings().ModelName);
    }

    [Fact]
    public void A_more_specific_layer_without_a_mark_keeps_the_inherited_default()
    {
        var resolved = SettingsResolver.Resolve(
            null,
            new SplaProject { Connections = [Connection("project", models: [Model("other")])] },
            [Connection("shared", models: [Model("shared-choice", isDefault: true)])],
            null);

        Assert.Equal("shared-choice", resolved.DefaultModelId);
    }

    [Fact]
    public void Multiple_marks_in_one_layer_name_the_file_and_every_conflicting_id()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => SettingsResolver.Resolve(
            null,
            new SplaProject(),
            null,
            [Connection("user", models: [
                Model("first-choice", isDefault: true),
                Model("second-choice", isDefault: true)])]));

        Assert.Contains("connections.yaml", exception.Message);
        Assert.Contains("first-choice", exception.Message);
        Assert.Contains("second-choice", exception.Message);
    }

    [Fact]
    public void FindModel_accepts_the_case_insensitive_display_name()
    {
        var resolved = SettingsResolver.Resolve(null, new SplaProject
        {
            Connections = [Connection("local", "Default", Model("qwen-id", "Qwen", "qwen@q4"))]
        });

        Assert.Equal("qwen-id", resolved.FindModel("default · qwen")?.Id);
    }

    [Fact]
    public void FindModel_accepts_the_exact_picker_text_copied_by_the_web_ui()
    {
        var resolved = SettingsResolver.Resolve(null, new SplaProject
        {
            Connections = [Connection("local", "Default", Model("qwen-id", "Qwen", "qwen@q4"))]
        });

        Assert.Equal("qwen-id", resolved.FindModel("Default | Qwen · qwen@q4")?.Id);
    }

    [Fact]
    public void FindModel_refuses_an_ambiguous_display_name()
    {
        var resolved = SettingsResolver.Resolve(null, new SplaProject
        {
            Connections =
            [
                Connection("first", "Same", Model("first-id", "Model")),
                Connection("second", "Same", Model("second-id", "Model"))
            ]
        });

        Assert.Null(resolved.FindModel("Same · Model"));
    }

    [Fact]
    public void FindModel_prefers_an_exact_id_over_another_entries_display_name()
    {
        var resolved = SettingsResolver.Resolve(null, new SplaProject
        {
            Connections =
            [
                Connection("display", "Friendly", Model("display-target", "Other")),
                Connection("id", "By id", Model("Friendly · Other"))
            ]
        });

        Assert.Equal("id", resolved.FindModel("Friendly · Other")?.Connection.Id);
    }

    [Fact]
    public void Connection_yaml_omits_false_but_round_trips_true()
    {
        var directory = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-default-model-{Guid.NewGuid():N}"));
        var path = Path.Combine(directory.FullName, "connections.yaml");

        ConfigLoader.SaveConnectionLayer(path,
            [Connection("local", models: [Model("ordinary"), Model("chosen", isDefault: true)])]);

        var yaml = File.ReadAllText(path);
        Assert.Equal(1, yaml.Split("default:", StringSplitOptions.None).Length - 1);
        Assert.Contains("default: true", yaml);
        var connection = Assert.Single(ConfigLoader.LoadConnectionLayer(path, ConnectionScope.User));
        Assert.False(connection.Models[0].Default);
        Assert.True(connection.Models[1].Default);
    }

    [Fact]
    public void Settings_editor_round_trip_preserves_the_default_mark()
    {
        var settings = SettingsResolver.Resolve(null, new SplaProject
        {
            Connections = [Connection("local", models: [Model("chosen", isDefault: true)])]
        });
        using var runtime = new AgentRuntime(settings, NullLoggerFactory.Instance);

        var echoed = SettingsOps.GetConnections(runtime).Connections;
        SettingsOps.SaveConnections(runtime, echoed);

        Assert.True(Assert.Single(Assert.Single(runtime.Settings.Connections).Models).Default);
        Assert.Equal("chosen", runtime.Settings.DefaultModelId);
    }

    [Fact]
    public void New_chat_is_seeded_with_the_configured_default_model()
    {
        var workspace = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-default-chat-{Guid.NewGuid():N}"));
        var settings = SettingsResolver.Resolve(null, new SplaProject
        {
            Connections = [Connection("local", models: [
                Model("first"),
                Model("chosen", isDefault: true)])]
        });
        settings.WorkspacePath = workspace.FullName;
        var chats = new ChatManager(settings);

        Assert.Equal("chosen", chats.CreateNewChat().ModelId);
    }
}
