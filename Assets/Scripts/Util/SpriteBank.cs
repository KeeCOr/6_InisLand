using UnityEngine;

namespace IL6
{
    public static class SpriteBank
    {
        // single sprite PNG 로드. 실패 시 null 반환 (폴백 없음).
        public static Sprite Load(string path)
        {
            var s = Resources.Load<Sprite>($"Sprites/{path}");
            if (s == null) Debug.LogWarning($"[SpriteBank] Missing sprite: Sprites/{path}");
            return s;
        }

        // multisprite PNG 에서 이름으로 서브 스프라이트 로드. 실패 시 null 반환 (폴백 없음).
        public static Sprite LoadSubSprite(string path, string spriteName)
        {
            var sprites = Resources.LoadAll<Sprite>($"Sprites/{path}");

            foreach (var s in sprites)
            {
                if (s != null && s.name == spriteName)
                    return s;
            }

            Debug.LogWarning($"[SpriteBank] 서브 스프라이트 없음: Sprites/{path}/{spriteName}");
            return null;
        }

        // ── 동물 ─────────────────────────────────────────────────────────────────
        public static Sprite Deer()         => Load("Animals/deer");
        public static Sprite Wolf()         => Load("Animals/wolf");
        public static Sprite Rabbit()       => Load("Animals/rabbit");
        public static Sprite Boar()         => Load("Animals/boar");
        public static Sprite Mammoth()      => Load("Animals/mammoth");
        public static Sprite Bear()         => Load("Animals/bear");
        public static Sprite DogPet()       => Load("Pets/dog");
        public static Sprite HawkPet()      => Load("Pets/hawk");

        // ── 적 ──────────────────────────────────────────────────────────────────
        public static Sprite Zombie()       => Load("Enemies/zombie");
        public static Sprite QuadrupedRobot() => Load("Enemies/quadruped_robot");
        public static Sprite BossFrostZombie()  => Load("Enemies/boss_frost_zombie");
        public static Sprite BossWinterKnight() => Load("Enemies/boss_winter_knight");
        public static Sprite BossIronGiant()    => Load("Enemies/boss_iron_giant");
        public static Sprite BossFrostLich()    => Load("Enemies/boss_frost_lich");

        // ── 동료 ─────────────────────────────────────────────────────────────────
        public static Sprite CompanionUncle()  => Load("Companions/uncle");
        public static Sprite CompanionAunt()   => Load("Companions/aunt");
        public static Sprite CompanionChild()  => Load("Companions/child");

        // ── 플레이어 ──────────────────────────────────────────────────────────────
        public static Sprite PlayerIdle(int index = 0) => LoadSubSprite("Player/RedHairGirl/axe", $"redhair_axe_{((index % 4) + 4) % 4}");

        // ── 역할명 → 동료 스프라이트 ─────────────────────────────────────────────
        public static Sprite CompanionByRole(string role) => CompanionSpriteForRole(role);

        public static Sprite CompanionSpriteForRole(string role)
        {
            if (IsChildRole(role)) return CompanionChild();
            string dedicated = (role ?? "").ToLowerInvariant() switch
            {
                "hunter" or "사냥꾼" => "hunter",
                "warrior" or "전사" => "warrior",
                "farmer" or "농부" => "farmer",
                "cook" or "요리사" => "cook",
                "healer" or "치유사" => "healer",
                _ => null,
            };
            if (dedicated != null) return FrontStatic("Companions/" + dedicated);
            if (IsFemaleRole(role)) return CompanionAunt();
            return CompanionUncle();
        }

        private static Sprite FrontStatic(string path) => Load("GeneratedStatic20261014/" + path);

        public static Vector3 CompanionScaleForRole(string role)
        {
            // Keep actor/collider scale stable; the 2026-10-26 art importer normalizes visual height.
            float scale = (role??"").ToLowerInvariant() switch
            {
                "hunter" or "사냥꾼" => 0.771241867f,
                "warrior" or "전사" => 0.805882400f,
                "farmer" or "농부" => 0.786274540f,
                "cook" or "요리사" => 0.756302552f,
                "healer" or "치유사" => 0.831785368f,
                "aunt" => 0.877995667f,
                "uncle" => 0.742362084f,
                "child" => 0.750980400f,
                _ => IsChildRole(role)?0.750980400f:IsFemaleRole(role)?0.877995667f:0.742362084f
            };
            return Vector3.one * scale;
        }

