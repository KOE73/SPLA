using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SPLA.Domain.Editor;
using SPLA.Editor.Schema;
using SPLA.Plugins.Sql;

namespace SPLA.UI.Avalonia.Views;

/// <summary>
/// ВРЕМЕННОЕ debug-окно для live-проверки Workspace-shell (Фаза 3, docs/schema-editor-plan.md).
/// Включается кнопкой 🧩 в MainWindow. Создаёт во временной папке гарантированные тестовые файлы
/// (.jsonl таблица-док + .md + .txt) и открывает на них shell: дерево + редактор + переключатели.
/// Убрать после verify.
/// </summary>
public partial class SchemaEditorDebugWindow : Window
{
    public SchemaEditorDebugWindow()
    {
        AvaloniaXamlLoader.Load(this);
        ExtendClientAreaTitleBarHeightHint = 30;
        var shell = this.FindControl<WorkspaceShellView>("Shell")!;

        var dir = CreateSampleWorkspace();

        // Forms-путь использует референсные схемы SQL-плагина (sql-table@1 / sql-table.ui).
        var schemas = new SchemaRegistry();
        schemas.Register(SqlSchemaProvider.Create());

        shell.Initialize(
            new FileContentBrowser(dir),
            new FileContentSource(),
            schemas,
            SqlSchemaProvider.TableSchemaName,
            SqlSchemaProvider.TableUiSchemaName);
    }

    /// <summary>Пишет тестовые файлы с вложенной структурой папок и возвращает путь корня.</summary>
    private static string CreateSampleWorkspace()
    {
        var dir = Path.Combine(Path.GetTempPath(), "spla-shell-demo");
        Directory.CreateDirectory(dir);

        // ── Корень ──────────────────────────────────────────────────────────
        File.WriteAllText(Path.Combine(dir, "readme.md"),
            "# Demo workspace\n\nThis is a **markdown** file.\n\n" +
            "```cs\npublic int Sum(int a, int b) => a + b;\n```\n\n" +
            "```sql\nSELECT name, amount FROM demo WHERE category = 1;\n```\n");

        File.WriteAllText(Path.Combine(dir, "notes.txt"), "plain text fallback\nsecond line\n");

        // ── tables/ ──────────────────────────────────────────────────────────
        var tables = Path.Combine(dir, "tables");
        Directory.CreateDirectory(tables);

        File.WriteAllText(Path.Combine(tables, "demo_table.jsonl"),
            """
            {"type":"meta","$schema":"sql-table@1","table":"DEMO_TABLE","rules":["sample rule A"],"joins":[{"to":"OTHER","on":"DemoId"}]}
            {"type":"field","name":"DemoId","data_type":"int","agg":"none","desc":"demo identifier"}
            {"type":"field","name":"Amount","data_type":"numeric","agg":"SUM","desc":"demo amount"}
            {"type":"field","name":"Category","data_type":"tinyint","agg":"filter","desc":"demo category"}
            """);

        File.WriteAllText(Path.Combine(tables, "orders.jsonl"),
            """
            {"type":"meta","$schema":"sql-table@1","table":"ORDERS","rules":[],"joins":[]}
            {"type":"field","name":"OrderId","data_type":"int","agg":"none","desc":"order id"}
            {"type":"field","name":"Total","data_type":"numeric","agg":"SUM","desc":"order total"}
            """);

        // ── tables/archive/ ──────────────────────────────────────────────────
        var archive = Path.Combine(tables, "archive");
        Directory.CreateDirectory(archive);

        File.WriteAllText(Path.Combine(archive, "old_table.jsonl"),
            """
            {"type":"meta","$schema":"sql-table@1","table":"OLD_TABLE","rules":[],"joins":[]}
            {"type":"field","name":"Id","data_type":"int","agg":"none","desc":"legacy id"}
            """);

        // ── src/ ─────────────────────────────────────────────────────────────
        var src = Path.Combine(dir, "src");
        Directory.CreateDirectory(src);

        File.WriteAllText(Path.Combine(src, "Sample.cs"),
            "using System;\n\n// demo class for syntax highlighting\npublic class Sample\n{\n" +
            "    public int Count { get; set; } = 42;\n\n" +
            "    public string Greet(string name) => $\"Hello, {name}!\";\n}\n");

        File.WriteAllText(Path.Combine(src, "query.sql"),
            "SELECT t.DemoId, t.Amount\nFROM DEMO_TABLE t\nWHERE t.Category = 1\nORDER BY t.Amount DESC;\n");

        return dir;
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        => BeginMoveDrag(e);

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
        => Close();
}
