using UnityEngine;
using UnityEngine.SceneManagement;

namespace IL6
{
    // A 2D ground decal: independent of the actor's SortingGroup and root scale.
    public sealed class GroundRangeDecal : MonoBehaviour
    {
        private static Sprite _disc;
        private SpriteRenderer _renderer;
        public float Radius { get; private set; }
        public bool Visible => _renderer != null && _renderer.enabled;
        public SpriteRenderer Renderer => _renderer;

        public static GroundRangeDecal Create(GameObject owner, string name)
        {
            var root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, owner.scene);
            return root.AddComponent<GroundRangeDecal>();
        }

        private void Awake()
        {
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = Disc();
            // Ground patches use -9500; Y-sorted actors/buildings are clamped to >= -9000.
            _renderer.sortingOrder = -9400;
            _renderer.enabled = false;
        }

        public void Show(Vector3 center, float radius, Color tint)
        {
            Radius = Mathf.Max(0, radius);
            transform.position = center;
            transform.localScale = Vector3.one * (Radius * 2f);
            _renderer.color = tint;
            _renderer.enabled = Radius > 0;
        }

        public void Hide() { if (_renderer != null) _renderer.enabled = false; }
        private void OnDisable() => Hide();

        private static Sprite Disc()
        {
            if (_disc != null) return _disc;
            const int size = 512;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { name = "Ground range decal", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float radius = new Vector2((x + .5f) / size * 2f - 1f, (y + .5f) / size * 2f - 1f).magnitude;
                    if (radius > 1f) continue;
                    // Quiet translucent fill, soft outer band and a dark rim for snowy ground.
                    float rim = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.976f, .99f, radius));
                    float fade = 1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.992f, 1f, radius));
                    float alpha = Mathf.Lerp(.045f, .095f, radius * radius) + rim * .32f;
                    Color color = Color.Lerp(Color.white, new Color(.13f, .20f, .23f), rim);
                    color.a = alpha * fade;
                    pixels[y * size + x] = color;
                }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            _disc = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * .5f, size, 0, SpriteMeshType.FullRect);
            _disc.name = "Ground range decal";
            return _disc;
        }
    }
}
