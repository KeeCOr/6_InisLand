using System;
using System.Collections.Generic;
namespace IL6
{
    public enum RuneUpgradeCategory { Stats, Allies, Melee, Ranged, Resources }
    public static class RuneUpgradeRules
    {
        public static RuneUpgradeCategory Category(RuneKind kind)=>kind switch {
            RuneKind.AllyBoost or RuneKind.SummonDog or RuneKind.SummonHawk=>RuneUpgradeCategory.Allies,
            RuneKind.MeleeDamageUp or RuneKind.PoisonBlade or RuneKind.LightningStrike or RuneKind.MeleeCleave=>RuneUpgradeCategory.Melee,
            RuneKind.RangedDamageUp or RuneKind.IceArrow or RuneKind.MultiShot or RuneKind.Pierce or RuneKind.Detonator=>RuneUpgradeCategory.Ranged,
            RuneKind.ResourceGift=>RuneUpgradeCategory.Resources,
            _=>RuneUpgradeCategory.Stats};
        public static List<RuneKind> Pick(PlayerProgression progression,uint seed)
        {
            var rng=new SeededRng(seed^(uint)progression.Level);
            var pools=new Dictionary<RuneUpgradeCategory,List<RuneKind>>();
            foreach(RuneKind kind in Enum.GetValues(typeof(RuneKind)))
            {
                if(progression.IsMastered(kind))continue;
                var category=Category(kind);
                if(!pools.TryGetValue(category,out var pool))pools[category]=pool=new List<RuneKind>();
                pool.Add(kind);
            }
            var result=new List<RuneKind>();var used=new HashSet<RuneUpgradeCategory>();
            while(result.Count<3 && pools.Count>0)
            {
                var categories=new List<RuneUpgradeCategory>();
                foreach(var pair in pools)if(!used.Contains(pair.Key))categories.Add(pair.Key);
                if(categories.Count==0){used.Clear();continue;}
                var category=categories[rng.IntRange(0,categories.Count-1)];var pool=pools[category];
                int index=rng.IntRange(0,pool.Count-1);result.Add(pool[index]);pool.RemoveAt(index);used.Add(category);
                if(pool.Count==0)pools.Remove(category);
            }
            return result;
        }
    }
}
