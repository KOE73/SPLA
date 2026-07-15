using Microsoft.Extensions.Logging;
using SPLA.Agent;
using SPLA.Demo.LogSentry;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Permissions;
using SPLA.Observability;
using SPLA.Runtime;

// ─────────────────────────────────────────────────────────────────────────────
// LogSentry: SPLA Runtime, standalone. Tails a log file, batches suspicious
// lines, sends each batch to the agent, prints the triage verdict. The triage
// prompt is the .spla project's instructions; the demo's knobs live in its
// sentry: section.
// ─────────────────────────────────────────────────────────────────────────────

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== SPLA LogSentry (демо автономного SPLA Runtime) ===");

var splaFile = args.FirstOrDefault(a => a.EndsWith(".spla", StringComparison.OrdinalIgnoreCase))
    ?? new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory }
        .Select(d => Path.Combine(d, "sentry.spla"))
        .FirstOrDefault(File.Exists);
if (splaFile == null || !File.Exists(splaFile))
{
    Console.WriteLine("Не найден файл проекта (.spla). Укажите его аргументом или положите sentry.spla рядом.");
    Console.WriteLine("не готово");
    return 1;
}
splaFile = Path.GetFullPath(splaFile);
Console.WriteLine($"Проект: {splaFile}");

SplaTelemetry.ConfigureGlobalLogs();
using var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b =>
{
    b.ClearProviders();
    b.AddProvider(SplaTelemetry.CreateFileLoggerProvider());
    b.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
});

var settings = ConfigLoader.LoadAndResolve(splaFile);
Directory.SetCurrentDirectory(settings.WorkspacePath);
var cfg = SentryConfig.Load(splaFile);
var logPath = Path.GetFullPath(cfg.LogFile, Path.GetDirectoryName(splaFile)!);
Console.WriteLine($"Endpoint:  {settings.Connections.FirstOrDefault()?.Endpoint ?? "(none)"}");
Console.WriteLine($"Лог:       {logPath}");
Console.WriteLine($"Маркеры:   {(cfg.Triggers.Count == 0 ? "(все строки)" : string.Join(", ", cfg.Triggers))}");

using var runtime = new AgentRuntime(settings, loggerFactory);
var chat = new ChatRegistry(runtime).CreateNew($"sentry {DateTime.Now:yyyy-MM-dd HH:mm}");

var callbacks = new AgentCallbacks
{
    OnDelta = chunk => { Console.Write(chunk); return Task.CompletedTask; },
    OnAssistantMessage = _ => { Console.WriteLine(); return Task.CompletedTask; },
    OnNotice = n => { Console.WriteLine($"\n[notice] {n}"); return Task.CompletedTask; },
    OnTokenUsage = (p, c) => { runtime.TokenUsageProject.Record(p, c); runtime.TokenUsageGlobal.Record(p, c); }
};
Func<ToolFunctionDefinition, string, Task<PermissionDecision>> deny =
    (def, _) => Task.FromResult(PermissionDecision.Deny);
Func<ClarifyRequest, Task<string?>> noClarify = _ => Task.FromResult<string?>(null);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
Console.WriteLine("Слежу за логом. Ctrl+C — остановить.\n");

// Хвостим файл с текущего конца: старые строки не разбираем, ждём новые.
long position = File.Exists(logPath) ? new FileInfo(logPath).Length : 0;
var pending = new List<string>();
var sentTurns = new Queue<string>();
var batchNo = 0;

while (!cts.IsCancellationRequested)
{
    try { await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, cfg.BatchSeconds)), cts.Token); }
    catch (OperationCanceledException) { break; }

    if (!File.Exists(logPath)) continue;
    var len = new FileInfo(logPath).Length;
    if (len < position) position = 0;               // файл усечён/ротация — читаем с начала
    if (len > position)
    {
        using var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        fs.Seek(position, SeekOrigin.Begin);
        using var reader = new StreamReader(fs);
        while (reader.ReadLine() is { } line)
        {
            if (cfg.Triggers.Count == 0 ||
                cfg.Triggers.Any(t => line.Contains(t, StringComparison.OrdinalIgnoreCase)))
                pending.Add(line);
        }
        position = fs.Position;
    }
    if (pending.Count == 0) continue;

    var batch = pending.Take(cfg.MaxLines).ToList();
    pending.RemoveRange(0, batch.Count);

    batchNo++;
    Console.WriteLine($"── Пачка #{batchNo} · {DateTime.Now:HH:mm:ss} · строк: {batch.Count} ──");
    string? userMsgId = null;
    try
    {
        await chat.SendAsync(
            $"Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}. Свежие строки лога:\n```\n{string.Join('\n', batch)}\n```",
            callbacks, deny, noClarify, cts.Token,
            onUserMessage: m => userMsgId = m.MsgId);
    }
    catch (OperationCanceledException) { break; }
    catch (Exception ex) { Console.WriteLine($"\n[error] ход не выполнен: {ex.Message}"); }
    Console.WriteLine();

    if (userMsgId != null)
    {
        sentTurns.Enqueue(userMsgId);
        if (sentTurns.Count > cfg.KeepHistory)
        {
            chat.Rewind(sentTurns.Peek(), before: true);
            sentTurns.Clear();
        }
    }
}

Console.WriteLine($"\nРазобрано пачек: {batchNo}.");
Console.WriteLine("готово");
return 0;
