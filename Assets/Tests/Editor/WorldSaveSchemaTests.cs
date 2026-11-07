using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class WorldSaveSchemaTests
    {
        private static WorldSave ValidWorld()=>new WorldSave
        {
            halfSize=5,cycle=new DayNightSnapshot{day=4},buildings=Array.Empty<SavedBuilding>(),wreckage=Array.Empty<SavedBuilding>(),
            construction=Array.Empty<SavedConstruction>(),companions=Array.Empty<SavedCompanion>(),player=new SavedPlayer{level=1,hp=100,runes=Array.Empty<RuneKind>()}
        };
        [Test] public void LegacySlotWithoutWorldStillLoads()
        {
            if(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IL6_TEST_SAVE_DIRECTORY")))Assert.Ignore("Requires isolated save");
            int slot=SaveLoad.ActiveSlot;SaveLoad.SelectSlot(2);string path=SaveLoad.StoragePath;byte[] original=File.Exists(path)?File.ReadAllBytes(path):null;
            try
            {
                File.WriteAllText(path,"{\"version\":1,\"currentDay\":6,\"resources\":{\"wood\":37},\"storyTutorialComplete\":true}");
                var legacy=SaveLoad.Load();Assert.That(legacy,Is.Not.Null);Assert.That(legacy.currentDay,Is.EqualTo(6));Assert.That(legacy.resources.wood,Is.EqualTo(37));Assert.That(legacy.world,Is.Null);
                Assert.That(SaveLoad.Save(legacy),Is.True);Assert.That(SaveLoad.Load().world,Is.Null,"Unity null inline serialization must not invent a saved village");
            }
            finally{if(original==null)File.Delete(path);else File.WriteAllBytes(path,original);SaveLoad.SelectSlot(slot);}
        }
        [Test] public void InvalidWorldCannotOverwriteValidSlot()
        {
            if(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IL6_TEST_SAVE_DIRECTORY")))Assert.Ignore("Requires isolated save");
            int slot=SaveLoad.ActiveSlot;SaveLoad.SelectSlot(2);string path=SaveLoad.StoragePath;byte[] original=File.Exists(path)?File.ReadAllBytes(path):null;
            try
            {
                var world=ValidWorld();Assert.That(SaveLoad.Save(new SaveFileV1{currentDay=4,world=world}),Is.True);
                var bytes=File.ReadAllBytes(path);Assert.That(SaveLoad.Load().hasWorld,Is.True);
                world.player.position=new Vector3(float.NaN,0,0);Assert.That(SaveLoad.Save(new SaveFileV1{world=world}),Is.False);CollectionAssert.AreEqual(bytes,File.ReadAllBytes(path));
            }
            finally{if(original==null)File.Delete(path);else File.WriteAllBytes(path,original);SaveLoad.SelectSlot(slot);}
        }
        [Test] public void DuplicateEntityIdsAreRejected()
        {
            var world=ValidWorld();var b=new SavedBuilding{id="same",kind=BuildingKind.House,level=1,hp=80,maxHp=100,scale=Vector3.one};
            world.buildings=new[]{b,b};Assert.That(WorldPersistence.IsValid(world),Is.False);
        }
        [Test] public void MissingNightEntitiesAndUnknownSchemaAreRejected()
        {
            var world=ValidWorld();world.cycle.phase=Phase.Night;Assert.That(WorldPersistence.IsValid(world),Is.False);
            world.night=new SavedNight{enemies=Array.Empty<SavedEnemy>()};Assert.That(WorldPersistence.IsValid(world),Is.True);
            world.schema=999;Assert.That(WorldPersistence.IsValid(world),Is.False);
        }
        [Test] public void RestoredRunesDoNotReplayResourceGiftOrLosePendingChoice()
        {
            var go=new GameObject("Progression restore test");
            try
            {
                var p=go.AddComponent<PlayerProgression>();p.RestoreSave(new SavedPlayer{level=3,xp=7,pending=true,runes=new[]{RuneKind.ResourceGift,RuneKind.RangedDamageUp,RuneKind.HpUp}});
                Assert.That(p.Level,Is.EqualTo(3));Assert.That(p.Xp,Is.EqualTo(7));Assert.That(p.LevelUpPending,Is.True);Assert.That(p.GetStacks(RuneKind.ResourceGift),Is.EqualTo(1));
                Assert.That(p.RangedDamageMultiplier,Is.EqualTo(1.25f));Assert.That(p.BonusMaxHp,Is.EqualTo(25));
                Assert.That(p.Applied.Count,Is.EqualTo(3));
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
        [Test] public void CycleRestoresLockedDurationWithoutReplayingPhaseEvents()
        {
            var cycle=new DayNightController(BalanceConfig.Instance);var save=new DayNightSnapshot{day=7,phase=Phase.Night,elapsedInPhase=15,duration=72,levelMultiplier=2};
            cycle.Restore(save);Assert.That(cycle.Day,Is.EqualTo(7));Assert.That(cycle.Phase,Is.EqualTo(Phase.Night));Assert.That(cycle.ElapsedInPhase,Is.EqualTo(15));Assert.That(cycle.PhaseDurationSec,Is.EqualTo(72));Assert.That(cycle.LevelDurationMul,Is.EqualTo(2));
        }
    }
}
