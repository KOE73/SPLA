# PLAN_20260819_core — динамическая регистрация built-in инструментов: открытый вопрос

**Статус: открыт, не начат.** Зафиксировано по вопросу пользователя в сессии 2026-08-19 при разборе
панели «Built-in tools» ([FeaturesPanel.vue](../../web/src/surfaces/Settings/FeaturesPanel.vue)):
почему переключение `core.*` capability требует перезапуска процесса.

## Текущее устройство

`AgentRuntime` регистрирует инструменты один раз, в конструкторе, и больше нигде:

```csharp
// src/agent/SPLA.Runtime/AgentRuntime.cs:326-334
var enabledIds = AgentFeatureCatalog.Resolve(settings.Capabilities, ...);
EnabledFeatureIds = new HashSet<string>(enabledIds, StringComparer.Ordinal);

var enabledFeatures = featureCatalog.Where(f => EnabledFeatureIds.Contains(f.Id)).ToList();
foreach (var feature in enabledFeatures)
    foreach (var tool in feature.Tools)
        McpHost.RegisterTool(tool);
```

Тем же списком `enabledFeatures` гасится и промпт-сегмент (`CoreFeaturePrompts.Load(id)`) — одна
точка решения на оба эффекта: и реальная доступность вызова, и упоминание в system prompt. Нигде в
кодовой базе нет `McpHost.UnregisterTool` или повторного вызова `RegisterTool` после старта — это не
намеренное ограничение политики, а следствие того, что `McpHost` и `AgentRuntime` никогда не
проектировались как reactive на `settings.Capabilities` после конструктора.

Это тот же паттерн, что и у [Service event bus](../../CHANGELOGS/current-log.md) и
[System prompt segments](../adr) — runtime собирается один раз при старте («built-at-send» есть только
у промпта per-message, но не у набора инструментов).

## Что нужно обдумать серьёзно, прежде чем делать

1. **Кто владеет жизненным циклом `McpHost` при live-toggle.** Сейчас один `AgentRuntime` = один
   процесс = один набор инструментов на весь его жизненный цикл. Hot-reload означает, что где-то
   должен появиться reactive-подписчик на изменение `settings.Capabilities` (файл `.spla` меняется
   на диске, либо `features.save` от клиента) и код, который добавляет/убирает записи из `McpHost`.
2. **Что происходит с уже начатым tool-call'ом**, если фичу выключили посреди выполнения. Нужна ли
   блокировка или это не проблема (MCP-вызов синхронный per-request, набор инструментов читается на
   старте каждого вызова)?
3. **Промпт уже отправлен модели** в текущем чате — system prompt пересобирается заново на каждое
   сообщение (см. [System prompt segments](../../CHANGELOGS/current-log.md), build-at-send в
   `SendAsync`), так что промпт-часть live-toggle получить легко. Проблема только в самом
   `McpHost.RegisterTool`.
4. **Согласованность с capability security model / security zones** — включение/выключение built-in
   инструмента на ходу должно проходить через ту же точку grant'ов (`ICapabilityGate`), что и
   остальные проверки, а не быть отдельным путём в обход.
5. **Нужно ли это вообще.** Панель уже честно говорит "restart to apply" и объясняет причину в
   описании — это не баг, а нераскрытая возможность. Если для реальных сценариев (плагины, ssh
   и т.п.) достаточно перезапуска процесса, hot-reload just for `core.*` может быть избыточен.

## Решение

Не принято. Два варианта на выбор, когда до этого дойдут руки:

- **(a) Оставить как есть**, но явно задокументировать причину в коде рядом с
  `AgentRuntime.RegisterTool` (комментарий уже частично есть) и в самой панели (сделано в этой
  сессии — подзаголовок в `FeaturesPanel.vue` и текст хинта `restartToApply`).
- **(b) Сделать динамически** — потребует reactive-обвязки вокруг `McpHost` и решения вопросов 1–4
  выше. Оценка объёма работ не делалась.
