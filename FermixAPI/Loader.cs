using Exiled.API.Features;
using FermixAPI.Core;
using System;

namespace FermixAPI {
    public sealed class Loader : Plugin<Config> {
        public static Loader Instance { get; private set; }
        public Loader() => Instance = this;

        public override string Name => "FermixAPI";
        public override string Author => "Fermix";
        public override string Prefix => "fermix";
        public override Version Version => new Version(FermixCore.VersionMajor, FermixCore.VersionMinor, FermixCore.VersionPatch);
        public override Version RequiredExiledVersion => new Version(9, 13, 3);

        public override void OnEnabled() {
            FermixCore.Initialize(this);
            base.OnEnabled();
        }

        public override void OnDisabled() {
            FermixCore.Shutdown();
            Instance = null;
            base.OnDisabled();
        }

        public override void OnReloaded() {
            FermixEvents.Refresh();
            base.OnReloaded();
        }
    }
}
