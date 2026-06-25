using System.Collections.Generic;
using System.Reflection;
using SPLA.Domain.Editor;

namespace SPLA.Plugins.Sql;

/// <summary>
/// Поставщик схем редактора от SQL-плагина (docs/schema-editor-plan.md, Фаза 2).
///
/// SQL — лишь первый, РЕФЕРЕНСНЫЙ источник схемы табличного JSONL: он вшивает «золотой стандарт»
/// `sql-table@1` (DATA-схема) и `sql-table.ui` (раскладка) как embedded-ресурсы и отдаёт их через
/// общий <see cref="EmbeddedJsonSchemaProvider"/>. Плагин при этом НЕ владеет форматом монопольно —
/// в проекте может лежать другая схема, а UI проект может перекрыть (см. порядок разрешения в Доке А).
///
/// Класс намеренно тонкий: вся механика чтения ресурсов — в переиспользуемом
/// <see cref="EmbeddedJsonSchemaProvider"/>. Здесь только карта «имя схемы → ресурс».
/// </summary>
public static class SqlSchemaProvider
{
    /// <summary>Имя референсной DATA-схемы табличного JSONL (с версией).</summary>
    public const string TableSchemaName = "sql-table@1";

    /// <summary>Имя референсной UI-схемы той же таблицы.</summary>
    public const string TableUiSchemaName = "sql-table.ui";

    /// <summary>
    /// Создать провайдер, резолвящий обе референсные схемы из ресурсов этой сборки.
    /// Имя ресурса = LogicalName из .csproj, который совпадает с именем схемы.
    /// </summary>
    public static IJsonSchemaProvider Create()
    {
        var map = new Dictionary<string, string>
        {
            [TableSchemaName]   = TableSchemaName,   // LogicalName "sql-table@1"
            [TableUiSchemaName] = TableUiSchemaName, // LogicalName "sql-table.ui"
        };

        return new EmbeddedJsonSchemaProvider(Assembly.GetExecutingAssembly(), map);
    }
}
