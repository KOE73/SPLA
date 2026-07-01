# Web client → Vue 3 + TypeScript + Vite — migration runbook

> Исполнительский план для агента с низким reasoning. Делай шаги **строго по порядку**.
> После каждого шага выполняй блок **Проверка**. Если проверка красная — чини, прежде чем идти дальше.
> Не импровизируй: все решения уже приняты ниже. Где написано «скопируй verbatim» — копируй буквально, без «улучшений».

---

## 0. Что уже есть (контекст для холодного старта)

- `.NET`-сервис отдаёт web-клиент как статику через `SPLA.Service/WebAssets.cs`:
  - dev: читает файлы прямо из `SPLA.Service/WebClient/`;
  - prod: из embedded-ресурсов сборки (`SPLA.Service.WebClient.*`).
- Клиент общается с сервером **только по WebSocket** на `/ws`. Конверт сообщения:
  `{ type: string, payload: any, chatId?: string, requestId?: string }`.
- Текущий клиент (НЕ трогаем до фазы 8):
  - `WebClient/app.js` (951 стр) — бутстрап + 8 «сюрфейсов» (chatList, statusBar, filters, chatLog, composer, debug, wire, settings), весь рендер через `createElement`.
  - `WebClient/surfaces.js` — реестр сюрфейсов + layout + шина + lifecycle (`dispose` уже есть, работает).
  - `WebClient/renderers.js` — движок markdown/mermaid/code-frame. **Ценный, переписывать нельзя — только оборачивать.**
  - `WebClient/app.css`, `WebClient/themes.css` — стили. Переезжают как есть.
  - `WebClient/lib/marked.min.js`, `WebClient/lib/mermaid.min.js` — вендоренные либы.
- Сервис в dev слушает порт **5071** (см. `.claude/launch.json`).

## Цель

Заменить **только слой представления** на Vue 3 SFC. Сервер, протокол, движок markdown, CSS — сохранить.
Новый код живёт в **новой папке `web/`** в корне репозитория. Старый клиент работает без изменений всю миграцию.

---

## ЖЁСТКИЕ ПРАВИЛА (нарушать нельзя)

1. **Не трогай ни одного `*.cs`** до фазы 8. В фазе 1 — только если проверка requestId провалится (точечно).
2. **Не трогай `SPLA.Service/WebClient/`** до фазы 8. Старый клиент должен работать всё время.
3. **Один компонент = один `.vue`-файл, ≤120 строк.** Больше — разбей на под-компоненты.
4. **Логику markdown/mermaid из `renderers.js` копируй verbatim** в composable. Не рефактори её.
5. **Стриминг чата: сохрани debounce 70 мс.** Не ре-рендери markdown на каждый токен.
6. **Рендереный markdown выводи через `v-html`**, а mermaid/подсветку запускай в `watch`/`onUpdated`. Внутрь `v-html`-узла Vue не лезет — это правильно.
7. После каждого изменения файла: `npx vue-tsc --noEmit` (типы) и `npm run build`. Красное — чини сразу.
8. `chatLog` мигрируется **последним** (фаза 7). Не раньше.
9. Не добавляй сторонних зависимостей кроме перечисленных (`vue`, `vite`, `@vitejs/plugin-vue`, `typescript`, `vue-tsc`). Markdown/mermaid берём из существующих `lib/`.

---

## Целевая структура `web/`

```
web/
  index.html
  package.json
  tsconfig.json
  vite.config.ts
  public/
    lib/marked.min.js        (скопировать из WebClient/lib/)
    lib/mermaid.min.js       (скопировать из WebClient/lib/)
  src/
    main.ts                  бутстрап Vue
    protocol/
      types.ts               TS-типы конверта + payload'ов
      SplaClient.ts          транспорт + типизированная шина + Promise-RPC
    state/
      store.ts               реактивное состояние приложения
    composables/
      useMarkdown.ts         ОБЁРТКА над логикой renderers.js (verbatim внутри)
    surfaces/
      registry.ts            имя → Vue-компонент
      ChatList.vue
      StatusBar.vue
      Filters.vue
      Composer.vue
      Debug.vue
      Wire.vue
      Settings/
        Settings.vue
        ConnectionsPanel.vue
        ConnectionCard.vue
        AppearancePanel.vue
        PluginsPanel.vue
      ChatLog/
        ChatLog.vue
        AssistantBubble.vue
        UserBubble.vue
        ToolLine.vue
        PermissionAsk.vue
        ClarifyAsk.vue
    layouts/
      layouts.ts             определения раскладок (перенести из WebClient/layouts.js)
      LayoutHost.vue         рендерит layout → слоты → компоненты
    styles/
      app.css                (скопировать из WebClient/app.css)
      themes.css             (скопировать из WebClient/themes.css)
  dist/                      результат `npm run build` (создаётся автоматически)
```

