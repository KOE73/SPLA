# SETUP — подключение Azure DevOps Code Search как MCP-сервера

[English](SETUP.en.md)

Runbook, а не готовый проект: сервер — чужой пакет (`@ahouben/azure-devops-mcp`), адрес и коллекция —
всегда свои. Все шаги, кроме одного явно помеченного, может выполнить агент с доступом к shell и
файловой системе. Секрет заводит и вводит человек — вне чата, см. шаг 4.

Основано на живой проверке подключения к TFS 2012 on-prem (`docs/secrets_ru.md`,
`agents/secrets.md` — источники истины по секретам и ссылкам на них).

## Что нужно знать заранее

- **URL коллекции Azure DevOps / TFS.** Публичный `dev.azure.com/<org>` или адрес on-prem сервера
  (`https://<host>/<collection>`).
- **Node.js `^20.19.0 || ^22.12.0 || >=23`** (реальная зависимость пакета через `yargs`, не
  произвольная «20+»). Проверка: `node --version`.
- Права на чтение репозиториев/проектов, которые собираетесь искать — у аккаунта, чей PAT будет
  использован.

## Шаг 1 — папка проекта и пакет (агент)

Создайте (или используйте существующую) рабочую папку — она станет `workspace` проекта SPLA:

```powershell
New-Item -ItemType Directory -Force <papka-proekta>
cd <papka-proekta>
npm init -y
npm install @ahouben/azure-devops-mcp
```

Зафиксируйте версию в `package.json`/`package-lock.json` — не полагайтесь на `npx`, который тянет
пакет заново при каждом старте и не гарантирует ту же версию.

Проверьте, что пакет ставится и понимает синтаксис, без обращения к реальному серверу:

```powershell
node node_modules/@ahouben/azure-devops-mcp/dist/index.js --help
```

## Шаг 2 — файл проекта `.spla` (агент)

Если проекта ещё нет — `spla init` в этой папке, иначе откройте существующий `<имя>.spla` и добавьте
секцию `mcp.servers`. Значение `AZURE_DEVOPS_PAT` — **ссылка**, не токен; ссылку агент пишет сам,
сам токен — нет (шаг 4):

```yaml
mcp:
  servers:
    - id: azdevops
      name: Azure DevOps Code Search
      enabled: false          # включить на шаге 5, после того как секрет сохранён
      transport: stdio
      command: node
      cwd: '<полный-путь-к-papka-proekta>'
      args:
        - node_modules/@ahouben/azure-devops-mcp/dist/index.js
        - <URL-коллекции>       # https://dev.azure.com/<org> или https://<host>/<collection>
        - '-a'
        - env
        - '-d'
        - core
        - search
        - repositories
      env:
        AZURE_DEVOPS_PAT: secret:user:devops:azdevops-pat#token
      origin: unnamed
```

Пояснения по значениям, которые легко перепутать:

- `-a env` заставляет пакет читать `AZURE_DEVOPS_PAT` из окружения процесса; без PAT в этой
  переменной он тихо переключается на Azure Identity (managed identity), которая **не работает**
  с on-prem TFS и обычно не работает с обычным PAT-only доступом.
- `AZURE_DEVOPS_PAT: secret:user:devops:azdevops-pat#token` — не переменная окружения Windows, а
  ссылка в хранилище секретов SPLA. **Область (`user`) в ссылке обязана буквально совпадать** с
  областью, под которой секрет реально сохранён на шаге 4 — поиска и подстановки между областями нет,
  несовпадение выглядит как «секрет не найден» и откатывается на Azure Identity ровно так же, как
  отсутствие PAT.
- Ключ `devops:azdevops-pat` и поле `#token` — соглашение, не требование пакета; называйте как
  удобно, лишь бы ссылка и реальная запись совпадали дословно.
- `plugins`/`permissions` можно оставить проектными умолчаниями, но учтите: этот клиент помечает
  **все** внешние MCP-инструменты как `Foreign/Write/High`, включая чистое чтение. `permissions.write:
  ask` — рабочий минимум (подтверждение первого вызова в чате); `write: deny` или режим `Research`
  заблокируют и поиск тоже.

## Шаг 3 — TLS для on-prem сервера с внутренним CA (агент, если применимо)

Публичный `dev.azure.com` пропустите. Для on-prem сервера за корпоративным CA Node по умолчанию не
доверяет цепочке (`UNABLE_TO_VERIFY_LEAF_SIGNATURE`), даже если Windows ей доверяет. Не отключайте
проверку — дайте Node доверенный PEM:

```yaml
      env:
        AZURE_DEVOPS_PAT: secret:user:devops:azdevops-pat#token
        NODE_EXTRA_CA_CERTS: 'C:\path\to\corporate-ca.pem'
```

`AZURE_DEVOPS_IGNORE_SSL_ERRORS=true` существует у пакета, но отключает проверку сертификата целиком
— не используйте её как обходной путь для этой проблемы.

## Шаг 4 — секрет: только человек, вне чата

Агент не выполняет этот шаг и не просит пользователя вставить PAT в чат — это прямой инвариант
хранилища секретов (`agents/secrets.md` §6).

1. Человек создаёт Personal Access Token только на чтение: `Code (Read)`, при необходимости
   `Project and Team (Read)`. Страница: `<URL-коллекции>/_usersSettings/tokens`
   (для `dev.azure.com` — `https://dev.azure.com/<org>/_usersSettings/tokens`). Никакого
   Write/Manage/Full access.
2. Человек сохраняет его в хранилище SPLA — терминал, скрытый ввод, значение не попадает ни в
   аргументы, ни в историю оболочки, ни в чат:

   ```powershell
   spla secret set devops:azdevops-pat --field token --user
   ```

   Область (`--user`/`--project`/`--shared`) — та же, что в ссылке из шага 2, **дословно**. То же
   самое можно сделать через Settings → Secrets в UI.

