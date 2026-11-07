using System;
using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 건물 짓는 중 임시 마커. 시간(초) 누적되면 실제 건물로 교체.
    /// 매 프레임 반경 안 동료 수에 비례해 빠르게 진행 (단독 약 8초, 동료 2명 도우면 ~3초).
    /// </summary>
    public sealed class ConstructionSite : MonoBehaviour
    {
        public BuildingKind Kind;
        public float TotalTime = 8f;       // 단독 작업 시 완료까지 걸리는 초
        public float Progress { get; private set; }
        public static void RestoreSave(SavedConstruction saved)
        {
            var go=new GameObject("ConstructionSite");go.transform.position=saved.position;go.transform.localScale=ConstructionPlacement.Scale(saved.kind);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=ConstructionPlacement.Sprite(saved.kind);sr.color=new Color(1,1,1,.5f);
            var site=go.AddComponent<ConstructionSite>();site.Kind=saved.kind;site.TotalTime=saved.total;site.Progress=saved.progress;
            site.OnComplete=(kind,pos)=>
            {
                WorldPersistence.SpawnCompleted(kind,pos);
                var store=GameSession.Instance?.Resources;
                if(kind==BuildingKind.Storage)store?.IncreaseCap(50);
                if(kind==BuildingKind.FoodStorage)store?.IncreaseCap(ResourceKind.Food,BuildingUpgradeRules.FoodStorageCapPerLevel);
            };
        }
        public float WorkerRadius = 2.5f;
        public float WorkerSpeedFactor = 1.0f; // 동료 1명당 속도 +1.0배 (즉 100% 가산)
        /// <summary>완료 시 호출 — SimpleHud 가 실제 건물 스폰 콜백 등록.</summary>
        public Action<BuildingKind, Vector3> OnComplete;

        private SpriteRenderer _sr;

        private void Awake()
        {
            YSort.Ensure(gameObject);
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            int workers = CountWorkersNearby();
            float dt = Time.deltaTime * (1f + workers * WorkerSpeedFactor);
            Progress += dt;

            // 진행도 시각 — 알파 점점 진해짐
            if (_sr != null)
            {
                Color c = _sr.color;
                c.a = Mathf.Lerp(0.45f, 1f, Progress / TotalTime);
                _sr.color = c;
            }

            if (Progress >= TotalTime)
            {
                Vector3 pos = transform.position;
                BuildingKind k = Kind;
                Action<BuildingKind, Vector3> cb = OnComplete;
                Destroy(gameObject);
                cb?.Invoke(k, pos);
            }
        }

        private int CountWorkersNearby()
        {
            int n = 0;
            var comps = UnityEngine.Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
            foreach (var c in comps)
            {
                if (c == null || c.IsDead) continue;
                // 채집/농사 중이면 다른 일 — 건설 공헌 X
                if (c.CurrentMode == Companion.Mode.Hiding) continue;
                if (c.CurrentMode == Companion.Mode.Working) continue;
                if (c.CurrentMode == Companion.Mode.Farming) continue;
                if (Vector2.Distance(transform.position, c.transform.position) <= WorkerRadius) n++;
            }
            // 플레이어 본인도 가까이 있으면 1명 카운트
            var p = GameObject.FindWithTag("Player");
            if (p != null && Vector2.Distance(transform.position, p.transform.position) <= WorkerRadius) n++;
            return n;
        }
    }
}
