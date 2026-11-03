using UnityEngine;

namespace IL6
{
    /// <summary>Shows the same manual action decision consumed by Space.</summary>
    public sealed class PlayerInteractionPreview : MonoBehaviour
    {
        private PlayerController _player;
        private PlayerAttackController _attack;
        private LineRenderer _targetRing, _aim;
        private GroundRangeDecal _bowDecal;
        private Material _material;
        private Vector2 _facing = Vector2.down;
        public ManualInteractionTarget DisplayedTarget { get; private set; }
        public bool Visible => (_targetRing != null && _targetRing.enabled) || (_aim != null && _aim.enabled);
        public float DisplayedRange { get; private set; }
        public bool BowRangeVisible => _bowDecal != null && _bowDecal.Visible;
        public GroundRangeDecal BowDecal => _bowDecal;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _attack = GetComponent<PlayerAttackController>();
            _material = new Material(Shader.Find("Sprites/Default"));
            _targetRing = CreateLine("Space target", true, 40);
            _aim = CreateLine("Space direction", false, 2);
            _bowDecal = GroundRangeDecal.Create(gameObject, "Bow range decal");
        }

        private LineRenderer CreateLine(string label, bool loop, int count)
        {
            var child = new GameObject(label);
            child.transform.SetParent(transform, false);
            var line = child.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.loop = loop;
            line.positionCount = count;
            line.startWidth = line.endWidth = .045f;
            line.sortingOrder = 5;
            line.enabled = false;
            return line;
        }

        private void LateUpdate()
        {
            DisplayedTarget = ManualWorldInteraction.Resolve(_player);
            _targetRing.enabled = _aim.enabled = false;
            _bowDecal.Hide();
            if (DisplayedTarget.Kind == ManualInteractionKind.None) return;
            var input = GetComponent<InputReader>();
            if (input != null && input.MoveAxis.sqrMagnitude > .001f) _facing = input.MoveAxis.normalized;
            var target = DisplayedTarget.Target;
            Color color = DisplayedTarget.Kind == ManualInteractionKind.Dialogue ? new Color(.6f,.9f,1f,.9f)
                : DisplayedTarget.Kind == ManualInteractionKind.Attack ? new Color(1f,.7f,.4f,.85f)
                : new Color(1f,.9f,.5f,.9f);
            if (target != null)
            {
                _targetRing.enabled = true;
                _targetRing.startColor = _targetRing.endColor = color;
                _targetRing.startWidth = _targetRing.endWidth = DisplayedTarget.Kind == ManualInteractionKind.Attack ? .07f : .045f;
                var collider = target.GetComponent<Collider2D>();
                Vector3 center = collider != null ? collider.bounds.center : target.transform.position;
                Vector2 size = collider != null ? (Vector2)collider.bounds.extents : new Vector2(.45f,.3f);
                size.x = Mathf.Clamp(size.x + .12f,.35f,1.6f);
                size.y = Mathf.Clamp(size.y + .12f,.25f,1.2f);
                for (int i=0;i<_targetRing.positionCount;i++)
                {
                    float angle = i * Mathf.PI * 2f / _targetRing.positionCount;
                    _targetRing.SetPosition(i,center + new Vector3(Mathf.Cos(angle)*size.x,Mathf.Sin(angle)*size.y));
                }
            }
            if (DisplayedTarget.Kind != ManualInteractionKind.Attack || _attack == null || _attack.Weapon == null) return;
            float range = _attack.Weapon.Range + (_attack.Progression != null ? _attack.Progression.BonusRange : 0f);
            DisplayedRange = range;
            if (_attack.Weapon.ProjectileSpeed > 0f)
            {
                _bowDecal.Show(transform.position, range, new Color(.55f,.8f,.9f,1f));
            }
            Vector2 direction = target != null ? ((Vector2)(target.transform.position-transform.position)).normalized : _facing;
            _aim.enabled = true;
            _aim.startColor = color;
            _aim.endColor = new Color(color.r,color.g,color.b,.65f);
            _aim.startWidth = .065f; _aim.endWidth = .025f;
            _aim.SetPosition(0,transform.position + (Vector3)direction * .25f);
            _aim.SetPosition(1,transform.position + (Vector3)direction * range);
        }

        private void OnDisable()
        {
            if (_targetRing != null) _targetRing.enabled = false;
            if (_aim != null) _aim.enabled = false;
            if (_bowDecal != null) _bowDecal.Hide();
        }
        private void OnDestroy()
        {
            if (_material != null) Destroy(_material);
            if (_bowDecal != null) Destroy(_bowDecal.gameObject);
        }
    }
}
