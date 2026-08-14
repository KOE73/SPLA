using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using SPLA.Domain.Settings;

namespace SPLA.CLI.Batch;

internal sealed class ChatRunSettings : CommandSettings
{
    [CommandOption("--prompt")]
    [Description("Prompt text, sent as the user turn. Repeatable — one cell per prompt × model.")]
    public string[] Prompts { get; init; } = [];

    [CommandOption("--prompt-file")]
    [Description("Prompt read from a file. Repeatable; can be combined with --prompt.")]
    public string[] PromptFiles { get; init; } = [];

    [CommandOption("--model")]
    [Description("Model entry id from the project's connections. Repeatable. 'all' = every entry.")]
    public string[] Models { get; init; } = [];

    [CommandOption("--out")]
    [Description("Where results go. A directory (created if missing) unless --overwrite is set. Screen only if omitted.")]
    public string? Out { get; init; }

    [CommandOption("--out-name")]
    [Description("File name template inside --out. Placeholders: {timestamp} {prompt} {model} {label}.")]
    public string OutName { get; init; } = "{timestamp} {label}";

    [CommandOption("--overwrite")]
    [Description("Treat --out as one literal file: every cell overwrites it, instead of one file per cell.")]
    public bool Overwrite { get; init; }

    [CommandOption("--md-clean")]
    [Description("Ask the model for one final clean-Markdown message with no chatty wrapping, suitable for a direct file dump.")]
    public bool MdClean { get; init; }

    [CommandOption("--sys-prompt")]
    [Description("Extra system-prompt text for this run, added after the project's own custom prompt — never replaces it.")]
    public string? SysPrompt { get; init; }

    [CommandOption("--skill")]
    [Description("Skill id handed to every cell's chat before its prompt runs.")]
    public string? Skill { get; init; }

    [CommandOption("--show-prompt")]
    [Description("Print the assembled system prompt's contributor manifest before running.")]
    public bool ShowPrompt { get; init; }

    [CommandOption("--show-prompt-file")]
    [Description("Write the full assembled system prompt text to this file before running.")]
    public string? ShowPromptFile { get; init; }

    [CommandOption("--stream")]
    [Description("Echo the model's streamed output to the console while it runs.")]
    public bool Stream { get; init; }

    [CommandOption("--temp")]
    public double? Temperature { get; init; }

    [CommandOption("--reasoning")]
    public string? ReasoningLevel { get; init; }

    [CommandOption("--timeout")]
    public int? TimeoutSeconds { get; init; }

    [CommandOption("--dry-run")]
    [Description("Print the matrix and the planned output names, run nothing.")]
    public bool DryRun { get; init; }

    public override ValidationResult Validate()
    {
        if (Prompts.Length == 0 && PromptFiles.Length == 0)
            return ValidationResult.Error("give at least one --prompt or --prompt-file");
        return ValidationResult.Success();
    }
}

