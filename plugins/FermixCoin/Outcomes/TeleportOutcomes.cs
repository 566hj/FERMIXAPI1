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
        public static void Register(List<Outcome> sink)
        {
            sink.Add(new Outcome(
                id: "C1",
                name: "Случайная комната зоны",
                rarity: Rarity.Common,
                message: "Хоп! Случайная комната твоей зоны.",
                comment: "Найди дорогу обратно. Или не находи, я не настаиваю.",
                action: p =>
                {
                    var zone = p.Zone == ZoneType.Unspecified ? ZoneType.LightContainment : p.Zone;
                    var room = Room.Random(zone) ?? Room.Random();
                    if (room == null)
                        return;
                    p.Teleport(room.Position + Vector3.up * 1.5f);
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
                    p.Teleport(target);
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
                        p.Teleport(pocket.Position + Vector3.up * 1.5f);
                    else
                        p.EnableEffect(EffectType.PocketCorroding, 1, 5f);

                    FermixScheduler.Delay(5f, () =>
                    {
                        if (p == null || !p.IsConnected || !p.IsAlive)
                            return;
                        // Возвращаем туда, где он был на момент броска.
                        p.Teleport(origin);
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
                        p.Teleport(surface.Position + Vector3.up * 1.5f);
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

                    p.Teleport(p.Position + dir * 4f + Vector3.up * 0.5f);
                    p.EnableEffect(EffectType.Concussed, 1, 2f);
                }));
        }
    }
}
