using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using SPLA.Domain.Settings;
using SPLA.Runtime;

namespace SPLA.CLI;

/// <summary><c>spla chat list</c> — print every saved chat and exit.</summary>
internal sealed class ChatListCommand(ResolvedSettings settings, ILoggerFactory loggerFactory)
    : Command<EmptyCommandSettings>
{
    protected override int Execute(CommandContext context, EmptyCommandSettings _, CancellationToken cancellationToken)
    {
        using var runtime = RuntimeBootstrap.Build(settings, loggerFactory);

        var chats = runtime.ChatManager.ListChats();
        if (chats.Count == 0) { AnsiConsole.MarkupLine("[grey]No saved chats.[/]"); return 0; }

        var table = new Table().AddColumn("Id").AddColumn("Title").AddColumn("Updated");
        foreach (var c in chats)
            table.AddRow(c.Id, c.Title.EscapeMarkup(), c.UpdatedAt.ToString("dd.MM HH:mm"));
        AnsiConsole.Write(table);
        return 0;
    }
}

internal sealed class ChatOpenSettings : CommandSettings
{
    [CommandArgument(0, "[id]")]
    public string? Id { get; init; }
}

/// <summary><c>spla chat open [id]</c> — resumes a saved chat (or starts a new one if omitted/unknown)
/// and drops into the interactive REPL.</summary>
internal sealed class ChatOpenCommand(ResolvedSettings settings, ILoggerFactory loggerFactory)
    : AsyncCommand<ChatOpenSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, ChatOpenSettings s, CancellationToken cancellationToken)
    {
        using var runtime = RuntimeBootstrap.Build(settings, loggerFactory);

        var session = s.Id is { Length: > 0 } id
            ? runtime.ChatManager.LoadChat(id) ?? runtime.ChatManager.CreateNewChat()
            : runtime.ChatManager.CreateNewChat();
        if (s.Id is { Length: > 0 }) Console.WriteLine($"Loaded chat: {session.Title}");

        var chat = new ChatRuntime(runtime, session);
        await InteractiveRepl.RunAsync(runtime, chat);
        return 0;
    }
}

internal sealed class ChatForkSettings : CommandSettings
{
    [CommandArgument(0, "<id>")]
    public required string Id { get; init; }

    [CommandOption("--model")]
    public string? Model { get; init; }
}

/// <summary><c>spla chat fork &lt;id&gt; [--model]</c> — duplicates a saved chat, optionally onto a
/// different model entry, and exits (the fork is not opened here — <c>chat open &lt;new-id&gt;</c>
/// does that).</summary>
internal sealed class ChatForkCommand(ResolvedSettings settings, ILoggerFactory loggerFactory)
    : Command<ChatForkSettings>
{
    protected override int Execute(CommandContext context, ChatForkSettings s, CancellationToken cancellationToken)
    {
        using var runtime = RuntimeBootstrap.Build(settings, loggerFactory);
        var forked = runtime.ChatManager.DuplicateChat(s.Id, s.Model);
        Console.WriteLine($"Forked to new chat: {forked.Id}");
        return 0;
    }
}
