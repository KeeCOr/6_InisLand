using UnityEngine;

namespace IL6
{
    public sealed partial class SimpleHud
    {
        private Rect BuildPlacementControlsRect => new Rect(MobileHudLayout.Width/2-170,16,340,52);
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
            CloseBuildCatalog();
            Sfx.Build();
            return true;
        }
    }
}