/// <summary>
/// <c>spla chat run</c> — runs one or more prompts against one or more models, each in its own fresh
/// chat, headlessly (no tool permission prompts — every tool call is denied). Prints results to the
/// screen, or writes them to files under <c>--out</c>.
/// </summary>
internal sealed class ChatRunCommand(ResolvedSettings settings, ILoggerFactory loggerFactory)
    : AsyncCommand<ChatRunSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, ChatRunSettings s, CancellationToken cancellationToken)
    {
        var prompts = new List<PromptItem>();
        for (var i = 0; i < s.Prompts.Length; i++)
            prompts.Add(new PromptItem($"text{i + 1}", s.Prompts[i]));
        foreach (var path in s.PromptFiles)
        {
            if (!File.Exists(path)) { AnsiConsole.MarkupLine($"[red]prompt file not found:[/] {path.EscapeMarkup()}"); return 2; }
            prompts.Add(new PromptItem(Path.GetFileNameWithoutExtension(path), await File.ReadAllTextAsync(path)));
        }

        // Purely additive, and built BEFORE the runtime: the composer takes its contributors at
        // construction and is immutable afterwards. See CliContributor's own doc comment.
        var cli = new CliContributor();
        if (s.SysPrompt is { Length: > 0 } sysPrompt)
            cli.AddText("sys-prompt", "CLI system prompt", sysPrompt);
        if (s.MdClean)
            cli.AddText("md-clean", "Output formatting",
                "Ответ дай одним финальным сообщением в чистом Markdown — без вступлений, " +
                "заключений и разговорных оговорок. Это сообщение будет сохранено в файл как есть.");

        using var runtime = RuntimeBootstrap.Build(settings, loggerFactory, [cli]);

        if (s.ShowPrompt || s.ShowPromptFile != null)
        {
            var composed = runtime.ComposeContext();
            if (s.ShowPrompt)
            {
                var manifest = new Table().AddColumn("Contributor").AddColumn("Source").AddColumn("Title").AddColumn("~Tokens");
                foreach (var e in composed.Manifest.Entries)
                    manifest.AddRow(e.Contributor, e.Source, e.Title, e.ApproxTokens.ToString());
                AnsiConsole.Write(manifest);
            }
            if (s.ShowPromptFile != null)
                await File.WriteAllTextAsync(s.ShowPromptFile, composed.SystemPrompt);
        }

        var wantAll = s.Models.Any(m => m.Equals("all", StringComparison.OrdinalIgnoreCase));
        var models = wantAll
            ? settings.Models
            : s.Models.Length > 0
                ? s.Models.Select(id => settings.FindModel(id)).Where(m => m != null).Cast<ResolvedModelEntry>().ToList()
                : settings.Models.Take(1).ToList();

        if (models.Count == 0) { AnsiConsole.MarkupLine("[red]no matching model entries — check --model / the project's connections[/]"); return 2; }

        var cells = prompts.SelectMany(p => models.Select(m => new BatchCell(p, m))).ToList();

        if (s.DryRun)
        {
            var table = new Table().AddColumn("Prompt").AddColumn("Model").AddColumn("Output");
            foreach (var cell in cells)
                table.AddRow(cell.Prompt.Name, cell.Model.DisplayName,
                    PlannedOutput(s, cell) ?? "(screen)");
            AnsiConsole.Write(table);
            return 0;
        }

        var runner = new BatchRunner(runtime, settings)
        {
            Temperature = s.Temperature,
            ReasoningLevel = s.ReasoningLevel,
            TimeoutSeconds = s.TimeoutSeconds,
            SkillId = s.Skill,
            Stream = s.Stream
        };

        var failures = 0;
        foreach (var cell in cells)
        {
            var result = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync($"{cell.Prompt.Name} · {cell.Model.DisplayName} …",
                    _ => runner.RunOneAsync(cell, CancellationToken.None));

            if (result.Status != "ok")
            {
                failures++;
                AnsiConsole.MarkupLine($"[red]✗[/] {cell.Prompt.Name} · {cell.Model.DisplayName} — {result.Status}: {result.Note?.EscapeMarkup()}");
                continue;
            }

            var outPath = ResolveOutputPath(s, cell);
            if (outPath == null)
            {
                AnsiConsole.Write(new Panel(result.Text!.EscapeMarkup())
                    .Header($"[green]{cell.Prompt.Name} · {cell.Model.DisplayName}[/]")
                    .Expand());
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                await File.WriteAllTextAsync(outPath, result.Text + "\n");
                AnsiConsole.MarkupLine($"[green]✓[/] {cell.Prompt.Name} · {cell.Model.DisplayName} → {outPath.EscapeMarkup()} ({result.Elapsed:mm\\:ss})");
            }
        }

        return failures == 0 ? 0 : 1;
    }

    /// <summary>Null means "screen" — the same rule <see cref="ResolveOutputPath"/> uses, kept separate
    /// only so <c>--dry-run</c> can call it without a real timestamp per cell.</summary>
    private static string? PlannedOutput(ChatRunSettings s, BatchCell cell)
    {
        if (s.Out == null) return null;
        return s.Overwrite
            ? s.Out
            : OutputNaming.BuildPath(s.Out, s.OutName, DateTimeOffset.Now, cell.Prompt, cell.Model.Id);
    }

    private static string? ResolveOutputPath(ChatRunSettings s, BatchCell cell)
    {
        if (s.Out == null) return null;
        if (s.Overwrite) return s.Out;
        Directory.CreateDirectory(s.Out);
        return OutputNaming.BuildPath(s.Out, s.OutName, DateTimeOffset.Now, cell.Prompt, cell.Model.Id);
    }
}
