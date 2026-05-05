using System.ComponentModel;
using Exiled.API.Interfaces;

namespace FermixCoin
{
    public sealed class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Максимальное количество подкидываний одной монетки до того как она «истратится» и пропадёт. Реальное число для конкретной монетки — случайное от 1 до этого значения.")]
        public int CoinMaxUses { get; set; } = 5;

        [Description("Шанс мега-джекпота: одновременно срабатывают ВСЕ одобренные исходы. Дробное значение (1.0 = 100%, 0.0001 = 0.01%).")]
        public double MegaJackpotChance { get; set; } = 0.0001;

        [Description("Подсветка монетки цветом редкости следующего исхода. Easter egg — про фичу мало кто знает.")]
        public bool RarityGlowEnabled { get; set; } = true;

        [Description("Показывать ли при выпадении исхода прикольный комментарий (хинт) дополнительно к основному сообщению.")]
        public bool ShowCommentHints { get; set; } = true;

        [Description("Глобальный broadcast при срабатывании мега-джекпота.")]
        public bool BroadcastMegaJackpot { get; set; } = true;
    }
}
