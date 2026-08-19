using Spectre.Console;
using SPLA.CLI.Wire;
using SPLA.Domain.Project;
using SPLA.Domain.Settings;
using SPLA.Service.Contracts;

namespace SPLA.CLI.Batch;

/// <summary>
/// <c>chat run</c> against a project somebody else already has open.
///
/// <para>The one-writer rule would otherwise make the most ordinary thing anyone does impossible:
/// leave the app open on a project and fire a scripted run at it. Rather than refusing, the run
/// connects to the live instance over the same protocol the windows use — which also means the run
/// appears in the open window instead of racing it for the same files.</para>
///
/// <para><b>Deliberately narrow.</b> A remote instance composes its own system prompt, so anything
/// this invocation would have added locally (<c>--sys-prompt</c>, <c>--md-clean</c>) cannot travel,
/// and the per-cell statistics come from the local pipeline that is not running here. Rather than
/// silently dropping those flags, the run is refused and says which one is in the way. What is
/// supported is stated once, in <see cref="Unsupported"/>, so the list cannot drift from the check.</para>
/// </summary>
internal static class RemoteChatRun
{
    /// <summary>The live instance holding this project, or null when nobody does (or when it is not
    /// serving — a REPL holds the project without offering an address to dial).</summary>
    public static InstanceInfo? LiveInstance(ResolvedSettings settings)
    {
        if (!settings.HasProject) return null;
        var info = InstanceLock.Read(Path.Combine(settings.WorkspacePath, ".spla"));
        return info?.Endpoint is { Length: > 0 } ? info : null;
    }

    /// <summary>The first flag that cannot cross the wire, or null when the run can be handed over.</summary>
    public static string? Unsupported(ChatRunSettings s)
    {
        if (s.SysPrompt is { Length: > 0 }) return "--sys-prompt";
        if (s.SysPromptFile is { Length: > 0 }) return "--sys-prompt-file";
        if (s.MdClean) return "--md-clean";
        if (s.ShowPrompt) return "--show-prompt";
        if (s.ShowPromptFile is { Length: > 0 }) return "--show-prompt-file";
        if (s.ShowStatistic) return "--show-statistic";
        if (s.StatisticFile.IsSet) return "--show-statistic-file";
        // Model, temperature and reasoning are properties of the chat on the other side. They could
        // be set over the wire, but a run that quietly changed a live instance's chat settings would
        // be reaching further into somebody else's session than a scripted run has any business to.
        if (s.Models.Length > 0) return "--model";
        if (s.Temperature.HasValue) return "--temp";
        if (s.ReasoningLevel is { Length: > 0 }) return "--reasoning";
        if (s.Skill is { Length: > 0 }) return "--skill";
        return null;
    }

    /// <summary>Explains a run that cannot be handed over, naming both halves of the problem.</summary>
    public static string Refusal(InstanceInfo holder, string flag)
        => $"This project is open by {holder.Describe()}, so this run would connect to it — "
         + $"but {flag} is applied to the prompt before it is sent, and the instance composes its own. "
         + "Drop the flag, or close what has the project.";

    /// <summary>
    /// Runs each prompt in its own fresh chat on the live instance, streaming the answer back.
    /// </summary>
    /// <returns>Process exit code: 0 when every prompt finished, 1 when any failed.</returns>
    public static async Task<int> RunAsync(
        InstanceInfo holder, ChatRunSettings s, IReadOnlyList<PromptItem> prompts,
        ResolvedSettings settings, CancellationToken ct)
    {
        AnsiConsole.MarkupLine($"[grey]Attached to[/] {holder.Describe().EscapeMarkup()}");

        // The instance's own token, if it wants one. A loopback service ignores it; a remote one does
        // not, and the reference form keeps it out of this process's command line.
        var token = settings.SecretResolver.Resolve(Environment.GetEnvironmentVariable("SPLA_SERVICE_TOKEN"));

        await using var client = await CliWireClient.ConnectAsync(holder.Endpoint!, token, ct);

        var failures = 0;
        foreach (var prompt in prompts)
        {
            var chatId = await client.NewChatAsync(prompt.Name, ct);
            var answer = new System.Text.StringBuilder();

            var error = await client.SendAndStreamAsync(
                chatId,
                prompt.Text,
                text =>
                {
                    answer.Append(text);
                    if (s.Stream) Console.Write(text);
                },
                note => AnsiConsole.MarkupLine($"[grey]{note.EscapeMarkup()}[/]"),
                // Headless, exactly as the local path: a scripted run has nobody to ask, and a run
                // that silently allowed tool calls would be a very different thing from what the
                // person typed. The window watching this chat sees the question resolve as denied.
                _ => new PermissionDecisionPayload { Decision = "deny" },
                ct);

            if (s.Stream) Console.WriteLine();

            if (error != null)
            {
                AnsiConsole.MarkupLine($"[red]{prompt.Name.EscapeMarkup()}:[/] {error.EscapeMarkup()}");
                failures++;
                continue;
            }

            var path = OutputPath(s, prompt, holder);
            if (path != null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
                await File.WriteAllTextAsync(path, answer.ToString(), ct);
                AnsiConsole.MarkupLine($"[green]→[/] {path.EscapeMarkup()}");
            }
            else if (!s.Stream)
            {
                AnsiConsole.WriteLine(answer.ToString());
            }
        }

        return failures == 0 ? 0 : 1;
    }

    /// <summary>Same naming as a local run, so a folder of results does not betray which side produced
    /// them. The model column is the instance's choice, not ours — it is named accordingly.</summary>
    private static string? OutputPath(ChatRunSettings s, PromptItem prompt, InstanceInfo holder)
    {
        if (s.Out == null) return null;
        if (s.Overwrite) return s.Out;
        Directory.CreateDirectory(s.Out);
        // The model was the instance's choice, not this invocation's, and saying so beats printing a
        // model id nobody here selected.
        return OutputNaming.BuildPath(s.Out, s.OutName, DateTimeOffset.Now, prompt, "instance");
    }
}
