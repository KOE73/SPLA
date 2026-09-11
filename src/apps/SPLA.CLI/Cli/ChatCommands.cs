using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using SPLA.CLI.Batch;
using SPLA.CLI.Wire;
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

    [CommandOption("--role")]
    [Description("Role to stamp on a NEW chat (id omitted, or an unknown id) — matched against the project's declared roles: list, case-insensitive. Has no effect when [id] resolves to an existing chat: that chat already carries whatever role it was created with.")]
    public string? Role { get; init; }
}

/// <summary><c>spla chat open [id]</c> — resumes a saved chat (or starts a new one if omitted/unknown)
/// and drops into the interactive REPL.</summary>
internal sealed class ChatOpenCommand(ResolvedSettings settings, ILoggerFactory loggerFactory)
    : AsyncCommand<ChatOpenSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, ChatOpenSettings s, CancellationToken cancellationToken)
    {
        string? role = null;
        if (s.Role is { Length: > 0 } requestedRole)
        {
            role = RoleValidation.Resolve(settings, requestedRole);
            if (role == null) return 2;
        }

        using var runtime = RuntimeBootstrap.Build(settings, loggerFactory);

        var existing = s.Id is { Length: > 0 } id ? runtime.ChatManager.LoadChat(id) : null;
        var session = existing ?? runtime.ChatManager.CreateNewChat(role: role);
        if (existing != null) Console.WriteLine($"Loaded chat: {session.Title}");

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

internal sealed class ChatCompactSettings : CommandSettings
{
    [CommandArgument(0, "<id>")]
    public required string Id { get; init; }
}

/// <summary><c>spla chat compact &lt;id&gt;</c> — batch-mode entry point for
/// <c>docs/adr/ADR_20260911-3_agent_compaction.md</c> §2.6. A project a live instance already holds
/// is compacted over the wire (mirrors <see cref="RemoteChatRun"/>'s attach-instead-of-refuse
/// pattern, one-writer rule intact); otherwise this process opens the chat itself.</summary>
internal sealed class ChatCompactCommand(ResolvedSettings settings, ILoggerFactory loggerFactory)
    : AsyncCommand<ChatCompactSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, ChatCompactSettings s, CancellationToken cancellationToken)
    {
        if (RemoteChatRun.LiveInstance(settings) is { } holder)
        {
            AnsiConsole.MarkupLine($"[grey]Attached to[/] {holder.Describe().EscapeMarkup()}");
            var token = settings.SecretResolver.Resolve(Environment.GetEnvironmentVariable("SPLA_SERVICE_TOKEN"));
            await using var client = await CliWireClient.ConnectAsync(holder.Endpoint!, token, cancellationToken);
            try
            {
                await client.CompactAsync(s.Id, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message.EscapeMarkup()}[/]");
                return 1;
            }
            Console.WriteLine("Compacted.");
            return 0;
        }

        using var runtime = RuntimeBootstrap.Build(settings, loggerFactory);
        var session = runtime.ChatManager.LoadChat(s.Id);
        if (session == null) { AnsiConsole.MarkupLine($"[red]No such chat: {s.Id.EscapeMarkup()}[/]"); return 2; }

        var chat = new ChatRuntime(runtime, session);
        var result = await chat.CompactAsync(cancellationToken);
        if (!result.Compacted)
        {
            var message = result.Refusal switch
            {
                ChatRuntime.CompactRefusal.Busy => "a turn is running",
                ChatRuntime.CompactRefusal.NothingToCompact => "nothing to compact yet",
                _ => result.Error ?? "the model call did not produce a summary"
            };
            AnsiConsole.MarkupLine($"[red]Compact failed —[/] {message.EscapeMarkup()}");
            return 1;
        }
        Console.WriteLine("Compacted.");
        return 0;
    }
}
