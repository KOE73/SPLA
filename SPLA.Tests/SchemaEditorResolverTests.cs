using System.IO;
using System.Text.Json;
using SPLA.Domain.Editor;
using SPLA.Plugins.Sql;

namespace SPLA.Tests;

/// <summary>
/// Фаза 2 schema-editor (docs/schema-editor-plan.md): проверяем путь резолва схем и контента
/// БЕЗ GUI. GUI (реальный рендер формы в WebView) проверяется вручную в конце.
/// </summary>
public class SchemaEditorResolverTests
{
    // ── SqlSchemaProvider отдаёт обе вшитые схемы ─────────────────────────────

    [Fact]
    public void SqlProvider_resolves_data_schema_as_valid_json_schema()
    {
        var provider = SqlSchemaProvider.Create();

        Assert.True(provider.CanResolve(SqlSchemaProvider.TableSchemaName));
        var schema = provider.Resolve(SqlSchemaProvider.TableSchemaName);
        Assert.NotNull(schema);

        // Содержимое — валидный JSON и несёт стандартные признаки JSON Schema 2020-12.
        using var doc = JsonDocument.Parse(schema!.Json);
        Assert.Equal("sql-table@1", doc.RootElement.GetProperty("$id").GetString());
        Assert.Equal("object", doc.RootElement.GetProperty("type").GetString());

        // enum агрегаций — это DATA-схема (а не UI): проверяем, что он на месте.
        var agg = doc.RootElement
            .GetProperty("properties").GetProperty("fields")
            .GetProperty("items").GetProperty("properties").GetProperty("agg")
            .GetProperty("enum");
        Assert.Contains("SUM", agg.EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public void SqlProvider_resolves_ui_schema()
    {
        var provider = SqlSchemaProvider.Create();

        Assert.True(provider.CanResolve(SqlSchemaProvider.TableUiSchemaName));
        var ui = provider.Resolve(SqlSchemaProvider.TableUiSchemaName);
        Assert.NotNull(ui);

        using var doc = JsonDocument.Parse(ui!.Json);
        Assert.Equal("VerticalLayout", doc.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public void SqlProvider_returns_null_for_unknown_name()
    {
        var provider = SqlSchemaProvider.Create();

        Assert.False(provider.CanResolve("nope@1"));
        Assert.Null(provider.Resolve("nope@1"));
    }

    // ── SchemaRegistry резолвит по первому совпадению ─────────────────────────

    [Fact]
    public void Registry_resolves_via_registered_provider()
    {
        var registry = new SchemaRegistry();
        registry.Register(SqlSchemaProvider.Create());

        Assert.True(registry.TryResolve("sql-table@1", out var schema));
        Assert.NotNull(schema);

        // Неизвестная схема — бросает (DATA-схема обязательна).
        Assert.Throws<KeyNotFoundException>(() => registry.Resolve("missing@1"));
    }

    // ── FileContentSource: round-trip чтения/записи ───────────────────────────

    [Fact]
    public void FileContentSource_round_trips_text()
    {
        var src = new FileContentSource();
        var path = Path.Combine(Path.GetTempPath(), $"spla-cs-test-{Guid.NewGuid():N}.jsonl");

        try
        {
            Assert.True(src.CanResolve(path));            // rooted путь — наш
            Assert.False(src.CanResolve("relative.txt")); // относительный — не наш

            const string payload = "{\"type\":\"field\",\"name\":\"Масса\"}";
            src.WriteText(path, payload);
            Assert.Equal(payload, src.ReadText(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    // ── FileContentBrowser: дерево + диспетч-подсказки, в паре с источником ────

    [Fact]
    public void FileContentBrowser_lists_folders_and_known_leaves_with_content_type()
    {
        var root = Path.Combine(Path.GetTempPath(), $"spla-browser-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(root, "sub"));
        File.WriteAllText(Path.Combine(root, "table_Asbest.jsonl"), "{}");
        File.WriteAllText(Path.Combine(root, "notes.md"), "# заметки");
        File.WriteAllText(Path.Combine(root, "ignored.bin"), "skip");

        try
        {
            var browser = new FileContentBrowser(root);
            var children = browser.GetChildren(null);

            // .bin (неизвестное расширение) отфильтрован: папка + 2 известных листа.
            Assert.Equal(3, children.Count);

            var folder = children.Single(n => n.Kind == ContentNodeKind.Folder);
            Assert.Equal("sub", folder.Label);

            // ContentType несёт подсказку для диспетчеризации редактора (.jsonl → schema, .md → markdown).
            var jsonl = children.Single(n => n.Label == "table_Asbest.jsonl");
            Assert.Equal("jsonl", jsonl.ContentType);
            Assert.Equal("md", children.Single(n => n.Label == "notes.md").ContentType);

            // Колонки дерева: лист несёт размер и дату; ветка — нет.
            Assert.Equal(2, jsonl.SizeBytes); // "{}" = 2 байта
            Assert.NotNull(jsonl.Modified);
            Assert.Null(folder.SizeBytes);

            // Симметрия: Ref листа потребляется источником без доп. трансляции.
            var src = new FileContentSource();
            Assert.True(src.CanResolve(jsonl.Ref));
            Assert.Equal("{}", src.ReadText(jsonl.Ref));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
