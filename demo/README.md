# demo/ — примеры SPLA

Демо разделены на два типа:

- [`workers/`](workers/) — автономные приложения-воркеры на `SPLA.Runtime`;
- [`projects/`](projects/) — готовые демо-проекты для запуска в SPLA.

## Workers

Каждый worker — маленькое самостоятельное приложение, которое подключает
`src/agent/SPLA.Runtime`, открывает обычный файл проекта `.spla` и получает
полноценного агента **без CLI, сервиса и UI**. Это и есть критерий правильности
границы рантайма: стороннее приложение + проект + модель = живой агент.

| Демо | Что делает |
|------|-----------|
| [VisionAgent](workers/VisionAgent/README.md) | Кадры с USB-камеры / RTSP / видеофайла → анализ кадра моделью (время события, горное оборудование, номера) → консоль, по кругу. |
| [LogSentry](workers/LogSentry/README.md) | Хвост лог-файла → пачки подозрительных строк → триаж-вердикт (ИНЦИДЕНТ/ШУМ/НАБЛЮДАТЬ) → консоль. |

Общий паттерн любого такого хоста:

```csharp
var settings = ConfigLoader.LoadAndResolve("my.spla");   // проект
using var runtime = new AgentRuntime(settings, loggerFactory); // весь стек агента
var chat = new ChatRegistry(runtime).CreateNew("my task");     // чат
await chat.SendAsync(text, callbacks, permission, clarify, ct, images);
```

Свои настройки демо держат в собственной секции того же `.spla`
(стандартный загрузчик незнакомые секции игнорирует) — один файл проекта
остаётся единственной точкой входа.

## Projects

Первый проектный пример — [`RemoteWeb`](projects/RemoteWeb/): удалённая работа через
браузерный Web-клиент и `SPLA.Server`.
