using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using SPLA.Domain.Editor;

namespace SPLA.Editor.Schema;

/// <summary>Запрос автосохранения формы: DocId файла + данные формы как JSON-объект {meta, fields[]}.</summary>
public sealed record SchemaSaveRequest(string DocId, string DataJson);

/// <summary>
/// Core schema-driven редактор (JSON Forms). Данные запекаются в страницу через
/// <see cref="WebEditorBase"/> — детерминированно. В Edit-режиме форма редактируема и шлёт autosave
/// по потере фокуса (page → host), с docId — чтобы хост писал в нужный файл.
/// </summary>
public partial class SchemaEditorView : WebEditorBase
{
    protected override string ResourceName => "SPLA.Editor.Schema.editor.html";
    protected override string RuntimeSubdir => "spla-schema-editor";

    /// <summary>Форма попросила сохранить (autosave-on-blur в Edit): DocId + данные {meta, fields[]}.</summary>
    public event EventHandler<SchemaSaveRequest>? SaveRequested;

    public SchemaEditorView()
    {
        InitializeComponent();
        InitBrowser(this.FindControl<NativeWebView>("EditorBrowser")!);
    }

    /// <summary>
    /// Показать схему данных, опциональную UI-схему и данные. <paramref name="readOnly"/> = true для
    /// View (форма заблокирована); <paramref name="docId"/> — id файла (вернётся в save).
    /// </summary>
    public void Load(JsonSchema dataSchema, JsonSchema? uiSchema, string dataJson, bool readOnly, string docId, string theme = "light")
    {
        // payload = {schema, uischema, data, readOnly, docId, theme}. schema/uischema/data — уже валидный JSON,
        // поэтому собираем подстановкой; readOnly/docId/theme — простые значения.
        var payload =
            "{\"schema\":" + dataSchema.Json +
            ",\"uischema\":" + (uiSchema?.Json ?? "null") +
            ",\"data\":" + dataJson +
            ",\"readOnly\":" + (readOnly ? "true" : "false") +
            ",\"docId\":" + JsonSerializer.Serialize(docId) +
            ",\"theme\":" + theme + "}";
        Show(payload);
    }

    protected override void OnPageMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("action", out var a) && a.GetString() == "save")
            {
                // data приходит строкой JSON (см. postSave на странице).
                var dataJson = doc.RootElement.TryGetProperty("data", out var d) ? d.GetString() : null;
                var docId = doc.RootElement.TryGetProperty("docId", out var id) ? id.GetString() : null;
                if (!string.IsNullOrEmpty(dataJson) && !string.IsNullOrEmpty(docId))
                    SaveRequested?.Invoke(this, new SchemaSaveRequest(docId!, dataJson!));
            }
        }
        catch
        {
            // Кривое сообщение — игнорируем.
        }
    }
}
