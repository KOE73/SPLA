# Стандартизация кнопок-действий в web-клиенте

Статус: аудит завершён, правки не внесены (проходим оптом отдельно).
Аудитория: в первую очередь агент (чек-лист «как делать новую кнопку»), во вторую — человек (список конкретных мест для правки).

## Зачем

В `web/src` одно и то же действие (удалить строку, добавить строку, свернуть/развернуть, переподключиться, протестировать соединение) в разных панелях реализовано визуально и структурно по-разному: разные классы, разные глифы, разная разметка. Это не единичные опечатки — паттерн повторяется в 6+ файлах, значит нужен один стандарт и общие компоненты/классы, а не точечные правки.

Ниже — что нашли (с точными местами), а затем — что предлагается сделать стандартом.

---

## 1. Удаление / Remove

Найдено **шесть разных визуальных реализаций** одного действия:

| Где | Разметка | Классы/CSS | Особенность |
|---|---|---|---|
| [ConnectionCard.vue:9](web/src/surfaces/Settings/ConnectionCard.vue:9) | `<button class="x" title="Remove">✕</button>` | `.conn-name-row .x` (app.css:397) | голый глиф, без рамки, красный только на hover |
| [ConnectionCard.vue:83](web/src/surfaces/Settings/ConnectionCard.vue:83) | `<button class="x">✕</button>` | `.conn-model-line .x` (app.css:478) | тот же глиф, но **другой** `.x` — с рамкой |
| [McpPanel.vue:61](web/src/surfaces/Settings/McpPanel.vue:61) | `<button class="btn ghost">✕ Remove</button>` | глобальный `.btn.ghost` | текст+глиф, кнопка с рамкой (наш недавний фикс — на неё и ориентируемся) |
| [KvRows.vue:21](web/src/surfaces/Settings/KvRows.vue:21) | `<button class="btn ghost" title="Remove">✕</button>` | `.btn.ghost` | только глиф, но уже на "правильном" базовом классе |
| [TableArrayRenderer.vue:46-54](web/src/surfaces/Workspace/editors/TableArrayRenderer.vue:46) | `<button class="tarr-del">✕</button>` | свой `.tarr-del` (файл, строки 218-227) | hover-цвет — **захардкоженный `#e55`**, а не `var(--danger)` |
| [ChatListItem.vue:12,17](web/src/surfaces/ChatListItem.vue:12) | `<span class="x" title="Delete permanently">✕</span>` | `.chat-item .x` (файл, строка 79) | это `<span>`, не `<button>`; невидим, пока не наведёшь на строку (`opacity:0` → `.8`) |
| [SecretsPanel.vue:49](web/src/surfaces/Settings/SecretsPanel.vue:49) | `<button class="btn ghost del" title="Delete entry">🗑</button>` | `.del` модификатор | глиф-корзина вместо ✕ |
| SSH-плагин [SettingsPanel.vue:37](src/plugins/SPLA.Plugins.Ssh/web/src/SettingsPanel.vue:37) | `<button>✕ Remove</button>` | без класса, общий `button{}` плагина | плагин вообще не подключён к app.css |
| SQL-плагин SettingsPanel.vue:42 | `<button>✕ Remove</button>` | то же | дубль того же паттерна |

Отдельно: `.btn.danger { background: var(--danger); color:#fff }` (app.css:56) **определён, но нигде не используется** — везде вместо заливки красным используют hover-подсветку текста в красный. То есть в состоянии покоя опасность действия визуально никак не выделена.

## 2. Добавление / Add

Формулировки и глифы почти везде разные:

