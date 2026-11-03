using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 울타리 문. 콜라이더가 있어 좀비/동물은 막히지만,
    /// 플레이어 접근 시 자동으로 열고, 통과 후 멀어지면 닫는다.
    /// 열린 동안 플레이어/동료만 통과하며 적의 충돌은 유지한다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Door : MonoBehaviour
    {
        private static readonly System.Collections.Generic.List<Door> _all = new();
        public static System.Collections.Generic.IReadOnlyList<Door> All => _all;

        public static Door FindNearest(Vector2 from)
        {
            Door best = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < _all.Count; i++)
            {
                var d = _all[i];
                if (d == null) continue;

                float dist = d.DistanceTo(from);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = d;
                }
            }

            return best;
        }

        [SerializeField] private bool isVertical;
        [SerializeField] private bool isOpen;
        public bool IsOpen => isOpen;
        public float AutoOpenDistance = 2.2f;
        public float AutoCloseDistance = 3.2f;
        public float AutoCloseDelay = 0.8f;

        private Collider2D _self;
        private Collider2D _playerCol;
        private PlayerController _player;
        private float _closeTimer;
        private SpriteRenderer _sr;

        private readonly System.Collections.Generic.HashSet<Collider2D> _hooked = new();
        private float _refreshTimer;

        private void Awake()
        {
            YSort.Ensure(gameObject);
            _self = GetComponent<Collider2D>();
            _sr = GetComponent<SpriteRenderer>();

            _all.Add(this);
        }

        private void Start()
        {
            RefreshSprite();

            TryHookPlayer();
            HookAllCompanions();
        }

        private void Update()
        {
            if (_playerCol == null)
            {
                TryHookPlayer();
            }

            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = 0.5f;
                TryHookPlayer();
                HookAllCompanions();
            }
            UpdateAutomaticOpening();
        }

        private void UpdateAutomaticOpening()
        {
            if (Time.timeScale <= 0f) return;
            bool livingPlayer = _playerCol != null && _playerCol.gameObject.activeInHierarchy
                && (_player == null || !_player.IsDead);
            float distance = livingPlayer ? DistanceTo(_playerCol.bounds.center) : float.MaxValue;
            if (livingPlayer && distance <= AutoOpenDistance)
            {
                _closeTimer = 0f;
                if (!isOpen) SetOpen(true);
                return;
            }
            if (!isOpen) return;
            if (distance <= Mathf.Max(AutoOpenDistance, AutoCloseDistance) || AllyInDoorway())
            {
                _closeTimer = 0f;
                return;
            }
            _closeTimer += Time.deltaTime;
            if (_closeTimer >= AutoCloseDelay) SetOpen(false);
        }

        private bool AllyInDoorway()
        {
            foreach (var col in _hooked)
                if (col != null && col.enabled && col.gameObject.activeInHierarchy
                    && DistanceTo(col.bounds.center) <= 0.7f) return true;
            return false;
        }

        public void Init(bool vertical, bool open = false)
        {
            isVertical = vertical;
            isOpen = open;
            RefreshSprite();
        }

        // 플레이어 주변에 있는 문을 찾음
        public bool IsNear(Vector2 from, float distance)
        {
            return DistanceTo(from) <= distance;
        }

        private float DistanceTo(Vector2 from)
            => _self != null && _self.enabled ? Vector2.Distance(from, _self.ClosestPoint(from))
                : Vector2.Distance(from, transform.position);

        public void SetOpen(bool open)
        {
            isOpen = open;
            _closeTimer = 0f;
            RefreshSprite();
            TryHookPlayer();
            HookAllCompanions();
            ApplyCollisionStateToAll();
        }

        public void Toggle()
        {
            SetOpen(!isOpen);
        }

        private void RefreshSprite()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) return;

            Sprite sprite = null;

            if (isVertical)
            {
                sprite = isOpen
                    ? SpriteBank.VerticalGateOpen()
                    : SpriteBank.VerticalGateClosed();
            }
            else
            {
                sprite = isOpen
                    ? SpriteBank.GateOpen()
                    : SpriteBank.GateClosed();
            }

            if (sprite != null)
            {
                _sr.sprite = sprite;
                _sr.color = Color.white;
                FenceVisual.ApplyGate(_sr);
            }
        }

        private void TryHookPlayer()
        {
            var p = GameObject.FindWithTag("Player");

            if (p == null)
            {
                var pc = Object.FindFirstObjectByType<PlayerController>();
                if (pc != null) p = pc.gameObject;
            }

            if (p == null) return;

            _player = p.GetComponent<PlayerController>();

            var cols = p.GetComponentsInChildren<Collider2D>();

            foreach (var col in cols)
            {
                if (col == null) continue;

                _playerCol = col;
                Hook(col);
            }
        }

        private void HookAllCompanions()
        {
            var comps = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);

            foreach (var c in comps)
            {
                if (c == null) continue;

                var cols = c.GetComponentsInChildren<Collider2D>();

                foreach (var col in cols)
                {
                    if (col != null)
                    {
                        Hook(col);
                    }
                }
            }
        }

        private void Hook(Collider2D col)
        {
            if (col == null || _hooked.Contains(col)) return;

            _hooked.Add(col);
            ApplyCollisionState(col);
        }

        // 플레이어에게 문 상태 적용
        private void ApplyCollisionState(Collider2D col)
        {
            if (_self == null || col == null) return;

            // 열려 있을 때만 플레이어/동료와 충돌 무시
            // 닫혀 있으면 충돌 다시 활성화
            Physics2D.IgnoreCollision(_self, col, isOpen);
        }

        // 동료들에게도 문 상태 공유
        private void ApplyCollisionStateToAll()
        {
            foreach (var col in _hooked)
            {
                if (col != null)
                {
                    ApplyCollisionState(col);
                }
            }
        }

        private void OnDestroy()
        {
            _all.Remove(this);

            foreach (var col in _hooked)
            {
                if (_self != null && col != null)
                {
                    Physics2D.IgnoreCollision(_self, col, false);
                }
            }

            _hooked.Clear();
        }
    }
}
