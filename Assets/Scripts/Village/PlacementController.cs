using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 마우스로 건물 배치. start(kind) 호출 후 마우스 클릭으로 confirm.
    /// VillageGrid에 셀 점유 등록.
    /// 배치 모드 중 건물 영향 반경을 LineRenderer 링으로 시각화.
    /// </summary>
    public sealed class PlacementController : MonoBehaviour
    {
        public Camera MainCamera;
        public ResourceStore Store;
        public VillageGrid Grid;
        public GameObject CampfirePrefab;
        public GameObject BarricadePrefab;

        private BuildingKind? _currentKind;
        private SpriteRenderer _cursor;
        private LineRenderer _rangeRing;
        private static int _nextEid = 1;

        private const int RingSegments = 64;

        private void Awake()
        {
            if (MainCamera == null) MainCamera = Camera.main;
            EnsureCursor();
            EnsureRangeRing();
        }

        private void EnsureCursor()
        {
            if (_cursor != null) return;
            var go = new GameObject("PlacementCursor");
            _cursor = go.AddComponent<SpriteRenderer>();
            _cursor.color = new Color(0.29f, 0.56f, 0.89f, 0.4f);
            _cursor.sortingOrder = 50;
            _cursor.enabled = false;
        }

        private void EnsureRangeRing()
        {
            if (_rangeRing != null) return;
            var go = new GameObject("PlacementRangeRing");
            _rangeRing = go.AddComponent<LineRenderer>();
            _rangeRing.useWorldSpace = true;
            _rangeRing.loop = true;
            _rangeRing.positionCount = RingSegments;
            _rangeRing.startWidth = 0.05f;
            _rangeRing.endWidth = 0.05f;
            _rangeRing.sortingOrder = 55;
            var mat = new Material(Shader.Find("Hidden/Internal-Colored"));
            _rangeRing.material = mat;
            _rangeRing.enabled = false;
        }

        public void Begin(BuildingKind kind)
        {
            _currentKind = kind;
            _cursor.enabled = true;
            int w = (kind == BuildingKind.Campfire) ? 2 : 1;
            int h = w;
            _cursor.size = new Vector2(w * Grid.TileSize, h * Grid.TileSize);
            _cursor.drawMode = SpriteDrawMode.Sliced;
            if (_cursor.sprite == null)
            {
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                _cursor.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0, 0), 1f);
            }

            float radius = RangeRadius(kind);
            Color ringColor = RangeColor(kind);
            if (radius > 0f)
            {
                _rangeRing.startColor = ringColor;
                _rangeRing.endColor = ringColor;
                _rangeRing.enabled = true;
            }
            else
            {
                _rangeRing.enabled = false;
            }
        }

        public void Cancel()
        {
            _currentKind = null;
            if (_cursor != null) _cursor.enabled = false;
            if (_rangeRing != null) _rangeRing.enabled = false;
        }

        private void Update()
        {
            if (_currentKind == null) return;

            Vector2 worldPos = MainCamera.ScreenToWorldPoint(Input.mousePosition);
            var tile = Grid.WorldToTile(worldPos);
            int w = _currentKind == BuildingKind.Campfire ? 2 : 1;
            int h = w;

            // 커서: 타일 중심으로 스냅
            float cx = tile.x * Grid.TileSize + (w * Grid.TileSize) * 0.5f;
            float cy = tile.y * Grid.TileSize + (h * Grid.TileSize) * 0.5f;
            _cursor.transform.position = new Vector3(tile.x * Grid.TileSize, tile.y * Grid.TileSize, 0);

            // 범위 링 위치 업데이트
            if (_rangeRing.enabled)
                UpdateRingPositions(new Vector2(cx, cy), RangeRadius(_currentKind.Value));

            if (Input.GetMouseButtonDown(0))
            {
                TryConfirm(tile.x, tile.y, w, h, _currentKind.Value);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cancel();
            }
        }

        private void UpdateRingPositions(Vector2 center, float radius)
        {
            for (int i = 0; i < RingSegments; i++)
            {
                float angle = 2f * Mathf.PI * i / RingSegments;
                _rangeRing.SetPosition(i, new Vector3(
                    center.x + radius * Mathf.Cos(angle),
                    center.y + radius * Mathf.Sin(angle),
                    0f));
            }
        }

        private static float RangeRadius(BuildingKind kind) => kind switch
        {
            BuildingKind.Campfire => 2.5f,   // CampfireAura.Radius
            BuildingKind.Watchtower => 8f,   // Watchtower.Range
            _ => 0f
        };

        private static Color RangeColor(BuildingKind kind) => kind switch
        {
            BuildingKind.Campfire => new Color(1f, 0.7f, 0.2f, 0.8f),   // 황금색
            BuildingKind.Watchtower => new Color(0.4f, 0.8f, 1f, 0.8f), // 하늘색
            _ => Color.white
        };

        private void TryConfirm(int tx, int ty, int w, int h, BuildingKind kind)
        {
            if (Store == null || Grid == null) return;
            int cost = (kind == BuildingKind.Campfire)
                ? BalanceConfig.Instance.CampfireCost
                : BalanceConfig.Instance.BarricadeCost;
            if (Store.Get(ResourceKind.Wood) < cost) return;
            if (!Grid.IsFree(tx, ty, w, h)) return;

            Store.Spend(ResourceKind.Wood, cost);
            int eid = _nextEid++;

            var prefab = kind == BuildingKind.Campfire ? CampfirePrefab : BarricadePrefab;
            if (prefab == null) { Debug.LogWarning("Building prefab not assigned."); return; }

            var center = Grid.TileToWorld(tx, ty);
            var go = Instantiate(prefab, new Vector3(center.x + (w - 1) * Grid.TileSize / 2f, center.y + (h - 1) * Grid.TileSize / 2f, 0), Quaternion.identity);
            var b = go.GetComponent<Building>();
            if (b != null) { b.Kind = kind; b.Eid = eid; b.Grid = Grid; }
            Grid.Place(tx, ty, w, h, eid);
            Cancel();
        }
    }
}
