# FermixAPI

Мощная библиотека для быстрой разработки плагинов SCP:SL на Exiled с интеграцией LabAPI.

## Поддерживаемые версии фреймворков

| Зависимость | Версия |
| --- | --- |
| EXILED  | **9.13.3** ([ExMod-Team/EXILED](https://github.com/ExMod-Team/EXILED)) |
| LabAPI  | **1.1.6** ([northwood-studios/LabAPI](https://github.com/northwood-studios/LabAPI)) |
| .NET    | net48 |

Минимальная требуемая версия EXILED проверяется самим фреймворком EXILED через
`Plugin.RequiredExiledVersion`. Версия LabAPI определяется в рантайме —
если LabAPI ниже рекомендуемой версии, FermixAPI выведет предупреждение
в лог и продолжит работу в режиме совместимости.

## Установка

1. Скомпилируйте проект (см. раздел [«Сборка»](#сборка)) или скачайте
   `FermixAPI.dll` из [релизов](../../releases).
2. Поместите `FermixAPI.dll` в папку `EXILED/Plugins`.
3. Перезапустите сервер.

## Сборка

FermixAPI зависит от сборок SCP:SL, EXILED и LabAPI. Их нужно положить
в папку `refs/` рядом с проектом.

```bash
# 1) Скачать EXILED + LabAPI релизы:
bash scripts/fetch-references.sh v9.13.3 1.1.6

# 2) Скопировать DLL'ки из SCPSL_Data/Managed/ в refs/
#    (Assembly-CSharp.dll, Assembly-CSharp-firstpass.dll, CommandSystem.Core.dll,
#     Mirror.dll, PluginAPI.dll, NorthwoodLib.dll, Pooling.dll, UnityEngine*.dll)

# 3) Сборка
dotnet build -c Release
```

Готовый `FermixAPI.dll` появится в `bin/Release/net48/`.

## Структура проекта

```
FermixAPI/
├── Core/
│   ├── FermixCore.cs       # Ядро API и статический доступ
│   ├── FermixEvents.cs     # Система событий с обертками
│   ├── FermixLog.cs        # Расширенное логирование
│   └── FermixScheduler.cs  # Таймеры и отложенные действия
├── Extensions/
│   ├── PlayerExtensions.cs # 50+ методов расширения для Player
│   └── FermixHint.cs       # Система подсказок и UI
├── Systems/
│   ├── FermixDoors.cs      # Управление дверями
│   ├── FermixWarhead.cs    # Управление боеголовкой
│   ├── FermixItems.cs      # Управление предметами
│   ├── FermixRooms.cs      # Управление комнатами
│   ├── FermixCams.cs       # Управление камерами
│   ├── FermixScp.cs        # Управление SCP
│   ├── FermixServer.cs     # Управление сервером
│   ├── FermixRound.cs      # Управление раундом
│   └── FermixRoles.cs      # Управление ролями
├── Integration/
│   └── LabApiIntegration.cs # Интеграция с LabAPI
├── Utils/
│   ├── FermixUtils.cs      # Общие утилиты
│   ├── FermixConfig.cs     # Работа с конфигурациями
│   └── FermixData.cs       # Хранение данных
├── Plugin.cs               # Точка входа плагина
└── Config.cs               # Конфигурация плагина
```

## Быстрый старт

### Базовое использование

```csharp
using FermixAPI.Core;
using FermixAPI.Extensions;
using FermixAPI.Systems;

// Получить всех живых игроков
var alivePlayers = FermixCore.AlivePlayers;

// Показать подсказку игроку
player.ShowHint("Добро пожаловать!", 5);

// Исцелить игрока до максимума
player.FullHeal();

// Телепортировать игрока
player.TeleportTo(RoomType.LczArmory);

// Выдать предмет
player.GiveItem(ItemType.GunE11SR);
```

### Работа с событиями

```csharp
// Подписка на событие смерти
FermixEvents.OnPlayerDied += (victim, attacker, damageType) =>
{
    FermixLog.Info($"{victim.Nickname} был убит {attacker?.Nickname ?? "сервером"}");
};

// Подписка на начало раунда
FermixEvents.OnRoundStarted += () =>
{
    FermixServer.BroadcastAll("Раунд начался!", 5);
};
```

### Таймеры и отложенные действия

```csharp
// Отложенное действие
FermixScheduler.Delay(5f, () =>
{
    FermixLog.Info("Прошло 5 секунд!");
});

// Повторяющийся таймер
var timer = FermixScheduler.Every(10f, () =>
{
    FermixServer.BroadcastAll("Напоминание каждые 10 секунд");
});

// Остановить таймер
timer.Stop();

// Действие для игрока
FermixScheduler.DelayForPlayer(player, 3f, p =>
{
    p.ShowHint("Прошло 3 секунды!", 3);
});
```

### Fluent API для ролей

```csharp
// Настроить игрока с помощью билдера
FermixRoles.CreateBuilder(player)
    .SetRole(RoleTypeId.NtfCaptain)
    .AtPosition(new Vector3(0, 1000, 0))
    .WithHealth(200)
    .WithItems(ItemType.GunE11SR, ItemType.Medkit, ItemType.ArmorHeavy)
    .WithBroadcast("Вы капитан MTF!", 5)
    .Apply();
```

### Работа с дверями

```csharp
// Открыть все двери в зоне
FermixDoors.OpenZone(ZoneType.LightContainment);

// Заблокировать дверь
FermixDoors.Lock(DoorType.GateA, DoorLockType.AdminCommand);

// Открыть все контрольные точки
FermixDoors.OpenAllCheckpoints();

// Найти ближайшую дверь к игроку
var nearestDoor = FermixDoors.GetNearest(player.Position);
```

### Работа с предметами

```csharp
// Создать предмет на позиции
FermixItems.Spawn(ItemType.KeycardO5, player.Position);

// Создать несколько предметов
FermixItems.SpawnMultiple(ItemType.Medkit, player.Position, 5);

// Очистить все предметы на карте
FermixItems.ClearAll();

// Найти все предметы типа
var guns = FermixItems.GetAllOfType(ItemType.GunE11SR);
```

### Работа с комнатами

```csharp
// Выключить свет в зоне
FermixRooms.BlackoutZone(ZoneType.HeavyContainment, 30f);

// Получить случайную комнату
var room = FermixRooms.GetRandom(ZoneType.Entrance);

// Телепортировать всех в комнату
FermixRooms.TeleportPlayersTo(RoomType.HczArmory, player => player.IsHuman);
```

### Хранение данных

```csharp
// Хранилище данных игроков
var playerData = new PlayerDataStore<PlayerStats>("player_stats");

// Получить данные
var stats = playerData.Get(player);
stats.Kills++;

// Данные автоматически сохраняются

// Временные данные с истечением
var cooldowns = new ExpiringDataStore<string, bool>(TimeSpan.FromSeconds(30));
cooldowns.Set(player.UserId, true);

if (cooldowns.Has(player.UserId))
{
    player.ShowHint("Кулдаун еще не прошел!");
}
```

### Интеграция с LabAPI

```csharp
// Проверить доступность LabAPI
if (LabApiIntegration.IsAvailable)
{
    // Вызвать метод LabAPI
    LabApiIntegration.InvokeMethod("SomeClass", "SomeMethod", args);
}

// Условное выполнение
LabApiIntegration.IfAvailableOrElse(
    () => { /* код с LabAPI */ },
    () => { /* альтернативный код */ }
);
```

## Extension методы для Player

```csharp
// Здоровье и исцеление
player.FullHeal();
player.HealPercent(50);
player.SetHealthPercent(75);
player.AddHealth(50);

// Урон
player.Damage(25f);
player.DamagePercent(10);
player.Kill("Причина смерти");

// Телепортация
player.TeleportTo(RoomType.HczArmory);
player.TeleportTo(otherPlayer);
player.TeleportToSpawn();
player.TeleportToRandom(ZoneType.Entrance);

// Эффекты
player.EnableEffect<Scp207>(30);
player.DisableEffect<Scp207>();
player.DisableAllEffects();
player.Flash(3f);
player.Blind(5f);

// Предметы
player.GiveItem(ItemType.GunE11SR);
player.GiveItems(ItemType.Medkit, ItemType.Adrenaline);
player.ClearInventory();
player.DropAllItems();
player.HasItem(ItemType.KeycardO5);

// Информация
player.ShowHint("Текст", 5);
player.Broadcast("Сообщение", 10);
player.SendConsole("Консольное сообщение");

// Проверки
player.IsScp();
player.IsHuman();
player.IsMtf();
player.IsChaos();
player.IsStaff();
player.IsAlly(otherPlayer);
player.IsEnemy(otherPlayer);

// Позиция
player.GetDistance(otherPlayer);
player.IsInRange(position, 10f);
player.GetNearestPlayer();
player.GetNearestEnemy();

// Прочее
player.Freeze();
player.Unfreeze();
player.SetScale(1.5f);
player.ResetScale();
player.Explode();
```

## Конфигурация

```yaml
# config.yml
is_enabled: true
debug: false

hints:
  default_duration: 5

warhead:
  auto_start_time: 900
  auto_detonate: true

doors:
  auto_open_checkpoints: false
```

## Лицензия

MIT License
