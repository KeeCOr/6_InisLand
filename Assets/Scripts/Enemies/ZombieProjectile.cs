using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 원거리 좀비(Archer)의 투사체. PlayerController/Companion/Building 명중 시 데미지.
    /// 기존 중세 화살 이미지를 진행 방향으로 회전해 표시.
    /// </summary>
    public sealed class ZombieProjectile : MonoBehaviour
    {
        public float Speed = 9f;
        public int Damage = 6;
        public float MaxLifetime = 3.5f;
        public float HitRadius = 0.5f;

        private Vector2 _direction = Vector2.right;
        private float _life;

        public void Aim(Transform target)
        {
            if (target == null) return;
            Vector2 d = (Vector2)target.position - (Vector2)transform.position;
            if (d.sqrMagnitude > 0.0001f) _direction = d.normalized;
            ProjectileVisual.Orient(transform,_direction);
        }

        private void Awake()
        {
            ProjectileVisual.Apply(gameObject,true,true);
        }

        private void Update()
        {
            _life += Time.deltaTime;
            if (_life > MaxLifetime) { Destroy(gameObject); return; }

            transform.position += (Vector3)(_direction * Speed * Time.deltaTime);

            // 명중 검사 — 짧은 OverlapCircle
            var hits = Physics2D.OverlapCircleAll(transform.position, HitRadius);
            foreach (var h in hits)
            {
                if (h == null) continue;
                var pc = h.GetComponent<PlayerController>();
                if (pc != null && !pc.IsDead) { pc.TakeDamage(Damage); Destroy(gameObject); return; }
                var cc = h.GetComponent<Companion>();
                if (cc != null && !cc.IsDead) { cc.TakeDamage(Damage); Destroy(gameObject); return; }
                var bb = h.GetComponent<Building>();
                if (bb != null && bb.CurrentHp > 0) { bb.TakeDamage(Damage); Destroy(gameObject); return; }
            }
        }
    }
}
