using System.Diagnostics;
using System.Text;
using SPLA.Agent;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Permissions;
using SPLA.Runtime;

namespace SPLA.Demo.Summarizer;

/// <summary>What one cell of the matrix produced.</summary>
public sealed record RunResult(
    string Status, string? OutputPath, string? Note, TimeSpan Elapsed, int Chars);

/// <summary>
/// Runs the matrix. One cell = one prompt variant × one model target, in a fresh chat, so no cell
/// ever sees another's answer — the whole point of comparing attempts is that they are independent.
/// </summary>
public sealed class Runner
{
    private readonly AgentRuntime _runtime;
    private readonly ChatRegistry _chats;
    private readonly ResolvedSettings _settings;
    private readonly SummarizeConfig _cfg;
    private readonly Options _opts;

    /// <summary>The wire name the lmstudio entry had before we started swapping models into it.</summary>
    private readonly string? _lmOriginalModel;
    private readonly ResolvedModelEntry? _lmEntry;

    public Runner(AgentRuntime runtime, ResolvedSettings settings, SummarizeConfig cfg, Options opts)
    {
        _runtime = runtime;
        _chats = new ChatRegistry(runtime);
        _settings = settings;
        _cfg = cfg;
        _opts = opts;

        _lmEntry = settings.Models.FirstOrDefault(m =>
            string.Equals(m.Provider, "lmstudio", StringComparison.OrdinalIgnoreCase));
        _lmOriginalModel = _lmEntry?.Entry.Model;
    }

    /// <summary>Puts the lmstudio entry back the way the manifest had it. The manifest is not rewritten
    /// — the swap only ever lived in memory — but a later run in the same process must not inherit it.</summary>
    public void RestoreLmEntry()
    {
        if (_lmEntry != null && _lmOriginalModel != null) _lmEntry.Entry.Model = _lmOriginalModel;
    }

    /// <summary>Makes <paramref name="target"/> the model LM Studio actually has in memory: unloads
    /// whatever is loaded, loads this key, and points the entry's wire name at it.</summary>
    private async Task PrepareLmAsync(ModelTarget target, CancellationToken ct)
    {
        if (target.LmKey == null || _lmEntry == null) return;

        var endpoint = _lmEntry.Endpoint ?? "";
        var apiKey = _lmEntry.ApiKey ?? "lm-studio";

        var loaded = await _runtime.ModelManagement.GetModelDetailsAsync(endpoint, apiKey, ct);
        var already = loaded.FirstOrDefault(m =>
            m.IsLoaded && string.Equals(m.Id, target.LmKey, StringComparison.OrdinalIgnoreCase));

        if (already == null)
        {
            foreach (var m in loaded.Where(m => m.IsLoaded))
                await _runtime.ModelManagement.UnloadModelAsync(endpoint, apiKey, m.UnloadId, ct);

            Console.WriteLine($"   загружаю в LM Studio: {target.LmKey} …");
            await _runtime.ModelManagement.LoadModelAsync(endpoint, apiKey, target.LmKey, ct);
        }

        _lmEntry.Entry.Model = target.LmKey;
    }

    /// <summary>Runs one cell and writes its file.</summary>
    public async Task<RunResult> RunOneAsync(
        string sourcePath, string sourceText, string outDir,
        PromptVariant prompt, ModelTarget target, CancellationToken ct)
    {
        var started = DateTimeOffset.Now;
        var clock = Stopwatch.StartNew();

        try { await PrepareLmAsync(target, ct); }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return new RunResult("сбой", null, $"LM Studio не дала модель: {ex.Message}", clock.Elapsed, 0);
        }

        // The prompt for this cell. Both placements are live per turn: the context surface is
        // recomposed on every LLM call, so mutating settings here is all it takes.
        var place = _opts.PromptPlace?.Trim().ToLowerInvariant() switch
        {
            "user" => PromptPlace.User,
            "both" => PromptPlace.Both,
            "system" => PromptPlace.System,
            _ => _cfg.Place
        };
        _settings.CustomPrompt = place is PromptPlace.System or PromptPlace.Both ? prompt.Text : null;
        if (_opts.Temperature is { } temp) _settings.Temperature = temp;
        if (_opts.ReasoningLevel is { } reasoning) _settings.ReasoningLevel = reasoning;