---

## ФАЗА 0 — Каркас Vite + Vue + TS (приложение пустое, но собирается)

**Шаги:**

1. Создай папку `web/` в корне репозитория.
2. Создай `web/package.json`:
   ```json
   {
     "name": "spla-web",
     "private": true,
     "type": "module",
     "scripts": {
       "dev": "vite",
       "build": "vue-tsc --noEmit && vite build",
       "typecheck": "vue-tsc --noEmit"
     },
     "dependencies": { "vue": "^3.5.0" },
     "devDependencies": {
       "@vitejs/plugin-vue": "^5.0.0",
       "typescript": "^5.6.0",
       "vite": "^6.0.0",
       "vue-tsc": "^2.1.0"
     }
   }
   ```
3. Создай `web/tsconfig.json`:
   ```json
   {
     "compilerOptions": {
       "target": "ES2022", "module": "ESNext", "moduleResolution": "Bundler",
       "strict": true, "jsx": "preserve", "lib": ["ES2022", "DOM", "DOM.Iterable"],
       "types": [], "skipLibCheck": true, "isolatedModules": true, "noEmit": true
     },
     "include": ["src/**/*.ts", "src/**/*.vue", "vite.config.ts"]
   }
   ```
4. Создай `web/vite.config.ts`:
   ```ts
   import { defineConfig } from "vite";
   import vue from "@vitejs/plugin-vue";

   export default defineConfig({
     plugins: [vue()],
     build: { outDir: "dist", emptyOutDir: true },
     server: {
       port: 5173,
       proxy: { "/ws": { target: "ws://localhost:5071", ws: true } }
     }
   });
   ```
5. Создай `web/index.html`:
   ```html
   <!doctype html>
   <html><head><meta charset="utf-8"><title>SPLA</title>
     <script src="/lib/marked.min.js"></script>
     <script src="/lib/mermaid.min.js"></script>
     <link rel="stylesheet" href="/src/styles/themes.css">
     <link rel="stylesheet" href="/src/styles/app.css">
   </head><body><div id="app"></div><script type="module" src="/src/main.ts"></script></body></html>
   ```
6. Скопируй `WebClient/lib/marked.min.js` и `WebClient/lib/mermaid.min.js` → `web/public/lib/`.
7. Скопируй `WebClient/app.css` → `web/src/styles/app.css`, `WebClient/themes.css` → `web/src/styles/themes.css`.
8. Создай `web/src/main.ts`:
   ```ts
   import { createApp, h } from "vue";
   createApp({ render: () => h("div", "SPLA web — каркас работает") }).mount("#app");
   ```
9. Выполни `npm install` в `web/`.

**Проверка:**
- `cd web && npm run build` → завершается без ошибок, появилась папка `web/dist/`.
- `cd web && npm run dev` → открой `http://localhost:5173`, видна надпись «каркас работает».
- Старый клиент (на порту 5071) по-прежнему открывается и работает.

**Готово, когда:** обе команды зелёные, обе страницы открываются.

---

## ФАЗА 1 — Типизированный протокол: `SplaClient` + типы + store

**Шаги:**

