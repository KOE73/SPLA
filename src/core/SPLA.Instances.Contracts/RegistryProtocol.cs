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
/// closing a project left its windows behind because they were never its to close. A kind is the
/// smallest thing that fixes all of it — the channel, the liveness rule and the transport are
/// unchanged, and a participant is still just "something that said hello and is still connected".</para>
///
/// <para>Values are strings rather than an enum on the wire: a hub of one vintage meeting a
/// participant of another must be able to carry a kind it does not recognise without failing the
/// connection, which an enum would turn into a parse error.</para>
///
/// <para>Named <c>ParticipantKind</c>, not <c>ParticipantRoles</c>: a registry participant is a
/// <i>kind</i> (agent, window, hub), and "role" now names a different thing — the deliberately
/// separate configuration an actor runs as (<c>ADR_20260827-2_core_roles</c> §5). On the wire the
/// field is <c>kind</c>; a participant registered before this rename still sends <c>role</c>, and
/// <see cref="RegisterFrame"/> reads either.</para>
/// </summary>
public static class ParticipantKind
{
    /// <summary>Holds a project and runs work in it. The historical, and still the default: a
    /// registration that names no kind predates kinds and is an agent.</summary>
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
[JsonConverter(typeof(RegisterFrameConverter))]
public sealed class RegisterFrame
{
    /// <summary>Manifest path, or whatever the instance calls its project. Opaque to the hub: on
    /// another machine it is not a path the hub could resolve, and it never tries.</summary>
    public string ProjectId { get; set; } = "";

    public string? ProjectName { get; set; }

    /// <summary>The same block the instance publishes in its own lock file — one description of an
    /// instance, whether it is read off a disk or off a socket.</summary>
    public InstanceInfo Info { get; set; } = new();

    /// <summary>What this participant is; one of <see cref="ParticipantKind"/>. Defaults to
    /// <see cref="ParticipantKind.Agent"/> so a participant built before kinds existed — or one that
    /// simply does not care — registers as what it always was.</summary>
    public string Kind { get; set; } = ParticipantKind.Agent;
}

/// <summary>
/// Hand-written (de)serialization for <see cref="RegisterFrame"/>, for exactly one reason: the wire
/// field was renamed <c>role</c> → <c>kind</c> (see <see cref="ParticipantKind"/>'s own remarks), and
/// a participant built before the rename still sends <c>role</c>. Writing always emits <c>kind</c>;
/// reading accepts either, preferring <c>kind</c> when a sender somehow has both. Every other field
/// on the frame would serialize identically through ordinary reflection — this converter exists only
/// because System.Text.Json has no built-in notion of "read this property under either of two names".
/// </summary>
internal sealed class RegisterFrameConverter : JsonConverter<RegisterFrame>
{
    public override RegisterFrame Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var kind = ReadString(root, "kind") ?? ReadString(root, "role");
        return new RegisterFrame
        {
            ProjectId = ReadString(root, "projectId") ?? "",
            ProjectName = ReadString(root, "projectName"),
            Info = root.TryGetProperty("info", out var info) && info.ValueKind != JsonValueKind.Null
                ? info.Deserialize<InstanceInfo>(options) ?? new()
                : new(),
            Kind = string.IsNullOrWhiteSpace(kind) ? ParticipantKind.Agent : kind
        };
    }

    public override void Write(Utf8JsonWriter writer, RegisterFrame value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("projectId", value.ProjectId);
        if (value.ProjectName is not null) writer.WriteString("projectName", value.ProjectName);
        writer.WritePropertyName("info");
        JsonSerializer.Serialize(writer, value.Info, options);
        writer.WriteString("kind", value.Kind);
        writer.WriteEndObject();
    }

    private static string? ReadString(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
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

    /// <summary>One of <see cref="ParticipantKind"/>. An observer showing only agents filters on it;
    /// one showing everything gets to say what each row is.</summary>
    public string Kind { get; set; } = ParticipantKind.Agent;

    /// <summary>When the hub last heard anything at all from this instance. An observer that cares
    /// about staleness has the number rather than a boolean somebody else's clock decided.</summary>
    public DateTimeOffset LastSeen { get; set; }

    /// <summary>
    /// The observer-side record for this row.
    ///
    /// <para>Lives here, once, because it was written twice — in the watcher and in the remote
    /// registry — and the two promptly disagreed: adding <see cref="Kind"/> to the wire updated one
    /// copy and left the other quietly reporting every window as an agent. A conversion duplicated
    /// per consumer is a conversion that drifts the next time the shape changes.</para>
    /// </summary>
    public InstanceRecord ToRecord()
    {
        InstanceStates.TryParse(State, out var state);
        return new InstanceRecord(
            ProjectId, ProjectName, Info, state, Clients,
            string.IsNullOrWhiteSpace(Kind) ? ParticipantKind.Agent : Kind);
    }
}

/// <summary>The listing an observer gets from <c>GET {hub}/registry/instances</c>.</summary>
public sealed class RegistryListResponse
{
    public List<RegisteredInstanceDto> Instances { get; set; } = new();
}

/// <summary>
/// One project a manager can act on — remembered by the machine, running or not.
///
/// <para>Deliberately not the same shape as <see cref="RegisteredInstanceDto"/>. That one answers
/// "what is up"; this answers "what could be", which is the question a manager opens with. A project
/// with nothing running is the interesting case, and the instance listing cannot represent it at
/// all.</para>
/// </summary>
public sealed class KnownProjectDto
{
    /// <summary>Manifest path — the same id everything else keys on.</summary>
    public string ProjectId { get; set; } = "";

    public string? Name { get; set; }

    /// <summary>False when the manifest is no longer on disk. Shown rather than hidden: a remembered
    /// project that has been moved or deleted is worth seeing so it can be forgotten on purpose.</summary>
    public bool Exists { get; set; }

    /// <summary>The agent's state, or null when nothing is holding this project.</summary>
    public string? State { get; set; }

    /// <summary>The agent's instance id, when one is running — what a stop is addressed to.</summary>
    public string? InstanceId { get; set; }

    /// <summary>How many windows are looking at it. Zero with a live agent is the headless case: work
    /// going on with nobody watching, which is exactly what the instance model set out to allow.</summary>
    public int Windows { get; set; }

    /// <summary>True when the agent published an address (<see cref="InstanceInfo.Endpoint"/>) — every
    /// such host maps <c>POST /mcp</c> unconditionally, so an endpoint existing at all is the whole
    /// test. False for a stdio-only instance (<c>spla mcp</c>'s "own body" mode, a bare REPL): it holds
    /// the project but offers nothing a second MCP client could dial.</summary>
    public bool McpAvailable { get; set; }
}

/// <summary>The listing a manager gets from <c>GET {hub}/registry/projects</c>.</summary>
public sealed class KnownProjectsResponse
{
    public List<KnownProjectDto> Projects { get; set; } = new();
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
