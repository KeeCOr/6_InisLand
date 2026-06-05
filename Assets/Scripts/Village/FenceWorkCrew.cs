using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    public sealed class FenceWorkCrew : MonoBehaviour
    {
        public enum JobKind { Repair, Rebuild }

        public static bool IsActive => _active != null;

        private static FenceWorkCrew _active;

        private readonly List<Companion> _workers = new();
        private JobKind _kind;
        private Vector3 _center;
        private int _healAmount;
        private float _remaining;

        public static int Begin(JobKind kind, Vector3 center, int healAmount, float durationSec = 7f, int maxWorkers = 3)
        {
            if (_active != null) return 0;

            var candidates = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
            var picked = new List<Companion>();
            foreach (var c in candidates)
            {
                if (c == null || c.IsDead) continue;
                if (c.CurrentMode != Companion.Mode.Follow) continue;
                if (Vector2.Distance(c.transform.position, center) > VillageStarter.CurrentHalfSize + 8f) continue;
                picked.Add(c);
                if (picked.Count >= maxWorkers) break;
            }

            if (picked.Count == 0) return 0;

            var go = new GameObject("FenceWorkCrew");
            go.transform.position = center + new Vector3(0f, -VillageStarter.CurrentHalfSize + 1.2f, 0f);
            var crew = go.AddComponent<FenceWorkCrew>();
            crew._kind = kind;
            crew._center = center;
            crew._healAmount = healAmount;
            crew._remaining = Mathf.Max(1f, durationSec);
            crew._workers.AddRange(picked);

            for (int i = 0; i < picked.Count; i++)
            {
                var marker = new GameObject($"FenceWorkSpot_{i + 1}");
                marker.transform.SetParent(go.transform, false);
                marker.transform.localPosition = new Vector3((i - (picked.Count - 1) * 0.5f) * 0.55f, 0f, 0f);
                picked[i].AssignFarm(marker.transform);
            }

            _active = crew;
            return picked.Count;
        }

        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining > 0f) return;

            Complete();
        }

        private void Complete()
        {
            int count = _kind == JobKind.Repair
                ? VillageStarter.RepairAllFences(_healAmount)
                : VillageStarter.RebuildMissingOuterFences(_center);

            string label = _kind == JobKind.Repair ? $"Fences repaired x{count}" : $"Fences rebuilt x{count}";
            GameFeel.FloatText(_center, label, new Color(0.75f, 1f, 0.75f));
            Sfx.Build();
            ReleaseWorkers();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            ReleaseWorkers();
            if (_active == this) _active = null;
        }

        private void ReleaseWorkers()
        {
            foreach (var c in _workers)
            {
                if (c == null || c.IsDead) continue;
                if (c.CurrentMode == Companion.Mode.Farming) c.ReleaseFarm();
            }
            _workers.Clear();
        }
    }
}
