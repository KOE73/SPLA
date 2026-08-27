# demo/mcp-servers — подключение внешних MCP-серверов

RU · [EN below](#en)

Другие категории `demo/` — самодостаточные приложения (`workers/`), готовые к запуску проекты
(`projects/`) или промпты для проверки уже встроенных инструментов (`prompts/`). Здесь — третье:
**SPLA как MCP-клиент**, подключающийся к чужому (foreign) MCP-серверу. У такого демо нет
единственного правильного `.spla` — сервер, версия пакета и адрес всегда чьи-то конкретные, — поэтому
вместо готового проекта здесь лежит **пошаговый runbook**: что скачать, как прописать в `.spla`, откуда
взять секрет и куда его положить, как перезапустить и как проверить, что дошло до реального сервера.

Runbook написан так, чтобы его мог провести агент с доступом к shell и файловой системе, от начала до
конца — **кроме одного шага**: сам секрет (PAT/ключ/пароль) заводит и вводит человек, вне чата, через
`spla secret set` или панель Settings → Secrets. Это не ограничение runbook'а, а инвариант хранилища
секретов SPLA — см. [`agents/secrets.md`](../../agents/secrets.md) §6: ни один MCP-инструмент не
принимает секрет аргументом, и агенту нельзя просить пользователя вставить пароль в чат.

| Набор | Внешний сервер | Что демонстрирует |
|-------|-----------------|--------------------|
| [azure-devops-code-search](azure-devops-code-search/) | `@ahouben/azure-devops-mcp` (npm, stdio) | подключение SPLA к MCP-серверу поиска кода Azure DevOps / TFS on-prem: установка пакета, ссылка `secret:` в конфиге вместо переменной окружения, обязательный полный рестарт после смены секрета, проверка без участия человека по логу и по `tools/list` |

---

<a name="en"></a>

# demo/mcp-servers — connecting foreign MCP servers

The other `demo/` categories are self-contained apps (`workers/`), ready-to-run projects
(`projects/`), or prompts that exercise tools SPLA already ships (`prompts/`). This one is a third
kind: **SPLA as an MCP client**, talking to somebody else's (foreign) MCP server. There is no single
correct `.spla` for that — the server, its package version and its address are always somebody's own —
so instead of a runnable project this holds a **step-by-step runbook**: what to download, what to put
in `.spla`, where the secret comes from and where it goes, how to restart, and how to verify the
connection actually reaches the real server.

The runbook is written so an agent with shell and filesystem access can carry it out start to finish —
**except one step**: the secret itself (a PAT, key, or password) is created and entered by a human,
out of band, via `spla secret set` or the Settings → Secrets panel. That is not a runbook limitation,
it is an invariant of SPLA's secret store — see [`agents/secrets.md`](../../agents/secrets.md) §6: no
MCP tool accepts a secret as an argument, and an agent must never ask the user to paste a password into
chat.

| Set | Foreign server | What it demonstrates |
|-----|-----------------|------------------------|
| [azure-devops-code-search](azure-devops-code-search/) | `@ahouben/azure-devops-mcp` (npm, stdio) | connecting SPLA to Azure DevOps / on-prem TFS's code-search MCP server: installing the package, a `secret:` reference instead of an env var, the mandatory full restart after a secret changes, verifying without a human via the log and `tools/list` |
