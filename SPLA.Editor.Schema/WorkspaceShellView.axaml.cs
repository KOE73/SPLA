using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SPLA.Domain.Editor;

namespace SPLA.Editor.Schema;

/// <summary>
/// Workspace-shell (Фаза 3, docs/schema-editor-plan.md §Фаза 3): дерево файлов слева (колонки
/// Имя/Размер/Дата) + редактор справа. Две оси-переключателя сверху:
///   Mode  : View (read-only, ничего не пишет) | Edit (autosave по потере фокуса)
///   Editor: Forms (SchemaEditorView) | CodeMirror (TextEditorView) — ЯВНЫЙ выбор для .jsonl.
///
/// Клик по файлу → грузит по текущим переключателям. Прочие текстовые типы всегда идут в CodeMirror
/// (текстовый фолбэк). Сохранение — autosave: страница шлёт SaveRequested по blur → IContentSource.WriteText.
///
/// Shell знает только нейтральные контракты из SPLA.Domain.Editor + два контрола редакторов.
/// </summary>
public partial class WorkspaceShellView : UserControl
{
    private IContentBrowser? _browser;
    private IContentSource? _source;
    private SchemaRegistry? _schemas;
    private string? _formsSchemaName;
    private string? _formsUiSchemaName;

    // Редакторы создаём лениво и переиспользуем (один NativeWebView на каждый тип).
    private SchemaEditorView? _formsEditor;
    private TextEditorView? _textEditor;

    private string? _currentRef;
    private string? _currentContentType;

    public WorkspaceShellView()
    {
        InitializeComponent();
        // Авто-переоткрытие при смене темы или density — ресурсы меняются → нужен новый payload.
        ActualThemeVariantChanged += (_, _) => ReopenCurrent();
        ResourcesChanged += (_, _) => ReopenCurrent();
    }

    // ── Сегментированные переключатели (взаимоисключающие пары ToggleButton) ─────

