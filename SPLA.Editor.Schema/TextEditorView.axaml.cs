using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;

namespace SPLA.Editor.Schema;

/// <summary>Запрос автосохранения: какой файл (DocId) и его новый текст.</summary>
public sealed record TextSaveRequest(string DocId, string Text);

/// <summary>
/// Универсальный текст-редактор (CodeMirror 6 + опциональный markdown-preview). Данные запекаются в
/// страницу через <see cref="WebEditorBase"/> — детерминированно. Обслуживает md/json/jsonl/yaml/txt.
///
/// Обратный канал: страница по потере фокуса в Edit шлёт {action:"save", docId, text}. DocId несёт id
/// файла, который реально редактировался, — хост пишет именно в него (а не в текущий выбранный).
/// </summary>
public partial class TextEditorView : WebEditorBase
{
    protected override string ResourceName => "SPLA.Editor.Schema.text.html";
    protected override string RuntimeSubdir => "spla-text-editor";

    /// <summary>Страница попросила сохранить (autosave-on-blur): DocId файла + новый текст.</summary>
    public event EventHandler<TextSaveRequest>? SaveRequested;

    public TextEditorView()
    {
        InitializeComponent();
        InitBrowser(this.FindControl<NativeWebView>("EditorBrowser")!);
    }

    /// <summary>
    /// Показать текст. <paramref name="contentType"/> задаёт подсветку и markdown-preview;
    /// <paramref name="readOnly"/> = true для View; <paramref name="docId"/> — id файла (вернётся в save).
    /// </summary>
    public void Load(string text, string contentType, bool readOnly, string docId, string theme = "light")
    {
        var payload = new JsonObject
        {
            ["text"] = text,
            ["contentType"] = contentType,
            ["readOnly"] = readOnly,
            ["docId"] = docId,
            ["theme"] = JsonNode.Parse(theme), // theme — уже JSON-объект, вставляем как node
        };
        Show(payload.ToJsonString());
    }

    protected override void OnPageMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("action", out var a) && a.GetString() == "save")
            {
                var text = doc.RootElement.TryGetProperty("text", out var t) ? t.GetString() : null;
                var docId = doc.RootElement.TryGetProperty("docId", out var d) ? d.GetString() : null;
                if (text is not null && !string.IsNullOrEmpty(docId))
                    SaveRequested?.Invoke(this, new TextSaveRequest(docId!, text));
            }
        }
        catch
        {
            // Кривое сообщение — игнорируем.
        }
    }
}
