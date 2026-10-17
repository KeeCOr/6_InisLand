using UnityEngine;
namespace IL6 {
public static class LootRules {
    public static float OreDropChance => Mathf.Clamp01(BalanceConfig.Instance.RockOreDropChance+.02f*VillageResearch.ActiveLevel(ResearchId.OreProspecting));
    public static bool RockDropsOre(float sample,float chance) => chance>=1f || (sample>=0f && sample<Mathf.Clamp01(chance));
}
}
