using UnityEngine;
using IL6.Events;

namespace IL6
{
    /// <summary>
    /// 단순 원형 시야 마스크. SpriteMask + 검정 오버레이를 카메라에 부착.
    /// 낮/저녁/새벽에는 비활성 (시야 무제한), 밤에만 활성.
    /// </summary>
    public sealed class VisionMask : MonoBehaviour
    {
        public Transform Target;
        public float RadiusUnits = 3.2f; // = 10 tiles * 32px / 100 PPU
        public Camera Cam;

        private SpriteRenderer _overlay;
        private SpriteMask _mask;

        private System.Action _unsubE, _unsubN, _unsubD, _unsubA;

        private void Awake()
        {
            if (Cam == null) Cam = Camera.main;
            BuildOverlay();
            BuildMask();
        }

        private void Start()
        {
            _unsubE = EventBus.Instance.Subscribe<EveningStartedPayload>(_ => SetActive(false));
            _unsubN = EventBus.Instance.Subscribe<NightStartedPayload>(_ => SetActive(true));
            _unsubD = EventBus.Instance.Subscribe<DawnStartedPayload>(_ => SetActive(false));
            _unsubA = EventBus.Instance.Subscribe<DayStartedPayload>(_ => SetActive(false));
            // 초기 상태 — 현재 페이즈에 맞춤 (보통 게임 시작 = Day → 비활성)
            var s = GameSession.Instance;
            SetActive(s != null && s.Cycle != null && s.Cycle.Phase == Phase.Night);
        }

        private void OnDestroy()
        {
            _unsubE?.Invoke(); _unsubN?.Invoke(); _unsubD?.Invoke(); _unsubA?.Invoke();
        }

        private void SetActive(bool on)
        {
            if (_overlay != null) _overlay.enabled = on;
            if (_mask != null) _mask.enabled = on;
        }

        private void BuildOverlay()
        {
            var go = new GameObject("VisionOverlay");
            go.transform.SetParent(transform);
            _overlay = go.AddComponent<SpriteRenderer>();
            _overlay.color = new Color(0, 0, 0, 0.85f);
            _overlay.sortingOrder = 1000;
            // 1x1 흰 텍스처
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _overlay.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            _overlay.drawMode = SpriteDrawMode.Sliced;
            _overlay.size = new Vector2(200, 200);
            _overlay.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        }

        private void BuildMask()
        {
            var go = new GameObject("VisionHole");
            go.transform.SetParent(transform);
            _mask = go.AddComponent<SpriteMask>();
            // 원 텍스처 만들기
            var tex = MakeCircleTexture(64);
            _mask.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f / (RadiusUnits * 2));
        }

        private static Texture2D MakeCircleTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float r = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - r, dy = y - r;
                    bool inside = dx * dx + dy * dy < r * r;
                    tex.SetPixel(x, y, inside ? Color.white : new Color(0, 0, 0, 0));
                }
            }
            tex.Apply();
            return tex;
        }

        private void LateUpdate()
        {
            if (Target == null || Cam == null) return;
            // 오버레이는 카메라 따라
            transform.position = new Vector3(Cam.transform.position.x, Cam.transform.position.y, 0);
            // 구멍은 타겟 (플레이어) 위에
            if (_mask != null) _mask.transform.position = new Vector3(Target.position.x, Target.position.y, 0);
        }

        public void SetRadius(float radiusUnits)
        {
            RadiusUnits = radiusUnits;
            if (_mask != null && _mask.sprite != null)
            {
                _mask.transform.localScale = new Vector3(radiusUnits / 3.2f, radiusUnits / 3.2f, 1);
            }
        }
    }
}
