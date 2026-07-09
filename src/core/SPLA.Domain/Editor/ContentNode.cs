using System;

namespace SPLA.Domain.Editor;

/// <summary>Вид узла в дереве обзора: ветка (можно раскрыть) или лист (можно открыть в редакторе).</summary>
public enum ContentNodeKind
{
    Folder,
    Leaf
}

/// <summary>
/// Один узел дерева обзора (см. <see cref="IContentBrowser"/>). Намеренно плоский DTO без поведения.
///
/// Ключевая симметрия с остальными контрактами: <see cref="Ref"/> — это та же самая ОПАКОВАЯ ссылка,
/// которую затем понимают <see cref="IContentSource"/> (читать/писать содержимое листа) и логика выбора
/// редактора. Браузер не обязан быть файловой системой — <see cref="Ref"/> может быть путём, ключом
/// проекта или blob-хэндлом; потребитель трактует его, не зная происхождения.
/// </summary>
/// <param name="Ref">Опаковая ссылка на узел; для листа — то, что отдаётся в IContentSource.</param>
/// <param name="Label">Отображаемое имя в дереве (напр. имя файла).</param>
/// <param name="Kind">Ветка или лист.</param>
/// <param name="ContentType">
/// Подсказка типа содержимого для ЛИСТА (напр. "jsonl", "md") — грубый хинт для выбора редактора.
/// ВАЖНО: финальный выбор Forms/CodeMirror делает пользователь явным переключателем в shell, а не
/// авто-логика по содержимому. Для веток — null.
/// </param>
/// <param name="SizeBytes">Размер листа в байтах для колонки дерева; для веток/неизвестно — null.</param>
/// <param name="Modified">Дата изменения для колонки дерева; если источник не знает — null.</param>
public sealed record ContentNode(
    string Ref,
    string Label,
    ContentNodeKind Kind,
    string? ContentType = null,
    long? SizeBytes = null,
    DateTime? Modified = null);
