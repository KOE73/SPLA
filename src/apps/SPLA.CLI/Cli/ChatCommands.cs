using SPLA.Domain.Models;
using SPLA.Service;

namespace SPLA.CLI;

/// <summary>The <c>spla chat …</c> sub-commands (list / fork / open) and the open-or-create fallback
/// that yields the <see cref="ChatSession"/> the REPL runs against.</summary>
internal static class ChatCommands
{
    /// <summary>Handles a terminal <c>chat</c> sub-command (list / fork) that prints and exits.
    /// Returns true when the process should stop after this call.</summary>
    public static bool TryHandleTerminal(AgentRuntime runtime, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: spla chat <list|open|fork> [id] [--model name]");
            return true;
        }

        var cmd = args[1].ToLower();
        if (cmd == "list")
        {
            Console.WriteLine("Saved chats:");
            foreach (var c in runtime.ChatManager.ListChats())
                Console.WriteLine($"- {c.Id} | {c.Title} | {c.UpdatedAt:dd.MM HH:mm}");
            return true;
        }

        if (cmd == "fork" && args.Length > 2)
        {
            var model = args.Length > 4 && args[3] == "--model" ? args[4] : null;
            var forked = runtime.ChatManager.DuplicateChat(args[2], model);
            Console.WriteLine($"Forked to new chat: {forked.Id}");
            return true;
        }

        return false;
    }

    /// <summary>Opens the chat named by <c>chat open &lt;id&gt;</c>, or creates a fresh one.</summary>
    public static ChatSession OpenOrCreate(AgentRuntime runtime, string[] args, bool isChatCommand)
    {
        if (isChatCommand && args.Length > 2 && args[1].ToLower() == "open")
        {
            var loaded = runtime.ChatManager.LoadChat(args[2]) ?? runtime.ChatManager.CreateNewChat();
            Console.WriteLine($"Loaded chat: {loaded.Title}");
            return loaded;
        }
        return runtime.ChatManager.CreateNewChat();
    }
}
