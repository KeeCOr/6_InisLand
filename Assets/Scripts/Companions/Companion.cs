using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 부하. 평소엔 Player 주변 원형 대형 슬롯에 머무르며 겹치지 않게 서로 밀어냄.
    /// AssignGather 시 해당 Gatherable 로 이동해 채집 후 복귀.
    /// 전투: 사거리 내 가장 가까운 좀비에게 주기적으로 투사체 발사 (연두색).
    /// </summary>
    public sealed class Companion : MonoBehaviour
    {
        public Transform Player;
        public float FormationRadius = 1.7f;
        public float FollowDistance = 1.8f;
        public float FollowStopDistance = 0.25f;
        public float MoveSpeed = 4.5f;
        public float GatherReach = 0.7f;
        public float SeparationRadius = 0.9f;

        [Header("Combat")]
        public float AttackRange = 5f;
        public int Damage = 6;
        public float AttackCooldown = 1.6f;
        public float ProjectileSpeed = 7f;

        public enum Mode { Follow, Working }
        public Mode CurrentMode { get; private set; } = Mode.Follow;
        public Gatherable Target { get; private set; }

        private float _attackCd;

        public void AssignGather(Gatherable target)
        {
            Target = target;
            CurrentMode = target != null ? Mode.Working : Mode.Follow;
        }

        private void Start()
        {
            if (Player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) Player = p.transform;
            }
        }

        private void Update()
        {
            _attackCd -= Time.deltaTime;
            if (CurrentMode == Mode.Working) { HandleWorking(); }
            else { HandleFollow(); }
            TryAttack();
        }

        private void HandleFollow()
        {
            if (Player == null) return;

            Vector2 slot = GetFormationSlot();
            Vector2 toSlot = slot - (Vector2)transform.position;
            float d = toSlot.magnitude;

            Vector2 moveDir = Vector2.zero;
            if (d > FollowStopDistance)
            {
                moveDir = toSlot.normalized;
                float speedFactor = Mathf.Clamp01(d / FollowDistance);
                moveDir *= speedFactor;
            }

            // 다른 동료와 겹치지 않게 분리 벡터 가산
            moveDir += SeparationFromOthers();

            if (moveDir.sqrMagnitude > 0.0001f)
            {
                transform.position += (Vector3)(moveDir.normalized * MoveSpeed * Time.deltaTime * Mathf.Clamp01(moveDir.magnitude));
            }
        }

        private void HandleWorking()
        {
            if (Target == null)
            {
                CurrentMode = Mode.Follow;
                return;
            }
            Vector2 toTarget = (Vector2)Target.transform.position - (Vector2)transform.position;
            float dist = toTarget.magnitude;
            if (dist < GatherReach)
            {
                var session = GameSession.Instance;
                if (session != null) Target.OnGathered(session.Resources);
                Target = null;
                CurrentMode = Mode.Follow;
                return;
            }
            var dir = toTarget.normalized + SeparationFromOthers() * 0.5f;
            transform.position += (Vector3)(dir.normalized * MoveSpeed * Time.deltaTime);
        }

        private Vector2 GetFormationSlot()
        {
            if (Player == null) return transform.position;
            var all = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
            int n = Mathf.Max(1, all.Length);
            int myId = GetInstanceID();
            int myIdx = 0;
            foreach (var c in all)
            {
                if (c != null && c != this && c.GetInstanceID() < myId) myIdx++;
            }
            float angle = (myIdx / (float)n) * Mathf.PI * 2f + Mathf.PI * 0.5f;
            return (Vector2)Player.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * FormationRadius;
        }

        private Vector2 SeparationFromOthers()
        {
            Vector2 sum = Vector2.zero;
            var all = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
            foreach (var c in all)
            {
                if (c == null || c == this) continue;
                Vector2 diff = (Vector2)transform.position - (Vector2)c.transform.position;
                float d = diff.magnitude;
                if (d > 0.001f && d < SeparationRadius)
                {
                    sum += diff.normalized * ((SeparationRadius - d) / SeparationRadius);
                }
            }
            return sum * 2f;
        }

        private void TryAttack()
        {
            if (_attackCd > 0f) return;
            var z = FindNearestZombie(AttackRange);
            if (z == null) return;
            SpawnProjectile(z);
            _attackCd = AttackCooldown;
        }

        private Zombie FindNearestZombie(float range)
        {
            var all = Object.FindObjectsByType<Zombie>(FindObjectsSortMode.None);
            Zombie best = null;
            float bestDist = range;
            foreach (var z in all)
            {
                if (z == null || z.IsDead) continue;
                float d = Vector2.Distance(transform.position, z.transform.position);
                if (d < bestDist) { best = z; bestDist = d; }
            }
            return best;
        }

        private void SpawnProjectile(Zombie target)
        {
            var go = new GameObject("CompProjectile");
            go.transform.position = transform.position;
            go.transform.localScale = Vector3.one * 0.28f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 9;

            var cf = go.AddComponent<ColorFallback>();
            cf.Tint = new Color(0.55f, 1f, 0.7f);
            cf.Shape = FallbackShape.Circle;
            cf.Circle = true;
            cf.PixelSize = 32;
            cf.OutlineWidth = 1;
            cf.OutlineColor = new Color(0.15f, 0.4f, 0.25f, 1f);

            var proj = go.AddComponent<Projectile>();
            proj.Speed = ProjectileSpeed;
            proj.Damage = Damage;
            proj.HitRadius = 0.4f;
            proj.Aim(target, transform.position);
        }
    }
}
