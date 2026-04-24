using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 플레이어 주변 청크(그리드 셀)를 스캔해 확률적으로 나무/사슴을 런타임 생성.
    /// 외부 prefab 없이 SpriteRenderer + ColorFallback + Gatherable (+ DeerAi) 를 코드로 조립.
    /// 멀어진 청크는 해당 콘텐츠 제거.
    /// 초기 스타터 존(3x3 청크) 은 스폰 스킵 — 씬에 배치된 오브젝트와 겹치지 않게.
    /// </summary>
    public sealed class ProceduralSpawner : MonoBehaviour
    {
        [Header("Grid")]
        public Transform Player;
        public int ChunkSize = 10;
        public int LoadRadius = 2;
        public int UnloadRadius = 4;

        [Header("Spawn probability (per slot)")]
        [Range(0, 1)] public float TreeChance = 0.5f;
        [Range(0, 1)] public float RockChance = 0.15f;
        [Range(0, 1)] public float DeerChance = 0.08f;
        public int SlotsPerChunk = 6;

        [Header("Starter zone to skip (centered at chunk (Cx, Cy))")]
        public int StarterCx = 1;
        public int StarterCy = 1;
        public int StarterRadius = 1;

        [Header("Deterministic")]
        public uint Seed = 20260425u;

        private class ChunkData { public List<GameObject> Spawned = new(); }
        private readonly Dictionary<(int, int), ChunkData> _loaded = new();

        private void Start()
        {
            if (Player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) Player = p.transform;
            }
        }

        private void Update()
        {
            if (Player == null) return;
            int pcx = Mathf.FloorToInt(Player.position.x / ChunkSize);
            int pcy = Mathf.FloorToInt(Player.position.y / ChunkSize);

            EnsureLoaded(pcx, pcy);
            UnloadFar(pcx, pcy);
        }

        private void EnsureLoaded(int pcx, int pcy)
        {
            for (int dy = -LoadRadius; dy <= LoadRadius; dy++)
            {
                for (int dx = -LoadRadius; dx <= LoadRadius; dx++)
                {
                    var key = (pcx + dx, pcy + dy);
                    if (!_loaded.ContainsKey(key)) LoadChunk(key);
                }
            }
        }

        private void UnloadFar(int pcx, int pcy)
        {
            var toRemove = new List<(int, int)>();
            foreach (var kv in _loaded)
            {
                int ddx = Mathf.Abs(kv.Key.Item1 - pcx);
                int ddy = Mathf.Abs(kv.Key.Item2 - pcy);
                if (Mathf.Max(ddx, ddy) > UnloadRadius)
                {
                    foreach (var go in kv.Value.Spawned) if (go != null) Destroy(go);
                    toRemove.Add(kv.Key);
                }
            }
            foreach (var k in toRemove) _loaded.Remove(k);
        }

        private void LoadChunk((int, int) key)
        {
            var data = new ChunkData();
            _loaded[key] = data;

            if (IsInStarterZone(key.Item1, key.Item2)) return;

            uint seed = Seed ^ unchecked((uint)((key.Item1 * 73856093) ^ (key.Item2 * 19349663)));
            var rng = new SeededRng(seed);

            float baseX = key.Item1 * ChunkSize;
            float baseY = key.Item2 * ChunkSize;

            for (int i = 0; i < SlotsPerChunk; i++)
            {
                float x = baseX + rng.Next() * ChunkSize;
                float y = baseY + rng.Next() * ChunkSize;
                float roll = rng.Next();
                float cumTree = TreeChance;
                float cumRock = cumTree + RockChance;
                float cumDeer = cumRock + DeerChance;
                if (roll < cumTree) data.Spawned.Add(CreateTree(x, y));
                else if (roll < cumRock) data.Spawned.Add(CreateRock(x, y));
                else if (roll < cumDeer) data.Spawned.Add(CreateDeer(x, y));
            }
        }

        private static GameObject CreateRock(float x, float y)
        {
            var go = new GameObject("Rock_proc");
            go.transform.position = new Vector3(x, y, 0);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;

            var gat = go.AddComponent<Gatherable>();
            gat.YieldKind = ResourceKind.Stone;
            gat.YieldAmount = 2;
            gat.DurationSec = 5f;
            gat.DestroyOnGather = true;

            var cf = go.AddComponent<ColorFallback>();
            cf.Tint = new Color(0.55f, 0.55f, 0.6f);
            cf.Shape = FallbackShape.Rounded;
            cf.Circle = false;
            cf.PixelSize = 64;
            cf.OutlineWidth = 2;
            cf.OutlineColor = new Color(0.2f, 0.2f, 0.25f, 1f);
            return go;
        }

        private bool IsInStarterZone(int cx, int cy)
        {
            return Mathf.Abs(cx - StarterCx) <= StarterRadius
                && Mathf.Abs(cy - StarterCy) <= StarterRadius;
        }

        private static GameObject CreateTree(float x, float y)
        {
            var go = new GameObject("Tree_proc");
            go.transform.position = new Vector3(x, y, 0);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            var gat = go.AddComponent<Gatherable>();
            gat.YieldKind = ResourceKind.Wood;
            gat.YieldAmount = 3;
            gat.DurationSec = 4f;
            gat.DestroyOnGather = true;

            var cf = go.AddComponent<ColorFallback>();
            cf.Tint = new Color(0.2f, 0.55f, 0.25f);
            cf.Shape = FallbackShape.Triangle;
            cf.Circle = true; // (Shape overrides to Triangle)
            cf.PixelSize = 64;
            cf.OutlineWidth = 2;
            cf.OutlineColor = new Color(0.05f, 0.15f, 0.05f, 1f);
            return go;
        }

        private static GameObject CreateDeer(float x, float y)
        {
            var go = new GameObject("Deer_proc");
            go.transform.position = new Vector3(x, y, 0);
            go.tag = "Untagged";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 8;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            var gat = go.AddComponent<Gatherable>();
            gat.YieldKind = ResourceKind.Meat;
            gat.YieldAmount = 2;
            gat.DurationSec = 2f;
            gat.DestroyOnGather = true;

            var ai = go.AddComponent<DeerAi>();
            ai.FleeRadius = 3.5f;
            ai.FleeSpeed = 3f;

            var cf = go.AddComponent<ColorFallback>();
            cf.Tint = new Color(0.55f, 0.4f, 0.25f);
            cf.Shape = FallbackShape.Circle;
            cf.Circle = true;
            cf.PixelSize = 64;
            cf.OutlineWidth = 2;
            cf.OutlineColor = new Color(0.2f, 0.12f, 0.05f, 1f);
            return go;
        }
    }
}
