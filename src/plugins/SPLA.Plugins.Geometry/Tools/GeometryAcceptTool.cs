using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Session;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Geometry.Tools;

/// <summary>
/// Closes the loop on an object: it is where it should be. The picture comes back with the accepted
/// object drawn muted, which is the answer to "is this settled" in the only language the loop uses.
/// </summary>
internal sealed class GeometryAcceptTool(ResolvedSettings projectSettings) : GeometryToolBase(projectSettings)
{
    public override string Name => "geom_accept";

    protected override ToolEffect Effect => ToolEffect.Write;

    protected override string Description =>
        "Marks an object as final once it sits right, and returns the picture with it drawn muted. " +
        "Omit the name to accept everything still being edited.";

    protected override string? Details =>
        "Accepting changes nothing about where an object is — it only says you are done placing it. " +
        "Calling geom_box or geom_point on it again puts it back into editing.";

    protected override Dictionary<string, object> Properties => new()
    {
        ["name"] = new
        {
            type = new[] { "string", "null" },
            description = "The object to accept. Null accepts every object still being edited."
        },
    };

    protected override Task<ToolResult> RunAsync(
        IAgentSession chat, GeometrySettings cfg, JsonElement args, CancellationToken ct)
    {
        var session = GeometrySessionRegistry.TryGet(chat);
        if (session is null) return Task.FromResult(NoSession);

        var name = Str(args, "name");
        string action;

        if (name is null)
        {
            var editing = session.Objects.Where(o => o.Status == ObjectStatus.Editing).ToList();
            if (editing.Count == 0)
                return Task.FromResult(ToolResult.Fail(
                    session.Objects.Count == 0
                        ? "Nothing has been marked yet, so there is nothing to accept."
                        : "Everything marked is already accepted.",
                    "nothing to accept"));

            foreach (var obj in editing) obj.Status = ObjectStatus.Accepted;
            action = $"accepted {string.Join(", ", editing.Select(o => $"'{o.Name}'"))}";
        }
        else
        {
            if (session.Object(name) is not { } obj)
                return Task.FromResult(ToolResult.Fail(
                    $"There is nothing called '{name}' to accept.", "unknown object"));

            obj.Status = ObjectStatus.Accepted;
            action = $"accepted '{name}'";
        }

        return Task.FromResult(RenderResult(chat, session, session.CurrentView, action, cfg));
    }
}
