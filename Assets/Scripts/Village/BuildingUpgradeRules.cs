using UnityEngine;

namespace IL6
{
    public readonly struct ResourceCost
    {
        public readonly int Wood;
        public readonly int Stone;
        public readonly int Food;
        public readonly int Frostbloom;
        public readonly int Ore;

        public ResourceCost(int wood, int stone = 0, int food = 0, int frostbloom = 0, int ore = 0)
        {
            Wood = wood;
            Stone = stone;
            Food = food;
            Frostbloom = frostbloom;
            Ore = ore;
        }

        public bool CanPay(ResourceStore store)
        {
            if (store == null) return false;
            return store.Get(ResourceKind.Wood) >= Wood
                && store.Get(ResourceKind.Stone) >= Stone
                && store.Get(ResourceKind.Food) >= Food
                && store.Get(ResourceKind.Frostbloom) >= Frostbloom
                && store.Get(ResourceKind.Ore) >= Ore;
        }

        public bool Pay(ResourceStore store)
        {
            if (!CanPay(store)) return false;
            if (Wood > 0 && !store.Spend(ResourceKind.Wood, Wood)) return false;
            if (Stone > 0 && !store.Spend(ResourceKind.Stone, Stone)) return false;
            if (Food > 0 && !store.Spend(ResourceKind.Food, Food)) return false;
            if (Frostbloom > 0 && !store.Spend(ResourceKind.Frostbloom, Frostbloom)) return false;
            if (Ore > 0 && !store.Spend(ResourceKind.Ore, Ore)) return false;
            return true;
        }

        public override string ToString()
        {
            string s = "";
            Append(ref s, Wood, "W");
            Append(ref s, Stone, "S");
            Append(ref s, Food, "F");
            Append(ref s, Frostbloom, "Fb");
            Append(ref s, Ore, "Ore");
            return string.IsNullOrEmpty(s) ? "0" : s;
        }

        private static void Append(ref string s, int amount, string suffix)
        {
            if (amount <= 0) return;
            if (!string.IsNullOrEmpty(s)) s += " + ";
            s += $"{amount}{suffix}";
        }
    }

    public static class BuildingUpgradeRules
    {
        public const int MaxLevel = 5;

        public static int BaseWoodCost(BuildingKind kind) => kind switch
        {
            BuildingKind.Campfire => 5,
            BuildingKind.House => 6,
            BuildingKind.Fence => 1,
            BuildingKind.Storage => 8,
            BuildingKind.Farm => 6,
            BuildingKind.Watchtower => 8,
            BuildingKind.Infirmary => 7,
            BuildingKind.HuntersHut => 8,
            BuildingKind.Brazier => 12,
            BuildingKind.Blacksmith => 10,
            BuildingKind.SeedStorage => 8,
            BuildingKind.Carpenter => 12,
            BuildingKind.TrainingCamp => 12,
            BuildingKind.FoodStorage => 10,
            BuildingKind.LookoutPost => 9,
            BuildingKind.Sawmill => 14,
            BuildingKind.Church => 12,
            _ => 5,
        };

        public static int BaseStoneCost(BuildingKind kind) => kind switch
        {
            BuildingKind.Watchtower => 4,
            BuildingKind.Brazier => 4,
            BuildingKind.Blacksmith => 8,
            BuildingKind.SeedStorage => 2,
            BuildingKind.Carpenter => 6,
            BuildingKind.TrainingCamp => 5,
            BuildingKind.FoodStorage => 3,
            BuildingKind.LookoutPost => 6,
            BuildingKind.Sawmill => 8,
            BuildingKind.Church => 6,
            _ => 0,
        };

        public static ResourceCost BuildCost(BuildingKind kind, float duplicateMultiplier)
        {
            // Fence segments stay affordable even when the starting village already has a full ring.
            if (kind == BuildingKind.Fence) return new ResourceCost(1,0);
            return new ResourceCost(
                Mathf.RoundToInt(BaseWoodCost(kind) * duplicateMultiplier),
                Mathf.RoundToInt(BaseStoneCost(kind) * duplicateMultiplier)
            );
        }

        public static ResourceCost UpgradeCost(BuildingKind kind, int currentLevel)
        {
            int next = Mathf.Clamp(currentLevel + 1, 2, MaxLevel);
            float mul = next switch
            {
                2 => 1.0f,
                3 => 1.7f,
                4 => 2.6f,
                _ => 3.8f,
            };

            int wood = Mathf.Max(1, Mathf.RoundToInt(BaseWoodCost(kind) * mul));
            int stone = Mathf.RoundToInt((BaseStoneCost(kind) + StoneUpgradeBias(kind)) * mul);
            int food = kind == BuildingKind.SeedStorage && next >= 3 ? next : 0;
            int frost = kind == BuildingKind.Brazier && next >= 4 ? 1 : 0;
            return new ResourceCost(wood, stone, food, frost);
        }

        private static int StoneUpgradeBias(BuildingKind kind) => kind switch
        {
            BuildingKind.Fence => 1,
            BuildingKind.Barricade => 1,
            BuildingKind.Campfire => 1,
            BuildingKind.Farm => 1,
            BuildingKind.FoodStorage => 1,
            BuildingKind.Sawmill => 2,
            _ => 0,
        };