1. `web/src/protocol/types.ts` — опиши конверт и payload'ы. Имена событий и поля бери из текущего `WebClient/app.js` (поиск по `c.sub(` и `c.send(`):
   ```ts
   export interface Envelope<P = unknown> { type: string; payload: P; chatId?: string; requestId?: string; }
   // Серверные события (приходят сами):
   export interface ServerEvents {
     "welcome": { theme?: string; density?: string };
     "chat.opened": { chatId: string; messages: ChatMessage[] };
     "chat.list.result": { chats: ChatSummary[] };
     "delta": { msgIndex: number; text: string };
     "reasoning": { msgIndex: number; text: string };
     "assistant.message": { msgIndex: number; message: ChatMessage };
     "llm.turn.start": { msgIndex: number };
     "turn.complete": Record<string, never>;
     "tool.started": { toolCall: { name: string } };
     "tool.result": { toolName: string; result: string };
     "connections.result": { connections: Connection[] };
     "connections.health": { health: Record<string, ConnHealth> };
     // ...допиши остальные по мере встречи в app.js
   }
   export interface ChatMessage { role: "user" | "assistant" | "tool"; content?: string; reasoning?: string; images?: string[]; }
   export interface ChatSummary { id: string; title?: string; }
   export interface Connection { id: string; clientId?: string; name?: string; provider?: string; endpoint?: string; model?: string; apiKey?: string; lockModel?: boolean; swapModel?: boolean; }
   export interface ConnHealth { ok: boolean | null; error?: string; }
   ```
   > Не обязательно описать ВСЁ сразу. Описывай payload по мере переноса экрана. Неизвестное — `unknown`, не `any`.

2. `web/src/protocol/SplaClient.ts` — транспорт + шина + RPC:
   ```ts
   import type { Envelope, ServerEvents } from "./types";
   type Handler<P> = (payload: P, env: Envelope<P>) => void;

   export class SplaClient {
     private ws?: WebSocket;
     private listeners = new Map<string, Set<Handler<any>>>();
     private pending = new Map<string, { resolve: (v: any) => void; reject: (e: any) => void; timer: number }>();

     connect(): void {
       const proto = location.protocol === "https:" ? "wss://" : "ws://";
       this.ws = new WebSocket(proto + location.host + "/ws");
       this.ws.onopen = () => this.send("hello", { clientName: "web", protocolVersion: "1" });
       this.ws.onclose = () => { this.emit("conn", { on: false } as any, {} as any); setTimeout(() => this.connect(), 1500); };
       this.ws.onmessage = ev => {
         let env: Envelope; try { env = JSON.parse(ev.data); } catch { return; }   // битый JSON — игнор
         if (env.requestId && this.pending.has(env.requestId)) {
           const p = this.pending.get(env.requestId)!; clearTimeout(p.timer); this.pending.delete(env.requestId);
           p.resolve(env.payload);
         }
         this.emit(env.type as keyof ServerEvents, env.payload as any, env);
       };
     }

     send(type: string, payload?: unknown, extra?: { chatId?: string; requestId?: string }): boolean {
       if (!this.ws || this.ws.readyState !== WebSocket.OPEN) return false;   // не врём: вернули false
       this.ws.send(JSON.stringify({ type, payload, ...extra }));
       return true;
     }

     /** Команда с ответом. Сервер ДОЛЖЕН вернуть тот же requestId. */
     invoke<R = unknown>(type: string, payload?: unknown, timeoutMs = 15000): Promise<R> {
       const requestId = crypto.randomUUID();
       return new Promise<R>((resolve, reject) => {
         const timer = window.setTimeout(() => { this.pending.delete(requestId); reject(new Error("timeout: " + type)); }, timeoutMs);
         this.pending.set(requestId, { resolve, reject, timer });
         if (!this.send(type, payload, { requestId })) { clearTimeout(timer); this.pending.delete(requestId); reject(new Error("socket closed")); }
       });
     }

     on<K extends keyof ServerEvents>(type: K, fn: Handler<ServerEvents[K]>): () => void {
       (this.listeners.get(type as string) ?? this.listeners.set(type as string, new Set()).get(type as string)!).add(fn as Handler<any>);
       return () => this.listeners.get(type as string)?.delete(fn as Handler<any>);
     }
     private emit<K extends keyof ServerEvents>(type: K, payload: ServerEvents[K], env: Envelope) {
       this.listeners.get(type as string)?.forEach(fn => { try { fn(payload, env); } catch (e) { console.error(type, e); } });
     }
   }
   export const client = new SplaClient();
   ```

