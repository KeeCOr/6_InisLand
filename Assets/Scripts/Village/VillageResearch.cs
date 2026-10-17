using System;
using UnityEngine;
namespace IL6
{
    // IDs are append-only because their integer values are used in save files.
    public enum ResearchId { Fence, Combat, Lumber, Crops, Vision, FoodCapacity, Morale, OreProspecting, Recruitment, ResourceSurvey }
    public readonly struct ResearchDefinition
    {
        public readonly ResearchId Id;
        public readonly BuildingKind Station;
        public readonly string Category, Name, Effect;
        public readonly int Wood, Stone;
        public ResearchDefinition(ResearchId id, BuildingKind station, string category, string name, string effect, int wood, int stone)
        { Id=id; Station=station; Category=category; Name=name; Effect=effect; Wood=wood; Stone=stone; }
        public ResourceCost Cost(int currentLevel) => new ResourceCost(Wood*(currentLevel+1),Stone*(currentLevel+1),ore:Id==ResearchId.OreProspecting?0:Mathf.Max(0,currentLevel-1));
    }
    public sealed class VillageResearch
    {
        public const int MaxLevel=10;
        public const int OreReward=5;
        public const float UseRange=3.5f;
        public static readonly ResearchDefinition[] Catalog = {
            new ResearchDefinition(ResearchId.Fence,BuildingKind.Carpenter,"방어","울타리 보강","단계마다 울타리 최대 체력 +20",12,6),
            new ResearchDefinition(ResearchId.Combat,BuildingKind.TrainingCamp,"전투","동료 전투 훈련","단계마다 동료 공격력 +10%",12,5),
            new ResearchDefinition(ResearchId.Lumber,BuildingKind.Sawmill,"생산","벌목 기술","단계마다 목재 획득량 +15%",14,8),
            new ResearchDefinition(ResearchId.Crops,BuildingKind.SeedStorage,"농업","수확 기술","단계마다 작물 수확량 +15%",8,2),
            new ResearchDefinition(ResearchId.Vision,BuildingKind.LookoutPost,"탐색","야간 정찰","단계마다 초소 시야 반경 +0.5",9,6),
            new ResearchDefinition(ResearchId.FoodCapacity,BuildingKind.FoodStorage,"보관","식량 보관 확장","단계마다 식량 보관 한도 +40",10,3),
            new ResearchDefinition(ResearchId.Morale,BuildingKind.Church,"지원","마음 돌보기","단계마다 사기 손실 완화 확률 +5%p",12,6),
            new ResearchDefinition(ResearchId.OreProspecting,BuildingKind.Blacksmith,"채광","광석 탐사","광석 5개 지급 · 채굴 드랍 확률 +2%p",12,6),
            new ResearchDefinition(ResearchId.Recruitment,BuildingKind.House,"영입","동료 소문 확산","단계마다 동료 등장·방문 빈도 +10%",8,3),
            new ResearchDefinition(ResearchId.ResourceSurvey,BuildingKind.Storage,"자원","자원 분포 조사","단계마다 나무·바위 생성 확률 +10%",10,4),
        };
        private readonly int[] _levels=new int[Catalog.Length];
        public int Level(ResearchId id) => (int)id>=0 && (int)id<_levels.Length ? _levels[(int)id] : 0;
        public static int ActiveLevel(ResearchId id) => GameSession.Instance != null ? GameSession.Instance.Research.Level(id) : 0;
        public static bool IsStation(BuildingKind kind) { foreach(var item in Catalog) if(item.Station==kind) return true; return false; }
        public static bool CanUse(ResearchDefinition item,Building station,Vector3 position)
            => station!=null && station.isActiveAndEnabled && station.CurrentHp>0 && station.Kind==item.Station
                && station.GetComponent<ConstructionSite>()==null && Vector2.Distance(station.transform.position,position)<=UseRange;
        public static bool RewardFits(ResearchId id,ResourceStore store)
            => store!=null && (id!=ResearchId.OreProspecting || store.GetCap(ResourceKind.Ore)-store.Get(ResourceKind.Ore)>=OreReward);
        public bool TryUpgrade(ResearchId id,Building station,Vector3 position,ResourceStore store)
        {
            if((int)id<0 || (int)id>=Catalog.Length) return false;
            var item=Catalog[(int)id];int level=Level(id);
            if(level>=MaxLevel || !CanUse(item,station,position) || !RewardFits(id,store) || !item.Cost(level).Pay(store)) return false;
            _levels[(int)id]++;
            if(id==ResearchId.OreProspecting) store.Add(ResourceKind.Ore,OreReward);
            foreach(var b in UnityEngine.Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                if(b.CurrentHp>0 && b.Kind==BuildingKind.Fence) b.RecalculateStats(false);
            return true;
        }
        public int[] Snapshot() => (int[])_levels.Clone();
        public void Restore(int[] levels)
        {
            Array.Clear(_levels,0,_levels.Length);
            if(levels==null) return;
            for(int i=0;i<Math.Min(levels.Length,_levels.Length);i++) _levels[i]=Mathf.Clamp(levels[i],0,MaxLevel);
        }
    }
}