| Где | Текст | Глиф |
|---|---|---|
| [ConnectionsPanel.vue:16](web/src/surfaces/Settings/ConnectionsPanel.vue:16) | "+ Add connection" | ASCII `+` |
| [McpPanel.vue:131](web/src/surfaces/Settings/McpPanel.vue:131) | "＋ Add server" | **полноширинный** `＋` (U+FF0B) |
| [KvRows.vue:23](web/src/surfaces/Settings/KvRows.vue:23) | "＋ add" | полноширинный `＋`, с маленькой буквы |
| [SkillsPanel.vue:20](web/src/surfaces/Settings/SkillsPanel.vue:20) | "+ add folder" | ASCII `+`, `.btn.tiny` |
| [SecretsPanel.vue:58](web/src/surfaces/Settings/SecretsPanel.vue:58) | "＋ New entry" | полноширинный `＋`, "New" с большой буквы |
| [TableArrayRenderer.vue:18-20](web/src/surfaces/Workspace/editors/TableArrayRenderer.vue:18) | "+ Add" | свой класс `.tarr-add`, залит `--accent-soft` — единственная "Add"-кнопка с цветом в покое |
| [ProjectPicker.vue:48](web/src/surfaces/ProjectPicker.vue:48) | "+ New Project…" | `.btn-new-project`, **на всю ширину**, пунктирная рамка — единственный такой случай |
| [ChatList.vue:17](web/src/surfaces/ChatList.vue:17) | "+ New" | свой класс `.btn-new`, визуально похож на `.btn.ghost`, но не переиспользует его |
| SSH-плагин SettingsPanel.vue:27 | "+ Add host" | без класса |
| SQL-плагин SettingsPanel.vue:27 | "+ Add Connection" | без класса, "Connection" с большой буквы — расходится даже с соседним SSH-плагином |

Проблема не только в `+`/`＋`: у каждого файла — свой класс кнопки-Add вместо одного общего.

## 3. Разворот/сворачивание строки (chevron)

Глиф везде одинаковый (▾/▸ — тут всё хорошо), но:

- [ConnectionCard.vue:80](web/src/surfaces/Settings/ConnectionCard.vue:80) — `<button class="conn-model-caret">`, шеврон **слева** от строки.
- [McpPanel.vue:62](web/src/surfaces/Settings/McpPanel.vue:62) — `<span class="chev">`, шеврон **справа**.
- [PluginsPanel.vue:17](web/src/surfaces/Settings/PluginsPanel.vue:17) — тот же `.chev`, но CSS-правило **скопировано заново** в scoped-стиль файла.
- [SkillsPanel.vue:58](web/src/surfaces/Settings/SkillsPanel.vue:58) — тот же `.chev`, CSS скопирован в третий раз.

`.chev` — три идентичных копии одного и того же правила в трёх файлах вместо одного места в app.css. Плюс путаница `<button>` vs `<span>` и позиция слева/справа.

## 4. Edit

Отдельной кнопки "редактировать" почти нет — редактирование почти везде реализовано через тот же chevron (разворот строки = режим правки). Исключение:

- [ChatListItem.vue:15](web/src/surfaces/ChatListItem.vue:15) — `<span class="x" title="Rename">✎</span>` — карандаш переиспользует тот же класс `.x`, что и удаление/восстановление/архивирование этой же строки (один класс — четыре разных действия, различаются только глифом).

## 5. Reconnect / Retry / Refresh

- [McpPanel.vue:57-60](web/src/surfaces/Settings/McpPanel.vue:57) — текстовая кнопка `.btn.ghost`, busy-состояние = смена текста на "reconnecting…".
- [ConnectionsPanel.vue:5](web/src/surfaces/Settings/ConnectionsPanel.vue:5) — глиф ↻, класс `.btn.ghost conn-recheck` — `conn-recheck` нигде не определён (мёртвый класс).
- [SecretsPanel.vue:18](web/src/surfaces/Settings/SecretsPanel.vue:18) — тот же глиф ↻, тот же паттерн — тут хорошо, единообразно.
- [ConnectionCard.vue:105-107](web/src/surfaces/Settings/ConnectionCard.vue:105) — "Test chat" / "…" при busy — тоже смена текста, но своя формулировка busy.
- SSH-плагин SettingsPanel.vue:64 — "Test connection", busy показывается **соседним** `<span class="muted">`, а не сменой текста кнопки.
- SQL-плагин SettingsPanel.vue:81 — "Test Connection" — та же логика, но с большой буквы (расхождение даже между двумя плагинами).
- [ConnectionLost.vue:20-23](web/src/surfaces/ConnectionLost.vue:20) — свой класс `.lost-btn` / `.lost-btn.primary`, который по сути дублирует связку `.btn.ghost` / `.btn` (filled), но никак с ними не связан.

## 6. Прочие мелкие действия строк