3. `web/src/state/store.ts` — реактивное состояние:
   ```ts
   import { reactive } from "vue";
   export const store = reactive({
     connected: false,
     currentChat: null as string | null,
     chats: [] as import("../protocol/types").ChatSummary[],
   });
   ```

4. **Проверь requestId на сервере.** В `SPLA.Service/ClientConnection.cs` найди, где формируется ответ на команды (`connection.models`, `connections.save`, `connection.test`). Убедись, что входящий `requestId` копируется в ответный конверт. Если **нет** — это единственная разрешённая правка `.cs` в этой фазе: пробрось `requestId` из запроса в ответ. Если да — ничего не делай.

**Проверка:**
- `npm run typecheck` зелёный.
- Временно в `main.ts`: `client.connect()` и `client.on("welcome", p => console.log("welcome", p))`. Запусти `npm run dev` при живом сервисе 5071 → в консоли браузера виден `welcome`. После проверки убери временный код.

**Готово, когда:** типы зелёные, `welcome` ловится через типизированный `client.on`.

---

## ФАЗА 2 — Реестр surface + layout как Vue

**Шаги:**

1. Перенеси определения раскладок из `WebClient/layouts.js` → `web/src/layouts/layouts.ts` (тот же `{ skeleton, placement }`, но `placement` указывает на имена компонентов).
2. `web/src/surfaces/registry.ts`:
   ```ts
   import type { Component } from "vue";
   // компоненты добавляются по мере переноса в следующих фазах
   export const surfaces: Record<string, Component> = {};
   ```
3. `web/src/layouts/LayoutHost.vue` — рендерит активный layout: по `placement` достаёт компонент из `surfaces` и кладёт в слот (`<component :is="...">`). Поддержи `?surface=<name>` — если задан, рендерит один компонент на весь экран (solo-окно).
4. В `main.ts` смонтируй `LayoutHost`, вызови `client.connect()`.

**Проверка:**
- `npm run typecheck` + `npm run build` зелёные.
- `npm run dev`: пустые слоты раскладки видны (компонентов ещё нет — это норм).

**Готово, когда:** каркас раскладки рендерится, solo-режим читает `?surface=`.

---

## ФАЗА 3 — Перенос `Settings` (первый реальный экран, самый болезненный)

> Образец `.vue` уже согласован — `<template>` это HTML с биндингами. Логика (`c.send`/`invoke`) в `<script setup>`.

**Шаги:**

1. `ConnectionCard.vue` — карточка одного подключения. Биндинги: `v-model` на name/endpoint/apiKey/model, `v-for` по провайдерам в `<select>`, `v-show="conn.provider === 'lmstudio'"` на чекбоксе Hot-swap, `:class` на индикаторе здоровья. Кнопки `@click`.
   - **Стабильный id:** при создании новой connection ставь `clientId: crypto.randomUUID()`. В `v-for` ключ — `conn.clientId ?? conn.id`. Это убирает баг гонки `_tmp_`.
2. `ConnectionsPanel.vue` — `v-for` по `connections`, рендерит `ConnectionCard`. Список моделей грузи через `await client.invoke("connection.models", {...})` (не через глобальную подписку), результат применяй к **той же** карточке по `clientId`.
3. `ModelPicker` — выпадающий список моделей: позиционируй правым краем к кнопке (как в текущем фиксе), но проще — отдельный маленький компонент с `:style`.
4. `AppearancePanel.vue` — тема/density/layout. Меняются мгновенно (авто-apply), без кнопки Save.
5. `PluginsPanel.vue` — список плагинов.
6. `Settings.vue` — навигация между панелями + кнопка Save для транзакционных вещей (connections/permissions/plugins).
   - **Save честный:** `try { await client.invoke("connections.save", ...); показать "Saved ✓" } catch { показать ошибку }`. Не показывай «Saved» до ответа сервера.
7. Зарегистрируй `Settings` в `surfaces.registry`.

