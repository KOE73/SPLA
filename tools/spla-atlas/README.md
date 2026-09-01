# spla-atlas

Реестр архитектурной модели SPLA: извлекает сущности из C# через Roslyn и сверяет
их с тем, что уже записано. Схемы не рисует и координат не расставляет — их
собирает человек в редакторе [`tools/spla-diagram`](../spla-diagram/).

**Кода пока нет.** Папка подготовлена, задание — в [`AGENTS.md`](AGENTS.md).
Контракт модели — **v3**:
[`CONTRACT.md`](../spla-diagram/docs/CONTRACT.md),
[`ADR_20260828`](../../docs/adr/ADR_20260828_diagrams_model-contract-v2.md) +
[`ADR_20260831`](../../docs/adr/ADR_20260831_diagrams_text-provenance-and-view-axes.md).

Пока её нет, реестры моделей ведутся руками: сверки схем с кодом в репозитории
сейчас не существует.

Автономная утилита: собственное решение `SplaAtlas.sln`, в `SPLA.slnx` не
добавляется, на проекты из `src/` не ссылается.

```bash
spla-atlas sync --project docs/diagrams/projects/core
spla-atlas sync --project docs/diagrams/projects/core --dry-run
```

Предыдущая версия — [`tools/spla-arch`](../spla-arch/) (Go). **Выведена из
обращения:** файлы, которые она собирала, удалены. Годится как референс логики
разрешения правил, не как образец.