        public static bool IsChildRole(string role)
        {
            string r = role ?? "";
            return r.Contains("Child") || r.Contains("child") || r.Contains("\uC544\uC774") || r.Contains("?꾩씠");
        }

        public static bool IsFemaleRole(string role)
        {
            string r = role ?? "";
            return r.Contains("Aunt") || r.Contains("aunt")
                || r.Contains("Farmer") || r.Contains("Elder") || r.Contains("Cook")
                || r.Contains("\uB18D") || r.Contains("\uB178\uC778")
                || r.Contains("?띾?") || r.Contains("?몄씤");
        }

        // ── 프랍 (나무 / 바위 / 장식물) ──────────────────────────────────────────
        public static Sprite SnowPineTree()       => Load("Props/NordicTrees20261102/spruce");
        public static Sprite BareTree()       => Load("Props/bare_tree");
        public static Sprite SnowRock()      => Load("Props/snowRock");
        public static Sprite SmallRocks()     => Load("Props/small_rocks");
        public static Sprite Stump()          => Load("Props/stump");
        public static Sprite SnowBush()       => Load("Props/snow_bush");
        public static Sprite Logs()           => FrontStatic("Buildings/storage");

        // ── 건물 ─────────────────────────────────────────────────────────────────
        public static Sprite Campfire()       => LoadSubSprite("Props/BonFire/bonfire", "bonFire_original");
        public static Sprite Cabin()          => FrontStatic("Buildings/cabin");
        public static Sprite Watchtower()     => FrontStatic("Buildings/watchtower");
        public static Sprite Farm()           => FrontStatic("Buildings/farm");
        public static Sprite Infirmary()      => FrontStatic("Buildings/infirmary");
        public static Sprite HuntersHut()     => FrontStatic("Buildings/hunters_hut");
        public static Sprite FenceVertical()  => Load("Props/fence_vertical");
        public static Sprite SnowFenceH()     => Load("Props/snow_fence_h");
        public static Sprite WoodBarricade()  => FrontStatic("Buildings/wood_barricade");
        public static Sprite StoneWall()      => Load("Props/stone_wall");
        public static Sprite SpikeBarricade() => Load("Props/spike_barricade");

        // 나무 울타리 문 (가로)
        public static Sprite GateOpen() => LoadSubSprite("Props/gate", "gate_open");
        public static Sprite GateClosed() => LoadSubSprite("Props/gate", "gate_closed");

        // 나무 울타리 문 (세로)
        public static Sprite VerticalGateOpen() => LoadSubSprite("Props/gate", "gate_vertical_open");
        public static Sprite VerticalGateClosed() => LoadSubSprite("Props/gate", "gate_vertical_closed");

        // 나무 울타리 유닛
        public static Sprite WoodenFenceUnit() => LoadSubSprite("Props/Fence", "wooden_fence_unit");

        // 나무 울타리 가로 버전
        public static Sprite WoodenFenceLeft() => LoadSubSprite("Props/Fence/fenceArchive", "wooden_fence_Left");

        public static Sprite WoodenFenceCenter() => LoadSubSprite("Props/Fence/fenceArchive", "wooden_fence_Center");

        public static Sprite WoodenFenceRight() => LoadSubSprite("Props/Fence/fenceArchive", "wooden_fence_Right");

        // 나무 울타리 세로 버전
        public static Sprite VerticalWoodenFenceBottom()
            => LoadSubSprite("Props/Fence/fenceArchive", "verticalFence_Wood_Bottom");

        public static Sprite VerticalWoodenFenceCenter()
            => LoadSubSprite("Props/Fence/fenceArchive", "verticalFence_Wood_Center");

        public static Sprite VerticalWoodenFenceTop()
            => LoadSubSprite("Props/Fence/fenceArchive", "verticalFence_Wood_Top");