**Проверка:**
- `npm run typecheck` + `build` зелёные.
- `npm run dev`: открой settings. Сравни с тем же экраном старого клиента (порт 5071): те же поля, провайдеры переключаются, Hot-swap скрыт у не-LMStudio, список моделей не вылезает за край, Save показывает успех/ошибку по факту ответа.

**Готово, когда:** settings визуально и функционально эквивалентен старому, баг `_tmp_` и фальшивый Save исправлены.

---

## ФАЗА 4 — `StatusBar`, `Filters`, `ChatList`

**Шаги:**
1. `ChatList.vue` — `v-for` по `store.chats`, `@click` open/rename/delete, `:class` active. Данные из `client.on("chat.list.result", ...)` → `store.chats`.
2. `StatusBar.vue` — индикаторы connected/project/mode/model + health-dot. `v-model` на `<select>` подключения.
3. `Filters.vue` — кнопки-тогглы видимости (`:class` on/off), переключают флаги в `store`.
4. Зарегистрируй все три.

**Проверка:** `typecheck` + `build` зелёные; в dev список чатов открывается/переименовывается/удаляется, статус-бар отражает состояние.

---

## ФАЗА 5 — `Composer`

**Шаги:**
1. `Composer.vue`:
   - `<textarea v-model="text" :disabled="!ready">`, `:disabled`/`:hidden` на Send/Stop через `computed`.
   - Вложения: `v-for` по `attachments`, кнопка удаления `@click`.
   - Drag/paste картинок: `@paste`, `FileReader` → `attachments.push`.
   - Enter без Shift = отправка (`@keydown.enter.exact.prevent="send"`).
   - `send()`: `client.send("chat.send", {...})`, очистить input/attachments, `turnActive=true`.
2. Зарегистрируй.

**Проверка:** `typecheck` + `build`; в dev отправка текста и картинок работает, Stop появляется во время хода, поле блокируется/разблокируется.

---

## ФАЗА 6 — `Debug`, `Wire`

**Шаги:**
1. `Wire.vue` — живой лог конвертов (in/out) с фильтром. Подпишись на исходящие через хук в `SplaClient` (добавь опциональный `onWire`-колбэк в клиент, эмить и для `send`, и для `onmessage`; для `send` эмить **только при успешной отправке** — `dir:"out"` не должен врать).
2. `Debug.vue` — debug-ящик (контекст/токены/что там было).
3. Зарегистрируй.

**Проверка:** `typecheck` + `build`; wire показывает реальные кадры, не врёт при закрытом сокете.

---

## ФАЗА 7 — `ChatLog` ПОСЛЕДНИМ (стриминг + markdown, самый тонкий)

> Здесь главные ловушки. Следуй точно.

**Шаги:**

1. `web/src/composables/useMarkdown.ts` — **обёртка над логикой `renderers.js`**:
   - Скопируй **verbatim** функции `renderMarkdown`, `enhanceCode`, `renderMermaid`, `mermaidTheme`, `escapeHtml`, `neutralizeRawHtml` из `WebClient/renderers.js` (строки 17-83).
   - Экспортируй composable, который: принимает `ref<string>` (raw markdown) и `ref<HTMLElement>` (контейнер), и в `watch` рендерит в контейнер тем же кодом. **Не переписывай парсинг.**
2. `AssistantBubble.vue`:
   - Принимает `msgIndex`. Хранит локально `raw` (накопленный текст) и `reasoning`.
   - Рендер тела: контейнер с `ref`, markdown заливается через скопированную логику (она сама ставит innerHTML — это ОК, Vue внутрь не лезет, т.к. это leaf-узел под `ref`, без Vue-детей).
   - **Стриминг:** на `delta` накапливай в `raw`, ре-рендер markdown через **debounce 70 мс** (скопируй паттерн `clearTimeout`/`setTimeout(...,70)` из `app.js:231-232`). reasoning — в `<details>`.
