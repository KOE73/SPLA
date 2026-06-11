# Интеграция с SPLA

Проект `SPLA.Tools.OneC` задуман как независимая библиотека, которую можно подключить к `SPLA.UI.Avalonia`.

## Шаги по подключению

1.  В `SPLA.slnx` уже добавлен путь к `SPLA.Tools.OneC.csproj`.
2.  В `SPLA.UI.Avalonia.csproj` необходимо добавить `ProjectReference` на `SPLA.Tools.OneC`.
3.  Для визуализации потребуется добавить NuGet-пакеты для WebView (например, `Avalonia.WebView` и `Avalonia.WebView.Desktop`).
4.  В `MainWindowViewModel.cs` (или где регистрируются инструменты `McpHost`) необходимо инстанцировать `OneCIndexDatabase` и зарегистрировать все классы, реализующие `IMcpTool` из пространства имён `SPLA.Tools.OneC.Tools`.
5.  В Avalonia UI необходимо добавить новые View и ViewModel для отображения списка объектов и WebView с графом.

В данной реализации код UI (Avalonia) оставлен на стороне основного проекта SPLA, чтобы `SPLA.Tools.OneC` оставался чистым и не зависел от графического фреймворка.
