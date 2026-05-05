using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using FermixAPI.Core;
using UnityEngine;

namespace FermixCoin.Outcomes
{
    /// <summary>Категория C: телепортация. Имбовые варианты — с пониженным шансом.</summary>
    public static class TeleportOutcomes
    {
        /// <summary>
        /// Whitelist комнат, в которые разрешено выкидывать игрока случайным
        /// телепортом (C1). Список взят из RA → Door Management — это все
        /// «полезные» точки на карте, без узких коридоров и закутков.
        /// </summary>
        private static readonly RoomType[] AllowedRooms =
        {
            // SCP-комнаты (из 049_ARMORY/079_*/096/106_*/173_*/939_CRYO/HID_*)
            RoomType.Hcz049,
            RoomType.Hcz079,
            RoomType.Hcz096,
            RoomType.Hcz106,
            RoomType.Hcz939,
            RoomType.HczHid,
            RoomType.Lcz173,
            // Тестовые / лаб-комнаты (HCZ_127_LAB, 914)
            RoomType.Hcz127,
            RoomType.Lcz914,
            RoomType.Lcz330,
            // Армори (HCZ_ARMORY, LCZ_ARMORY)
            RoomType.HczArmory,
            RoomType.LczArmory,
            // Чекпоинты (CHECKPOINT_EZ_HCZ_A, CHECKPOINT_LCZ_A, CHECKPOINT_LCZ_B)
            RoomType.HczEzCheckpointA,
            RoomType.HczEzCheckpointB,
            RoomType.LczCheckpointA,
            RoomType.LczCheckpointB,
            // Прочее в EZ (LCZ_WC, INTERCOM, GATE_A/B, ESCAPE_*)
            RoomType.LczToilets,
            RoomType.EzIntercom,
            RoomType.EzGateA,
            RoomType.EzGateB,
            RoomType.EzShelter,
            // Поверхность (SURFACE_GATE) и шахта Альфы (SURFACE_NUKE)
            RoomType.Surface,
            RoomType.HczNuke,
        };

        /// <summary>Подъём над полом, чтобы игрок не клипал внутрь и не падал.</summary>
        private const float SafeUpOffset = 1.0f;

        public static void Register(List<Outcome> sink)
        {
            sink.Add(new Outcome(
                id: "C1",
                name: "Случайная комната с карты",
                rarity: Rarity.Common,
                message: "Хоп! Случайная комната с карты.",
                comment: "Найди дорогу обратно. Или не находи, я не настаиваю.",
                action: p =>
                {
                    var room = PickAllowedRoom();
                    if (room == null)
                        return;
                    p.Teleport(room.Position + Vector3.up * SafeUpOffset);
                }));

            sink.Add(new Outcome(
                id: "C2",
                name: "К случайному игроку",
                rarity: Rarity.Rare,
                message: "Тебя засосало к случайному игроку.",
                comment: "Привет! Извини, я не сам пришёл.",
                weightMultiplier: 0.4f,
                action: p =>
                {
                    var others = Player.List.Where(x => x.IsAlive && x != p && x.IsConnected).ToList();
                    if (others.Count == 0)
                        return;
                    var target = others[UnityEngine.Random.Range(0, others.Count)];
                    p.Teleport(target.Position + Vector3.up * SafeUpOffset);
                }));

            sink.Add(new Outcome(
                id: "C3",
                name: "Pocket Dimension на 5 секунд",
                rarity: Rarity.Epic,
                message: "Ты в Pocket Dimension... ровно 5 секунд.",
                comment: "Дыши спокойно. SCP-106 в курсе твоего визита.",
                weightMultiplier: 0.4f,
                action: p =>
                {
                    var pocket = Room.Get(RoomType.Pocket);
                    var origin = p.Position;

                    if (pocket != null)
                        p.Teleport(pocket.Position + Vector3.up * SafeUpOffset);
                    else
                        p.EnableEffect(EffectType.PocketCorroding, 1, 5f);

                    FermixScheduler.Delay(5f, () =>
                    {
                        if (p == null || !p.IsConnected || !p.IsAlive)
                            return;
                        // Возвращаем туда, где он был на момент броска.
                        p.Teleport(origin + Vector3.up * SafeUpOffset);
                    });
                }));

            sink.Add(new Outcome(
                id: "C4",
                name: "Поверхность",
                rarity: Rarity.Rare,
                message: "Ты на поверхности!",
                comment: "Свежий воздух, солнышко, MTF... сюрприз.",
                weightMultiplier: 0.4f,
                action: p =>
                {
                    var surface = Room.Get(RoomType.Surface);
                    if (surface != null)
                        p.Teleport(surface.Position + Vector3.up * SafeUpOffset);
                }));

            sink.Add(new Outcome(
                id: "C5",
                name: "Толчок",
                rarity: Rarity.Common,
                message: "Тебя пнули в случайном направлении!",
                comment: "Спасибо монетке за бесплатное движение.",
                action: p =>
                {
                    var dir = new Vector3(
                        UnityEngine.Random.Range(-1f, 1f),
                        0.3f,
                        UnityEngine.Random.Range(-1f, 1f)).normalized;

                    p.Teleport(p.Position + dir * 4f + Vector3.up * SafeUpOffset);
                    p.EnableEffect(EffectType.Concussed, 1, 2f);
                }));
        }

        /// <summary>
        /// Возвращает случайную комнату из whitelist'а, которая реально
        /// существует на текущей карте (некоторые типы могут отсутствовать
        /// в зависимости от seed'а / DLC). Если не нашли — fallback на любую.
        /// </summary>
        private static Room PickAllowedRoom()
        {
            var pool = new List<Room>();
            foreach (var rt in AllowedRooms)
            {
                var room = Room.Get(rt);
                if (room != null)
                    pool.Add(room);
            }

            if (pool.Count == 0)
                return Room.Random();

            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }
    }
}
