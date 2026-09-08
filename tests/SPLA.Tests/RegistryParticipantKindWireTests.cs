using System.Text.Json;
using SPLA.Instances;

namespace SPLA.Tests;

/// <summary>
/// Wave 0 of <c>docs/plans/PLAN_20260902_agent_roles-and-correspondence.md</c>:
/// <c>ParticipantRoles</c> becomes <c>ParticipantKind</c>, and the wire field goes from <c>role</c>
/// to <c>kind</c>. A running fleet is never upgraded atomically, so a hub of the new vintage must
/// still register a participant that only ever learned to say <c>role</c> — see
/// <see cref="RegisterFrame"/>'s own remarks on why it carries a hand-written converter for exactly
/// this field.
/// </summary>
public sealed class RegistryParticipantKindWireTests
{
    [Fact]
    public void An_old_format_frame_carrying_role_still_resolves_to_the_right_kind()
    {
        const string legacyJson = """{"projectId":"C:\\p\\project.spla","projectName":"P","info":{},"role":"window"}""";

        var frame = JsonSerializer.Deserialize<RegisterFrame>(legacyJson, RegistryJson.Options);

        Assert.NotNull(frame);
        Assert.Equal(ParticipantKind.Window, frame!.Kind);
        Assert.Equal("C:\\p\\project.spla", frame.ProjectId);
        Assert.Equal("P", frame.ProjectName);
    }

    [Fact]
    public void A_current_format_frame_carrying_kind_resolves_normally()
    {
        const string currentJson = """{"projectId":"C:\\p\\project.spla","info":{},"kind":"agent"}""";

        var frame = JsonSerializer.Deserialize<RegisterFrame>(currentJson, RegistryJson.Options);

        Assert.NotNull(frame);
        Assert.Equal(ParticipantKind.Agent, frame!.Kind);
    }

    [Fact]
    public void A_frame_naming_neither_kind_nor_role_defaults_to_agent()
    {
        const string bareJson = """{"projectId":"C:\\p\\project.spla","info":{}}""";

        var frame = JsonSerializer.Deserialize<RegisterFrame>(bareJson, RegistryJson.Options);

        Assert.NotNull(frame);
        Assert.Equal(ParticipantKind.Agent, frame!.Kind);
    }

    [Fact]
    public void Writing_a_frame_always_emits_kind_never_role()
    {
        var frame = new RegisterFrame { ProjectId = "p", Kind = ParticipantKind.Window };

        var json = JsonSerializer.Serialize(frame, RegistryJson.Options);

        Assert.Contains("\"kind\":\"window\"", json);
        Assert.DoesNotContain("\"role\"", json);
    }

    [Fact]
    public void Registering_with_a_legacy_role_only_frame_files_the_participant_under_the_right_kind()
    {
        var hub = new RegistryHub();
        var legacyFrame = JsonSerializer.Deserialize<RegisterFrame>(
            """{"projectId":"C:\\p\\project.spla","info":{"instanceId":"i1"},"role":"window"}""",
            RegistryJson.Options)!;

        using var registration = hub.Register(legacyFrame, (_, _) => Task.CompletedTask);

        var listed = Assert.Single(hub.List());
        Assert.Equal(ParticipantKind.Window, listed.Kind);
    }
}