    private void OnModeClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Кликнутый — активен, парный — выключаем (имитация segmented single-select).
        var edit = ReferenceEquals(sender, ModeEdit);
        ModeEdit.IsChecked = edit;
        ModeView.IsChecked = !edit;
        ReopenCurrent();
    }

    private void OnEditorClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        var forms = ReferenceEquals(sender, EditorForms);
        EditorForms.IsChecked = forms;
        EditorText.IsChecked = !forms;
        ReopenCurrent();
    }

    /// <summary>
    /// Привязать shell к источникам. <paramref name="schemas"/> + имена схем нужны только для пути
    /// Forms (редактирование .jsonl как структуры); без них Forms деградирует до CodeMirror.
    /// </summary>
    public void Initialize(
        IContentBrowser browser,
        IContentSource source,
        SchemaRegistry? schemas = null,
        string? formsSchemaName = null,
        string? formsUiSchemaName = null)
    {
        _browser = browser;
        _source = source;
        _schemas = schemas;
        _formsSchemaName = formsSchemaName;
        _formsUiSchemaName = formsUiSchemaName;

        // Верхний уровень дерева = дети корня (сам корень узлом не показываем).
        Tree.ItemsSource = BuildNodes(parentRef: null);
    }

    private bool EditMode => ModeEdit.IsChecked == true;
    private bool FormsChosen => EditorForms.IsChecked == true;

    private string BuildWebTheme()
    {
        string Hex(string key, string fallback)
        {
            object? res = null;
            this.TryFindResource(key, out res);
            if (res is SolidColorBrush b) return $"#{b.Color.R:X2}{b.Color.G:X2}{b.Color.B:X2}";
            return fallback;
        }
        double Num(string key, double fallback)
        {
            object? res = null;
            this.TryFindResource(key, out res);
            return res is double d ? d : fallback;
        }

        var bg = Hex("AppBackgroundBrush", "#1e1e1e");
        var isDark = System.Drawing.ColorTranslator.FromHtml(bg) is var c
                     && (c.R + c.G + c.B) < 384;

        return new System.Text.Json.Nodes.JsonObject
        {
            ["bg"]          = bg,
            ["bgPanel"]     = Hex("PanelBackgroundBrush",  isDark ? "#252526" : "#f5f5f5"),
            ["bgInput"]     = Hex("InputBackgroundBrush",  isDark ? "#3c3c3c" : "#ffffff"),
            ["border"]      = Hex("PanelBorderBrush",      isDark ? "#444444" : "#cccccc"),
            ["borderFocus"] = Hex("AccentBrush",           "#007acc"),
            ["text"]        = Hex("TextForegroundBrush",   isDark ? "#d4d4d4" : "#1f1f1f"),
            ["textSub"]     = Hex("SubTextForegroundBrush",isDark ? "#9d9d9d" : "#666666"),
            ["accent"]      = Hex("AccentBrush",           "#007acc"),
            ["fontSize"]    = Num("BaseFontSize",          13),
            ["smallSize"]   = Num("SmallFontSize",         11),
            ["dark"]        = isDark,
        }.ToJsonString();
    }

    // ── Дерево ────────────────────────────────────────────────────────────────

    private ObservableCollection<BrowserNode> BuildNodes(string? parentRef)
    {
        var result = new ObservableCollection<BrowserNode>();
        if (_browser is null) return result;

        foreach (var node in _browser.GetChildren(parentRef))
        {
            // Папки раскрываем лениво при первом доступе к Children (через фабрику ниже).
            result.Add(new BrowserNode(node, n => BuildNodes(n)));
        }
        return result;
    }

    private void OnTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Tree.SelectedItem is not BrowserNode { IsLeaf: true } leaf) return;
        _currentRef = leaf.Ref;
        _currentContentType = leaf.ContentType;
        OpenCurrent();
    }

    private void ReopenCurrent()
    {
        if (_currentRef is not null) OpenCurrent();
    }

    // ── Открытие файла по текущим переключателям ────────────────────────────────

    private void OpenCurrent()
    {
        if (_source is null || _currentRef is null) return;

        var text = _source.ReadText(_currentRef);
        var isJsonl = string.Equals(_currentContentType, "jsonl", StringComparison.OrdinalIgnoreCase);

        // Forms доступен только для .jsonl, при выбранном Forms и наличии схемы. Иначе — CodeMirror.
        if (isJsonl && FormsChosen && _schemas is not null && _formsSchemaName is not null)
            OpenInForms(text);
        else
            OpenInText(text);
    }

    private void OpenInForms(string jsonlText)
    {
        var schema = _schemas!.Resolve(_formsSchemaName!);
        JsonSchema? ui = null;
        if (_formsUiSchemaName is not null) _schemas!.TryResolve(_formsUiSchemaName, out ui);

        if (_formsEditor is null)
        {
            _formsEditor = new SchemaEditorView();
            _formsEditor.SaveRequested += (_, save) =>
            {
                // Обратный адаптер: объект {meta, fields[]} формы → JSONL → запись в свой файл (DocId).
                _source?.WriteText(save.DocId, ObjectToJsonl(save.DataJson, _formsSchemaName));
            };
        }
        EditorHost.Child = _formsEditor;

        // JSONL-документ таблицы → объект {meta, fields[]}, которого ждёт схема sql-table.
        var dataJson = JsonlToObject(jsonlText);
        _formsEditor.Load(schema, ui, dataJson, readOnly: !EditMode, docId: _currentRef!, theme: BuildWebTheme());

        StatusText.Text = $"Forms · {_formsSchemaName} · {(EditMode ? "edit" : "view")}";
    }

    private void OpenInText(string text)
    {
        if (_textEditor is null)
        {
            _textEditor = new TextEditorView();
            _textEditor.SaveRequested += (_, save) =>
            {
                // autosave: пишем в ФАЙЛ, который реально редактировался (save.DocId),
                // а не в текущий выбранный — иначе текст уехал бы в другой файл при переключении.
                _source?.WriteText(save.DocId, save.Text);
            };
        }
        EditorHost.Child = _textEditor;

        var contentType = _currentContentType ?? "txt";
        _textEditor.Load(text, contentType, readOnly: !EditMode, docId: _currentRef!, theme: BuildWebTheme());
        StatusText.Text = $"CodeMirror · {contentType} · {(EditMode ? "edit" : "view")}";
    }

    // ── JSONL ↔ объект (форма sql-table: meta-строка + строки-поля) ─────────────
    // TODO(Фаза 3+): вынести в IEditorContentAdapter, чтобы shell не знал про sql-table формат
    // и чтобы появился обратный путь (объект→JSONL) для autosave Forms.
    private static string JsonlToObject(string jsonl)
    {
        var meta = new JsonObject();
        var fields = new JsonArray();

        foreach (var line in jsonl.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) continue;

            JsonObject? obj;
            try { obj = JsonNode.Parse(trimmed) as JsonObject; }
            catch { continue; }
            if (obj is null) continue;

            var type = obj["type"]?.GetValue<string>();
            // Дискриминатор строки в объект схемы не попадает.
            obj.Remove("type");
            obj.Remove("$schema");

            if (type == "field")
                fields.Add(obj.DeepClone());
            else
                meta = obj; // строка meta
        }

        var root = new JsonObject { ["meta"] = meta, ["fields"] = fields };
        return root.ToJsonString();
    }

    // Обратный путь: объект {meta, fields[]} формы → JSONL (meta-строка + строки-поля).
    // Восстанавливаем дискриминатор "type" и "$schema" в meta — то, что снимали в JsonlToObject.
    private static string ObjectToJsonl(string objectJson, string? schemaName)
    {
        JsonObject? root;
        try { root = JsonNode.Parse(objectJson) as JsonObject; }
        catch { return objectJson; }
        if (root is null) return objectJson;

        var lines = new List<string>();

        // meta-строка.
        var meta = (root["meta"] as JsonObject)?.DeepClone() as JsonObject ?? new JsonObject();
        var metaLine = new JsonObject { ["type"] = "meta" };
        if (schemaName is not null) metaLine["$schema"] = schemaName;
        foreach (var kv in meta) metaLine[kv.Key] = kv.Value?.DeepClone();
        lines.Add(metaLine.ToJsonString());

        // строки-поля.
        if (root["fields"] is JsonArray fields)
        {
            foreach (var f in fields)
            {
                if (f is not JsonObject fo) continue;
                var fieldLine = new JsonObject { ["type"] = "field" };
                foreach (var kv in fo) fieldLine[kv.Key] = kv.Value?.DeepClone();
                lines.Add(fieldLine.ToJsonString());
            }
        }

        return string.Join("\n", lines);
    }
}

