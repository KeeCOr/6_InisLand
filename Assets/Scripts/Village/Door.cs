using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 울타리 문. 콜라이더가 있어 좀비/동물은 막히지만, 플레이어/동료는 통과 가능.
    /// 자기 위치에 변경된 플레이어 콜라이더가 들어오는 경우(씬 재로드 등) 도 처리.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Door : MonoBehaviour
    {
        // 정적 레지스트리 — 동료 길 찾기에서 가장 가까운 문을 빠르게 찾기 위함.
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
                if (dist < bestDist) { bestDist = dist; best = d; }
            }
            return best;
        }

        private Collider2D _self;
        public float FriendlyPassageRadius = 1.45f;
        private float _refreshTimer;

        private void Awake()
        {
            _self = GetComponent<Collider2D>();
            _all.Add(this);
        }

        private void Start()
        {
            TryHookPlayer();
            HookAllCompanions();
        }

        private void Update()
        {
            // 새로 영입된 동료가 있을 수 있으니 0.5s 간격으로 갱신
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = 0.5f;
                TryHookPlayer();
                HookAllCompanions();
            }
        }

        private void TryHookPlayer()
        {
            // 태그로 먼저 찾고, 없으면 컴포넌트로 폴백
            var p = GameObject.FindWithTag("Player");
            if (p == null)
            {
                var pc = Object.FindFirstObjectByType<PlayerController>();
                if (pc != null) p = pc.gameObject;
            }
            if (p == null) return;
            var cols = p.GetComponentsInChildren<Collider2D>();
            foreach (var col in cols)
                Hook(col);
        }

        private void HookAllCompanions()
        {
            var comps = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
            foreach (var c in comps)
            {
                if (c == null) continue;
                var cols = c.GetComponentsInChildren<Collider2D>();
                foreach (var col in cols)
                    Hook(col);
            }
        }

        private void Hook(Collider2D col)
        {
            if (col == null) return;
            Physics2D.IgnoreCollision(_self, col, true);

            // 플레이어 스케일이 커지면 문 자체는 통과해도 양옆 울타리에 걸릴 수 있다.
            // 문 주변 울타리도 우호 유닛에게만 열어 둬서 실제 통로 폭을 확보한다.
            var nearby = Physics2D.OverlapCircleAll(transform.position, FriendlyPassageRadius);
            foreach (var blocker in nearby)
            {
                if (blocker == null || blocker == col || blocker.isTrigger) continue;
                if (blocker == _self || IsFence(blocker))
                    Physics2D.IgnoreCollision(blocker, col, true);
            }
        }

        private static bool IsFence(Collider2D col)
        {
            var building = col.GetComponent<Building>();
            return building != null && building.Kind == BuildingKind.Fence;
        }

        private void OnDestroy()
        {
            _all.Remove(this);
            // Physics2D.IgnoreCollision 은 대상 콜라이더가 사라지면 자동 해제됨 — 별도 정리 불필요
        }
    }
}
