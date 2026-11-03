using UnityEngine;

namespace IL6
{
    public sealed partial class SimpleHud
    {
        private Rect BuildPlacementControlsRect => new Rect(MobileHudLayout.Width/2-170,16,340,52);
        private Rect BuildPlacementFeedbackRect => new Rect(MobileHudLayout.Width/2-310,82,620,102);
        private void DrawBuildPlacementControls()
        {
            if(_buildCatalogOpen) return;
            var r=BuildPlacementControlsRect;
            if(UiTheme.Button(new Rect(r.x,r.y,164,48),"건물 목록",_btn)) {_pendingBuildKind=null;_buildCatalogOpen=true;}
            if(UiTheme.Button(new Rect(r.x+176,r.y,164,48),"건설 종료",_btn)) CloseBuildCatalog();
        }
        private bool _buildPaused;
        private bool _buildRepairMode;
        public bool EnterBuildRepairMode()
        {
            if (!BuildUiAvailable || Attacker==null || !Attacker.IsConstructionHammer) return false;
            _pendingBuildKind=null;_buildCatalogOpen=false;_buildRepairMode=true;
            ReleaseBuildPause();
            return true;
        }
        private float _buildPreviousScale = 1f;
        private bool BuildUiAvailable => _buildPaused && !_paused && !_tutorialPaused && !_recruitCutPaused
            && Player != null && !Player.IsDead && (Progression == null || !Progression.LevelUpPending);

        private void BeginBuildPause()
        {
            if (_buildPaused) return;
            _buildPreviousScale = Time.timeScale;
            Gather?.Cancel();
            _buildPaused = true;
            Time.timeScale = 0f;
            InputReader.HudBlocked = true;
        }

        private void ReleaseBuildPause()
        {
            if (!_buildPaused) return;
            _buildPaused = false;
            bool anotherPause = _paused || _tutorialPaused || _recruitCutPaused
                || (Player != null && Player.IsDead) || (Progression != null && Progression.LevelUpPending);
            Time.timeScale = anotherPause ? 0f : _buildPreviousScale;
            InputReader.HudBlocked = ControlsBlocked;
        }

        public bool TryPlaceBuildingAt(Vector3 position)
        {
            if (!BuildUiAvailable || _buildCatalogOpen || _pendingBuildKind == null
                || float.IsNaN(position.x) || float.IsNaN(position.y)
                || float.IsInfinity(position.x) || float.IsInfinity(position.y)) return false;
            var session = GameSession.Instance;
            if (session == null) return false;
            var kind = _pendingBuildKind.Value;
            position = ConstructionPlacement.Snap(position);
            if (!ConstructionPlacement.IsAvailable(kind, position)) return false;
            if (kind == BuildingKind.Farm && FarmBuilding.CurrentFarmCount() >= FarmBuilding.MaxFarmsAllowed()) return false;
            var cost = BuildingUpgradeRules.BuildCost(kind, CostMul(kind));
            if (!cost.Pay(session.Resources)) return false;
            position.z = 0f;
            SpawnConstructionSite(kind, position);
            // Keep the selected kind and paused construction session for the next segment.
            Sfx.Build();
            return true;
        }

        private static string ConstructionCostText(ResourceCost cost, ResourceStore store)
            => $"보유/필요 · 목재 {store.Get(ResourceKind.Wood):N0}/{cost.Wood} · 석재 {store.Get(ResourceKind.Stone):N0}/{cost.Stone}";

