---
name: release-checklist
description: Чек-лист для выпуска новой версии (тэг + GitHub Release).
---

# SKILL — Выпустить релиз

## Пре-реквизиты

- На `dev` уже есть набор изменений, который нужно зарелизить.
- Все изменения собираются 0/0.
- PR `dev → main` открыт **либо** уже смержен пользователем.

## Шаги

### 1. Определить версию

Применить semver (см. [`WORKFLOW.md`](../WORKFLOW.md#правила-версионирования-semver)):

| Что меняется | Bump |
| --- | --- |
| Только конфигов / выкручены шансы / косметика | `2.X.Y → 2.X.(Y+1)` (patch) |
| Новый плагин / системный модуль / API | `2.X.Y → 2.(X+1).0` (minor) |
| Ломаешь публичный API | `2.X.Y → 3.0.0` (major) |
| Встраиваешь новую внешнюю зависимость | `2.X.Y → 2.(X+1).0` (minor) |

Текущая версия — посмотри `git describe --tags --abbrev=0`.

### 2. Поднять `<Version>` в csproj

```bash
# В корневом FermixAPI.csproj и в plugins/FermixCoin/FermixCoin.csproj
# поменять <Version>2.X.Y</Version>
```

### 3. Поднять `VersionMajor/Minor/Patch` в FermixCore

```csharp
// Core/FermixCore.cs
public const int VersionMajor = 2;
public const int VersionMinor = 3;
public const int VersionPatch = 0;
```

Эта версия выводится в логе `FermixLog.Info($"Ядро FermixAPI v{Version}")`.

### 4. Если в плагине FermixCoin тоже изменения

- Поднять `Version` в `plugins/FermixCoin/FermixCoin.csproj`.
- Если там есть свой константный VERSION — поднять и там.

### 5. Собрать локально

```bash
dotnet build -c Release
dotnet build plugins/FermixCoin/FermixCoin.csproj -c Release
```

0/0. Если warning — пофикси.

### 6. Закоммитить версионный bump

```bash
git add FermixAPI.csproj plugins/FermixCoin/FermixCoin.csproj Core/FermixCore.cs
git commit -m "Bump version to 2.X.Y"
git push origin dev
```

### 7. Открыть PR и дождаться merge

См. [`WORKFLOW.md`](../WORKFLOW.md). PR `dev → main`. CI должен быть
зелёным (4/4). Пользователь мерджит.

### 8. Подтянуть main в dev

```bash
git checkout dev
git fetch origin
git merge --ff-only origin/main
git push origin dev
```

### 9. Тэгнуть

```bash
# Пример для v2.3.0
git tag v2.3.0
git push origin v2.3.0
```

CI workflow `.github/workflows/build.yml` сам:
1. Соберёт FermixAPI.dll и FermixCoin.dll в Release.
2. Создаст GitHub Release.
3. Загрузит ассеты:
   - `FermixAPI.dll`
   - `FermixAPI.pdb`
   - `FermixCoin.dll`
   - `FermixCoin.pdb`
   - `FermixAPI-vX.Y.Z.zip` — всё в одном архиве + README + examples.

### 10. Проверить релиз на GitHub

```
https://github.com/566hj/FERMIXAPI1/releases/tag/v2.X.Y
```

Должны быть все 5 ассетов. Если нет — посмотри логи workflow на
странице Actions.

### 11. Сообщить пользователю

```
Релиз v2.X.Y опубликован: <ссылка на release>

Что внутри:
- ...
- ...

Что проверить в игре:
- ...
- ...

Кладёшь FermixAPI.dll и FermixCoin.dll в EXILED/Plugins/, рестартуешь
сервер.
```

## Чек-лист

- [ ] Версия определена правильно (semver)
- [ ] `<Version>` обновлён в обоих csproj
- [ ] `VersionMajor/Minor/Patch` обновлён в FermixCore
- [ ] Локальная сборка 0/0
- [ ] Коммит-bump запушен в `dev`
- [ ] PR `dev → main` смержен
- [ ] `dev` подтянут до `main`
- [ ] Тэг `vX.Y.Z` запушен
- [ ] CI собрал Release
- [ ] Все ассеты присутствуют на странице релиза
- [ ] Пользователю отправлено сообщение со ссылкой и чек-листом