        var userTurn = _cfg.UserFrame
            .Replace("{document}", sourceText)
            .Replace("{prompt}", place is PromptPlace.User or PromptPlace.Both ? prompt.Text : "");

        var chat = _chats.CreateNew($"{prompt.Name} · {target.Label} · {started:yyyy-MM-dd HH:mm}");
        chat.ApplySettings(mode: null, modelId: target.EntryId);

        // Does the document plainly not fit? Worth knowing before a 4B model spends ten minutes
        // discovering it. The estimate is deliberately crude — ~3 characters per token for Russian.
        var window = await chat.GetContextLengthAsync(ct);
        var estimate = (userTurn.Length + prompt.Text.Length) / 3;
        if (_cfg.SkipIfTooLarge && window is > 0 && estimate + 2000 > window)
            return new RunResult("не влезает", null,
                $"~{estimate} токенов против окна {window}", clock.Elapsed, 0);

        var answer = new StringBuilder();
        var stream = new StringBuilder();
        long promptTokens = 0, completionTokens = 0;
        var abandoned = 0;
        var quiet = _opts.Quiet || !_cfg.Echo;

        var callbacks = new AgentCallbacks
        {
            OnDelta = chunk =>
            {
                stream.Append(chunk);
                if (!quiet) Console.Write(chunk);
                return Task.CompletedTask;
            },
            OnReasoning = chunk =>
            {
                if (!quiet && _cfg.ShowReasoning) Console.Write(chunk);
                return Task.CompletedTask;
            },
            OnAssistantMessage = m =>
            {
                if (!string.IsNullOrWhiteSpace(m.Content)) answer.Append(m.Content);
                if (!quiet) Console.WriteLine();
                return Task.CompletedTask;
            },
            OnAttempt = _ => abandoned++,
            OnNotice = n => { Console.WriteLine($"\n   [notice] {n}"); return Task.CompletedTask; },
            OnTokenUsage = (p, c) =>
            {
                promptTokens += p ?? 0;
                completionTokens += c ?? 0;
                _runtime.TokenUsageProject.Record(p, c);
                _runtime.TokenUsageGlobal.Record(p, c);
            }
        };

        Func<ToolFunctionDefinition, string, Task<PermissionDecision>> deny =
            (_, _) => Task.FromResult(PermissionDecision.Deny);
        Func<ClarifyRequest, Task<string?>> noClarify = _ => Task.FromResult<string?>(null);

        var timeout = _opts.TimeoutSeconds ?? _cfg.TimeoutSeconds;
        using var runCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (timeout > 0) runCts.CancelAfter(TimeSpan.FromSeconds(timeout));

        try
        {
            await chat.SendAsync(userTurn, callbacks, deny, noClarify, runCts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return new RunResult("таймаут", null, $"нет ответа за {timeout} с", clock.Elapsed, 0);
        }
        catch (Exception ex)
        {
            return new RunResult("сбой", null, ex.Message, clock.Elapsed, 0);
        }

        clock.Stop();

        var text = Unfence((answer.Length > 0 ? answer : stream).ToString().Trim());
        if (text.Length == 0)
            return new RunResult("пусто", null, "модель не выдала текста", clock.Elapsed, 0);

        var outPath = BuildOutputPath(outDir, sourcePath, prompt, target, started);
        var body = _cfg.FrontMatter
            ? FrontMatter(sourcePath, sourceText, prompt, target, place, window,
                  promptTokens, completionTokens, abandoned, clock.Elapsed, started) + text + "\n"
            : text + "\n";

        await File.WriteAllTextAsync(outPath, body, new UTF8Encoding(false), ct);
        return new RunResult("готово", outPath, null, clock.Elapsed, text.Length);
    }