        private void DrawConstructionPlacementPreview()
        {
            if (_pendingBuildKind == null || !BuildUiAvailable || _buildCatalogOpen) return;
            var cam = Camera.main;
            var session = GameSession.Instance;
            if (cam == null || session == null) return;
            var kind = _pendingBuildKind.Value;
            Vector3 position = ConstructionPlacement.Snap(MouseWorldPos(cam));
            var cost = BuildingUpgradeRules.BuildCost(kind, CostMul(kind));
            bool farmAllowed = kind != BuildingKind.Farm || FarmBuilding.CurrentFarmCount() < FarmBuilding.MaxFarmsAllowed();
            string reason = ConstructionPlacement.BlockedReason(ConstructionPlacement.IsAvailable(kind, position), farmAllowed, cost.CanPay(session.Resources));
            bool available = reason.Length == 0;
            Color tint = available ? new Color(.3f,.9f,.6f,.8f) : new Color(1f,.25f,.2f,.85f);
            DrawConstructionGrid(cam, position);
            Rect footprint = ConstructionScreenRect(cam, ConstructionPlacement.Footprint(kind, position));
            UiTheme.Rect(footprint, new Color(tint.r,tint.g,tint.b,.19f));
            // Keep grid cells visible inside the exact existing placement footprint.
            float unit = Mathf.Abs(cam.WorldToScreenPoint(position + Vector3.right).x - cam.WorldToScreenPoint(position).x) / MobileHudLayout.Scale;
            float spacing = unit * ConstructionPlacement.CellSize;
            if (spacing >= 4f)
            {
                for (float x=footprint.xMin; x<footprint.xMax; x+=spacing)
                    UiTheme.Rect(new Rect(x,footprint.yMin,1,footprint.height),tint);
                for (float y=footprint.yMin; y<footprint.yMax; y+=spacing)
                    UiTheme.Rect(new Rect(footprint.xMin,y,footprint.width,1),tint);
            }
            var sprite = ConstructionPlacement.Sprite(kind);
            if(kind==BuildingKind.Fence)
            {
                Vector3 screen=cam.WorldToScreenPoint(position);
                FenceVisual.DrawHorizontalPreview(new Vector2(screen.x/MobileHudLayout.Scale,MobileHudLayout.Height-screen.y/MobileHudLayout.Scale),unit,new Color(tint.r,tint.g,tint.b,.62f));
            }
            else if (sprite != null)
            {
                Vector3 size = ConstructionPlacement.Scale(kind);
                Vector3 screen = cam.WorldToScreenPoint(position);
                float width = sprite.rect.width / sprite.pixelsPerUnit * unit * size.x;
                float height = sprite.rect.height / sprite.pixelsPerUnit * unit * size.y;
                var rect = new Rect(screen.x/MobileHudLayout.Scale-sprite.pivot.x/sprite.rect.width*width,
                    MobileHudLayout.Height-screen.y/MobileHudLayout.Scale-(1f-sprite.pivot.y/sprite.rect.height)*height,width,height);
                var previous=GUI.color;
                GUI.color=new Color(tint.r,tint.g,tint.b,.62f);
                DrawSpriteInRect(sprite,rect);
                GUI.color=previous;
            }
            UiTheme.Rect(new Rect(footprint.xMin,footprint.yMin,footprint.width,2),tint);
            UiTheme.Rect(new Rect(footprint.xMin,footprint.yMax-2,footprint.width,2),tint);
            UiTheme.Rect(new Rect(footprint.xMin,footprint.yMin,2,footprint.height),tint);
            UiTheme.Rect(new Rect(footprint.xMax-2,footprint.yMin,2,footprint.height),tint);
            var panel=BuildPlacementFeedbackRect;
            UiTheme.Panel(panel);
            var statusStyle=new GUIStyle(_section){fontSize=15,alignment=TextAnchor.MiddleCenter,normal={textColor=tint}};
            HudText.Label(new Rect(panel.x+10,panel.y+8,panel.width-20,24),$"{BuildingName(kind)} · {(available ? "설치 가능" : reason)}",statusStyle);
            var detailStyle=new GUIStyle(_labelSubtle){fontSize=13,alignment=TextAnchor.MiddleCenter};
            HudText.Label(new Rect(panel.x+10,panel.y+36,panel.width-20,22),ConstructionCostText(cost,session.Resources),detailStyle);
            HudText.Label(new Rect(panel.x+10,panel.y+64,panel.width-20,22),"땅 클릭: 연속 설치 · 우클릭 / ESC: 건설 종료 · 게임 정지 중",detailStyle);
        }

        private static Rect ConstructionScreenRect(Camera camera, Rect world)
        {
            Vector3 minimum=camera.WorldToScreenPoint(new Vector3(world.xMin,world.yMin,0));
            Vector3 maximum=camera.WorldToScreenPoint(new Vector3(world.xMax,world.yMax,0));
            return Rect.MinMaxRect(Mathf.Min(minimum.x,maximum.x)/MobileHudLayout.Scale,
                MobileHudLayout.Height-Mathf.Max(minimum.y,maximum.y)/MobileHudLayout.Scale,
                Mathf.Max(minimum.x,maximum.x)/MobileHudLayout.Scale,
                MobileHudLayout.Height-Mathf.Min(minimum.y,maximum.y)/MobileHudLayout.Scale);
        }
    }
}