3. `UserBubble.vue`, `ToolLine.vue` — простые, текст + картинки (`v-for`).
4. `PermissionAsk.vue`, `ClarifyAsk.vue` — кнопки `@click`, по выбору `client.send("permission.decision"/"clarify.choice", ..., { requestId })` и убрать блок.
5. `ChatLog.vue`:
   - Реактивный массив `messages`. На `chat.opened` — заполнить; на `llm.turn.start` — добавить пустой assistant-пузырь; на `delta`/`reasoning`/`assistant.message` — обновить нужный по `msgIndex`; на `tool.started`/`tool.result`/`notice`/`error` — добавить строку.
   - `v-for` по `messages` → соответствующий компонент пузыря.
   - Автоскролл вниз: после обновления (`nextTick`/`onUpdated`) `scrollTop = scrollHeight`. Сохрани поведение «скроллим после debounce-рендера».
6. Зарегистрируй `ChatLog`.

**Проверка:**
- `typecheck` + `build` зелёные.
- В dev отправь сообщение → ответ стримится плавно, markdown/код/mermaid рендерятся, reasoning раскрывается, tool-строки появляются, скролл едет вниз. Сравни с тем же диалогом в старом клиенте — поведение идентично.
- Прогони сообщение с ```mermaid```-блоком и с блоком кода — диаграмма и copy-кнопка работают.

**Готово, когда:** чат в новом клиенте неотличим по поведению от старого, стриминг не лагает.

---

## ФАЗА 8 — Переключение serving на новый клиент + чистка

> Только теперь трогаем `.cs` и старую папку.

**Шаги:**

1. `cd web && npm run build` → `web/dist/` содержит собранный клиент.
2. Интеграция сборки в `.csproj` (`SPLA.Service.csproj`): добавь pre-build шаг, который перед сборкой .NET выполняет `npm --prefix web run build` (Target с `Exec`), и `<EmbeddedResource>` теперь указывает на `web/dist/**` вместо `WebClient/**`.
3. `SPLA.Service/WebAssets.cs`:
   - `FindDevWebClientDir` → искать `web/dist` (или `web/dist` рядом). Префикс embedded-ресурса → под новый путь.
   - Логику отдачи файла не меняй — она остаётся (plain статика).
4. Запусти сервис (порт 5071) **без** vite dev. Открой `http://localhost:5071` → должен открыться новый Vue-клиент, всё работает по WS.
5. Когда новый клиент подтверждён рабочим:
   - Удали старые `WebClient/app.js`, `surfaces.js`, `renderers.js`, `layouts.js` (CSS уже скопирован в `web/src/styles/`; либо оставь `WebClient/` пустым/удали).
   - Обнови `SPLA.slnx`: убери ссылки на удалённые файлы, добавь новую папку `web/` (ключевые файлы).
6. Финальная проверка: `dotnet build` всего решения зелёный; сервис стартует; UI работает в браузере и в нативной Avalonia-обёртке (она грузит тот же web по WS).

**Проверка:**
- `dotnet build SPLA.slnx` зелёный.
- `http://localhost:5071` отдаёт новый клиент, чат/настройки/всё работает.
- Нативное окно (если запускается) показывает тот же UI.

**Готово, когда:** сервис отдаёт собранный Vue-клиент, старые JS-файлы удалены, решение собирается.

---

## Команды-памятка

```bash
cd web
npm install              # один раз
npm run dev              # разработка: http://localhost:5173 (проксирует /ws на :5071)
npm run typecheck        # проверка типов (vue-tsc --noEmit)
npm run build            # сборка в web/dist/
```
Сервис в dev: запускать как обычно (порт 5071). Старый клиент остаётся на 5071 до фазы 8.

## Откат

До фазы 8 откат тривиален: удалить папку `web/` — `.cs` и `WebClient/` не тронуты, старый клиент работает.
Фаза 8 обратима через git (правки `.csproj`/`WebAssets.cs` и удаление файлов в одном коммите — `git revert`).

## Порядок фаз (чек-лист)

- [ ] Ф0 каркас Vite+Vue+TS
- [ ] Ф1 SplaClient + типы + store (+ проверка requestId на сервере)
- [ ] Ф2 реестр surface + LayoutHost
- [ ] Ф3 Settings
- [ ] Ф4 StatusBar / Filters / ChatList
- [ ] Ф5 Composer
- [ ] Ф6 Debug / Wire
- [ ] Ф7 ChatLog (последним)
- [ ] Ф8 переключение serving + чистка
