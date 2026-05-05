using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Enums;
using FermixCoin.Outcomes;
using PlayerEvents = Exiled.Events.Handlers.Player;
using ServerEvents = Exiled.Events.Handlers.Server;

namespace FermixCoin
{
    /// <summary>
    /// Главный entry-point плагина «FermixCoin». Подкидывание монетки запускает
    /// один из ~40 случайных исходов; редкость следующего исхода видна по цвету
    /// свечения монетки (easter egg, в README не описан).
    ///
    /// Зависит от FermixAPI ≥ 2.1.0 (FermixGlow, FermixHint, FermixScheduler).
    /// </summary>
    public sealed class Plugin : Plugin<Config>
    {
        public override string Name => "FermixCoin";
        public override string Prefix => "fermix_coin";
        public override string Author => "Fermix";
        public override Version Version { get; } = new Version(1, 1, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 13, 3);
        public override PluginPriority Priority => PluginPriority.Lower;

        public static Plugin Singleton { get; private set; }

        /// <summary>Состояния всех «активных» монеток на сервере.</summary>
        public Dictionary<ushort, CoinState> CoinStates { get; } = new Dictionary<ushort, CoinState>();

        private CoinHandler _handler;

        public override void OnEnabled()
        {
            Singleton = this;

            OutcomeRegistry.Initialize();
            CoinGlowController.Register();

            _handler = new CoinHandler();
            PlayerEvents.FlippingCoin += _handler.OnFlippingCoin;
            PlayerEvents.PickingUpItem += _handler.OnPickingUpItem;
            ServerEvents.RestartingRound += _handler.OnRestartingRound;

            Log.Info($"FermixCoin {Version} включён. Зарегистрировано исходов: {OutcomeRegistry.All.Count}.");

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            if (_handler != null)
            {
                PlayerEvents.FlippingCoin -= _handler.OnFlippingCoin;
                PlayerEvents.PickingUpItem -= _handler.OnPickingUpItem;
                ServerEvents.RestartingRound -= _handler.OnRestartingRound;
                _handler = null;
            }

            CoinGlowController.Unregister();
            CoinStates.Clear();
            Singleton = null;

            base.OnDisabled();
        }
    }
}
