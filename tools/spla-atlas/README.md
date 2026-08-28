# spla-atlas

Реестр архитектурной модели SPLA: извлекает сущности из C# через Roslyn и сверяет
их с тем, что уже записано. Схемы не рисует и координат не расставляет — их
собирает человек в редакторе [`tools/spla-diagram`](../spla-diagram/).

**Кода пока нет.** Папка подготовлена, задание — в [`AGENTS.md`](AGENTS.md),
решение и обоснование — в
[`ADR_20260828_diagrams_model-contract-v2`](../../docs/adr/ADR_20260828_diagrams_model-contract-v2.md).

Автономная утилита: собственное решение `SplaAtlas.sln`, в `SPLA.slnx` не
добавляется, на проекты из `src/` не ссылается.

```bash
spla-atlas sync --project docs/diagrams/projects/core
spla-atlas sync --project docs/diagrams/projects/core --dry-run
spla-atlas convert --from docs/diagrams/model-core-full.json --to docs/diagrams/projects/core
```

Предыдущая версия — [`tools/spla-arch`](../spla-arch/) (Go). Работает и не
трогается до конца переезда; годится как референс, не как образец.
