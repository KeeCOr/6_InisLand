using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 울타리 문. 콜라이더가 있어 좀비/동물은 막히지만,
    /// 플레이어/동료 콜라이더와는 충돌 무시.
    /// Open/Close는 우선 스프라이트 표현용으로만 사용.
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

                float dist = Vector2.Distance(from, d.transform.position);
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

        private Collider2D _self;
        private Collider2D _playerCol;
        private SpriteRenderer _sr;

        private readonly System.Collections.Generic.HashSet<Collider2D> _hooked = new();
        private float _refreshTimer;

        private void Awake()
        {
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
                HookAllCompanions();
            }
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
            return Vector2.Distance(from, transform.position) <= distance;
        }

        public void SetOpen(bool open)
        {
            isOpen = open;
            RefreshSprite();

            ApplyCollisionStateToAll();
        }

        public void Toggle()
        {
            SetOpen(!isOpen);
        }

        private void RefreshSprite()
        {
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