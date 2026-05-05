using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using FermixAPI;
using FermixAPI.Core;
using FermixAPI.Systems;
using PlayerApi = Exiled.API.Features.Player;

namespace FermixCoin
{
    /// <summary>
    /// Хук на <see cref="Exiled.Events.Handlers.Player.FlippingCoin"/>:
    /// 1) Применяет заранее свёрстанный исход.
    /// 2) Увеличивает счётчик бросков; на исчерпании монетка испаряется.
    /// 3) Если есть ещё броски — обновляет следующий исход (и подсветку).
    /// </summary>
    public sealed class CoinHandler
    {
        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (!ev.IsAllowed || ev.Pickup == null)
                return;

            if (ev.Pickup.Type != ItemType.Coin)
                return;

            // На первом подборе монетки регистрируем её в реестре, чтобы свечение
            // могло раскрасить её цветом следующего исхода. Если кто-то её уже
            // подкидывал — не трогаем.
            var states = Plugin.Singleton.CoinStates;
            if (!states.ContainsKey(ev.Pickup.Serial))
            {
                var maxUses = UnityEngine.Random.Range(1, Plugin.Singleton.Config.CoinMaxUses + 1);
                states[ev.Pickup.Serial] = new CoinState
                {
                    Uses = 0,
                    MaxUses = maxUses,
                    NextOutcome = OutcomeRegistry.RollOne(),
                };
            }
        }

        public void OnFlippingCoin(FlippingCoinEventArgs ev)
        {
            if (!ev.IsAllowed || ev.Player == null || ev.Item == null)
                return;

            var states = Plugin.Singleton.CoinStates;
            var serial = ev.Item.Serial;

            // Гарантируем существование состояния (если PickingUpItem почему-то не сработал).
            if (!states.TryGetValue(serial, out var state))
            {
                state = new CoinState
                {
                    Uses = 0,
                    MaxUses = UnityEngine.Random.Range(1, Plugin.Singleton.Config.CoinMaxUses + 1),
                    NextOutcome = OutcomeRegistry.RollOne(),
                };
                states[serial] = state;
            }

            state.Uses++;

            // Mega-Jackpot — отдельный бросок ДО применения обычного исхода.
            // На джекпоте обычный исход просто заменяется большим набором.
            var rng = UnityEngine.Random.value;
            bool isMega = rng < (float)Plugin.Singleton.Config.MegaJackpotChance;

            try
            {
                if (isMega)
                    ApplyMegaJackpot(ev.Player);
                else
                    ApplyOutcome(ev.Player, state.NextOutcome);
            }
            catch (Exception ex)
            {
                Log.Error($"[FermixCoin] исход '{state.NextOutcome?.Id}' упал: {ex}");
            }

            // Лимит бросков?
            if (state.Uses >= state.MaxUses)
            {
                ev.Player.RemoveItem(ev.Item);
                states.Remove(serial);
                FermixHint.Send(ev.Player, "<color=#888888>Монетка рассыпалась в труху...</color>", 4f);
            }
            else
            {
                // Обновляем следующий исход — новая редкость → новое свечение.
                state.NextOutcome = OutcomeRegistry.RollOne();
            }
        }

        public void OnRestartingRound()
        {
            Plugin.Singleton.CoinStates.Clear();
        }

        private static void ApplyOutcome(PlayerApi player, Outcome outcome)
        {
            if (player == null || outcome == null)
                return;

            // Действие может выкинуть exception — пусть валится в верхнем catch.
            outcome.Action(player);

            // Базовое сообщение в виде стэк-хинта (под редкость).
            var color = outcome.Rarity.ToHex();
            FermixHint.SendColored(player, $"<b><color={color}>{outcome.Message}</color></b>", color, 5f);

            if (Plugin.Singleton.Config.ShowCommentHints && !string.IsNullOrEmpty(outcome.Comment))
            {
                FermixHint.SendColored(player, $"<i><color=#cccccc>{outcome.Comment}</color></i>", FermixHint.Gray, 5f);
            }
        }

        private static void ApplyMegaJackpot(PlayerApi player)
        {
            if (player == null)
                return;

            FermixHint.SendColored(player, "<b><color=#FF00FF>★ МЕГА-ДЖЕКПОТ ★</color></b>", "#FF00FF", 8f);

            if (Plugin.Singleton.Config.BroadcastMegaJackpot)
            {
                FermixServer.GlobalBroadcast(
                    $"<color=#FF00FF>★ МЕГА-ДЖЕКПОТ ★</color> у игрока <b>{player.Nickname}</b>! Сейчас будет весело.",
                    duration: 8);
            }

            // Все одобренные исходы разом. Ставим небольшие задержки, чтобы хинты
            // не перетёрлись и сервер не лёг от пачки одновременных role-смен.
            int idx = 0;
            foreach (var outcome in OutcomeRegistry.All)
            {
                var captured = outcome;
                FermixScheduler.Delay(0.05f * idx, () =>
                {
                    if (player == null || !player.IsConnected || !player.IsAlive)
                        return;
                    try
                    {
                        captured.Action(player);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"[FermixCoin] mega-jackpot: '{captured.Id}' упал: {ex.Message}");
                    }
                });
                idx++;
            }
        }
    }
}