- **Copy to clipboard** в [SecretsPanel.vue](web/src/surfaces/Settings/SecretsPanel.vue) реализован **двумя разными способами в одном файле**: строка 43 — `.chip-btn` (прозрачная, проявляется на hover), строка 46 — `.btn.ghost.tiny` с текстом "⧉ ref".
- **Вкл/выкл источника**: в SkillsPanel.vue:90-93 это текстовая кнопка `.btn.tiny` с надписью "on"/"off"; в PluginsPanel.vue:11 то же самое действие — обычный `<input type="checkbox">`. Одно действие — два разных типа контрола в соседних панелях.
- Круглая кнопка-инфо `.pi-btn` (app.css:441) и квадратная `.icon-btn` (app.css:225) — ещё два самостоятельных "мелких кнопочных" семейства, не пересекающихся с `.x`/`.btn.ghost`.

---

## Сквозные проблемы (сводка)

1. **Пять независимых `.x`-классов** с одним и тем же именем, но разным CSS и даже разным смыслом (remove / close / rename / restore / archive) — в app.css их три (`.conn-name-row .x`, `.conn-head .x`, `.conn-model-line .x`), плюс по одному в ChatListItem.vue и ProjectPicker.vue.
2. **`.btn.danger` определён, но не используется нигде** — опасность действия нигде не видна в состоянии покоя, только на hover.
3. **`+` vs `＋`** — вперемешку, без правила.
4. **`.chev` скопирован в 3 файла** вместо одного общего правила.
5. **Плагины (SSH, SQL) визуально не связаны с app.css** — у каждого свой `button{}` с почти теми же значениями (`border-radius:5px`, `padding:2px 10px`), продублированный между собой — при смене темы разъедутся незаметно.
6. **Одно и то же действие "включить/выключить строку"** — где-то чекбокс, где-то текстовая кнопка on/off.
7. **"Test …"** в ConnectionCard vs в плагинах — два визуально несвязанных подхода плюс разный регистр текста между двумя плагинами.
8. **Busy-состояние** показывается тремя способами: смена текста кнопки / соседний `<span>` / просто `disabled` без индикации.

---

## Предложение (для обсуждения, не применено)

Завести в `app.css` (или отдельном `actions.css`) небольшой набор именованных классов под конкретные *роли*, а не переиспользовать нейтральный `.x`/`.btn.ghost` под что попало:

- `.btn-remove` — рамка, глиф `✕`, текст "Remove" опционален через слот/атрибут — красный **не только на hover**, а сразу приглушённо-красный (border/color), чтобы отличаться от нейтральных ghost-кнопок с первого взгляда.
- `.btn-add` — рамка, единый глиф (зафиксировать ASCII `+`, отказаться от `＋`), единая формулировка `+ Add <noun>`.
- `.chev` — оставить как есть, но вынести из трёх файлов в app.css один раз; зафиксировать позицию (предлагается: справа, как в McpPanel — она читается лучше при длинных именах строк слева).
- `.btn-reconnect`/`.btn-retry` — единый паттерн busy-состояния: **всегда** смена текста кнопки на "…" или конкретное продолженное действие, никогда не через соседний `<span>`.
- Решить: чекбокс или текстовая on/off-кнопка для "включить/выключить строку" — выбрать одно и применить везде (PluginsPanel и SkillsPanel сейчас расходятся).
- Плагинам (SSH/SQL) либо выдать общий `plugin-actions.css` с теми же именами классов, либо явно документировать, что они живут в изолированном визуальном контуре и почему.

Как компоненты (если переходить на переиспользуемые Vue-компоненты, не только классы): `<RowRemoveButton>`, `<RowAddButton>`, `<RowChevron>`, `<AsyncActionButton busy-label="...">` — по одному компоненту на роль, чтобы вариативность физически была невозможна (нельзя написать другой глиф/цвет, не поменяв компонент).

## Как использовать этот документ

Агенту — при добавлении новой кнопки в любой Settings-панели: сначала проверить, есть ли уже роль в этом списке, и переиспользовать существующий класс/компонент, а не копировать разметку из соседнего файла. Пока стандарт не внедрён — ориентироваться на `McpPanel.vue`/`KvRows.vue` (`.btn.ghost` + текст) как на наименее плохой из существующих вариантов.

Человеку — список выше это и есть план правок "оптом": можно идти по разделам 1-6 сверху вниз.
