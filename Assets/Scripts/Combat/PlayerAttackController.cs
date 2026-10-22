using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 사거리 내 가장 가까운 공격 대상(좀비/사슴)을 자동 공격.
    /// Weapon.ProjectileSpeed > 0 이면 투사체 발사, 아니면 즉시 대미지.
    /// Weapon 미할당 시 기본 Magic Bolt 런타임 생성.
    /// </summary>
    public sealed class PlayerAttackController : MonoBehaviour
    {
        public WeaponDefinition Weapon;
        public uint RngSeed = 2026u;
        public PlayerProgression Progression;
        public float RecentlyAttackingWindowSec = 0.8f;

        public int WeaponIndex { get; private set; } = -1;
        public Color CurrentProjectileColor { get; private set; } = new Color(1f, 0.92f, 0.3f);
        public bool IsRecentlyAttacking => Time.time - _lastAttackAt <= RecentlyAttackingWindowSec;

        public void SwitchToWeapon(int idx)
        {
            var actions = GetComponent<PlayerActionState>();
            if (actions != null && actions.IsBusy) return;
            WeaponIndex = idx;
            Weapon = WeaponCatalog.Get(idx);
            CurrentProjectileColor = WeaponCatalog.ProjectileColor(WeaponIndex);
            var player = GetComponent<PlayerController>();
            if (player != null) player.SetWeaponVisual(WeaponIndex);
        }

        public void CycleWeapon(int delta)
        {
            int n = WeaponCatalog.All.Count;
            int next = WeaponIndex < 0 ? 0 : ((WeaponIndex + delta) % n + n) % n;
            SwitchToWeapon(next);
        }

        private float _cooldown;
        private float _interactionCooldown;
        private float _lastAttackAt = -100f;
        private SeededRng _rng;
        private Transform _self;

        private void Awake()
        {
            _self = transform;
            if (GetComponent<MeleeRangeIndicator>() == null) gameObject.AddComponent<MeleeRangeIndicator>();
            _rng = new SeededRng(RngSeed);
            if (Progression == null) Progression = GetComponent<PlayerProgression>();
            if (Weapon == null)
            {
                SwitchToWeapon(0); // Longsword 기본
            }
        }

        public bool IsConstructionHammer => Weapon != null && Weapon.Id == "construction_hammer";
        public float ManualTreeInteractRadius = 2.2f;

        public bool TryManualAction()
        {
            if (IsConstructionHammer)
            {
                if (_interactionCooldown>0f) return false;
                bool repaired=ManualWorldInteraction.TryRepair(GetComponent<PlayerController>(),GameSession.Instance!=null?GameSession.Instance.Resources:null);
                if(repaired) _interactionCooldown=.35f;
                return repaired;
            }
            var player = GetComponent<PlayerController>();
            if (InputReader.HudBlocked || Time.timeScale <= 0f || (player != null && player.IsDead) || Weapon == null) return false;
            float range = Weapon.Range + (Progression != null ? Progression.BonusRange : 0f);
            // A combat target keeps priority even while the weapon is cooling down.
            if (FindNearestTarget(range) != null)
            {
                var logging = GetComponent<GatherController>();
                if (logging != null && logging.IsActive) { logging.Cancel(); return false; }
                return TryAttack();
            }
            var gather = GetComponent<GatherController>();
            if(gather!=null && gather.IsActive) return true;
            var state=GetComponent<PlayerActionState>();if(state!=null && state.IsBusy) return false;
            if(_interactionCooldown>0) return false;
            bool handled;
            if(ManualWorldInteraction.TryUse(player,GameSession.Instance!=null?GameSession.Instance.Resources:null,out handled))
            {_interactionCooldown=.35f;return true;}
            if(handled) return false;
            return gather != null && gather.TryStartNearby(ManualTreeInteractRadius);
        }
        public bool HasManualActionTarget => IsConstructionHammer ? ManualWorldInteraction.HasRepairTarget(transform.position)
            : Weapon != null && FindNearestTarget(Weapon.Range+(Progression!=null?Progression.BonusRange:0)) != null
            || ManualWorldInteraction.HasRepairTarget(transform.position)
            || ManualWorldInteraction.Hint(transform.position) != "공격"
            || GetComponent<GatherController>() is GatherController gather && (gather.IsActive || gather.FindNearbyTarget(ManualTreeInteractRadius) != null);
        public string ManualActionLabel
        {
            get
            {
                if (IsConstructionHammer) return "수리";
                if (Weapon != null && FindNearestTarget(Weapon.Range+(Progression!=null?Progression.BonusRange:0)) != null) return "공격";
                var gather = GetComponent<GatherController>();
                if (gather != null && gather.IsActive) return gather.NearbyActionLabel(ManualTreeInteractRadius);
                string hint = ManualWorldInteraction.Hint(transform.position);
                return hint != "공격" ? hint : gather != null ? gather.NearbyActionLabel(ManualTreeInteractRadius) : "공격";
            }
        }
        private void Update()
        {
            _cooldown -= Time.deltaTime;
            _interactionCooldown -= Time.deltaTime;
        }

        public bool TryAttack()
        {
            if (IsConstructionHammer) return false;
            var player = GetComponent<PlayerController>();
            if (InputReader.HudBlocked || Time.timeScale <= 0f || (player != null && player.IsDead) || Weapon == null || _cooldown > 0f) return false;
            float effectiveRange = Weapon.Range + (Progression != null ? Progression.BonusRange : 0f);
            MonoBehaviour nearest = FindNearestTarget(effectiveRange);
            if (nearest == null) return false;
            var actions = GetComponent<PlayerActionState>();
            if (actions != null && !actions.TryBegin(PlayerAction.Attack, nearest.transform.position - transform.position)) return false;

            float dmgMul = Progression != null ? Progression.DamageMultiplier : 1f;
            int baseDmg = Mathf.RoundToInt(Weapon.BaseDamage * dmgMul);
            // _rng 가 어떤 경유로든 null 이면 즉석 초기화 (Awake 미실행 케이스 방어)
            if (_rng == null) _rng = new SeededRng(RngSeed);
            int dmg = DamageCalc.Compute(new DamageCalc.Input
            {
                Base = baseDmg,
                Armor = 0,
                CritRoll = _rng.Next(),
                CritChance = Weapon.CritChance,
                CritMult = Weapon.CritMultiplier,
            });

            if (Weapon.ProjectileSpeed > 0f)
            {
                int extra = Progression != null ? Progression.MultiShotExtra : 0;
                SpawnProjectileSpread(nearest, dmg, 1 + extra);
            }
            else
            {
                DealImmediate(nearest, dmg);
                int extraHits = Progression != null ? Progression.MultiShotExtra : 0;
                if (extraHits > 0) HitExtraNearby(nearest, dmg, extraHits, effectiveRange);
            }
            float cdMul = Progression != null ? Progression.CooldownMultiplier : 1f;
            _cooldown = Weapon.CooldownSec * cdMul;
            _lastAttackAt = Time.time;
            return true;
        }

        private void SpawnProjectileSpread(MonoBehaviour target, int dmg, int count)
        {
            Vector2 baseDir = ((Vector2)target.transform.position - (Vector2)_self.position).normalized;
            for (int i = 0; i < count; i++)
            {
                float angleOffset = count > 1 ? (i - (count - 1) * 0.5f) * 14f : 0f;
                Vector2 dir = Quaternion.Euler(0, 0, angleOffset) * baseDir;
                var proj = SpawnProjectile(target, dmg);
                if (i > 0) proj.AimDirection(dir);
            }
        }

        private void HitExtraNearby(MonoBehaviour primary, int dmg, int extra, float range)
        {
            var hits = Physics2D.OverlapCircleAll(_self.position, range);
            int remaining = extra;
            foreach (var h in hits)
            {
                if (remaining <= 0) break;
                var z = h.GetComponent<Zombie>();
                if (z != null && !z.IsDead && (object)z != primary && Vector2.Distance(_self.position,z.transform.position) <= range)
                {
                    DealImmediate(z, dmg);
                    remaining--;
                }
            }
        }

        private MonoBehaviour FindNearestTarget(float range)
        {
            var hits = Physics2D.OverlapCircleAll(_self.position, range);
            MonoBehaviour best = null;
            float bestDist = float.MaxValue;
            foreach (var h in hits)
            {
                MonoBehaviour candidate = null;
                var z = h.GetComponent<Zombie>();
                if (z != null && !z.IsDead) candidate = z;
                if (candidate == null)
                {
                    var a = h.GetComponent<AnimalAi>();
                    if (a != null && !a.IsDead) candidate = a;
                }
                if (candidate == null) continue;
                float dist = Vector2.Distance(_self.position, candidate.transform.position);
                if (dist > range) continue;
                if (dist < bestDist) { best = candidate; bestDist = dist; }
            }
            return best;
        }

        private Projectile SpawnProjectile(MonoBehaviour target, int dmg)
        {
            var go = new GameObject("Projectile");
            go.transform.position = _self.position;
            go.transform.localScale = Vector3.one * 0.9f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 50;
            // Projectile.Awake 가 자체적으로 sprite 만들고 sr.color 로 색칠 — ColorFallback 안 씀.
            sr.color = Color.Lerp(CurrentProjectileColor, Color.white, 0.25f);

            float speedMul = Progression != null ? Progression.ProjectileSpeedMultiplier : 1f;
            var proj = go.AddComponent<Projectile>();
            proj.Speed = Weapon.ProjectileSpeed * speedMul;
            proj.Damage = dmg;
            proj.HitRadius = 0.5f;
            proj.OwnerProgression = Progression;
            proj.Aim(target, _self.position);
            return proj;
        }

        private void DealImmediate(MonoBehaviour target, int dmg)
        {
            if (target is Zombie z && !z.IsDead)
            {
                GameFeel.Slash(_self.position, target.transform.position, new Color(1f, 0.95f, 0.7f));
                z.TakeDamage(dmg);
                if (!z.IsDead) MeleeKnockback.Apply(z, _self.position);
                if (Progression != null)
                {
                    if (Progression.GetStacks(RuneKind.PoisonBlade) > 0) z.ApplyPoison(Progression.PoisonDurationCalc, Progression.PoisonDpsCalc);
                    if (Progression.GetStacks(RuneKind.IceArrow) > 0) z.ApplySlow(Progression.IceSlowDurationCalc);
                    if (Progression.GetStacks(RuneKind.LightningStrike) > 0)
                    {
                        if (Random.value < Progression.LightningChance)
                            Projectile.ChainLightning(z.transform.position, z, Progression.LightningJumps, Progression.LightningDmg);
                    }
                    if (z.IsDead && Progression.GetStacks(RuneKind.Detonator) > 0)
                        Projectile.Detonate(z.transform.position, Progression.DetonateDmg, Progression.DetonateRadius);
                    // 흡혈 (근접 처치 포함)
                    if (z.IsDead && Progression.VampirismHeal > 0)
                    {
                        var pc = Progression.GetComponent<PlayerController>();
                        if (pc != null) pc.Heal(Progression.VampirismHeal);
                    }
                }
            }
            else if (target is AnimalAi animal)
            {
                GameFeel.Slash(_self.position, target.transform.position, new Color(1f, 0.95f, 0.7f));
                animal.TakeDamage(dmg);
                if (!animal.IsDead) MeleeKnockback.Apply(animal, _self.position);
            }
        }

        public float CurrentCooldown => Mathf.Max(0f, _cooldown);
    }
}
