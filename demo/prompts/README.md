# demo/prompts — готовые промпты для проверки инструментов

RU · [EN below](#en)

Здесь лежат промпты, которыми инструменты SPLA проверяются **на живом стенде**, а не в юнит-тестах.
Юнит-тест показывает, что код делает то, что задумано; такой промпт показывает другое — понимает ли
модель, что ей дали, и внятно ли инструмент отказывает, когда должен отказать. Оба вопроса нельзя
закрыть в CI, поэтому промпты живут отдельно.

Каждый набор состоит из двух частей: **SETUP** — что настроить до запуска (без этого промпт не
поедет), и **PROMPT** — текст, который копируется агенту в чат целиком.

| Набор | Что проверяет | Нужен стенд |
|-------|---------------|-------------|
| [sftp-transfer](sftp-transfer/) | передача файлов по SFTP и контейнеры `.tar`: листинг, скачивание в папку и в контейнер, чтение и правка контейнера, лимиты, границы путей, запрет записи на read-only хост | Linux-хост с SSH (два, один только для чтения) |

Если промпт проверяет не только успешные пути, но и отказы, скажи агенту об этом прямо в первом
абзаце. Иначе он воспримет отказ как препятствие и начнёт искать обход, и прогон превратится в
проверку изобретательности модели вместо проверки инструмента. В `sftp-transfer` это сделано
именно так — там отказов примерно половина.

---

<a name="en"></a>

# demo/prompts — ready-made prompts for exercising tools

These are the prompts used to check SPLA's tools **against a live rig**, not in unit tests. A unit
test shows the code does what it was written to do; a prompt like this shows something else — whether
the model understands what it was handed, and whether a tool refuses clearly when it should. Neither
question fits in CI, so the prompts live separately.

Each set has two parts: **SETUP** — what to configure first (nothing runs without it), and
**PROMPT** — the text to paste to the agent as a whole.

| Set | What it exercises | Rig needed |
|-----|-------------------|------------|
| [sftp-transfer](sftp-transfer/) | file transfer over SFTP and `.tar` containers: listing, downloading to a folder and to a container, reading and editing a container, limits, path boundaries, refusing writes to a read-only host | a Linux host with SSH (two, one read-only) |

If a prompt checks refusals as well as happy paths, say so to the agent in the opening paragraph.
Otherwise it treats a refusal as an obstacle and goes looking for a way around it, and the run turns
into a measure of the model's ingenuity instead of a check on the tool. `sftp-transfer` does exactly
that — about half its steps are refusals.
