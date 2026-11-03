using UnityEngine;

namespace IL6
{
    [DefaultExecutionOrder(1000)]
    public sealed class MeleeKnockback : MonoBehaviour
    {
        public const float PushDistance = 1.5f;
        public const float PushDuration = .22f;
        public const float RecoveryDuration = .12f;
        private Rigidbody2D _body;
        private Zombie _zombie;
        private AnimalAi _animal;
        private Vector2 _velocity;
        private float _remaining;
        private float _recovery;
        public bool Active => _remaining > 0f || _recovery > 0f;

        public static bool IsActive(GameObject target) => target.TryGetComponent<MeleeKnockback>(out var push) && push.Active;

        public static void Apply(MonoBehaviour target, Vector2 origin)
        {
            if(target==null || Time.timeScale<=0f || InputReader.HudBlocked)return;
            if (!target.TryGetComponent<Rigidbody2D>(out var body) || body.bodyType == RigidbodyType2D.Static) return;
            var push = target.GetComponent<MeleeKnockback>();
            if (push == null) push = target.gameObject.AddComponent<MeleeKnockback>();
            push._body = body;
            push._zombie=target as Zombie;push._animal=target as AnimalAi;
            Vector2 direction = body.position - origin;
            if (direction.sqrMagnitude < .0001f) direction = Vector2.right;
            push._velocity = direction.normalized * (PushDistance / PushDuration);
            push._remaining = PushDuration;
            push._recovery = RecoveryDuration;
            body.velocity=push._velocity;
        }

        private void FixedUpdate()
        {
            if (!Active || _body == null) return;
            if((_zombie!=null && _zombie.IsDead) || (_animal!=null && _animal.IsDead))
            { OnDisable();return; }
            if(InputReader.HudBlocked){_body.velocity=Vector2.zero;return;}
            // Physics resolves fences and walls; never teleport through a blocker.
            if(_remaining>0f)
            {
                float step = Mathf.Min(Time.fixedDeltaTime, _remaining);
                _body.velocity = _velocity * (step / Time.fixedDeltaTime);
                _remaining = Mathf.Max(0f,_remaining-step);
                // Let physics integrate the final push before stopping on the next tick.
                return;
            }
            _body.velocity=Vector2.zero;
            _recovery=Mathf.Max(0f,_recovery-Time.fixedDeltaTime);
        }

        private void OnDisable()
        {
            if (Active && _body != null) _body.velocity = Vector2.zero;
            _remaining = 0f;
            _recovery = 0f;
        }
    }
}
