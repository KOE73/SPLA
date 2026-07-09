using SPLA.Domain.Project;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>Project browser: list/recent, open by id, create (by name into the user's area in server
/// mode, else by manifest path).</summary>
internal sealed class ProjectHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
    [
        MessageTypes.ProjectList, MessageTypes.ProjectRecent,
        MessageTypes.ProjectOpen, MessageTypes.ProjectCreate,
    ];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.ProjectList   => List(ctx, ctx.Session.ListProjects()),
        MessageTypes.ProjectRecent => List(ctx, ctx.Session.RecentProjects()),
        MessageTypes.ProjectOpen   => Open(ctx),
        MessageTypes.ProjectCreate => Create(ctx),
        _ => Task.CompletedTask
    };

    private static Task List(RequestContext ctx, IReadOnlyList<ProjectDescriptor> projects)
        => ctx.Reply(MessageTypes.ProjectListResult,
            new ProjectListResultPayload { Projects = projects.Select(ProtocolProjection.ToDto).ToList() });

    private static Task Open(RequestContext ctx)
    {
        var p = ctx.Payload<ProjectOpenPayload>();
        if (string.IsNullOrWhiteSpace(p?.ProjectId))
            return ctx.Error("ProjectId is required.");

        ctx.Session.MarkProjectOpen(p.ProjectId);
        var opened = ctx.Session.Registry.Open(p.ProjectId);
        return ctx.Reply(MessageTypes.ProjectContext, ProtocolProjection.ToContext(p.ProjectId, opened.Runtime));
    }

    private static Task Create(RequestContext ctx)
    {
        var p = ctx.Payload<ProjectCreatePayload>();
        string manifestPath;
        if (ctx.Session.UserProvider is { } userProvider && ctx.Session.UserArea is { } userArea)
        {
            // Server mode: the user gives only a NAME; the project is created inside their own area
            // and registered in their per-user provider (so it shows up on refresh).
            if (string.IsNullOrWhiteSpace(p?.Name))
                return ctx.Error("Project name is required.");

            var safe = ProtocolProjection.SanitizeName(p.Name);
            manifestPath = System.IO.Path.Combine(userArea, safe, safe + ".spla");
            userProvider.Create(new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath, Name = p.Name });
        }
        else
        {
            if (string.IsNullOrWhiteSpace(p?.ManifestPath))
                return ctx.Error("ManifestPath is required.");

            manifestPath = p.ManifestPath;
            ctx.Session.Registry.Create(new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath, Name = p.Name });
        }

        ctx.Session.MarkProjectOpen(manifestPath);
        var opened = ctx.Session.Registry.Open(manifestPath);
        return ctx.Reply(MessageTypes.ProjectContext, ProtocolProjection.ToContext(manifestPath, opened.Runtime));
    }
}
