using UnityEngine;
namespace IL6
{
    public static class SpawnResearchRules
    {
        public static float Multiplier(int level) => 1f+.1f*Mathf.Clamp(level,0,VillageResearch.MaxLevel);
        public static float NpcInterval(float baseSeconds,int level) => Mathf.Max(1f,baseSeconds)/Multiplier(level);
        // Preserve NPC and animal probabilities if resource density fills the remaining slots.
        public static Vector4 Chances(float tree,float rock,float animal,float npc,int resourceLevel,int recruitLevel)
        {
            float n=Mathf.Clamp01(npc*Multiplier(recruitLevel));
            float a=Mathf.Clamp(animal,0,1f-n);
            float t=Mathf.Max(0,tree)*Multiplier(resourceLevel),r=Mathf.Max(0,rock)*Multiplier(resourceLevel);
            float budget=Mathf.Max(0,1f-n-a),total=t+r;
            if(total>budget && total>0) {float scale=budget/total;t*=scale;r*=scale;}
            return new Vector4(t,r,a,n);
        }
    }
}
