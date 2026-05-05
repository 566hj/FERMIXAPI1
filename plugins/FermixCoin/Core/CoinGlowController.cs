using System.Collections.Generic;
using FermixAPI.Systems;

namespace FermixCoin
{
    /// <summary>
    /// Регистрирует в FermixGlow по одной подсветке на редкость. Каждая подсветка
    /// проверяет: если данный серийник лежит в реестре монеток и его следующий
    /// исход — этой редкости — подсвечивай. Так монетка плавно меняет цвет
    /// между бросками без внешнего тика.
    /// </summary>
    public static class CoinGlowController
    {
        private static readonly List<string> _glowIds = new();

        public static void Register()
        {
            if (!Plugin.Singleton.Config.RarityGlowEnabled)
                return;

            // Common / Uncommon / Rare / Epic / Legendary — статичная подсветка
            // соответствующего цвета.
            RegisterRarityGlow(Rarity.Common,    RarityColors.CommonHex,    intensity: 1.0f);
            RegisterRarityGlow(Rarity.Uncommon,  RarityColors.UncommonHex,  intensity: 1.2f);
            RegisterRarityGlow(Rarity.Rare,      RarityColors.RareHex,      intensity: 1.4f);
            RegisterRarityGlow(Rarity.Epic,      RarityColors.EpicHex,      intensity: 1.6f);
            RegisterRarityGlow(Rarity.Legendary, RarityColors.LegendaryHex, intensity: 1.8f);

            // Mythic-радуга специально не регистрируем: монетка с заранее свёрстанным
            // мифическим исходом не существует в обычном пуле — Mythic выпадает
            // только через отдельный mega-jackpot roll, который ВНЕ NextOutcome.
        }

        public static void Unregister()
        {
            foreach (var id in _glowIds)
                FermixGlow.RemoveGlow(id);
            _glowIds.Clear();
        }

        private static void RegisterRarityGlow(Rarity rarity, string hex, float intensity)
        {
            var id = $"FermixCoin_Rarity_{rarity}";
            FermixGlow.AddGlowHex(
                id: id,
                itemCheck: serial =>
                {
                    if (Plugin.Singleton == null)
                        return false;
                    if (!Plugin.Singleton.CoinStates.TryGetValue(serial, out var state))
                        return false;
                    return state?.NextOutcome != null && state.NextOutcome.Rarity == rarity;
                },
                hexColor: hex,
                intensity: intensity,
                range: 4f,
                updateInterval: 0.25f,
                glowInHands: false);
            _glowIds.Add(id);
        }
    }
}