        public static int BaseHp(BuildingKind kind, BalanceConfig b) => kind switch
        {
            BuildingKind.Campfire => b.CampfireHp,
            BuildingKind.Brazier => b.BrazierHp,
            BuildingKind.Blacksmith => b.BlacksmithHp,
            BuildingKind.SeedStorage => b.SeedStorageHp,
            BuildingKind.Carpenter => b.CarpenterHp,
            BuildingKind.TrainingCamp => b.TrainingCampHp,
            BuildingKind.FoodStorage => b.FoodStorageHp,
            BuildingKind.LookoutPost => b.LookoutPostHp,
            BuildingKind.Sawmill => b.SawmillHp,
            BuildingKind.Church => 180,
            BuildingKind.Barricade => b.BarricadeHp,
            BuildingKind.Fence => b.FenceHp,
            BuildingKind.House => 140,
            BuildingKind.Storage => 200,
            BuildingKind.Farm => 90,
            BuildingKind.Watchtower => 200,
            BuildingKind.Infirmary => 180,
            BuildingKind.HuntersHut => 160,
            _ => b.BarricadeHp,
        };

        public static int HighestLevel(BuildingKind kind)
        {
            int best = 0;
            var bs = Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
            foreach (var b in bs)
            {
                if (b == null || b.CurrentHp <= 0 || b.Kind != kind) continue;
                best = Mathf.Max(best, b.Level);
            }
            return best;
        }

        public static float FenceHpMultiplier()
        {
            // Carpenter research still adds 20 HP per tier when base durability changes.
            return 1f + 20f * VillageResearch.ActiveLevel(ResearchId.Fence) / Mathf.Max(1,BalanceConfig.Instance.FenceHp);
        }

        public static float CropYieldMultiplier()
        {
            int seedStorage = HighestLevel(BuildingKind.SeedStorage);
            return 1f + 0.15f * seedStorage + .15f * VillageResearch.ActiveLevel(ResearchId.Crops);
        }

        public const int FoodStorageCapPerLevel = 40;

        public static float TrainingDamageMultiplier()
        {
            int training = HighestLevel(BuildingKind.TrainingCamp);
            return 1f + 0.10f * training + .10f * VillageResearch.ActiveLevel(ResearchId.Combat);
        }

        public static int FoodStorageCapBonus()
        {
            int total = 0;
            var all = Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
            foreach (var b in all)
            {
                if (b == null || b.CurrentHp <= 0 || b.Kind != BuildingKind.FoodStorage) continue;
                total += Mathf.Max(1, b.Level) * FoodStorageCapPerLevel;
            }
            return total;
        }

        public static float LookoutVisionRadius()
        {
            int lookout = HighestLevel(BuildingKind.LookoutPost);
            return lookout <= 0 ? 0f : 4.5f + 0.6f * (lookout - 1) + .5f * VillageResearch.ActiveLevel(ResearchId.Vision);
        }

        public static float SawmillWoodYieldMultiplier()
        {
            int sawmill = HighestLevel(BuildingKind.Sawmill);
            return 1f + 0.15f * sawmill + .15f * VillageResearch.ActiveLevel(ResearchId.Lumber);
        }

        public static float ChurchMoraleLossReductionChance()
        {
            int church = HighestLevel(BuildingKind.Church);
            return church <= 0 ? 0f : Mathf.Clamp01(0.20f + 0.10f * (church - 1) + .05f * VillageResearch.ActiveLevel(ResearchId.Morale));
        }

        public static int ReduceMoraleLoss(int baseLoss, uint seed)
        {
            if (baseLoss <= 0) return 0;
            float chance = ChurchMoraleLossReductionChance();
            if (chance <= 0f) return baseLoss;
            var rng = new SeededRng(seed);
            return rng.Next() < chance ? Mathf.CeilToInt(baseLoss * 0.5f) : baseLoss;
        }

        public static float HighMoraleEfficiencyMultiplier(int morale)
        {
            int church = HighestLevel(BuildingKind.Church);
            if (church <= 0 || morale < 80) return 1f;
            return 1f + 0.05f * church;
        }

        public static string UpgradeSummary(BuildingKind kind, int nextLevel) => kind switch
        {
            BuildingKind.SeedStorage => $"작물 해금/수확 +{nextLevel * 15}%",
            BuildingKind.Carpenter => "시설 내구도 증가 · 울타리는 보강 연구로 강화",
            BuildingKind.Brazier => "열/시야/화염 피해 증가",
            BuildingKind.Blacksmith => "열원 강화 + 장비 테크 기반",
            BuildingKind.TrainingCamp => $"동료 공격력 +{nextLevel * 10}%",
            BuildingKind.FoodStorage => $"+{FoodStorageCapPerLevel} Food 보관",
            BuildingKind.LookoutPost => "밤 시야 반경 증가",
            BuildingKind.Sawmill => $"목재 채집 +{nextLevel * 15}%",
            BuildingKind.Church => $"Morale protection {20 + (nextLevel - 1) * 10}% / high morale efficiency +{nextLevel * 5}%",
            BuildingKind.Farm => "수확량/농부/성장 개선",
            BuildingKind.Fence => "목공소에서 울타리 보강 연구",
            BuildingKind.Campfire => "열/시야/연료량 증가",
            _ => "HP와 기능 효율 증가",
        };
    }
}
