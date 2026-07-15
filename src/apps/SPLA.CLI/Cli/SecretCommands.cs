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
                if (key == null) { Console.Error.WriteLine("usage: spla secret set <key> [--field <name>] [--project|--machine]"); return 2; }
                if (scope == SecretScope.Project && !projectOpen)
                {
                    Console.Error.WriteLine("No project is open — use --machine or run inside a project.");
                    return 2;
                }
                var field = FlagValue(args, "--field") ?? SecretFields.Password;
                var value = ReadHidden($"Value for '{key}#{field}' ({scope}): ");
                if (string.IsNullOrEmpty(value)) { Console.Error.WriteLine("Empty value — aborted."); return 2; }

                // Merge over the existing entry so adding a field never wipes its siblings.
                var fields = (await settings.Secrets.GetEntryAsync(key, scope))?.Fields
                    .ToDictionary(f => f.Key, f => f.Value, StringComparer.OrdinalIgnoreCase)
                    ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                fields[field] = value;
                await settings.Secrets.SetEntryAsync(key, fields, scope);
                Console.WriteLine($"Stored '{key}#{field}' in {scope} scope.");
                return 0;
            }

            case "delete":
            {
                var key = PositionalArg(args);
                if (key == null) { Console.Error.WriteLine("usage: spla secret delete <key> [--field <name>] [--project|--machine]"); return 2; }
                if (FlagValue(args, "--field") is { } fieldName)
                {
                    var entry = await settings.Secrets.GetEntryAsync(key, scope);
                    var fields = entry?.Fields.ToDictionary(f => f.Key, f => f.Value, StringComparer.OrdinalIgnoreCase);
                    if (fields is null || !fields.Remove(fieldName))
                    {
                        Console.WriteLine($"'{key}#{fieldName}' not found in {scope} scope.");
                        return 1;
                    }
                    await settings.Secrets.SetEntryAsync(key, fields, scope); // empty set deletes the entry
                    Console.WriteLine($"Deleted '{key}#{fieldName}' from {scope} scope.");
                    return 0;
                }
                var removed = await settings.Secrets.DeleteAsync(key, scope);
                Console.WriteLine(removed ? $"Deleted '{key}' from {scope} scope." : $"'{key}' not found in {scope} scope.");
                return removed ? 0 : 1;
            }

            default:
                Console.Error.WriteLine("usage: spla secret <list|set|delete> [<key>] [--field <name>] [--project|--machine]");
                return 2;
        }
    }

    private static async Task ListAsync(ISecretStore store, SecretScope scope)
    {
        var entries = await store.ListEntriesAsync(scope);
        Console.WriteLine($"Secrets ({scope} scope): {entries.Count}");
        foreach (var e in entries) Console.WriteLine($"  {e.Key} ({string.Join(", ", e.Fields)})");
    }

    /// <summary>Value following a flag token (e.g. <c>--field user</c>), or null.</summary>
    private static string? FlagValue(string[] args, string flag)
    {
        var i = Array.FindIndex(args, a => a.Equals(flag, StringComparison.OrdinalIgnoreCase));
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    /// <summary>Scope from flags. Default: project when one is open, else machine.</summary>
    private static SecretScope ParseScope(string[] args, bool projectOpen)
    {
        if (args.Any(a => a.Equals("--machine", StringComparison.OrdinalIgnoreCase))) return SecretScope.Machine;
        if (args.Any(a => a.Equals("--project", StringComparison.OrdinalIgnoreCase))) return SecretScope.Project;
        return projectOpen ? SecretScope.Project : SecretScope.Machine;
    }

    /// <summary>First non-flag token after the sub-command (the key), or null. Skips flag values
    /// (the token after <c>--field</c>).</summary>
    private static string? PositionalArg(string[] args)
    {
        for (var i = 2; i < args.Length; i++)
        {
            if (args[i].Equals("--field", StringComparison.OrdinalIgnoreCase)) { i++; continue; }
            if (!args[i].StartsWith("--", StringComparison.Ordinal)) return args[i];
        }
        return null;
    }

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
