using UnityEngine;

namespace IL6
{
    [DefaultExecutionOrder(1000)]
    public sealed class MeleeKnockback : MonoBehaviour
    {
        private Rigidbody2D _body;
        private Vector2 _velocity;
        private float _remaining;
        public bool Active => _remaining > 0f;

        public static bool IsActive(GameObject target) => target.TryGetComponent<MeleeKnockback>(out var push) && push.Active;

        public static void Apply(MonoBehaviour target, Vector2 origin)
        {
            if (!target.TryGetComponent<Rigidbody2D>(out var body) || body.bodyType == RigidbodyType2D.Static) return;
            var push = target.GetComponent<MeleeKnockback>();
            if (push == null) push = target.gameObject.AddComponent<MeleeKnockback>();
            push._body = body;
            Vector2 direction = body.position - origin;
            if (direction.sqrMagnitude < .0001f) direction = Vector2.right;
            push._velocity = direction.normalized * (0.8f / .18f);
            push._remaining = .18f;
        }

        private void FixedUpdate()
        {
            if (!Active || _body == null) return;
            // Physics resolves fences and walls; never teleport through a blocker.
            float step = Mathf.Min(Time.fixedDeltaTime, _remaining);
            _body.velocity = _velocity * (step / Time.fixedDeltaTime);
            _remaining -= step;
            if (_remaining <= 0f) _body.velocity = Vector2.zero;
        }

        private void OnDisable()
        {
            if (Active && _body != null) _body.velocity = Vector2.zero;
            _remaining = 0f;
        }
    }
}
