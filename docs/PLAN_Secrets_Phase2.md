# Секреты фазы 2 + SSH-плагин — как построено (as-built)

Реализовано и протестировано 2026-07-12 (ветка `roslyn-project-tools`). «Почему» (все решения с
обоснованиями) — в [DESIGN_Secrets_and_SSH.md](DESIGN_Secrets_and_SSH.md). Здесь — «что и где» плюс
как настраивать и тестировать.

## Инварианты (держать при любом развитии)

1. **Значение секрета не покидает сервер/хост** — не появляется в ответах протокола, broadcast'ах,
   логах, сообщениях об ошибках. Наружу идут только ключи, скоупы, имя бэкенда.
2. **Никаких MCP-инструментов для доступа к хранилищу** — агенту/LLM не выдаются. Только CLI и
   (будущая) web-панель по служебному протоколу.
3. **Никаких Windows-зависимостей в `src/core/SPLA.Domain`** — DPAPI живёт отдельным проектом.
4. **Один активный бэкенд**, выбирается настройкой. Без композитов, без миграции plaintext→DPAPI.
5. **`ISecretStore`** (`src/core/SPLA.Domain/Secrets/ISecretStore.cs`) — стабильный контракт,
   2 скоупа Project/Machine. Не менять сигнатуры.

## SPLA_HOME

`ConfigLoader.GetDefaultsDir()` (`src/core/SPLA.Domain/Settings/ConfigLoader.cs`): непустая env
`SPLA_HOME` подменяет `~/.spla` целиком (`Path.GetFullPath`). Иначе — `UserProfile/.spla` как раньше.
Машинный token-usage переведён на `GetDefaultsDir()` (`src/service/SPLA.Service/AgentRuntime.cs`) —
других мест, вычислявших домашнюю папку напрямую, не осталось. Новых прямых обращений к `~/.spla`
мимо `GetDefaultsDir()` не добавлять.

Назначение: параллельные инстансы (рабочий на dpapi + дев-копия на plaintext) с изолированными
домашними папками. Запуск дев-копии: `SPLA_HOME=<папка>`.

## Секреты: бэкенды и выбор

- **`FileSecretStore`** — plaintext YAML (`<ws>/.spla/secrets.yaml`, `<home>/secrets.yaml`). Дефолт,
  вариант нормы для локалки и тестов. Не удалять.
- **`DpapiFileSecretStore`** (`src/core/SPLA.Secrets.Dpapi/`, `net10.0`, отдельный проект) — тот же
  файловый макет, но файлы `secrets.dpapi.yaml`; ключи открытым текстом, значения `dpapi:<base64>`
  (`ProtectedData.Protect`, `CurrentUser`, entropy `SPLA.SecretStore.v1`). Листинг не расшифровывает;
  битое/чужое значение → null + warning, не бросает. `[SupportedOSPlatform("windows")]` + гвард
  `OperatingSystem.IsWindows()` — поэтому проект остаётся нейтральным `net10.0` и подключается прямой
  ссылкой (не reflection).
- **Общий формат** — `SecretYamlFile` (internal в Domain, `InternalsVisibleTo` для Dpapi/Tests).
- **Выбор**: `secrets.backend: file|dpapi` в `~/.spla/defaults.yaml` (`SplaSecretsSection`, только
  машинный уровень — не в проектном `.spla`). `ConfigLoader.ResolveSecretStore` читает настройку;
  для не-`file` дёргает `ConfigLoader.SecretStoreFactory` (регистрируется приложением). Нет фабрики /
  не Windows / вернула null → fallback на `FileSecretStore` + однократный warning.
- **Регистрация фабрики**: `DpapiSecrets.Register()` в `Program.cs` CLI и `App.axaml.cs` UI (до
  первого `LoadAndResolve`). `SPLA.Server` намеренно не трогали — он платформенно-нейтрален и грузит
  платформенные DLL рефлексией; там backend останется `file` до отдельного решения.

## CLI: `spla secret`

`src/apps/SPLA.CLI/Cli/SecretCommands.cs`, ветка в `Program.cs` до создания `AgentRuntime`
(сервис не нужен, работает через `ctx.Settings.Secrets`):

- `spla secret list [--project|--machine]` — только ключи и скоуп.
- `spla secret set <key> [--project|--machine]` — значение **скрытым вводом** (ReadKey; при
  перенаправленном stdin — обычная строка). Значение аргументом не принимается (не оседает в истории
  шелла). Дефолт скоупа: project, если открыт, иначе machine.
- `spla secret delete <key> [--project|--machine]`.

