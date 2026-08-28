# Проект `core`

Пусто. Заполнится разовым прогоном конвертера:

```bash
spla-atlas convert --from docs/diagrams/model-core-full.json --to docs/diagrams/projects/core
```

Ожидаемое содержимое и форма каждого файла — в [`../AGENTS.md`](../AGENTS.md) §3.

```
project.json  entities.json  relations.json  containers.json
text.en.json  text.ru.json   views/semantic-atlas.view.json
```

До переезда источником остаётся
[`docs/diagrams/model-core-full.json`](../../model-core-full.json), который
собирает старый [`tools/spla-arch`](../../../../tools/spla-arch/).

**`views/` — ручная работа владельца.** Ни утилита, ни агент туда не пишут.
