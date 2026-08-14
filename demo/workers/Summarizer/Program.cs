using Microsoft.Extensions.Logging;
using SPLA.Demo.Summarizer;
using SPLA.Domain.Settings;
using SPLA.Observability;
using SPLA.Runtime;

// ─────────────────────────────────────────────────────────────────────────────
// Summarizer: SPLA Runtime, standalone. Takes one document out of a folder and runs a matrix over
// it — every prompt variant × every model target, one fresh chat each — writing every result back
// into that folder under a name carrying the stamp, the prompt and the model. The prompts are
// ordinary files; the models are the project's connections plus whatever LM Studio has downloaded.
// ─────────────────────────────────────────────────────────────────────────────

Console.OutputEncoding = System.Text.Encoding.UTF8;

var opts = Options.Parse(args);
if (opts.Help || args.Length == 0)
{
    Console.WriteLine(Options.Usage);
    return opts.Help ? 0 : 1;
}
if (opts.Error != null)
{
    Console.WriteLine($"Аргументы: {opts.Error}\n");
    Console.WriteLine(Options.Usage);
    return 1;
}

Console.WriteLine("=== SPLA Summarizer ===");

// ── Where to work, resolved to absolute BEFORE anything changes the current directory ──
var target = Path.GetFullPath(opts.Target ?? Directory.GetCurrentDirectory());
var targetIsFile = File.Exists(target);
var folder = targetIsFile ? Path.GetDirectoryName(target)! : target;
if (!Directory.Exists(folder))
{
    Console.WriteLine($"Папки нет: {folder}");
    Console.WriteLine("не готово");
    return 1;
}

var splaFile = opts.SplaFile is { } given
    ? Path.GetFullPath(given)
    : Discovery.FindProjectFileUpwards(folder);
if (splaFile == null || !File.Exists(splaFile))
{
    Console.WriteLine($"Не нашёл файл проекта (.spla) ни в {folder}, ни выше. Укажите --project <файл.spla>.");
    Console.WriteLine("не готово");
    return 1;
}

var explicitSource = opts.Source is { } s ? Path.GetFullPath(s) : targetIsFile ? target : null;
var outDir = opts.OutDir is { } o ? Path.GetFullPath(o) : null;

// ── Logs, secrets, settings ──
SplaTelemetry.ConfigureGlobalLogs();
using var loggerFactory = LoggerFactory.Create(b =>
{
    b.ClearProviders();
    b.AddProvider(SplaTelemetry.CreateFileLoggerProvider());
    b.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger("SPLA.Summarizer");

// The project's connections point at secret: references; without this the DPAPI store is invisible
// and every remote endpoint would arrive unauthenticated.
SPLA.Secrets.Dpapi.DpapiSecrets.Register(msg => logger.LogWarning("{Message}", msg));

var settings = ConfigLoader.LoadAndResolve(splaFile);
var cfg = SummarizeConfig.Load(splaFile);
var promptsDir = Path.GetFullPath(cfg.PromptsDir, settings.WorkspacePath);
Directory.SetCurrentDirectory(settings.WorkspacePath);

Console.WriteLine($"Проект:   {splaFile}");
Console.WriteLine($"Папка:    {folder}");
Console.WriteLine($"Промпты:  {promptsDir}");

// ── What to run ──
string? sourceError = null;
var sourcePath = explicitSource ?? Discovery.PickSource(folder, cfg, out sourceError);
if (sourcePath == null || !File.Exists(sourcePath))
{
    Console.WriteLine(sourceError ?? $"Исходник не найден: {sourcePath ?? explicitSource}");
    Console.WriteLine("не готово");
    return 1;
}
outDir ??= Path.GetDirectoryName(sourcePath)!;
Directory.CreateDirectory(outDir);

var sourceText = await File.ReadAllTextAsync(sourcePath);
Console.WriteLine($"Исходник: {Path.GetFileName(sourcePath)} ({sourceText.Length} симв., ~{sourceText.Length / 3} токенов)");

var prompts = Discovery.LoadPrompts(opts, cfg, promptsDir, out var promptError);
if (promptError != null && prompts.Count == 0)
{
    Console.WriteLine(promptError);
    Console.WriteLine("не готово");
    return 1;
}
if (promptError != null) Console.WriteLine($"[!] {promptError}");

using var runtime = new AgentRuntime(settings, loggerFactory);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); Console.WriteLine("\n[стоп] прерываю…"); };

var targets = await Discovery.ResolveTargetsAsync(opts, cfg, settings, runtime.ModelManagement, cts.Token);
if (targets.Count == 0)
{
    Console.WriteLine("Ни одной модели для прогона. Задайте --model <id> или --lm <ключ>.");
    Console.WriteLine("не готово");
    return 1;
}

Console.WriteLine($"\nПромпты ({prompts.Count}):");
foreach (var p in prompts)
    Console.WriteLine($"  {p.Name,-24} {(p.Path == null ? "(из командной строки)" : Path.GetFileName(p.Path))}");
Console.WriteLine($"Модели ({targets.Count}):");
foreach (var t in targets)
    Console.WriteLine($"  {t.Display}");

var runner = new Runner(runtime, settings, cfg, opts);

if (opts.List || opts.DryRun)
{
    Console.WriteLine($"\nМатрица: {prompts.Count} × {targets.Count} = {prompts.Count * targets.Count} прогонов");
    if (opts.DryRun)
    {
        var stamp = DateTimeOffset.Now;
        foreach (var p in prompts)
            foreach (var t in targets)
                Console.WriteLine("  " + Path.GetFileName(runner.BuildOutputPath(outDir, sourcePath, p, t, stamp)));
    }
    Console.WriteLine("готово");
    return 0;
}

// ── The matrix. Model outer, prompt inner: a local model is expensive to load, so every prompt it
//    is going to see is asked while it is in memory. ──
var total = prompts.Count * targets.Count;
var done = 0;
var ok = 0;
var failures = new List<string>();

foreach (var t in targets)
{
    foreach (var p in prompts)
    {
        if (cts.IsCancellationRequested) break;
        done++;

        Console.WriteLine($"\n── [{done}/{total}] {p.Name} · {t.Display} ─────────────────");
        RunResult result;
        try
        {
            result = await runner.RunOneAsync(sourcePath, sourceText, outDir, p, t, cts.Token);
        }
        catch (OperationCanceledException) { break; }
        catch (Exception ex)
        {
            result = new RunResult("сбой", null, ex.Message, TimeSpan.Zero, 0);
            logger.LogError(ex, "Run failed. Prompt={Prompt} Target={Target}", p.Name, t.Label);
        }

        if (result.OutputPath != null)
        {
            ok++;
            Console.WriteLine($"   {result.Status}: {Path.GetFileName(result.OutputPath)} " +
                              $"({result.Chars} симв., {result.Elapsed:hh\\:mm\\:ss})");
        }
        else
        {
            var note = $"{p.Name} · {t.Display} — {result.Status}: {result.Note}";
            failures.Add(note);
            Console.WriteLine($"   {result.Status}: {result.Note}");
        }
    }
    if (cts.IsCancellationRequested) break;
}

runner.RestoreLmEntry();

Console.WriteLine($"\nПрогонов: {done} из {total}, удачных: {ok}.");
if (failures.Count > 0)
{
    Console.WriteLine("Не получилось:");
    foreach (var f in failures) Console.WriteLine($"  {f}");
}
Console.WriteLine($"Результаты: {outDir}");
Console.WriteLine(ok > 0 ? "готово" : "не готово");
return ok > 0 ? 0 : 1;
