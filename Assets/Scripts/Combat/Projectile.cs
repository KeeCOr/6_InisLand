using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 직선 또는 호밍 투사체. 타겟이 살아있으면 매 프레임 방향 갱신,
    /// 죽거나 사라지면 마지막 유효 방향으로 계속 직진.
    /// 히트 시 Zombie 는 TakeDamage, DeerAi 는 Gatherable.OnGathered 로 처리.
    /// </summary>
    public sealed class Projectile : MonoBehaviour
    {
        public float Speed = 8f;
        public int Damage = 10;
        public float MaxLifetime = 3f;
        public float HitRadius = 0.35f;

        private MonoBehaviour _target;
        private Vector2 _direction = Vector2.right;
        private float _life;

        public void Aim(MonoBehaviour target, Vector3 spawnPos)
        {
            _target = target;
            if (target != null)
            {
                Vector2 d = (Vector2)target.transform.position - (Vector2)spawnPos;
                if (d.sqrMagnitude > 0.0001f) _direction = d.normalized;
            }
        }

        public void AimDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude > 0.0001f) _direction = dir.normalized;
        }

        private void Update()
        {
            _life += Time.deltaTime;
            if (_life > MaxLifetime)
            {
                Destroy(gameObject);
                return;
            }

            bool targetAlive = IsTargetAlive(_target);

            // 타겟 유효 → 호밍. 아니면 마지막 방향 유지.
            if (targetAlive)
            {
                Vector2 toTarget = (Vector2)_target.transform.position - (Vector2)transform.position;
                if (toTarget.sqrMagnitude > 0.0001f) _direction = toTarget.normalized;
            }

            transform.position += (Vector3)(_direction * Speed * Time.deltaTime);

            if (targetAlive)
            {
                float d = Vector2.Distance(transform.position, _target.transform.position);
                if (d < HitRadius)
                {
                    DealDamage(_target);
                    Destroy(gameObject);
                }
            }
        }

        private static bool IsTargetAlive(MonoBehaviour target)
        {
            if (target == null) return false;
            if (target is Zombie z) return !z.IsDead;
            if (target is DeerAi) return true;
            return false;
        }

        private void DealDamage(MonoBehaviour target)
        {
            if (target is Zombie z && !z.IsDead)
            {
                z.TakeDamage(Damage);
                return;
            }
            if (target is DeerAi deer)
            {
                var g = deer.GetComponent<Gatherable>();
                var session = GameSession.Instance;
                if (g != null && session != null)
                {
                    g.OnGathered(session.Resources);
                }
                else if (deer != null && deer.gameObject != null)
                {
                    Destroy(deer.gameObject);
                }
            }
        }
    }
}
