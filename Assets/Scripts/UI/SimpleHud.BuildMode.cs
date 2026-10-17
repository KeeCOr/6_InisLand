using UnityEngine;

namespace IL6
{
    public sealed partial class SimpleHud
    {
        private bool _buildPaused;
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
            if (kind == BuildingKind.Farm && FarmBuilding.CurrentFarmCount() >= FarmBuilding.MaxFarmsAllowed()) return false;
            var cost = BuildingUpgradeRules.BuildCost(kind, CostMul(kind));
            if (!cost.Pay(session.Resources)) return false;
            position.z = 0f;
            SpawnConstructionSite(kind, position);
            SetHudMode(Attacker != null && Attacker.WeaponIndex == 1 ? HudMode.Ranged : HudMode.Melee);
            Sfx.Build();
            return true;
        }
    }
}