## Шаг 5 — включить сервер и перезапустить (агент)

```yaml
mcp:
  servers:
    - id: azdevops
      enabled: true    # было false на шаге 2
```

**Резолв ссылки на секрет происходит один раз, при первом подключении сервера в рамках уже
запущенного процесса SPLA.** Правка `.spla` на лету и кнопка «Reconnect» в панели MCP это
подключение не пересобирают — нужен полный перезапуск процесса, который держит проект:

```powershell
spla stop <manifest.spla>     # или без аргумента — из папки проекта
spla start <manifest.spla>
```

`spla ps` покажет новый PID и порт (`ENDPOINT`), под которым проект снова поднялся.

## Шаг 6 — проверка без участия человека (агент)

Полноценный вызов инструмента (`azdevops_core_list_projects` и т.п.) — это `Foreign/Write/High`,
он требует подтверждения в чате, а headless-запросу подтверждать нечем. Это ожидаемое поведение, не
баг. Но проверить, что **PAT действительно резолвится и сервер поднимается**, можно без единого
клика человека:

1. Лог процесса, `<workspace>/.spla/logs/spla-<дата>.log`. Ищите по `azdevops`:

   ```powershell
   Select-String -Path ".spla\logs\spla-*.log" -Pattern "azdevops"
   ```

   Успех выглядит так:
   ```
   MCP server started. Server=azdevops Command=node
   MCP server ready. Server=azdevops Name=Azure DevOps MCP Server Tools=26
   ```
   Провал по PAT — не в логе SPLA, а в первом же вызове инструмента: `ChainedTokenCredential
   authentication failed. CredentialUnavailableError: EnvironmentCredential is unavailable.` Если
   видите это — секрет не резолвился (проверьте область в ссылке и что действительно был рестарт,
   а не только правка файла).

2. Список инструментов через собственный исходящий MCP этого проекта SPLA (нужен `mcp.enabled: true`
   и `mcp.port` в `.spla` — не путать с внешним сервером, который проект *потребляет*; это
   endpoint, которым проект сам себя *отдаёт*):

   ```powershell
   curl -s -X POST "http://127.0.0.1:<mcp.port>/mcp" -H "Content-Type: application/json" `
     -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}' | Select-String azdevops
   ```

   Двадцать шесть `azdevops_*` в ответе — сервер подключён и отдал инструменты. Это ещё не
   доказательство, что PAT валиден против реального Azure DevOps (это подтвердит только пункт 1 —
   отсутствие ошибки авторизации в логе после первого реального вызова), но подтверждает, что вся
   цепочка конфиг → секрет → процесс → MCP-клиент собралась правильно.

3. Финальную проверку — что PAT действительно принят сервером — делает человек одним сообщением в
   чате (например «найди во всех репозиториях SqlConnection»), подтверждает первый вызов
   инструмента, и смотрит на результат. Дальше подтверждения не нужны в рамках того же чата/сессии.

## Что дальше

Пример готового рабочего промпта в чат после успешной проверки:

```
Только чтение. Найди во всех доступных репозиториях использование <класс/функция>;
покажи проект, репозиторий и путь.
```

Не подтверждайте через этот сервер создание веток, PR, комментариев и другие операции записи — домен
`repositories` в `-d core search repositories` включает и write-методы пакета, это не read-only
allowlist сам по себе; единственная реальная граница — права PAT в Azure DevOps.

## Потенциальная фича: несколько коллекций

Не реализовано, но укладывается в существующую модель без изменений в SPLA — фиксирую как задел.

Пакет принимает ровно одну коллекцию на процесс: `organization` — обязательный позиционный аргумент,
списка внутри одного запуска нет (`node .../index.js --help`). Для второй коллекции (другой TFS-сервер
или `dev.azure.com/<другая-org>`) не нужен второй пакет — нужна вторая запись в `mcp.servers`, второй
node-процесс рядом с первым, с собственным `id`:

```yaml
mcp:
  servers:
    - id: azdevops                 # первая коллекция — как в шаге 2
      args: [..., <URL-коллекции-1>, ...]
      env:
        AZURE_DEVOPS_PAT: secret:user:devops:azdevops-pat#token

    - id: azdevops2                # вторая — свой id = свой префикс инструментов (azdevops2_*)
      name: Azure DevOps Code Search (вторая коллекция)
      transport: stdio
      command: node
      cwd: <та-же-papka-proekta>
      args:
        - node_modules/@ahouben/azure-devops-mcp/dist/index.js
        - <URL-коллекции-2>
        - '-a'
        - env
        - '-d'
        - core
        - search
        - repositories
      env:
        AZURE_DEVOPS_PAT: secret:user:devops:azdevops2-pat#token   # тот же секрет, если PAT один на обе коллекции
      level: enabled
```

Нюансы:

- `id` обязан быть уникальным — он же префикс инструментов; совпадение `id` двух записей ломает
  именование, а не просто перезаписывает одну другой.
- `package.json`/`node_modules` один на весь проект — второй раз пакет ставить не нужно, второй записи
  достаточно указать тот же `command`/`cwd`.
- PAT можно переиспользовать тем же секретом, если аккаунт валиден в обеих коллекциях — тогда обе
  записи ссылаются на одну и ту же ссылку `secret:...`; если логины разные — отдельный
  `spla secret set` под отдельным ключом, шаг 4 повторяется для второго ключа один в один.
- Шаг 5 (рестарт) и шаг 6 (проверка по логу/`tools/list`) не меняются — просто в логе появится вторая
  пара строк `MCP server started/ready. Server=azdevops2`, и в ответе `tools/list` — второй набор
  `azdevops2_*`.
