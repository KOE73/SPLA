using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPLA.Domain.Editor;

/// <summary>
/// Реализация <see cref="IContentBrowser"/> поверх каталога на диске. Выдаёт ссылки в виде полных
/// путей — ровно тех, что понимает <see cref="FileContentSource"/> (Path.IsPathRooted). Браузер и
/// источник так образуют пару: обзор отдаёт <see cref="ContentNode.Ref"/> = путь, источник по нему читает.
///
/// По умолчанию листьями считаются интересные редактору файлы (.jsonl, .md); расширяемо через
/// <c>leafExtensions</c>. ContentType листа выводится из расширения — по нему shell выберет редактор.
/// </summary>
public sealed class FileContentBrowser : IContentBrowser
{
    private readonly string _rootPath;

    // Расширение (с точкой, lower) → тип содержимого для диспетчеризации редактора.
    private readonly IReadOnlyDictionary<string, string> _leafTypes;

    public FileContentBrowser(string rootPath, IReadOnlyDictionary<string, string>? leafExtensions = null)
    {
        _rootPath = rootPath;
        // По умолчанию показываем интересные редактору текстовые типы. CodeMirror жуёт любой plain text,
        // поэтому txt/sql/log/yaml/json тоже листья (фолбэк-редактор), а .jsonl/.md — со спец-обработкой.
        _leafTypes = leafExtensions ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jsonl"] = "jsonl",
            [".md"] = "md",
            [".json"] = "json",
            [".yaml"] = "yaml",
            [".yml"] = "yaml",
            [".sql"] = "sql",
            [".cs"] = "cs",
            [".c"] = "c",
            [".h"] = "c",
            [".cpp"] = "cpp",
            [".java"] = "java",
            [".txt"] = "txt",
            [".log"] = "txt",
        };
    }

    public string RootLabel => new DirectoryInfo(_rootPath).Name;

    public IReadOnlyList<ContentNode> GetChildren(string? parentRef)
    {
        // null → корень; иначе раскрываем переданную ветку (это всегда путь к каталогу).
        var dir = parentRef ?? _rootPath;
        if (!Directory.Exists(dir))
            return Array.Empty<ContentNode>();

        var nodes = new List<ContentNode>();

        // Папки — ветки, сверху, по алфавиту. Размер/дату для веток не заполняем (колонки пустые).
        foreach (var sub in Directory.EnumerateDirectories(dir).OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            nodes.Add(new ContentNode(sub, Path.GetFileName(sub), ContentNodeKind.Folder));

        // Файлы — листья; берём только известные расширения, чтобы не засорять дерево мусором.
        // Размер и дата изменения нужны для колонок дерева (см. раскладку Workspace-shell).
        foreach (var file in Directory.EnumerateFiles(dir).OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            var ext = Path.GetExtension(file);
            if (!_leafTypes.TryGetValue(ext, out var contentType))
                continue;

            var info = new FileInfo(file);
            nodes.Add(new ContentNode(
                file, Path.GetFileName(file), ContentNodeKind.Leaf, contentType,
                SizeBytes: info.Length, Modified: info.LastWriteTime));
        }

        return nodes;
    }
}
