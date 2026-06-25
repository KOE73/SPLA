using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SPLA.Editor.Schema;

/// <summary>
/// База для редакторов на WebView с ДЕТЕРМИНИРОВАННОЙ моделью загрузки.
///
/// Принцип: никакого асинхронного push через InvokeScript (его первый вызов WebView2 может проглотить —
/// отсюда раньше брались флаги готовности, таймеры-ретраи и ready-рукопожатие). Вместо этого данные
/// ЗАПЕКАЮТСЯ в HTML как <c>window.__SPLA_PAYLOAD</c> ПЕРЕД навигацией; страница читает их синхронно при
/// загрузке. Каждый показ = свежая страница со своими данными внутри.
///
/// Единственное реальное состояние — простой lifecycle-gate: пока контрол не присоединён к дереву,
/// навигировать WebView нельзя, поэтому показ откладывается до <see cref="Control.Loaded"/>. Это не
/// гонка, а корректный порядок инициализации: ровно один флаг, ровно один отложенный payload.
///
/// Обратный канал (page → host) — обычный postMessage, обрабатывается в <see cref="OnPageMessage"/>.
/// </summary>
public abstract class WebEditorBase : UserControl
{
    private NativeWebView? _browser;
    private bool _attached;
    private string? _pendingPayloadJson;

    /// <summary>Имя embedded-ресурса с self-contained HTML-бандлом.</summary>
    protected abstract string ResourceName { get; }

    /// <summary>Подкаталог во временной папке для генерируемых HTML.</summary>
    protected abstract string RuntimeSubdir { get; }

    /// <summary>Вызывается подклассом в конструкторе после InitializeComponent.</summary>
    protected void InitBrowser(NativeWebView browser)
    {
        _browser = browser;
        _browser.WebMessageReceived += (_, args) =>
        {
            var json = ExtractWebMessage(args);
            if (!string.IsNullOrWhiteSpace(json)) OnPageMessage(json!);
        };
        Loaded += OnLoadedInternal;
        // Контрол могут снять с дерева (свап редакторов в shell). Пока он не присоединён — навигировать
        // нельзя, поэтому показ откладываем до следующего Loaded. Детерминированный lifecycle-gate.
        Unloaded += (_, _) => _attached = false;
    }

    private void OnLoadedInternal(object? sender, RoutedEventArgs e)
    {
        _attached = true;
        if (_pendingPayloadJson is { } pending)
        {
            _pendingPayloadJson = null;
            RenderNow(pending);
        }
    }

    /// <summary>
    /// Показать данные. <paramref name="payloadJson"/> — JSON-объект, который страница прочтёт из
    /// <c>window.__SPLA_PAYLOAD</c>. Если контрол ещё не присоединён — показ отложится до Loaded.
    /// </summary>
    protected void Show(string payloadJson)
    {
        if (_attached)
            RenderNow(payloadJson);
        else
            _pendingPayloadJson = payloadJson; // покажем при Loaded
    }

    private void RenderNow(string payloadJson)
    {
        var html = ReadEmbeddedBundle();

        // Запекаем payload как глобал ПЕРЕД модульным скриптом. Классический inline-скрипт выполняется
        // синхронно и раньше любого module-скрипта (те отложены), так что глобал гарантированно готов.
        // Значение прокидываем строкой через JSON.parse — это безопасно для любых символов (вкл. </script>
        // внутри данных не встречается, но двойная сериализация исключает поломку синтаксиса).
        var literal = JsonSerializer.Serialize(payloadJson);
        var inject = $"<script>window.__SPLA_PAYLOAD=JSON.parse({literal});</script>";
        html = html.Replace("</body>", inject + "</body>");

        var path = Path.Combine(GetRuntimeDir(), $"view-{Environment.ProcessId}-{Guid.NewGuid():N}.html");
        File.WriteAllText(path, html, Encoding.UTF8);
        _browser!.Navigate(new Uri(path));
    }

    /// <summary>Обработать сообщение от страницы (page → host). По умолчанию — ничего.</summary>
    protected virtual void OnPageMessage(string json) { }

    private string ReadEmbeddedBundle()
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{ResourceName}' not found. Did the web bundle build?");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private string GetRuntimeDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), RuntimeSubdir);
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string? ExtractWebMessage(object args)
    {
        if (args is string text) return text;
        foreach (var prop in new[] { "Body", "Message", "Data", "WebMessageAsJson", "WebMessageAsString", "Source" })
        {
            var value = args.GetType().GetProperty(prop)?.GetValue(args)?.ToString();
            if (!string.IsNullOrWhiteSpace(value)) return value;
        }
        return args.ToString();
    }
}
