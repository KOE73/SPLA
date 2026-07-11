using SPLA.Domain.Models;
using SPLA.Service;

namespace SPLA.CLI;

/// <summary>The primary interactive console loop: reads a line, handles the local <c>/skills</c>
/// commands, otherwise drives one agent turn against <paramref name="chat"/> and streams it to
/// stdout.</summary>
internal static class InteractiveRepl
{
    public static async Task RunAsync(AgentRuntime runtime, ChatRuntime chat)
    {
        while (true)
        {
            Console.Write("\nYou: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) break;
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;

            if (TryHandleSkillsCommand(runtime, chat, input)) continue;

            var callbacks = ConsoleHandlers.RichCallbacks(runtime);
            var permHandler = ConsoleHandlers.Permission(colored: true);
            var clarifyHandler = ConsoleHandlers.Clarify();

            Console.Write("SPLA: ");
            try
            {
                await chat.SendAsync(input, callbacks, permHandler, clarifyHandler, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Error]: {ex.Message}");
            }
        }
    }

    /// <summary>Handles the local <c>/skills</c> and <c>/skills load &lt;id&gt;</c> commands; returns
    /// true when the input was one of them (and should not be sent to the agent).</summary>
    private static bool TryHandleSkillsCommand(AgentRuntime runtime, ChatRuntime chat, string input)
    {
        if (input.Equals("/skills", StringComparison.OrdinalIgnoreCase))
        {
            var all = runtime.SkillManager.GetAll();
            if (all.Count == 0) Console.WriteLine("No skills available.");
            else foreach (var s in all)
                Console.WriteLine($"  [{(s.IsEnabled ? "on " : "off")}] {s.Id} — {s.Description}");
            return true;
        }

        if (input.StartsWith("/skills load ", StringComparison.OrdinalIgnoreCase))
        {
            var id = input["/skills load ".Length..].Trim();
            var body = runtime.SkillManager.LoadBody(id);
            if (body == null) { Console.WriteLine($"Skill '{id}' not found."); return true; }
            chat.InjectMessage(ChatRole.User, $"[Skill loaded: {id}]\n\n{body}");
            Console.WriteLine($"Skill '{id}' loaded into context.");
            return true;
        }

        return false;
    }
}
