using UnityEngine;
namespace IL6
{
    /// <summary>Shared world-grid, preview and runtime building dimensions.</summary>
    public static class ConstructionPlacement
    {
        public const float CellSize = 1f;
        public static string BlockedReason(bool spaceAvailable, bool farmAllowed, bool canPay)
        {
            if (!spaceAvailable) return "건물과 겹침 · 빈 격자로 이동";
            if (!farmAllowed) return "농장 한도 · 씨앗 창고 확장 필요";
            if (!canPay) return "자원 부족";
            return string.Empty;
        }
        public static Vector3 Snap(Vector3 position) => new Vector3(
            Mathf.Round(position.x / CellSize) * CellSize,
            Mathf.Round(position.y / CellSize) * CellSize, 0f);

        public static Vector3 Scale(BuildingKind kind)
        {
            // Per-building sizes; use a uniform scale to preserve the painted proportions.
            float size = kind switch
            {
                BuildingKind.Barricade => 1.56f,
                BuildingKind.House => 3.2f,
                BuildingKind.Storage => 2.4f,
                BuildingKind.Farm => 2.6f,
                BuildingKind.Watchtower => 1.8f,
                BuildingKind.Infirmary => 2.8f,
                BuildingKind.HuntersHut => 2.4f,
                BuildingKind.SeedStorage => 1.8f,
                BuildingKind.Carpenter => 2.2f,
                BuildingKind.Brazier => 1.3f,
                BuildingKind.Blacksmith => 2.7f,
                BuildingKind.TrainingCamp => 2.5f,
                BuildingKind.FoodStorage => 2.6f,
                BuildingKind.LookoutPost => 1.5f,
                BuildingKind.Sawmill => 2.8f,
                BuildingKind.Church => 3.3f,
                BuildingKind.Campfire => 1.17f,
                BuildingKind.Fence => 1.0f,
                _ => 1f
            };
            return new Vector3(size,size,1f);
        }

        public static Sprite Sprite(BuildingKind kind) => kind switch
        {
            BuildingKind.Barricade => SpriteBank.WoodBarricade(),
            BuildingKind.House => SpriteBank.Cabin(),
            BuildingKind.Storage => SpriteBank.Logs(),
            BuildingKind.Farm => SpriteBank.Farm(),
            BuildingKind.Watchtower => SpriteBank.Watchtower(),
            BuildingKind.Infirmary => SpriteBank.Infirmary(),
            BuildingKind.HuntersHut => SpriteBank.HuntersHut(),
            BuildingKind.SeedStorage => SpriteBank.Facility(BuildingKind.SeedStorage),
            BuildingKind.Carpenter => SpriteBank.Facility(BuildingKind.Carpenter),
            BuildingKind.Brazier => SpriteBank.Facility(BuildingKind.Brazier),
            BuildingKind.Blacksmith => SpriteBank.Facility(BuildingKind.Blacksmith),
            BuildingKind.TrainingCamp => SpriteBank.Facility(BuildingKind.TrainingCamp),
            BuildingKind.FoodStorage => SpriteBank.Facility(BuildingKind.FoodStorage),
            BuildingKind.LookoutPost => SpriteBank.Facility(BuildingKind.LookoutPost),
            BuildingKind.Sawmill => SpriteBank.Facility(BuildingKind.Sawmill),
            BuildingKind.Church => SpriteBank.Facility(BuildingKind.Church),
            BuildingKind.Campfire => SpriteBank.Campfire(),
            BuildingKind.Fence => SpriteBank.WoodenFenceCenter(),
            _ => null
        };
        public static Rect Footprint(BuildingKind kind, Vector3 position)
        {
            Vector3 scale = Scale(kind);
            return new Rect(position.x-scale.x/2f, position.y-scale.y/2f, scale.x, scale.y);
        }
        public static bool IsAvailable(BuildingKind kind, Vector3 snappedPosition)
        {
            Rect target = Footprint(kind, snappedPosition);
            // A small inset allows footprints to touch at their edges.
            target.xMin += .01f; target.xMax -= .01f;
            target.yMin += .01f; target.yMax -= .01f;
            foreach (var building in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
            {
                if (building.CurrentHp <= 0) continue;
                Vector3 p=building.transform.position, size=building.transform.lossyScale;
                if (target.Overlaps(new Rect(p.x-Mathf.Abs(size.x)/2f,p.y-Mathf.Abs(size.y)/2f,Mathf.Abs(size.x),Mathf.Abs(size.y)))) return false;
            }
            foreach (var site in Object.FindObjectsByType<ConstructionSite>(FindObjectsSortMode.None))
                if (target.Overlaps(Footprint(site.Kind, site.transform.position))) return false;
            return true;
        }
    }
}
