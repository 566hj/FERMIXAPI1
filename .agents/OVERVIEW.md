# OVERVIEW — что вообще в этом репозитории

## Кто пользователь

- **Fermix** (`grigoreve123@gmail.com`) — администратор частного
  SCP:SL-сервера. Пишет на русском, любит «прикольные» механики,
  любит цвет и эмодзи в хинтах в игре, но **не** в коде и сообщениях
  Devin (см. `CONVENTIONS.md`).
- Его сервер использует EXILED `9.13.3` и LabAPI `1.1.6`.
- Он сам **не** пишет код выпускающим образом, но активно тестирует
  плагины «вживую» на сервере и присылает скриншоты багов.
- Релизы он скачивает как готовые DLL из GitHub Releases и кладёт в
  `EXILED/Plugins/`.

## Что в репозитории

```
FERMIXAPI1/
├── FermixAPI.csproj          ← главный проект (DLL = «FermixAPI.dll»)
├── Plugin.cs                  ← EXILED-плагин-обёртка над FermixCore
├── Core/                      ← FermixCore, Scheduler, HintStack, Events, Paths
├── Systems/                   ← FermixGlow, FermixDoors, FermixRoles, ...
├── Extensions/                ← FermixHint (публичный API), PlayerExtensions
├── Commands/                  ← ResurrectCommand, RoundTimeCommand, TpsCommand, ...
├── Integration/               ← LabApiIntegration, LabApiCommands, LabApiEvents
├── Utils/                     ← FermixConfigUtils, FermixData, FermixLog
├── Internal/                  ← ВНУТРЕННЯЯ реализация (не входит в публичный API)
│   └── HintEngine/            ← ВСТРОЕННЫЙ hint-движок (форк HintServiceMeow)
│       ├── Core/                ← PlayerDisplay, HintCollection, парсер RichText
│       ├── UI/                  ← CommonHint helpers (не используются в FermixHint)
│       ├── Plugin/              ← Стабы Plugin/PluginConfig (НЕ EXILED-плагин)
│       ├── TextWidth            ← embedded-resource с таблицей ширин символов
│       └── README.md            ← как обновлять движок
├── plugins/
│   └── FermixCoin/            ← отдельный плагин-потребитель FermixAPI
│       ├── FermixCoin.csproj  ← собирается отдельно
│       ├── Core/              ← CoinHandler, CoinGlowController, ...
│       └── Outcomes/          ← ~30 исходов (A1-A4, B1-B6, C1-C5, D1-D6, ...)
├── examples/                  ← примеры использования API (НЕ часть DLL)
├── vendor/                    ← копии исходников EXILED / LabAPI / HintServiceMeow
├── refs/                      ← бинарные DLL-ссылки для компиляции
├── .github/workflows/build.yml ← CI: собирает FermixAPI.dll + FermixCoin.dll и
│                                  кладёт в Releases при пуше тега «vX.Y.Z»
└── .agents/                   ← ЭТА ПАПКА (для другого ИИ)
```

## Архитектурный принцип

**FermixAPI — это плагин-API.** Он сам полноценный EXILED-плагин
(в `Plugin.cs` есть `OnEnabled`/`OnDisabled`), но его смысл — давать
другим плагинам набор удобных классов: `FermixHint`, `FermixGlow`,
`FermixScheduler`, `FermixRoles`, и т.д.

**FermixCoin** — плагин-потребитель. Лежит в `plugins/FermixCoin/`,
имеет собственный `FermixCoin.csproj` с
`<ProjectReference Include="../../FermixAPI.csproj" />`. Собирается
отдельно. На сервере оба DLL кладутся в одну папку
`EXILED/Plugins/`, и FermixCoin использует FermixAPI через `using FermixAPI;`.

## Жизненный цикл

```
EXILED → загружает FermixAPI.dll
     ↓
FermixAPI.Plugin.OnEnabled()
     ↓
FermixCore.Initialize(plugin)
   ├── FermixPaths.Initialize()
   ├── FermixConfigUtils.Initialize()
   ├── FermixData.Initialize()
   ├── Handlers.Server.WaitingForPlayers += OnWaitingForPlayers
   │     (на старте раунда: Patcher.Patch() для движка хинтов)
   ├── Handlers.Player.Left += OnPlayerLeft
   │     (на уход игрока: PlayerDisplay.Destruct(hub))
   ├── FermixEvents.Register()
   ├── FermixScheduler.Initialize()
   ├── FermixHintStack.Initialize()
   ├── Systems.FermixInput.Initialize()
   └── Systems.FermixGlow.Initialize()
     ↓
EXILED → загружает FermixCoin.dll
     ↓
FermixCoin.Plugin.OnEnabled() → FermixCore.EnsureInitialized()
                              → CoinHandler.Register()
```

## Что важно понимать про hint-движок

Чтобы хинты от FermixCoin (и любых других плагинов) гарантированно
показывались на сервере, FermixAPI **встраивает в себя**
HintServiceMeow (HSM) — код лежит в [`Internal/HintEngine/`](../Internal/HintEngine/),
оригинал — в [`vendor/HintServiceMeow/`](../vendor/HintServiceMeow/). HSM
патчит `player.ShowHint` через Harmony и кооперативно объединяет
хинты от разных плагинов в один пайплайн рендеринга.

Из этого следует:

- Никогда не зови `player.ShowHint(...)` напрямую — иди через
  `FermixHint.Send(player, msg, dur)` или другой публичный API
  `FermixHint`.
- Если ты добавляешь hint-логику в `Internal/HintEngine/`, помни:
  namespace — `FermixAPI.Hints.*`, а не `HintServiceMeow.*` (это
  исторический namespace из форка, сохранённый ради
  совместимости Harmony-патчей).
- Чтобы обращаться к нативному SCP:SL `Hints` API в коде внутри
  `Internal/HintEngine/`, используй явное `global::Hints.X` (иначе
  компилятор ловит наш `FermixAPI.Hints` и падает с CS0246).

## Что считать «готово»

Минимальный критерий «готово» для любой задачи:

- `dotnet build -c Release` для FermixAPI **и** FermixCoin → 0/0.
- Если в задаче была визуальная часть — пользователь подтвердил, что
  на сервере выглядит так, как он хотел.
- PR `dev → main` зелёный по CI (4/4 проверки).
- Тэг `vX.Y.Z` создан (после merge'а пользователем) и Release
  опубликован с актуальными DLL.