        // 나무 울타리 코너 버전
        public static Sprite WoodenFenceTopRightCorner()
            => LoadSubSprite("Props/Fence/fenceArchive", "Fence_Wood_TopRightCorner");
        public static Sprite WoodenFenceTopLeftCorner()
            => LoadSubSprite("Props/Fence/fenceArchive", "Fence_Wood_TopLeftCorner");
        public static Sprite WoodenFenceBottomRightCorner()
            => LoadSubSprite("Props/Fence/fenceArchive", "Fence_Wood_BottomRightCorner");
        public static Sprite WoodenFenceBottomLeftCorner()
            => LoadSubSprite("Props/Fence/fenceArchive", "Fence_Wood_BottomLeftCorner");

        // 돌담 유닛
        public static Sprite StoneWallUnit()
            => LoadSubSprite("Props/Fence/fenceArchive", "stone_wall_unit");

        // 돌담 가로 버전
        public static Sprite StoneWallLeft()
            => LoadSubSprite("Props/Fence/fenceArchive", "stone_wall_Left");

        public static Sprite StoneWallRight()
            => LoadSubSprite("Props/Fence/fenceArchive", "stone_wall_Right");

        public static Sprite StoneWallCenter()
            => LoadSubSprite("Props/Fence/fenceArchive", "stone_wall_Center");

        // 돌담 세로 버전
        public static Sprite VerticalStoneWallBottom()
            => LoadSubSprite("Props/Fence/fenceArchive", "verticalStoneWall_Bottom");

        public static Sprite VerticalStoneWallCenter()
            => LoadSubSprite("Props/Fence/fenceArchive", "verticalStoneWall_Center");

        public static Sprite VerticalStoneWallTop()
            => LoadSubSprite("Props/Fence/fenceArchive", "verticalStoneWall_Top");

        // 돌담 코너 버전
        public static Sprite StoneWallTopRightCorner()
            => LoadSubSprite("Props/Fence/fenceArchive", "StoneWall_TopRightCorner");
        public static Sprite StoneWallTopLeftCorner()
            => LoadSubSprite("Props/Fence/fenceArchive", "StoneWall_TopLeftCorner");
        public static Sprite StoneWallBottomRightCorner()
            => LoadSubSprite("Props/Fence/fenceArchive", "StoneWall_BottomRightCorner");
        public static Sprite StoneWallBottomLeftCorner()
            => LoadSubSprite("Props/Fence/fenceArchive", "StoneWall_BottomLeftCorner");

        // ── BuildingKind → 스프라이트 ─────────────────────────────────────────────
        public static Sprite Facility(BuildingKind kind)
        {
            string name = kind switch
            {
                BuildingKind.SeedStorage => "seed_storage",
                BuildingKind.Carpenter => "carpenter",
                BuildingKind.Blacksmith => "blacksmith",
                BuildingKind.TrainingCamp => "training_camp",
                BuildingKind.FoodStorage => "food_storage",
                BuildingKind.LookoutPost => "lookout_post",
                BuildingKind.Sawmill => "sawmill",
                BuildingKind.Church => "church",
                BuildingKind.Brazier => "brazier",
                _ => null
            };
            return name != null ? Load("Buildings/Identity20261019/"+name) : null;
        }

        public static Sprite BuildingByKind(BuildingKind kind) => ConstructionPlacement.Sprite(kind);

        public static Sprite DiscoveryByReward(ResourceKind kind) => GroundResourceArt.Site(kind);

        public static Sprite AnimalByName(string procName) => procName switch
        {
            "Deer_proc" => Deer(),
            "Wolf_proc" => Wolf(),
            "Rabbit_proc" => Rabbit(),
            "SnowHare_proc" => FrontStatic("Animals/snow_hare"),
            "Boar_proc" => Boar(),
            "Mammoth_proc" => Mammoth(),
            "Bear_proc" => Bear(),
            "Fox_proc" => FrontStatic("Animals/fox"),
            _ => null,
        };

        // Recovered helper methods omitted by the upstream main merge.
        private static Sprite LoadQuiet(string path)
        {
            return Resources.Load<Sprite>($"Sprites/{path}");
        }

