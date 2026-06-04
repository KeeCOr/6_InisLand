using UnityEngine;

namespace IL6
{
    public enum CompanionTraitKind
    {
        None,
        Brave,
        QuickHands,
        ColdResistant,
        LightEater,
        Defender,
        Caregiver,
    }

    public sealed class CompanionTrait : MonoBehaviour
    {
        public CompanionTraitKind Kind = CompanionTraitKind.None;
        public string DisplayName = "No Trait";
        [TextArea(1, 2)] public string Description = "";

        public float DamageMultiplier = 1f;
        public float AttackCooldownMultiplier = 1f;
        public float MaxHpMultiplier = 1f;
        public float GatherSpeedMultiplier = 1f;
        public float FoodMultiplier = 1f;

        public static CompanionTrait AssignRandom(GameObject go, string role, SeededRng rng)
        {
            if (go == null) return null;
            var trait = go.GetComponent<CompanionTrait>();
            if (trait != null && trait.Kind != CompanionTraitKind.None) return trait;
            if (trait == null) trait = go.AddComponent<CompanionTrait>();
            trait.Configure(PickKind(role, rng));
            return trait;
        }

        public void Configure(CompanionTraitKind kind)
        {
            Kind = kind;
            DamageMultiplier = 1f;
            AttackCooldownMultiplier = 1f;
            MaxHpMultiplier = 1f;
            GatherSpeedMultiplier = 1f;
            FoodMultiplier = 1f;

            switch (kind)
            {
                case CompanionTraitKind.Brave:
                    DisplayName = "Brave";
                    Description = "Deals more damage in combat.";
                    DamageMultiplier = 1.18f;
                    break;
                case CompanionTraitKind.QuickHands:
                    DisplayName = "Quick Hands";
                    Description = "Helps gathering finish faster.";
                    GatherSpeedMultiplier = 1.25f;
                    break;
                case CompanionTraitKind.ColdResistant:
                    DisplayName = "Cold Resistant";
                    Description = "Has higher maximum HP.";
                    MaxHpMultiplier = 1.15f;
                    break;
                case CompanionTraitKind.LightEater:
                    DisplayName = "Light Eater";
                    Description = "Consumes less daily food.";
                    FoodMultiplier = 0.75f;
                    break;
                case CompanionTraitKind.Defender:
                    DisplayName = "Defender";
                    Description = "Attacks a little faster and survives longer.";
                    AttackCooldownMultiplier = 0.88f;
                    MaxHpMultiplier = 1.08f;
                    break;
                case CompanionTraitKind.Caregiver:
                    DisplayName = "Caregiver";
                    Description = "Works steadily and eats a little less.";
                    GatherSpeedMultiplier = 1.12f;
                    FoodMultiplier = 0.9f;
                    break;
                default:
                    DisplayName = "No Trait";
                    Description = "";
                    break;
            }
        }

        public static float DamageMultiplierFor(Companion c)
        {
            var trait = c != null ? c.GetComponent<CompanionTrait>() : null;
            return trait != null ? trait.DamageMultiplier : 1f;
        }

        public static float AttackCooldownMultiplierFor(Companion c)
        {
            var trait = c != null ? c.GetComponent<CompanionTrait>() : null;
            return trait != null ? trait.AttackCooldownMultiplier : 1f;
        }

        public static float MaxHpMultiplierFor(Companion c)
        {
            var trait = c != null ? c.GetComponent<CompanionTrait>() : null;
            return trait != null ? trait.MaxHpMultiplier : 1f;
        }

        public static float FoodMultiplierFor(Companion c)
        {
            var trait = c != null ? c.GetComponent<CompanionTrait>() : null;
            return trait != null ? trait.FoodMultiplier : 1f;
        }

        public static float GatherSpeedMultiplierFor(Gatherable target)
        {
            if (target == null) return 1f;
            float bonus = 0f;
            var comps = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
            foreach (var c in comps)
            {
                if (c == null || c.IsDead) continue;
                if (c.CurrentMode != Companion.Mode.Working || c.Target != target) continue;
                var trait = c.GetComponent<CompanionTrait>();
                if (trait == null) continue;
                bonus += Mathf.Max(0f, trait.GatherSpeedMultiplier - 1f);
            }
            return 1f + Mathf.Min(0.75f, bonus);
        }

        private static CompanionTraitKind PickKind(string role, SeededRng rng)
        {
            if (rng == null) rng = new SeededRng((uint)Time.frameCount);
            string r = role ?? "";

            if (r.Contains("Hunter") || r.Contains("Warrior") || r.Contains("전사") || r.Contains("사냥"))
            {
                return rng.IntRange(0, 3) switch
                {
                    0 => CompanionTraitKind.Brave,
                    1 => CompanionTraitKind.Defender,
                    2 => CompanionTraitKind.QuickHands,
                    _ => CompanionTraitKind.ColdResistant,
                };
            }

            if (r.Contains("Child") || r.Contains("아이"))
            {
                return rng.IntRange(0, 2) switch
                {
                    0 => CompanionTraitKind.QuickHands,
                    1 => CompanionTraitKind.LightEater,
                    _ => CompanionTraitKind.Caregiver,
                };
            }

            return rng.IntRange(0, 4) switch
            {
                0 => CompanionTraitKind.QuickHands,
                1 => CompanionTraitKind.LightEater,
                2 => CompanionTraitKind.Caregiver,
                3 => CompanionTraitKind.ColdResistant,
                _ => CompanionTraitKind.Defender,
            };
        }
    }
}