    /// <summary>The output path: source base name, the marker, the stamp, which prompt, which model.
    /// The stamp and the model are what the person asked for — several attempts must not blend, and
    /// the attempt is worthless if you cannot tell which model made it.</summary>
    public string BuildOutputPath(
        string outDir, string sourcePath, PromptVariant prompt, ModelTarget target, DateTimeOffset stamp)
    {
        var baseName = Path.GetFileNameWithoutExtension(sourcePath);
        var tail = $" - {_cfg.OutputMarker} {stamp:yyyyMMdd-HHmmss} {prompt.Name} {target.Label}";

        // Windows path components stop at 255, and the folder path eats into it. Only the base name is
        // trimmed: the tail is the whole reason the file is identifiable.
        var room = Math.Max(8, 150 - tail.Length);
        if (baseName.Length > room) baseName = baseName[..room].TrimEnd();

        var path = Path.Combine(outDir, Discovery.Sanitize(baseName + tail) + ".md");

        // Two targets serving the same model in the same second would otherwise overwrite each other.
        var n = 2;
        while (File.Exists(path))
            path = Path.Combine(outDir, Discovery.Sanitize(baseName + tail) + $" ({n++}).md");

        return path;
    }

    /// <summary>The run's own record, so a folder of attempts stays readable without a log.</summary>
    private string FrontMatter(
        string sourcePath, string sourceText, PromptVariant prompt, ModelTarget target,
        PromptPlace place, int? window, long promptTokens, long completionTokens,
        int abandoned, TimeSpan elapsed, DateTimeOffset started)
    {
        var entry = _settings.FindModel(target.EntryId);
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine($"source: \"{Path.GetFileName(sourcePath)}\"");
        sb.AppendLine($"source_chars: {sourceText.Length}");
        sb.AppendLine($"prompt: \"{prompt.Name}\"");
        if (prompt.Path != null) sb.AppendLine($"prompt_file: \"{Path.GetFileName(prompt.Path)}\"");
        sb.AppendLine($"prompt_place: {place.ToString().ToLowerInvariant()}");
        sb.AppendLine($"model: \"{target.WireModel ?? "auto"}\"");
        sb.AppendLine($"model_entry: \"{target.EntryId}\"");
        // The entry asked for whatever was loaded; `model:` above is what that turned out to be at
        // planning time. Recorded because the two can disagree if the runtime is reloaded mid-matrix.
        if (entry?.Model is "auto" or "" or null) sb.AppendLine("model_requested: auto");
        if (entry?.Provider is { Length: > 0 } provider) sb.AppendLine($"provider: {provider}");
        if (entry?.Endpoint is { Length: > 0 } endpoint) sb.AppendLine($"endpoint: \"{endpoint}\"");
        sb.AppendLine($"loaded_via_lmstudio: {(target.LmKey != null).ToString().ToLowerInvariant()}");
        sb.AppendLine($"temperature: {_settings.Temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (_settings.ReasoningLevel is { Length: > 0 } r) sb.AppendLine($"reasoning: \"{r}\"");
        if (window is > 0) sb.AppendLine($"context_window: {window}");
        if (promptTokens > 0) sb.AppendLine($"tokens_prompt: {promptTokens}");
        if (completionTokens > 0) sb.AppendLine($"tokens_completion: {completionTokens}");
        if (abandoned > 0) sb.AppendLine($"attempts_abandoned: {abandoned}");
        sb.AppendLine($"elapsed: \"{elapsed:hh\\:mm\\:ss}\"");
        sb.AppendLine($"started: \"{started:yyyy-MM-ddTHH:mm:sszzz}\"");
        sb.AppendLine("generated_by: SPLA Summarizer");
        sb.AppendLine("---");
        sb.AppendLine();
        return sb.ToString();
    }

    /// <summary>Unwraps an answer that is one big fenced block — a small model asked for Markdown
    /// often hands back Markdown inside a code fence. Only touched when the fence wraps everything,
    /// so a summary that legitimately contains a code block is left alone.</summary>
    private static string Unfence(string text)
    {
        if (!text.StartsWith("```")) return text;

        var firstBreak = text.IndexOf('\n');
        if (firstBreak < 0) return text;

        var opener = text[..firstBreak].Trim();
        // An opener carrying anything but a language word is not a wrapper.
        if (opener.Length > 3 && opener[3..].Any(char.IsWhiteSpace)) return text;
        if (!text.TrimEnd().EndsWith("```")) return text;

        var inner = text.TrimEnd();
        inner = inner[..^3].TrimEnd();
        // A second fence inside means the wrapper theory is wrong: this is real fenced content.
        if (inner[(firstBreak)..].Contains("```")) return text;

        return inner[(firstBreak + 1)..].Trim();
    }
}
