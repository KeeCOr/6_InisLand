using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 런타임에 눈 ParticleSystem 생성. 에셋 없이 작동.
    /// Camera 자식에 부착하면 카메라 따라 움직이며 상시 내리는 눈.
    /// </summary>
    public sealed class SnowEmitter : MonoBehaviour
    {
        public int Rate = 34;
        public int BlizzardRate = 150;
        public float AreaWidth = 32f;
        public float TopOffset = 10f;
        public float FallSpeed = 1.2f;
        public float BlizzardFallSpeed = 3.8f;
        public float Lifetime = 10f;
        public float ParticleSize = 0.12f;
        public float BlizzardParticleSize = 0.16f;
        public float DriftSpeed = 0.35f;
        public float BlizzardDriftSpeed = 3.4f;

        private ParticleSystem _particles;
        private ParticleSystem.EmissionModule _emission;
        private ParticleSystem.MainModule _main;
        private ParticleSystem.ShapeModule _shape;
        private ParticleSystem.VelocityOverLifetimeModule _velocity;
        private ParticleSystem.NoiseModule _noise;
        private Camera _camera;
        private NightController _night;
        private float _stormBlend;

        private void Awake()
        {
            var go = new GameObject("SnowParticles");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, TopOffset, 0);

            _camera = GetComponent<Camera>();
            _night = Object.FindFirstObjectByType<NightController>();
            _particles = go.AddComponent<ParticleSystem>();
            _main = _particles.main;
            _main.startLifetime = Lifetime;
            _main.startSpeed = 0f;
            _main.startSize = ParticleSize;
            _main.startColor = new Color(1f, 1f, 1f, 0.85f);
            _main.gravityModifier = 0f;
            _main.loop = true;
            _main.simulationSpace = ParticleSystemSimulationSpace.World;
            _main.maxParticles = 900;

            _emission = _particles.emission;
            _emission.rateOverTime = Rate;

            _shape = _particles.shape;
            _shape.shapeType = ParticleSystemShapeType.Box;
            _shape.scale = new Vector3(AreaWidth, 0.1f, 0f);

            _velocity = _particles.velocityOverLifetime;
            _velocity.enabled = true;
            _velocity.space = ParticleSystemSimulationSpace.World;
            // Unity 요구사항: x/y/z 가 같은 모드여야 함 — 둘 다 TwoConstants 로 통일
            _velocity.y = new ParticleSystem.MinMaxCurve(-FallSpeed * 1.1f, -FallSpeed * 0.9f);
            _velocity.x = new ParticleSystem.MinMaxCurve(-DriftSpeed, DriftSpeed);
            _velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            _noise = _particles.noise;
            _noise.enabled = true;
            _noise.strength = 0.14f;
            _noise.frequency = 0.45f;
            _noise.scrollSpeed = 0.18f;

            var sizeOverLife = _particles.sizeOverLifetime;
            sizeOverLife.enabled = true;
            var curve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.1f, 1f),
                new Keyframe(1f, 0.8f));
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 15;

            // 런타임 머티리얼: Built-in Sprites/Default → URP 대체 → 최후 Unlit
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            var mat = new Material(shader);
            var tex = MakeSnowflakeTexture(16);
            mat.mainTexture = tex;
            renderer.sharedMaterial = mat;
        }

        private void LateUpdate()
        {
            if (_particles == null) return;

            bool blizzard = false;
            if (_night == null) _night = Object.FindFirstObjectByType<NightController>();
            if (_night != null)
                blizzard = _night.CurrentPhase == Phase.Night && _night.IsBlizzard;

            float targetBlend = blizzard ? 1f : 0f;
            _stormBlend = Mathf.MoveTowards(_stormBlend, targetBlend, Time.deltaTime * (blizzard ? 1.8f : 1.2f));

            float rate = Mathf.Lerp(Rate, BlizzardRate, _stormBlend);
            float fall = Mathf.Lerp(FallSpeed, BlizzardFallSpeed, _stormBlend);
            float drift = Mathf.Lerp(DriftSpeed, BlizzardDriftSpeed, _stormBlend);
            float size = Mathf.Lerp(ParticleSize, BlizzardParticleSize, _stormBlend);
            float alpha = Mathf.Lerp(0.78f, 0.94f, _stormBlend);

            _emission.rateOverTime = rate;
            _main.startSize = size;
            _main.startColor = new Color(1f, 1f, 1f, alpha);
            _main.maxParticles = Mathf.CeilToInt(Mathf.Lerp(650f, 1800f, _stormBlend));
            _velocity.y = new ParticleSystem.MinMaxCurve(-fall * 1.18f, -fall * 0.82f);
            _velocity.x = new ParticleSystem.MinMaxCurve(-drift * 1.15f, drift * 0.35f);
            _noise.strength = Mathf.Lerp(0.14f, 1.1f, _stormBlend);
            _noise.frequency = Mathf.Lerp(0.45f, 1.35f, _stormBlend);
            _noise.scrollSpeed = Mathf.Lerp(0.18f, 1.6f, _stormBlend);

            float width = AreaWidth;
            if (_camera != null && _camera.orthographic)
                width = Mathf.Max(AreaWidth, _camera.orthographicSize * _camera.aspect * 2.6f);
            _shape.scale = new Vector3(width, 0.1f, 0f);
        }

        private static Texture2D MakeSnowflakeTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
            };
            float r = size / 2f;
            var transparent = new Color(0, 0, 0, 0);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - r, dy = y - r;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(1f - d / r);
                    alpha = alpha * alpha; // soften edges
                    tex.SetPixel(x, y, alpha > 0.01f ? new Color(1f, 1f, 1f, alpha) : transparent);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
