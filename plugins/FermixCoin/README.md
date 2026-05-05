# FermixCoin

Плагин-«монетка-гамблер» для SCP:SL (EXILED 9.13.x), построенный на [FermixAPI](../../README.md).

Подкидывание монетки запускает один из ~30 случайных исходов: предметы,
эффекты, активированные гранаты под ноги, телепорты, смены классов,
blackout в комнате, тревога Alpha Warhead и т.д. Каждая монетка имеет
случайный лимит бросков (1..N) — после этого рассыпается.

## Установка

1. Установи [FermixAPI](../../README.md) в `EXILED/Plugins/`.
2. Положи `FermixCoin.dll` рядом с `FermixAPI.dll`.
3. Перезапусти сервер.

## Сборка из исходников

```bash
# из корня репо FERMIXAPI1
dotnet build plugins/FermixCoin/FermixCoin.csproj -c Release
# результат: plugins/FermixCoin/bin/Release/FermixCoin.dll
```

## Конфиг

Файл создаётся при первом запуске в
`EXILED/Configs/<port>/FermixCoin.yml`:

```yaml
is_enabled: true
debug: false
# Максимум бросков одной монетки (реальное число — рандом 1..N).
coin_max_uses: 5
# Шанс мега-джекпота (одновременно срабатывают все исходы).
mega_jackpot_chance: 0.0001
# Включить подсветку монетки (FermixGlow).
rarity_glow_enabled: true
# Показывать ли смешные комментарии-хинты.
show_comment_hints: true
# Глобальный broadcast при срабатывании мега-джекпота.
broadcast_mega_jackpot: true
```

## Совместимость

- EXILED **9.13.3**
- FermixAPI **2.1.0+**
- .NET Framework **4.8**

## Лицензия

CC BY-SA 3.0 (как и оригинальный DereCoin).
