# Конвейер LLM — схемы

Визуальное приложение к [`ADR_20260731_llm_pipeline.md`](ADR_20260731_llm_pipeline.md). Текст решений и
обоснования — там; здесь только устройство. При расхождении прав основной документ.

---

## 1. Устройство конвейера

Прямоугольник `КОНВЕЙЕР` — это то, что печётся один раз и дальше не меняется. Внутри — стадии;
порядок задаётся стадией middleware, а не порядком регистрации.

```mermaid
%%{init: {'flowchart': {'subGraphTitleMargin': {'top': 12, 'bottom': 12}, 'nodeSpacing': 40, 'rankSpacing': 50, 'wrappingWidth': 450, 'htmlLabels': true}}}%%
flowchart TB
    CALLER["**Вызывающий** - <i>ConversationOrchestrator · SpawnedAgentRunner · воркер</i>"]
    CTX["**LlmTurnContext** - <i>создаётся на каждый ход: сообщения · инструменты · настройки · modelId · приёмники OnDelta / OnReasoning</i>"]
    GW["**ILlmGateway** - <i>единственный публикуемый вход</i>"]

    CALLER --> CTX --> GW

    subgraph PIPE["КОНВЕЙЕР — испечён один раз, неизменяем, потокобезопасен"]
        direction TB

        subgraph ST1["① Trace"]
            direction TB
            M1["**Trace** - <i>трасса хода; снаружи всего, чтобы отказ тоже был виден</i>"]
            M2["**ConnectionResolve** - <i>id → дескриптор + провайдер + capabilities; дальше id никто не резолвит</i>"]
            M1 --> M2
        end

        subgraph ST2["② Policy — открыта плагинам"]
            direction TB
            M3["**Authorize** - <i>можно ли этому caller'у это подключение и эту модель</i>"]
            M4["**Privacy** - <i>можно ли ЭТОТ контент в ЭТО подключение</i>"]
            M5["**Quota** - <i>предпроверка бюджета</i>"]
            M3 --> M4 --> M5
        end

        subgraph ST3["③ Content — открыта плагинам"]
            direction TB
            M6["**CapabilityGuard** - <i>vision / tools / reasoning против capabilities → внятная ошибка ДО сети</i>"]
            M7["**ContentPolicy** - <i>даунскейл картинок, лимиты вложений</i>"]
            M6 --> M7
        end

        subgraph ST4["④ Retry — запечатана"]
            M8["**Retry** - <i>только транзиентные, только то же подключение и та же модель</i>"]
        end

        subgraph ST5["⑤ Accounting — запечатана"]
            M9["**Usage** - <i>try/finally вокруг вызова: отмена и ошибка тоже дают строку</i>"]
        end

        subgraph ST6["⑥ Transport — запечатана"]
            M10["**Credentials** - <i>резолв секрета в последний момент, дальше материал не оседает</i>"]
        end

        ST1 --> ST2 --> ST3 --> ST4 --> ST5 --> ST6
    end

    GW --> ST1
    ST6 --> PROV["**ILlmClient** - <i>терминальный шаг, единственное место, знающее провод: LM Studio · OpenAI · Anthropic · Google</i>"]
    PROV --> RES["**LlmTurnResult** - <i>message · rawUsage · modelReported · status</i>"]
    RES -.- BACK["обратный ход через те же слои — назад к вызывающему"]

    classDef open fill:#1f5c2e22,stroke:#2f9e4f,stroke-width:2px
    classDef sealedStage fill:#5c1f1f22,stroke:#b34747,stroke-width:2px
    classDef neutral fill:#3a3a3a22,stroke:#888,stroke-width:1px
    class ST2,ST3 open
    class ST4,ST5,ST6 sealedStage
    class ST1 neutral
```

Зелёные стадии открыты плагинам, красные запечатаны хостом. `UseFromPlugin` бросает исключение при
попытке встать в запечатанную — «плагин обнулил учёт» невозможен **по построению**, а не по ревью.

`Retry` — отдельная стадия не для красоты: будь он частью `Transport`, он оказался бы внутри
`Accounting`, и три сетевые попытки записались бы одной строкой. Каждая попытка стоит денег.

---

## 2. Два потока: вызов и данные

Главный вопрос про конвейер — «а живой вывод рассуждений не сломается?». Не сломается, потому что
поток данных проходит мимо слоёв.

