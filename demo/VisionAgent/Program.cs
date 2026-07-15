using Microsoft.Extensions.Logging;
using OpenCvSharp;
using SPLA.Agent;
using SPLA.Demo.Vision;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Permissions;
using SPLA.Observability;
using SPLA.Runtime;
using Spectre.Console;

// ─────────────────────────────────────────────────────────────────────────────
// VisionAgent: SPLA Runtime, standalone. No CLI, no service, no UI — a small
// host app points the runtime at a .spla project file and gets a full agent.
// Loop: grab a frame from the video source → send it to the model as a chat
// turn (the analysis prompt comes from the project's instructions) → print
// the answer → repeat.
// ─────────────────────────────────────────────────────────────────────────────

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== SPLA VisionAgent (демо автономного SPLA Runtime) ===");

// 1. Проект: аргументом или vision.spla рядом с exe / в текущей папке.
var splaFile = args.FirstOrDefault(a => a.EndsWith(".spla", StringComparison.OrdinalIgnoreCase))
    ?? new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory }
        .Select(d => Path.Combine(d, "vision.spla"))
        .FirstOrDefault(File.Exists);

if (splaFile == null || !File.Exists(splaFile))
{
    Console.WriteLine("Не найден файл проекта (.spla). Укажите его аргументом или положите vision.spla рядом.");
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
Console.WriteLine($"Workspace: {settings.WorkspacePath}");
Console.WriteLine($"Endpoint:  {settings.Connections.FirstOrDefault()?.Endpoint ?? "(none)"}");

var vision = VisionConfig.Load(splaFile);
Console.WriteLine($"Источник:  {vision.Source}  ·  интервал {vision.IntervalSeconds:0.#} с");

// 2. Агент: весь стек одной строкой — это и есть смысл выделенного Runtime.
using var runtime = new AgentRuntime(settings, loggerFactory);
var chats = new ChatRegistry(runtime);
var chat = chats.CreateNew($"vision {DateTime.Now:yyyy-MM-dd HH:mm}");

// Рассуждения модели — серым через Spectre.Console (ключ vision.show_reasoning). Они приходят
// двумя путями: отдельным каналом (стримятся сюда чанками) или инлайном <think>…</think>,
// который клиент вырезает из ответа и отдаёт только в финальном сообщении — тогда показываем
// их разом после ответа. Токены за ход копятся здесь, а печатаются подвалом кадра в цикле.
var inReasoning = false;
var reasoningStreamed = false;
int? turnPrompt = null, turnCompletion = null;
void EndReasoning() { if (inReasoning) { AnsiConsole.WriteLine(); inReasoning = false; } }
var callbacks = new AgentCallbacks
{
    OnReasoning = vision.ShowReasoning
        ? chunk =>
        {
            if (!inReasoning) { AnsiConsole.Markup("[grey]💭 [/]"); inReasoning = true; reasoningStreamed = true; }
            AnsiConsole.Markup($"[grey]{Markup.Escape(chunk)}[/]");
            return Task.CompletedTask;
        }
        : null,
    OnDelta = chunk => { EndReasoning(); Console.Write(chunk); return Task.CompletedTask; },
    OnAssistantMessage = msg =>
    {
        EndReasoning();
        Console.WriteLine();
        // Инлайновые <think>-рассуждения не стримятся — они есть только в собранном сообщении.
        if (vision.ShowReasoning && !reasoningStreamed && !string.IsNullOrWhiteSpace(msg.Reasoning))
            AnsiConsole.MarkupLine($"[grey]💭 {Markup.Escape(msg.Reasoning.Trim())}[/]");
        return Task.CompletedTask;
    },
    OnNotice = n => { EndReasoning(); Console.WriteLine($"\n[notice] {n}"); return Task.CompletedTask; },
    OnTokenUsage = (p, c) =>
    {
        runtime.TokenUsageProject.Record(p, c);
        runtime.TokenUsageGlobal.Record(p, c);
        if (p is int pi) turnPrompt = (turnPrompt ?? 0) + pi;
        if (c is int ci) turnCompletion = (turnCompletion ?? 0) + ci;
    }
};
// Headless-режим: инструментам разрешений не даём, уточняющих вопросов не ждём.
Func<ToolFunctionDefinition, string, Task<PermissionDecision>> deny =
    (def, _) =>
    {
        Console.WriteLine($"[permission] авто-отказ: {def.Name}");
        return Task.FromResult(PermissionDecision.Deny);
    };
Func<ClarifyRequest, Task<string?>> noClarify = _ => Task.FromResult<string?>(null);

// 3. Источник видео.
using var capture = OpenSource(vision.Source, splaFile, out var isLive, out var sourceLabel);
if (capture == null || !capture.IsOpened())
{
    Console.WriteLine($"Не удалось открыть источник видео: {sourceLabel}");
    Console.WriteLine("не готово");
    return 1;
}
Console.WriteLine($"Источник открыт: {sourceLabel}  ({capture.FrameWidth}x{capture.FrameHeight})");
Console.WriteLine("Ctrl+C — остановить.\n");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

string? saveDir = string.IsNullOrWhiteSpace(vision.SaveFrames)
    ? null
    : Path.GetFullPath(vision.SaveFrames, Path.GetDirectoryName(splaFile)!);
if (saveDir != null) Directory.CreateDirectory(saveDir);

// Окна предпросмотра (vision.show_windows): Live — непрерывный поток, Sent — кадр в модели.
using var display = vision.ShowWindows ? new FrameDisplay() : null;

// Live-источник читается непрерывно отдельным потоком: окно Live живёт без рывков даже пока
// модель думает, а на анализ всегда уходит самый свежий кадр (буфер не копится).
Mat? latestLive = null;
var latestGate = new object();
if (isLive)
{
    new Thread(() =>
    {
        using var buf = new Mat();
        while (!cts.IsCancellationRequested)
        {
            if (capture.Read(buf) && !buf.Empty())
            {
                display?.ShowLive(buf);
                lock (latestGate) { latestLive?.Dispose(); latestLive = buf.Clone(); }
            }
            else Thread.Sleep(50);
        }
    }) { IsBackground = true, Name = "vision-capture" }.Start();
}

var sentTurns = new Queue<string>();   // msgId каждого отправленного кадра — для обрезки контекста
var frameNo = 0;
using var frame = new Mat();

while (!cts.IsCancellationRequested)
{
    if (isLive)
    {
        Mat? snap = null;
        lock (latestGate) { snap = latestLive; latestLive = null; }
        if (snap == null)
        {
            try { await Task.Delay(200, cts.Token); } catch (OperationCanceledException) { break; }
            continue;
        }
        snap.CopyTo(frame);
        snap.Dispose();
    }
    else
    {
        if (!capture.Read(frame) || frame.Empty())
        {
            Console.WriteLine("\nВидеофайл закончился.");
            break;
        }
        display?.ShowLive(frame);
    }

    frameNo++;
    var eventTime = isLive
        ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        : TimeSpan.FromMilliseconds(capture.Get(VideoCaptureProperties.PosMsec)).ToString(@"hh\:mm\:ss");

    using var toSend = vision.MaxWidth > 0 && frame.Width > vision.MaxWidth
        ? frame.Resize(new OpenCvSharp.Size(vision.MaxWidth, frame.Height * vision.MaxWidth / frame.Width))
        : frame.Clone();
    display?.ShowSent(toSend);
    Cv2.ImEncode(".jpg", toSend, out var jpeg,
        new[] { (int)ImwriteFlags.JpegQuality, Math.Clamp(vision.JpegQuality, 1, 100) });
    if (saveDir != null)
        File.WriteAllBytes(Path.Combine(saveDir, $"frame-{frameNo:D5}.jpg"), jpeg);
    var dataUrl = "data:image/jpeg;base64," + Convert.ToBase64String(jpeg);

    AnsiConsole.MarkupLine($"[bold yellow]── Кадр #{frameNo} · {eventTime} · {jpeg.Length / 1024} КБ ──[/]");
    var sw = System.Diagnostics.Stopwatch.StartNew();
    string? userMsgId = null;
    reasoningStreamed = false;
    turnPrompt = turnCompletion = null;
    try
    {
        await chat.SendAsync(
            "Analyze this frame per the instructions.",
            callbacks, deny, noClarify, cts.Token,
            images: new[] { dataUrl },
            onUserMessage: m => userMsgId = m.MsgId);
    }
    catch (OperationCanceledException) { break; }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[error] ход не выполнен: {ex.Message}");
    }
    var totalProject = runtime.TokenUsageProject.Total;
    var totalGlobal = runtime.TokenUsageGlobal.Total;
    AnsiConsole.MarkupLine(
        $"[grey]⏱ {sw.Elapsed.TotalSeconds:0.0} с[/]" +
        $"  ·  [deepskyblue1]🎫 вход {turnPrompt?.ToString("N0") ?? "?"} / выход {turnCompletion?.ToString("N0") ?? "?"}[/]" +
        $"  ·  [grey]проект Σ {totalProject.TotalTokens:N0} · машина Σ {totalGlobal.TotalTokens:N0}[/]");
    AnsiConsole.WriteLine();

    // Контекст: каждый кадр (или каждые keep_history кадров) начинаем с чистого листа,
    // иначе картинки быстро переполнят окно локальной модели.
    if (userMsgId != null)
    {
        sentTurns.Enqueue(userMsgId);
        if (sentTurns.Count > vision.KeepHistory)
        {
            chat.Rewind(sentTurns.Peek(), before: true);
            sentTurns.Clear();
        }
    }

    if (isLive)
    {
        try { await Task.Delay(TimeSpan.FromSeconds(Math.Max(0.2, vision.IntervalSeconds)), cts.Token); }
        catch (OperationCanceledException) { break; }
    }
    else
    {
        // Видеофайл: не спим, а перематываем вперёд по видео-времени на интервал.
        capture.Set(VideoCaptureProperties.PosMsec,
            capture.Get(VideoCaptureProperties.PosMsec) + vision.IntervalSeconds * 1000);
    }
}

Console.WriteLine($"\nОбработано кадров: {frameNo}.");
Console.WriteLine("готово");
return 0;

// ── Источник: usb:N | rtsp://… | путь к файлу (относительно .spla) ──────────
static VideoCapture? OpenSource(string source, string splaFile, out bool isLive, out string label)
{
    source = source.Trim();
    if (source.StartsWith("usb:", StringComparison.OrdinalIgnoreCase) || int.TryParse(source, out _))
    {
        var index = int.Parse(source.StartsWith("usb:", StringComparison.OrdinalIgnoreCase) ? source[4..] : source);
        isLive = true;
        label = $"USB-камера #{index}";
        var cap = new VideoCapture(index);
        cap.Set(VideoCaptureProperties.BufferSize, 1);
        return cap;
    }
    if (source.Contains("://"))
    {
        isLive = true;
        label = source;
        var cap = new VideoCapture(source);
        cap.Set(VideoCaptureProperties.BufferSize, 1);
        return cap;
    }
    var path = Path.GetFullPath(source, Path.GetDirectoryName(splaFile)!);
    isLive = false;
    label = path;
    return File.Exists(path) ? new VideoCapture(path) : null;
}
