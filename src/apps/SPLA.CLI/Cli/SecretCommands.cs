using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;

namespace SPLA.CLI;

/// <summary>
/// <c>spla secret list|set|delete</c> — manages the global secret store from the terminal, straight
/// through <see cref="ResolvedSettings.Secrets"/> (no running service needed). Values are entered via
/// hidden prompt and never accepted as command-line arguments, so they don't land in shell history;
/// and they are never printed back — listing shows keys and scope only. This is the CLI half of the
/// "secrets go in out-of-band, never through the chat/LLM" rule.
/// </summary>
internal static class SecretCommands
{
    public static bool IsSecretCommand(string[] args) =>
        args.Length > 0 && args[0].Equals("secret", StringComparison.OrdinalIgnoreCase);

    /// <summary>Runs the secret sub-command. Returns the process exit code.</summary>
    public static async Task<int> RunAsync(ResolvedSettings settings, string[] args)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "";
        var projectOpen = settings.ProjectFilePath != null;
        var scope = ParseScope(args, projectOpen);

        switch (sub)
        {
            case "list":
                await ListAsync(settings.Secrets, scope);
                return 0;

            case "set":
            {
                var key = PositionalArg(args);
                if (key == null) { Console.Error.WriteLine("usage: spla secret set <key> [--project|--machine]"); return 2; }
                if (scope == SecretScope.Project && !projectOpen)
                {
                    Console.Error.WriteLine("No project is open — use --machine or run inside a project.");
                    return 2;
                }
                var value = ReadHidden($"Value for '{key}' ({scope}): ");
                if (string.IsNullOrEmpty(value)) { Console.Error.WriteLine("Empty value — aborted."); return 2; }
                await settings.Secrets.SetAsync(key, value, scope);
                Console.WriteLine($"Stored '{key}' in {scope} scope.");
                return 0;
            }

            case "delete":
            {
                var key = PositionalArg(args);
                if (key == null) { Console.Error.WriteLine("usage: spla secret delete <key> [--project|--machine]"); return 2; }
                var removed = await settings.Secrets.DeleteAsync(key, scope);
                Console.WriteLine(removed ? $"Deleted '{key}' from {scope} scope." : $"'{key}' not found in {scope} scope.");
                return removed ? 0 : 1;
            }

            default:
                Console.Error.WriteLine("usage: spla secret <list|set|delete> [<key>] [--project|--machine]");
                return 2;
        }
    }

    private static async Task ListAsync(ISecretStore store, SecretScope scope)
    {
        var keys = await store.ListKeysAsync(scope);
        Console.WriteLine($"Secrets ({scope} scope): {keys.Count}");
        foreach (var k in keys) Console.WriteLine($"  {k}");
    }

    /// <summary>Scope from flags. Default: project when one is open, else machine.</summary>
    private static SecretScope ParseScope(string[] args, bool projectOpen)
    {
        if (args.Any(a => a.Equals("--machine", StringComparison.OrdinalIgnoreCase))) return SecretScope.Machine;
        if (args.Any(a => a.Equals("--project", StringComparison.OrdinalIgnoreCase))) return SecretScope.Project;
        return projectOpen ? SecretScope.Project : SecretScope.Machine;
    }

    /// <summary>First non-flag token after the sub-command (the key), or null.</summary>
    private static string? PositionalArg(string[] args) =>
        args.Skip(2).FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal));

    /// <summary>Reads a line with no echo. Backspace edits; Enter ends. Never renders the characters.
    /// When stdin is redirected (pipe / script / test), ReadKey isn't available — read a plain line
    /// instead, which is already unechoed in that case.</summary>
    private static string ReadHidden(string prompt)
    {
        Console.Write(prompt);
        if (Console.IsInputRedirected)
        {
            var line = Console.ReadLine() ?? "";
            Console.WriteLine();
            return line;
        }
        var buffer = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); break; }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0) buffer.Length--;
                continue;
            }
            if (!char.IsControl(key.KeyChar)) buffer.Append(key.KeyChar);
        }
        return buffer.ToString();
    }
}