```mermaid
%%{init: {'flowchart': {'subGraphTitleMargin': {'top': 12, 'bottom': 12}, 'nodeSpacing': 40, 'rankSpacing': 50, 'wrappingWidth': 260, 'htmlLabels': true}}}%%
flowchart LR
    subgraph CALLPATH["Поток ВЫЗОВА — 2 раза за ход: вниз и вверх"]
        direction LR
        A1["Trace"] --> A2["Policy"] --> A3["Content"] --> A4["Retry"] --> A5["Accounting"] --> A6["Transport"] --> A7["провайдер"]
    end

    SOCK["**сокет провайдера** - <i>цикл чтения SSE</i>"]
    SINK["**ctx.OnDelta** / **ctx.OnReasoning**"]
    UI["**WebSocket → браузер** / **Spectre.Console → консоль воркера**"]

    SOCK ==>|"поток ДАННЫХ — тысячи раз за ход"| SINK
    SINK ==> UI
    A7 -.->|"зовёт приёмники прямо из цикла"| SOCK

    NOTE["Middleware в потоке вызова и физически отсутствует в потоке данных.<br/>Первый токен приходит в ту же миллисекунду, сколько бы слоёв ни было."]

    classDef note fill:#3a3a1f22,stroke:#b3a047,stroke-width:1px
    class NOTE note
```

Отсюда три правила: приёмники остаются приёмниками (никаких `IAsyncEnumerable` вверх через слои);
обёртка приёмника пересылает чанк **немедленно** и только потом что-то с ним делает; в обёртке —
только дешёвая неблокирующая работа, потому что приёмник ожидается внутри цикла чтения сокета.

---

## 3. Сборка и время жизни

Три уровня с тремя разными временами жизни. Путать их — главный способ сгноить такую архитектуру.

```mermaid
%%{init: {'flowchart': {'subGraphTitleMargin': {'top': 12, 'bottom': 12}, 'nodeSpacing': 40, 'rankSpacing': 50, 'wrappingWidth': 260, 'htmlLabels': true}}}%%
flowchart TB
    subgraph L1["① Хост — один раз при старте процесса"]
        direction LR
        H1["**SPLA.CLI** - <i>профиль local</i>"]
        H2["**SPLA.Server** - <i>профиль server</i>"]
        H3["**воркер** - <i>bare или worker</i>"]
        BP["**LlmPipelineBlueprint** - <i>список middleware по стадиям + хостовые реализации политик</i>"]
        H1 --> BP
        H2 --> BP
        H3 --> BP
    end

    subgraph L2["② AgentRuntime — в конструкторе, после загрузки плагинов"]
        direction TB
        P1["LoadPlugins()"] --> P2["ProviderRegistry.From(plugins)"]
        P2 --> P3["**blueprint.Build(registry, settings)** - <i>валидация порядка здесь, на старте</i>"]
        P3 --> P4["**ILlmGateway** - <i>неизменяем, один на рантайм, общий для всех чатов</i>"]
    end

    subgraph L3["③ Ход — не собирается НИЧЕГО"]
        direction TB
        T1["new LlmTurnContext { … }"] --> T2["gateway.InvokeAsync(ctx, ct)"]
    end

    BP --> P3
    P4 --> T2

    NOTE2["Провайдеры приходят плагинами, а плагины включаются по проекту — поэтому выпечка именно здесь, а не при старте процесса."]
    P3 -.- NOTE2

    classDef host fill:#1f3c5c22,stroke:#4787b3,stroke-width:2px
    classDef rt fill:#1f5c2e22,stroke:#2f9e4f,stroke-width:2px
    classDef turn fill:#5c4a1f22,stroke:#b39147,stroke-width:2px
    class L1 host
    class L2 rt
    class L3 turn
```

Две ловушки, которые эта схема должна напоминать:

- **не захватывать `ResolvedSettings` в замыкание middleware** — настройки живые, читать через
  аксессор, иначе правка подключения перестанет доезжать;
- **правка подключений не требует пересборки** (резолвятся на ходе), смена набора плагинов —
  требует, и это уже событие уровня рантайма.

---

## 4. Граница, которую нельзя двигать

```mermaid
%%{init: {'flowchart': {'subGraphTitleMargin': {'top': 12, 'bottom': 12}, 'nodeSpacing': 40, 'rankSpacing': 50, 'wrappingWidth': 260, 'htmlLabels': true}}}%%
flowchart TB
    subgraph AGENT["Ход агента — N сетевых вызовов, живёт НАД gateway"]
        direction TB
        U["сообщение пользователя"] --> L1{"нужен вызов модели?"}
        L1 -->|да| G["**gateway.InvokeAsync** - <i>= ОДИН вызов к ОДНОЙ модели</i>"]
        G --> TC{"модель попросила инструменты?"}
        TC -->|да| EX["**выполнить инструменты** - <i>дописать результаты в диалог</i>"]
        EX --> L1
        TC -->|нет| DONE["ответ пользователю"]
    end

    classDef gw fill:#5c1f4a22,stroke:#b347a0,stroke-width:2px
    class G gw
```

Всё, что рассуждает о ходе целиком — уплотнение контекста, цикл инструментов, сборка промпта, —
**не middleware**. Иначе получится второй оркестратор, конкурирующий с настоящим.
