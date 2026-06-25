# TODO: SQLite / SQLitePCLRaw override — снять, когда станет можно

> Маркер для поиска: **`SQLITE_VULN_OVERRIDE`**

## Контекст

`SPLA.Plugins.OneC` использует `Microsoft.Data.Sqlite`. Этот пакет (вплоть до
**10.0.9** включительно) транзитивно тянет **SQLitePCLRaw 2.1.x**, чей нативный
пакет `SQLitePCLRaw.lib.e_sqlite3`:

- **деприкейтнут** (преемник — `SourceGear.sqlite3`);
- помечен advisory **GHSA-2m69-gcr7-jv3q** (high, NU1903) — уязвимость в самой
  встроенной нативной SQLite, **без фикса на ветке 2.1.x**.

## Что сделано (текущий обход)

В [SPLA.Plugins.OneC.csproj](../SPLA.Plugins.OneC/SPLA.Plugins.OneC.csproj):

```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.9" />
<!-- override: SQLITE_VULN_OVERRIDE -->
<PackageReference Include="SQLitePCLRaw.bundle_e_sqlite3" Version="3.0.3" />
```

Override-пин принудительно поднимает стек до **SQLitePCLRaw 3.0.x**, где
`lib.e_sqlite3` заменён на `SourceGear.sqlite3 3.50.4.5` (SQLite 3.50.4, без CVE).
После этого: `lib.e_sqlite3` исчезает из дерева, NU1903 нет, NU1605 нет,
сборка зелёная.

## Что нужно сделать (когда снять обход)

Как только **Microsoft.Data.Sqlite сам перейдёт на SQLitePCLRaw >= 3.0.0**:

1. Обновить `Microsoft.Data.Sqlite` до этой версии.
2. Удалить явный `SQLitePCLRaw.bundle_e_sqlite3` PackageReference и комментарий.
3. `dotnet restore` + `dotnet build` для `SPLA.Plugins.OneC`.
4. Проверить: в `dotnet list ... package --include-transitive` нет
   `SQLitePCLRaw.lib.e_sqlite3` и нет NU1903.

Если убрать override и NU1903 возвращается — значит ещё рано, оставить как есть.

> Дублируется запланированной задачей `drop-sqlitepclraw-override-onec`
> (проверка ~23.09.2026).