        public static Sprite LoadUiSprite(string path)
        {
            var s = Resources.Load<Sprite>($"UI/{path}");
            if (s == null) Debug.LogWarning($"[SpriteBank] Missing UI sprite: UI/{path}");
            return s;
        }

        private static Sprite LoadSpriteOrUi(string spritePath, string uiPath)
        {
            var s = LoadQuiet(spritePath);
            if (s != null) return s;

            s = Resources.Load<Sprite>($"UI/{uiPath}");
            if (s != null) return s;

            Debug.LogWarning($"[SpriteBank] Missing sprite fallback: Sprites/{spritePath} or UI/{uiPath}");
            return null;
        }

        public static Sprite Player() => Load("Player/player");

        public static Sprite PineTree() => LoadSpriteOrUi("Props/pine_tree", "hud/hud-wood");

        public static Sprite PineTree02() => LoadSpriteOrUi("Props/pine_tree_02", "hud/hud-wood");

        public static Sprite PineTree03() => LoadSpriteOrUi("Props/pine_tree_03", "hud/hud-wood");

        public static Sprite PineTree04() => LoadSpriteOrUi("Props/pine_tree_04", "hud/hud-wood");

        public static Sprite BareTree02() => LoadSpriteOrUi("Props/bare_tree_02", "hud/hud-wood");

        public static Sprite BareTree03() => LoadSpriteOrUi("Props/bare_tree_03", "hud/hud-wood");

        public static Sprite SnowTree01() => LoadSpriteOrUi("Props/snow_tree_01", "hud/hud-wood");

        public static Sprite SnowTree02() => LoadSpriteOrUi("Props/snow_tree_02", "hud/hud-wood");

        public static Sprite TreeVariant(int variant)
        {
            switch ((uint)variant % TreeVariantCount)
            {
                case 0: return SnowPineTree();
                case 1: return Load("Props/NordicTrees20261102/pine");
                case 2: return Load("Props/NordicTrees20261102/birch");
                case 3: return Load("Props/NordicTrees20261102/fir");
                case 4: return Load("Props/NordicTrees20261102/sparse_spruce");
                case 5: return Load("Props/NordicTrees20261102/birch_cluster");
                case 6: return Load("Props/NordicTrees20261102/oak");
                case 7: return Load("Props/NordicTrees20261102/cedar");
                default: return Load("Props/NordicTrees20261102/larch");
            }
        }
        public const int TreeVariantCount = 9;

        public static Sprite SnowRocks() => LoadSpriteOrUi("Props/snow_rocks", "hud/hud-stone");

        public static Sprite CropPotatoIcon() => LoadUiSprite("hud/hud-crop-potato");

        public static Sprite CropTurnipIcon() => LoadUiSprite("hud/hud-crop-turnip");

        public static Sprite CropWheatIcon() => LoadUiSprite("hud/hud-crop-wheat");

        public static Sprite CropHarvestIcon() => LoadUiSprite("hud/hud-crop-harvest");

        public static Sprite FoodIcon() => LoadUiSprite("hud/hud-food");

        public static Sprite WoodIcon() => LoadUiSprite("hud/hud-wood");

        public static Sprite StoneIcon() => LoadUiSprite("hud/hud-stone");

        public static Sprite MeatIcon() => LoadUiSprite("hud/hud-meat");

        public static Sprite HomeIcon() => LoadUiSprite("hud/hud-home");

        public static Sprite TempIcon() => LoadUiSprite("hud/hud-temp");

        public static Sprite HpIcon() => LoadUiSprite("hud/hud-hp");

        public static Sprite ThreatIcon() => LoadUiSprite("hud/hud-threat");

        public static Sprite PopulationIcon() => LoadUiSprite("hud/hud-population");

        public static Sprite UpgradeIcon() => LoadUiSprite("hud/hud-upgrade");

        public static Sprite SnowFenceLeft() => WoodenFenceLeft();

        public static Sprite SnowFenceCenter() => WoodenFenceCenter();

        public static Sprite SnowFenceRight() => WoodenFenceRight();
    }
}
