using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 게임 시작 시 마을 자리에 모닥불 + 울타리 링을 자동 스폰.
    /// SnowfieldController 가 Start 에서 1회 호출. 이미 마을이 있으면(저장 데이터로 복귀 등) 스킵.
    /// </summary>
    public static class VillageStarter
    {
        /// <summary>중심점 주변에 모닥불 + 8개 울타리 링 스폰. 이미 근처에 모닥불 있으면 스킵.</summary>
        public static void SpawnStarterVillage(Vector3 center, float fenceRadius = 4.5f, int fenceCount = 12)
        {
            // 이미 모닥불이 가까이 있으면 (씬 재진입 등) 추가 스폰 안 함
            var existing = Object.FindObjectsByType<Building>(FindObjectsSortMode.None);
            foreach (var b in existing)
            {
                if (b == null) continue;
                if (b.Kind != BuildingKind.Campfire) continue;
                if (Vector2.Distance(center, b.transform.position) < 6f) return;
            }

            SpawnCampfire(center);

            // 울타리 링
            for (int i = 0; i < fenceCount; i++)
            {
                float a = (i / (float)fenceCount) * Mathf.PI * 2f;
                Vector3 pos = center + new Vector3(Mathf.Cos(a) * fenceRadius, Mathf.Sin(a) * fenceRadius, 0f);
                float rotDeg = a * Mathf.Rad2Deg + 90f; // 링에 접하도록
                SpawnFence(pos, rotDeg);
            }
        }

        public static GameObject SpawnFence(Vector3 pos, float rotDeg)
        {
            var go = new GameObject("Fence");
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, 0, rotDeg);
            go.transform.localScale = new Vector3(0.9f, 0.18f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            var cf = go.AddComponent<ColorFallback>();
            cf.Tint = new Color(0.55f, 0.4f, 0.22f);
            cf.Shape = FallbackShape.Square;
            cf.Circle = false;
            cf.PixelSize = 32;
            cf.OutlineWidth = 2;
            cf.OutlineColor = new Color(0.2f, 0.13f, 0.05f, 1f);

            var b = go.AddComponent<Building>();
            b.Kind = BuildingKind.Fence;
            return go;
        }

        public static GameObject SpawnCampfire(Vector3 pos)
        {
            var go = new GameObject("Campfire");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.9f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;

            var cf = go.AddComponent<ColorFallback>();
            cf.Tint = new Color(1f, 0.5f, 0.1f);
            cf.Shape = FallbackShape.Rounded;
            cf.Circle = false;
            cf.PixelSize = 64;
            cf.OutlineWidth = 2;
            cf.OutlineColor = new Color(0.3f, 0.1f, 0f, 1f);

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;

            var aura = go.AddComponent<CampfireAura>();
            aura.Radius = 2.5f;
            aura.DamagePerSecond = 6f;
            aura.TickInterval = 0.5f;

            var b = go.AddComponent<Building>();
            b.Kind = BuildingKind.Campfire;

            var hp = go.AddComponent<HpBarUi>();
            hp.Building = b;
            hp.Offset = new Vector2(0f, 0.7f);
            hp.Size = new Vector2(1.0f, 0.12f);
            hp.BgColor = new Color(0.05f, 0.05f, 0.08f, 0.9f);
            hp.FillColor = new Color(1f, 0.55f, 0.2f);
            return go;
        }
    }
}
