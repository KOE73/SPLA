using System.Text.Json;
using System.Text.Json.Serialization;
using SPLA.Domain.Project;

namespace SPLA.Instances;

/// <summary>
/// The hub's whole vocabulary. Deliberately its own, tiny, and independent of the chat protocol:
/// registration must keep working when the chat protocol changes shape, and a hub that spoke the
/// chat protocol would drag its entire contract into every instance that only wants to say "I exist,
/// here is where, here is what I am doing".
/// </summary>
public static class RegistryFrames
{
    /// <summary>Instance → hub, first frame on the channel. Body <see cref="RegisterFrame"/>.</summary>
    public const string Register = "register";

    /// <summary>Instance → hub, whenever its state changes. Body <see cref="StatusFrame"/>.</summary>
    public const string Status = "status";

    /// <summary>Hub → instance, once, after a registration it accepted. No body.</summary>
    public const string Accepted = "accepted";

    /// <summary>Hub → participant: somebody asked, through the hub, that it stop. For an agent that
    /// means shutting down; for a window it means closing. Body <see cref="StopFrame"/>.</summary>
    public const string Stop = "stop";

    /// <summary>Hub → participant: come to the front. Only a window can do anything with it; an agent
    /// ignores it. No body — "which window" is answered by which channel it arrives on.</summary>
    public const string Focus = "focus";
}

/// <summary>
/// What a participant is, so the hub can address the right ones.
///
/// <para>The registry used to hold only agents, and every symptom of that showed up as something the
/// hub could not do: it could not raise an existing window because it did not know one existed, and
/// closing a project left its windows behind because they were never its to close. A role is the
/// smallest thing that fixes all of it — the channel, the liveness rule and the transport are
/// unchanged, and a participant is still just "something that said hello and is still connected".</para>
///
/// <para>Values are strings rather than an enum on the wire: a hub of one vintage meeting a
/// participant of another must be able to carry a role it does not recognise without failing the
/// connection, which an enum would turn into a parse error.</para>
/// </summary>
public static class ParticipantRoles
{
    /// <summary>Holds a project and runs work in it. The historical, and still the default: a
    /// registration that names no role predates roles and is an agent.</summary>
    public const string Agent = "agent";

    /// <summary>A view onto a project. Holds nothing, can be raised, and closes on request.</summary>
    public const string Window = "window";

    /// <summary>The machine-wide shell that owns the tray. Registers so that a second one can find
    /// out it is not the first.</summary>
    public const string Hub = "hub";
}

/// <summary>One frame on the registration channel. A type and an opaque body, so a hub and an
/// instance of slightly different vintages can ignore what they do not know instead of failing to
/// parse the connection.</summary>
public sealed class RegistryFrame
{
    public string Type { get; set; } = "";
    public JsonElement? Body { get; set; }
}

/// <summary>What an instance says about itself when it arrives.</summary>
public sealed class RegisterFrame
{
    /// <summary>Manifest path, or whatever the instance calls its project. Opaque to the hub: on
    /// another machine it is not a path the hub could resolve, and it never tries.</summary>
    public string ProjectId { get; set; } = "";

    public string? ProjectName { get; set; }

    /// <summary>The same block the instance publishes in its own lock file — one description of an
    /// instance, whether it is read off a disk or off a socket.</summary>
    public InstanceInfo Info { get; set; } = new();

    /// <summary>What this participant is; one of <see cref="ParticipantRoles"/>. Defaults to
    /// <see cref="ParticipantRoles.Agent"/> so a participant built before roles existed — or one that
    /// simply does not care — registers as what it always was.</summary>
    public string Role { get; set; } = ParticipantRoles.Agent;
}

/// <summary>What an instance is doing now. Pushed, never polled: a badge that updates on a poll is
/// not a badge, and the channel already exists for registration.</summary>
public sealed class StatusFrame
{
    /// <summary>One of <see cref="InstanceStates"/>' names.</summary>
    public string State { get; set; } = "";

    public int Clients { get; set; }
}

/// <summary>A stop relayed by the hub on somebody's behalf.</summary>
public sealed class StopFrame
{
    public bool Force { get; set; }
}

/// <summary>One registered instance as the hub reports it to an observer.</summary>
public sealed class RegisteredInstanceDto
{
    public string ProjectId { get; set; } = "";
    public string? ProjectName { get; set; }
    public InstanceInfo Info { get; set; } = new();
    public string State { get; set; } = "";
    public int Clients { get; set; }

    /// <summary>One of <see cref="ParticipantRoles"/>. An observer showing only agents filters on it;
    /// one showing everything gets to say what each row is.</summary>
    public string Role { get; set; } = ParticipantRoles.Agent;

    /// <summary>When the hub last heard anything at all from this instance. An observer that cares
    /// about staleness has the number rather than a boolean somebody else's clock decided.</summary>
    public DateTimeOffset LastSeen { get; set; }

    /// <summary>
    /// The observer-side record for this row.
    ///
    /// <para>Lives here, once, because it was written twice — in the watcher and in the remote
    /// registry — and the two promptly disagreed: adding <see cref="Role"/> to the wire updated one
    /// copy and left the other quietly reporting every window as an agent. A conversion duplicated
    /// per consumer is a conversion that drifts the next time the shape changes.</para>
    /// </summary>
    public InstanceRecord ToRecord()
    {
        InstanceStates.TryParse(State, out var state);
        return new InstanceRecord(
            ProjectId, ProjectName, Info, state, Clients,
            string.IsNullOrWhiteSpace(Role) ? ParticipantRoles.Agent : Role);
    }
}

/// <summary>The listing an observer gets from <c>GET {hub}/registry/instances</c>.</summary>
public sealed class RegistryListResponse
{
    public List<RegisteredInstanceDto> Instances { get; set; } = new();
}

/// <summary>Serialization settings shared by both ends of the registration channel. Kept here rather
/// than duplicated: a hub and an instance disagreeing about casing is the kind of bug that only
/// appears between two builds.</summary>
public static class RegistryJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
