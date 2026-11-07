using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IL6
{
    // Optional versioned extension: old V1 files without world keep their original starter path.
    [Serializable] public class WorldSave
    {
        public int schema = 1;
        public DayNightSnapshot cycle;
        public float halfSize;
        public SavedBuilding[] buildings;
        public SavedBuilding[] wreckage;
        public SavedConstruction[] construction;
        public SavedCompanion[] companions;
        public SavedPlayer player;
        public SavedNight night;
        public int kills, companionsLost, maxCompanions, deathDay, fedDays;
        public int[] resourceCaps;
    }
    [Serializable] public class SavedBuilding
    {
        public string id, sprite;
        public BuildingKind kind;
        public Vector3 position, scale;
        public float rotation;
        public int level, hp, maxHp;
        public bool gate, vertical, outer;
        public int crop, nightsPassed;
        public bool harvestReady, withered;
        public float coldYield;
    }
    [Serializable] public class SavedConstruction
    {
        public BuildingKind kind;
        public Vector3 position;
        public float total, progress;
    }
    [Serializable] public class SavedCompanion
    {
        public string id, name, role, partnerId, partnerName, farmId, shelterId;
        public Vector3 position, scale;
        public int level, xp, hp, maxHp, morale, damage, birthDay, ageDays;
        public bool combat, childGrowth, isChild, everPartnered;
        public float speed, range, cooldown;
        public CompanionTraitKind trait;
        public Companion.Equip equipment;
        public Companion.Stance stance;
        public CompanionFamily.Sex sex;
    }
    [Serializable] public class SavedPlayer
    {
        public Vector3 position;
        public int level, xp, hp, weapon;
        public bool pending;
        public RuneKind[] runes;
    }
    [Serializable] public class SavedNight
    {
        public Phase phase;
        public int pending, kills;
        public float spawnTimer, warningRemaining, blizzardAccum;
        public bool blizzard, bossNight, bossSpawned, warningPending;
        public uint rng;
        public SavedEnemy[] enemies;
    }
    [Serializable] public class SavedEnemy
    {
        public string name, sprite;
        public Vector3 position, scale;
        public Color color;
        public bool boss, ranged;
        public int hp, maxHp, damage, rangedDamage, poisonDps;
        public float speed, sight, range, cooldown, attackTimer, rangedTimer, poison, poisonTick, slow;
    }

    public static class WorldPersistence
    {
        public static WorldSave Capture(GameSession session, PlayerController player)
        {
            var buildings = Object.FindObjectsByType<Building>(FindObjectsSortMode.None).Where(b => b.CurrentHp > 0).ToArray();
            var companions = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None).Where(c => !c.IsDead).ToArray();
            foreach (var b in buildings) b.EnsureSaveId();
            foreach (var c in companions) c.EnsureSaveId();
            return new WorldSave
            {
                cycle = session.Cycle.Snapshot(), halfSize = VillageStarter.CurrentHalfSize,
                buildings = buildings.Select(CaptureBuilding).ToArray(),
                wreckage = Object.FindObjectsByType<FenceWreckage>(FindObjectsSortMode.None).Where(f => f.CanRestore).Select(f => f.CaptureSave()).ToArray(),
                construction = Object.FindObjectsByType<ConstructionSite>(FindObjectsSortMode.None).Select(s => new SavedConstruction {kind=s.Kind,position=s.transform.position,total=s.TotalTime,progress=s.Progress}).ToArray(),
                companions = companions.Select(CaptureCompanion).ToArray(),
                player = player.GetComponent<PlayerProgression>().CaptureSave(player),
                night = Object.FindFirstObjectByType<NightController>()?.CaptureSave(),
                resourceCaps = ((ResourceKind[])Enum.GetValues(typeof(ResourceKind))).Select(session.Resources.GetCap).ToArray(),
            };
        }
        public static SavedBuilding CaptureBuilding(Building b)
        {
            var body=b.GetComponent<SpriteRenderer>();var farm=b.GetComponent<FarmBuilding>();
            return new SavedBuilding { id=b.SaveId, kind=b.Kind, position=b.transform.position,scale=b.transform.lossyScale,rotation=b.transform.eulerAngles.z,
                sprite=body!=null && body.sprite!=null?body.sprite.name:"", gate=b.GetComponent<Door>()!=null,
                vertical=body!=null && body.sprite!=null && body.sprite.name.ToLowerInvariant().Contains("vertical"),
                outer=b.transform.parent!=null && b.transform.parent.name=="VillageOuterFence",level=b.Level,hp=b.CurrentHp,maxHp=b.MaxHp,
                crop=farm!=null?(int)farm.CurrentCrop:0,nightsPassed=farm!=null?farm.NightsPassed:0,harvestReady=farm!=null && farm.HarvestReady,
                withered=farm!=null && farm.IsWithered,coldYield=farm!=null?farm.ColdYieldMultiplier:1f};
        }
        private static SavedCompanion CaptureCompanion(Companion c)
        {
            var family=c.GetComponent<CompanionFamily>();var growth=c.GetComponent<VillageChildGrowth>();
            var partner=family!=null?family.FindPartner():null;var partnerComp=partner!=null?partner.GetComponent<Companion>():null;
            var farm=c.FarmStation!=null?c.FarmStation.GetComponent<Building>():null;
            return new SavedCompanion { id=c.SaveId,name=c.name,role=c.GetComponent<DirectionalCharacterVisual>()?.CharacterKey??"Hunter",position=c.transform.position,scale=c.transform.localScale,
                level=c.Level,xp=c.Xp,hp=c.CurrentHp,maxHp=c.MaxHp,morale=c.Morale,combat=c.IsCombat,speed=c.MoveSpeed,range=c.AttackRange,damage=c.Damage,cooldown=c.AttackCooldown,
                equipment=c.Equipped,stance=c.CurrentStance,trait=c.GetComponent<CompanionTrait>()?.Kind??CompanionTraitKind.None,
                sex=family!=null?family.BiologicalSex:CompanionFamily.Sex.Unknown,isChild=family!=null && family.IsChild,everPartnered=family!=null && family.EverPartnered,
                partnerId=partnerComp!=null?partnerComp.SaveId:null,partnerName=family!=null?family.PartnerName:null,
                childGrowth=growth!=null,birthDay=growth!=null?growth.BirthDay:0,ageDays=growth!=null?growth.AgeDays:0,
                farmId=farm!=null?farm.SaveId:null,shelterId=c.Shelter!=null?c.Shelter.SaveId:null};
        }
        public static GameObject SpawnBuilding(SavedBuilding saved)
        {
            GameObject go;
            if(saved.kind==BuildingKind.Fence) go=SpawnFence(saved);
            else go=SpawnCompleted(saved.kind,saved.position);
            go.transform.position=saved.position;go.transform.rotation=Quaternion.Euler(0,0,saved.rotation);go.transform.localScale=saved.scale;
            if(saved.outer)VillageStarter.RestoreRingMember(go.transform);
            var building=go.GetComponent<Building>();building.SaveId=saved.id;building.RestoreSave(saved.level,saved.hp,saved.maxHp);
            var farm=go.GetComponent<FarmBuilding>();if(farm!=null)farm.RestoreSave(saved);
            return go;
        }
        public static GameObject SpawnFence(SavedBuilding s)
        {
            if(s.gate){var gate=VillageStarter.SpawnGate(s.position);gate.GetComponent<Door>().Init(s.vertical,false);return gate;}
            switch(s.sprite)
            {
                case "wooden_fence_Left":return VillageStarter.SpawnFence(s.position,0,FencePieceType.Left);
                case "wooden_fence_Right":return VillageStarter.SpawnFence(s.position,0,FencePieceType.Right);
                case "Fence_Wood_TopLeftCorner":return VillageStarter.SpawnFenceCorner(s.position,FenceCornerPieceType.TopLeft);
                case "Fence_Wood_TopRightCorner":return VillageStarter.SpawnFenceCorner(s.position,FenceCornerPieceType.TopRight);
                case "Fence_Wood_BottomLeftCorner":return VillageStarter.SpawnFenceCorner(s.position,FenceCornerPieceType.BottomLeft);
                case "Fence_Wood_BottomRightCorner":return VillageStarter.SpawnFenceCorner(s.position,FenceCornerPieceType.BottomRight);
                default:return s.vertical?VillageStarter.SpawnVerticalFence(s.position,VerticalFencePieceType.Center):VillageStarter.SpawnFence(s.position,0,FencePieceType.Center);
            }
        }
        public static GameObject SpawnCompleted(BuildingKind kind,Vector3 p) => kind switch
        {
            BuildingKind.Campfire=>VillageStarter.SpawnCampfire(p), BuildingKind.Barricade=>BuildingFactory.SpawnBarricade(p),
            BuildingKind.Fence=>VillageStarter.SpawnFence(p,0,FencePieceType.Center), BuildingKind.House=>BuildingFactory.SpawnHouse(p),
            BuildingKind.Storage=>BuildingFactory.SpawnStorage(p),BuildingKind.Farm=>BuildingFactory.SpawnFarm(p),BuildingKind.Watchtower=>BuildingFactory.SpawnWatchtower(p),
            BuildingKind.Infirmary=>BuildingFactory.SpawnInfirmary(p),BuildingKind.HuntersHut=>BuildingFactory.SpawnHuntersHut(p),
            BuildingKind.Brazier=>BuildingFactory.SpawnBrazier(p),BuildingKind.Blacksmith=>BuildingFactory.SpawnBlacksmith(p),
            BuildingKind.SeedStorage=>BuildingFactory.SpawnSeedStorage(p),BuildingKind.Carpenter=>BuildingFactory.SpawnCarpenter(p),
            BuildingKind.TrainingCamp=>BuildingFactory.SpawnTrainingCamp(p),BuildingKind.FoodStorage=>BuildingFactory.SpawnFoodStorage(p),
            BuildingKind.LookoutPost=>BuildingFactory.SpawnLookoutPost(p),BuildingKind.Sawmill=>BuildingFactory.SpawnSawmill(p),BuildingKind.Church=>BuildingFactory.SpawnChurch(p),
            _=>throw new ArgumentOutOfRangeException(nameof(kind))
        };
        public static void Restore(WorldSave save,GameSession session,PlayerController player)
        {
            var buildings=new Dictionary<string,Building>();
            VillageStarter.RestoreBounds(save.halfSize);
            foreach(var b in save.buildings){var building=SpawnBuilding(b).GetComponent<Building>();buildings.Add(b.id,building);}
            foreach(var debris in save.wreckage)FenceWreckage.RestoreSave(debris);
            foreach(var site in save.construction)ConstructionSite.RestoreSave(site);
            session.Resources.RestoreCaps(save.resourceCaps);
            player.GetComponent<PlayerProgression>().RestoreSave(save.player);
            player.RestoreSave(save.player.hp);player.transform.position=save.player.position;
            var attacker=player.GetComponent<PlayerAttackController>();if(attacker!=null)attacker.SwitchToWeapon(save.player.weapon);
            session.Cycle.Restore(save.cycle);
            var companions=new Dictionary<string,Companion>();
            foreach(var s in save.companions)
            {
                var go=new GameObject(s.name);go.transform.position=s.position;go.transform.localScale=s.scale;go.AddComponent<SpriteRenderer>().sprite=SpriteBank.CompanionSpriteForRole(s.role);
                go.AddComponent<CompanionTrait>().Configure(s.trait);
                var c=go.AddComponent<Companion>();c.SaveId=s.id;c.Player=player.transform;c.Level=s.level;c.Xp=s.xp;c.Morale=s.morale;c.IsCombat=s.combat;c.MoveSpeed=s.speed;c.AttackRange=s.range;c.Damage=s.damage;c.AttackCooldown=s.cooldown;c.Equipped=s.equipment;c.SetStance(s.stance);c.RestoreHealth(s.maxHp,s.hp);
                go.AddComponent<DirectionalCharacterVisual>().Configure(s.role);
                var f=go.AddComponent<CompanionFamily>();f.BiologicalSex=s.sex;f.IsChild=s.isChild;f.EverPartnered=s.everPartnered;f.PartnerName=s.partnerName;
                if(s.childGrowth){var g=go.AddComponent<VillageChildGrowth>();g.BirthDay=s.birthDay;g.AgeDays=s.ageDays;g.RestoreHealthAfterStart=s.hp;}
                companions.Add(s.id,c);
            }
            foreach(var s in save.companions)
            {
                var c=companions[s.id];var f=c.GetComponent<CompanionFamily>();
                if(!string.IsNullOrEmpty(s.partnerId) && companions.TryGetValue(s.partnerId,out var partner))f.PartnerInstanceId=partner.gameObject.GetInstanceID();
                if(!string.IsNullOrEmpty(s.farmId) && buildings.TryGetValue(s.farmId,out var farm))farm.GetComponent<FarmBuilding>()?.TryAssignWorker(c);
                if(!string.IsNullOrEmpty(s.shelterId) && buildings.TryGetValue(s.shelterId,out var shelter))c.HideInBuilding(shelter);
            }
            Object.FindFirstObjectByType<NightController>()?.RestoreSave(save.night,save.cycle.phase);
        }
        public static bool IsValid(WorldSave s)
        {
            if(s==null)return true;
            if(s.schema!=1 || s.cycle==null || !Enum.IsDefined(typeof(Phase),s.cycle.phase) || s.cycle.day<1 || !Finite(s.cycle.elapsedInPhase) || s.cycle.elapsedInPhase<0 || !Finite(s.cycle.duration) || !Finite(s.cycle.levelMultiplier) || !Finite(s.halfSize) || s.halfSize<5 || s.player==null || s.player.level<1 || s.player.runes==null || !Position(s.player.position))return false;
            if(s.buildings==null || s.companions==null || s.wreckage==null || s.construction==null)return false;
            var ids=new HashSet<string>();
            foreach(var b in s.buildings)if(b==null || string.IsNullOrEmpty(b.id) || !ids.Add(b.id) || !Enum.IsDefined(typeof(BuildingKind),b.kind) || !Position(b.position) || !Position(b.scale) || b.hp<1 || b.maxHp<b.hp || b.level<1)return false;
            foreach(var c in s.companions)if(c==null || string.IsNullOrEmpty(c.id) || !ids.Add(c.id) || !Position(c.position) || !Position(c.scale) || c.hp<1 || c.maxHp<1 || c.level<1 || !Finite(c.speed) || !Finite(c.range) || !Finite(c.cooldown) || !Enum.IsDefined(typeof(CompanionTraitKind),c.trait) || !Enum.IsDefined(typeof(Companion.Stance),c.stance) || !Enum.IsDefined(typeof(Companion.Equip),c.equipment))return false;
            foreach(var b in s.wreckage)if(b==null || !Position(b.position) || !Position(b.scale) || b.kind!=BuildingKind.Fence)return false;
            foreach(var c in s.construction)if(c==null || !Enum.IsDefined(typeof(BuildingKind),c.kind) || !Position(c.position) || !Finite(c.total) || c.total<=0 || !Finite(c.progress) || c.progress<0)return false;
            foreach(var rune in s.player.runes)if(!Enum.IsDefined(typeof(RuneKind),rune))return false;
            if(s.night!=null)
            {
                if(s.night.pending<0 || s.night.kills<0 || !Finite(s.night.spawnTimer) || !Finite(s.night.warningRemaining) || !Finite(s.night.blizzardAccum) || s.night.enemies==null)return false;
                foreach(var e in s.night.enemies)if(e==null || !Position(e.position) || !Position(e.scale) || e.hp<1 || e.maxHp<e.hp || !Finite(e.speed) || !Finite(e.range) || !Finite(e.cooldown))return false;
            }
            else if(s.cycle.phase==Phase.Night)return false;
            return true;
        }
        private static bool Position(Vector3 v)=>Finite(v.x)&&Finite(v.y)&&Finite(v.z);
        private static bool Finite(float f)=>!float.IsNaN(f)&&!float.IsInfinity(f);
    }
}
