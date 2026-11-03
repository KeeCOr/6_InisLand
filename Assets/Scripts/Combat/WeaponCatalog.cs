using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 근접·활·건설 망치 선택 목록. 건설 망치는 피해를 주지 않는 건설 도구다.
    /// </summary>
    public static class WeaponCatalog
    {
        private static List<WeaponDefinition> _all;
        public static IReadOnlyList<WeaponDefinition> All
        {
            get
            {
                if (_all == null) _all = Build();
                return _all;
            }
        }

        public static WeaponDefinition Get(int idx)
        {
            if (_all == null) _all = Build();
            return _all[((idx % _all.Count) + _all.Count) % _all.Count];
        }

        public static Color ProjectileColor(int idx)
        {
            return idx switch
            {
                0 => new Color(0.95f, 0.95f, 1f),    // Longsword: 흰 칼날 (근접: 투사체 없음)
                1 => new Color(0.6f, 0.85f, 0.4f),   // Bow: 녹색 화살
                _ => Color.yellow,
            };
        }

        private static List<WeaponDefinition> Build()
        {
            return new List<WeaponDefinition>
            {
                // 근접: 느리지만 강타 (cd 0.7→1.0, dmg 16→28). 사거리 짧음.
                Make("longsword", "Longsword", dmg: 28, range: 1.8f, cd: 1.0f, projSpd: 0f, crit: 0.10f),
                // 원거리: 빠르지만 약함 (cd 1.1→0.55, dmg 10→6). 사거리 길고 투사체.
                Make("bow", "Bow", dmg: 6, range: 10.0f, cd: 0.55f, projSpd: 12f, crit: 0.12f),
                Make("construction_hammer", "건설 망치", dmg: 0, range: 0f, cd: 0f, projSpd: 0f, crit: 0f),
            };
        }

        private static WeaponDefinition Make(string id, string name, int dmg, float range, float cd, float projSpd, float crit)
        {
            var w = ScriptableObject.CreateInstance<WeaponDefinition>();
            w.Id = id; w.DisplayName = name;
            w.BaseDamage = dmg; w.Range = range; w.CooldownSec = cd;
            w.CritChance = crit; w.CritMultiplier = 2f;
            w.HitRadius = 0.4f; w.ProjectileSpeed = projSpd;
            return w;
        }
    }
}
