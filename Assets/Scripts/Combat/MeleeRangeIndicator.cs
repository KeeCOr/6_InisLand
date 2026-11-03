using UnityEngine;

namespace IL6
{
    public sealed class MeleeRangeIndicator : MonoBehaviour
    {
        private PlayerAttackController _attack;
        private PlayerController _player;
        private GroundRangeDecal _decal;
        public float DisplayedRange { get; private set; }
        public bool Visible => _decal != null && _decal.Visible;
        public GroundRangeDecal Decal => _decal;

        private void Awake()
        {
            _attack = GetComponent<PlayerAttackController>();
            _player = GetComponent<PlayerController>();
            _decal = GroundRangeDecal.Create(gameObject, "Melee range decal");
        }

        private void LateUpdate()
        {
            bool melee = _attack != null && _attack.Weapon != null && !_attack.IsConstructionHammer && _attack.Weapon.ProjectileSpeed <= 0f;
            bool attackFlash = _player != null && !_player.IsDead && _player.Actions != null && _player.Actions.State == PlayerAction.Attack;
            bool visible = melee && !InputReader.HudBlocked && Time.timeScale > 0f
                && (_player == null || attackFlash || ManualWorldInteraction.Resolve(_player).Kind == ManualInteractionKind.Attack);
            if (!visible) { _decal.Hide(); return; }
            DisplayedRange = _attack.Weapon.Range + (_attack.Progression != null ? _attack.Progression.BonusRange : 0f);
            bool flash = _attack.IsRecentlyAttacking && _attack.CurrentCooldown > 0f;
            Color color = flash ? new Color(1f,.86f,.4f,1f) : new Color(1f,.96f,.78f,.8f);
            _decal.Show(transform.position, DisplayedRange, color);
        }

        private void OnDisable() { if (_decal != null) _decal.Hide(); }
        private void OnDestroy() { if (_decal != null) Destroy(_decal.gameObject); }
    }
}