Web-панель Secrets (протокол `secrets.get/set/delete` + `SecretsPanel.vue`) — **отложена**: канал
ввода мимо чата уже закрыт CLI. Добавлять позже тем же паттерном, что `plugins.get/save`
(`Protocol.cs` / `SettingsHandlers.cs` / `web/src/surfaces/Settings/`), значения write-only.

## SSH-плагин (v1)

`src/plugins/SPLA.Plugins.Ssh/` (SSH.NET 2024.2.0). Инструменты:

- **`ssh_run`** — одна **read-only** команда на хосте, stdout/stderr/exit (одноразовая, без состояния).
  Гвард `ReadOnlyGuard` проверяет команду **до** коннекта.
- **`ssh_list_hosts`** — список хостов (имя, адрес, юзер, тип auth). Пароли не показывает.
- **`ssh_session_exec`** — read-only команда в **персистентной** pty-сессии; вывод **стримится
  человеку живьём** через `ProgressScope`; состояние (`cd`/env) сохраняется между вызовами. Это
  «живая консоль», фаза A.
- **`ssh_session_close`** — закрыть персистентную сессию.

**Живая консоль (фаза A).** `Session/SshSession.cs` — обёртка над `ShellStream` (реюзабельное ядро;
`RunAsync` стримит чанки через колбэк `onChunk`, `SendRaw` — шов для ввода человека в фазе B).
`Session/SshSessionRegistry.cs` — сессии живут между вызовами (образец `SqlConnectionRegistry`).
Завершение команды в raw-pty детектится уникальным маркером `{cmd}; echo {MARKER}$?` (несёт exit code);
ANSI/OSC-мусор вычищается, prompt упрощается при открытии. Живой тест против реального хоста подтвердил
персистентность (`cd` → `pwd`) и стриминг.

Конфиг — в `.spla` под `plugins.ssh.settings` (`SshSettings`/`SshHostConfig`). Пароль — ссылка
`secret:`/`env:`, резолвится в момент коннекта (`SshConnectionFactory`), в чат/LLM не попадает.
Поддержка password и private-key auth. Копирование плагина: цель `CopyPlugin` кладёт dll+deps+meta в
`apps/<app>/bin/.../plugins/ssh` (путь `..\..\apps\<app>` — рантайм грузит из
`AppContext.BaseDirectory/plugins`; у старых плагинов путь `..\SPLA.CLI` — латентный баг, здесь не
повторён).

**Read-only гвард** (`ReadOnlyGuard.cs`) — аллоулист, fail-closed: каждый сегмент пайплайна должен
начинаться с известной read-only команды; строка отбивается при `> >>`, `` ` ``/`$()`, `sudo/su/doas`,
мутирующих флагах (`find -delete`, `systemctl stop`, `curl -o`, `dpkg -i`, …). Не песочница: защищает
от изменения, не от чтения (чтение ограничивают права учётки). Аллоулист расширяется ревью, не догадкой.

### Настройка (пример)

```yaml
# в <project>.spla
plugins:
  ssh:
    enabled: true
    settings:
      default_host: box
      timeout_seconds: 15
      hosts:
        box:
          host: 192.168.199.250
          port: 22
          user: koe
          password: secret:ssh/box/koe   # значение — в хранилище, не здесь
          description: test box
```

```
spla secret set ssh/box/koe --machine    # ввести пароль скрытно
```

## Тесты (`tests/SPLA.Tests`)

- `ReadOnlyGuardTests` — 10 разрешённых + 24 отклонённых команды.
- `SecretStoreTests` — DPAPI round-trip и шифрование at rest, листинг без расшифровки, project>machine,
  delete, и `ConfigLoader` с `SPLA_HOME`: фабрика даёт DPAPI, без фабрики — fallback на file. Всё
  DPAPI-специфичное guard'ится `OperatingSystem.IsWindows()`; файлы — во временной папке, не в `~/.spla`.
- Живой SSH-тест против реального хоста прогонялся одноразовым throwaway-тестом (env-driven, удалён
  после прогона): `whoami`→exit 0/юзер, `uname -a`→Linux-баннер, `rm -rf`→Refused, пароль не светится.

## Вне скоупа (сознательно не делали)

Миграция plaintext→DPAPI, Windows Credential Manager, композитные хранилища, Linux-бэкенд (libsecret),
редакция секретов в логах агента, TLS для не-loopback. Живая консоль фазы A (персистентная сессия со
стримингом) — **сделана**; фаза B (xterm-терминал в web-UI со свободным вводом человека в pty, поверх
`SshSession.SendRaw`) — следующий крупный шаг: см. [DESIGN_Secrets_and_SSH.md](DESIGN_Secrets_and_SSH.md).
