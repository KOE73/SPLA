using SPLA.Domain.Project;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// What this process is doing, and whether it may be told to stop. Both messages answer with the
/// same <see cref="InstanceStatusPayload"/> shape — a refused stop is a status answer with a reason
/// attached, not a different kind of message.
/// </summary>
internal sealed class InstanceHandlers : IMessageHandler
{
    /// <summary>Same threshold <see cref="ServiceOptions.StallAfter"/> defaults to. A handler has no
    /// <see cref="ServiceOptions"/> to read (it only sees <see cref="IClientSession"/>), and the value
    /// is a judgement about how long silence means "stuck" rather than something a deployment tunes —
    /// so a literal here matching the default is simpler than threading the option through.</summary>
    private static readonly TimeSpan StallAfter = TimeSpan.FromMinutes(10);

    public IEnumerable<string> HandledTypes => [MessageTypes.InstanceStatus, MessageTypes.InstanceStop];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.InstanceStatus => Status(ctx),
        MessageTypes.InstanceStop   => Stop(ctx),
        _ => Task.CompletedTask
    };

    private static Task Status(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        return ctx.Reply(MessageTypes.InstanceStatusResult, Describe(ctx, entry.Runtime, projectId));
    }

    private static Task Stop(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var runtime = entry.Runtime;
        var p = ctx.Payload<InstanceStopPayload>() ?? new InstanceStopPayload();

        var state = runtime.State(StallAfter);
        if (!p.Force && !InstanceStates.MayEvict(state))
        {
            var status = Describe(ctx, runtime, projectId);
            status.Stopping = false;
            status.Refusal = RefusalFor(state);
            return ctx.Reply(MessageTypes.InstanceStatusResult, status);
        }

        // Forced past a busy state: nothing may be left blocked on a person who will never be asked
        // again, and no turn may be left running past the process that was running it — the same
        // reasoning AgentRuntime.Dispose already applies on an orderly exit, made explicit here because
        // a stop that is about to happen anyway must not leave half-finished work looking alive.
        if (p.Force)
            runtime.Turns.CancelAll();

        var accepted = Describe(ctx, runtime, projectId);
        accepted.Stopping = true;

        // Reply before the process goes away — a client that asked "did it work" deserves an answer,
        // not a dropped socket it has to interpret. ShutdownRequested only signals intent; the host
        // (ServeCommand, subscribed next to LeaseExpired) decides how the process actually ends.
        var reply = ctx.Reply(MessageTypes.InstanceStatusResult, accepted);
        ctx.Session.Registry.RequestShutdown();
        return reply;
    }

    private static InstanceStatusPayload Describe(RequestContext ctx, SPLA.Runtime.AgentRuntime runtime, string projectId)
        => new()
        {
            InstanceId = runtime.Instance?.Info.InstanceId ?? "",
            Mode = runtime.Instance?.Info.Mode ?? "",
            State = InstanceStates.Name(runtime.State(StallAfter)),
            ProjectName = runtime.Settings.ProjectName,
            Clients = ctx.Session.Hub.CountForProject(projectId)
        };

    /// <summary>Names what is holding the instance open, for a refusal a person reads rather than
    /// decodes from the state name alone.</summary>
    private static string RefusalFor(InstanceState state) => state switch
    {
        InstanceState.Working => "a turn is running",
        InstanceState.Waiting => "waiting for an answer",
        InstanceState.Stalled => "a turn stopped halfway",
        _ => "busy"
    };
}
