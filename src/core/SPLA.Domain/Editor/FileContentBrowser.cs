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
/// Листьями считаются ВСЕ файлы: фильтр по расширению скрывал бы конфиги с непривычными именами.
/// ContentType листа выводится из расширения (по нему shell выберет редактор), а для незнакомых
/// расширений — из содержимого: текст открывается фолбэк-редактором, двоичное помечается
/// <c>"binary"</c> и в редактор не идёт.
/// </summary>
public sealed class FileContentBrowser : IContentBrowser
{
    private readonly string _rootPath;

    // Расширение (с точкой, lower) → тип содержимого для диспетчеризации редактора.
    private readonly IReadOnlyDictionary<string, string> _leafTypes;

    public FileContentBrowser(string rootPath, IReadOnlyDictionary<string, string>? leafExtensions = null)
    {
        _rootPath = rootPath;
        // Карта не фильтрует, а лишь подсказывает подсветку: .jsonl/.md со спец-обработкой,
        // остальное — язык для CodeMirror. Чего тут нет — определяем по содержимому.
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

        // Файлы — листья; показываем все, тип содержимого решает, откроется ли файл в редакторе.
        // Размер и дата изменения нужны для колонок дерева (см. раскладку Workspace-shell).
        foreach (var file in Directory.EnumerateFiles(dir).OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            var info = new FileInfo(file);
            var contentType = ResolveContentType(file, info.Length);

            nodes.Add(new ContentNode(
                file, Path.GetFileName(file), ContentNodeKind.Leaf, contentType,
                SizeBytes: info.Length, Modified: info.LastWriteTime));
        }

        return nodes;
    }

    // Заведомо двоичные расширения — не тратим на них чтение с диска.
    private static readonly HashSet<string> KnownBinaryExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".dll", ".pdb", ".so", ".dylib", ".bin", ".obj", ".lib", ".a",
            ".zip", ".gz", ".7z", ".rar", ".tar", ".nupkg", ".jar", ".cab", ".msi",
            ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".webp", ".tif", ".tiff",
            ".pdf", ".docx", ".xlsx", ".pptx", ".odt", ".ods",
            ".mp3", ".mp4", ".wav", ".avi", ".mkv", ".mov", ".ogg", ".webm",
            ".ttf", ".otf", ".woff", ".woff2", ".db", ".sqlite", ".mdf", ".ldf",
        };

    /// <summary>
    /// Тип содержимого: сперва известное расширение, затем чёрный список двоичных, затем — нюх
    /// по первым килобайтам. Пустой файл считаем текстовым: его не жалко открыть.
    /// </summary>
    private string ResolveContentType(string file, long sizeBytes)
    {
        var ext = Path.GetExtension(file);
        if (_leafTypes.TryGetValue(ext, out var known))
            return known;
        if (KnownBinaryExtensions.Contains(ext))
            return "binary";
        if (sizeBytes == 0)
            return "txt";
        return LooksBinary(file) ? "binary" : "txt";
    }

    /// <summary>
    /// Нюх по первым 4 КБ: нулевой байт — верный признак двоичного, как и заметная доля
    /// управляющих символов вне таба/перевода строки. UTF-16 с BOM пропускаем как текст.
    /// </summary>
    private static bool LooksBinary(string file)
    {
        try
        {
            Span<byte> buffer = stackalloc byte[4096];
            using var stream = new FileStream(
                file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var read = stream.Read(buffer);
            if (read <= 0)
                return false;

            var head = buffer[..read];

            // UTF-16/32 BOM: нули внутри — норма, это текст.
            if (read >= 2 && ((head[0] == 0xFF && head[1] == 0xFE) || (head[0] == 0xFE && head[1] == 0xFF)))
                return false;

            var control = 0;
            foreach (var b in head)
            {
                if (b == 0)
                    return true;
                if (b < 0x09 || (b > 0x0D && b < 0x20))
                    control++;
            }

            return control * 100 / read > 5;
        }
        catch (IOException)
        {
            // Файл занят или недоступен — не гадаем, пусть попробует открыться как текст.
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