/// <summary>
/// Обёртка <see cref="ContentNode"/> для дерева: добавляет форматированные колонки и ленивых детей.
/// Дети грузятся при первом обращении к <see cref="Children"/> (раскрытие папки).
/// </summary>
public sealed class BrowserNode
{
    private readonly ContentNode _node;
    private readonly Func<string, ObservableCollection<BrowserNode>> _childrenFactory;
    private ObservableCollection<BrowserNode>? _children;

    public BrowserNode(ContentNode node, Func<string, ObservableCollection<BrowserNode>> childrenFactory)
    {
        _node = node;
        _childrenFactory = childrenFactory;
    }

    public string Ref => _node.Ref;
    public string Label => _node.Label;
    public bool IsLeaf => _node.Kind == ContentNodeKind.Leaf;
    public string? ContentType => _node.ContentType;

    public string SizeText => _node.SizeBytes is { } b ? FormatSize(b) : string.Empty;
    public string DateText => _node.Modified?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;

    // Лист детей не имеет; папка грузит их лениво.
    public ObservableCollection<BrowserNode>? Children =>
        IsLeaf ? null : (_children ??= _childrenFactory(_node.Ref));

    private static string FormatSize(long bytes) =>
        bytes < 1024 ? $"{bytes} B" :
        bytes < 1024 * 1024 ? $"{bytes / 1024.0:0.#} KB" :
        $"{bytes / (1024.0 * 1024):0.#} MB";
}
