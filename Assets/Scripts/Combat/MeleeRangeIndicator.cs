using UnityEngine;

namespace IL6
{
    public sealed class MeleeRangeIndicator : MonoBehaviour
    {
        private PlayerAttackController _attack;
        private PlayerController _player;
        private LineRenderer _line;
        private Material _material;
        public float DisplayedRange { get; private set; }
        public bool Visible => _line != null && _line.enabled;

        private void Awake()
        {
            _attack = GetComponent<PlayerAttackController>();
            _player = GetComponent<PlayerController>();
            var child = new GameObject("Melee range");
            child.transform.SetParent(transform, false);
            _line = child.AddComponent<LineRenderer>();
            _material = new Material(Shader.Find("Sprites/Default"));
            _line.sharedMaterial = _material;
            _line.useWorldSpace = true;
            _line.loop = true;
            _line.positionCount = 96;
            _line.sortingOrder = 4;
            _line.enabled = false;
        }

        private void LateUpdate()
        {
            bool melee = _attack != null && _attack.Weapon != null && !_attack.IsConstructionHammer && _attack.Weapon.ProjectileSpeed <= 0f;
            bool gathering = GetComponent<GatherController>() is GatherController gather && gather.IsActive;
            _line.enabled = melee && !gathering && (_player == null || !_player.IsDead) && !InputReader.HudBlocked && Time.timeScale > 0f;
            if (!_line.enabled) return;
            DisplayedRange = _attack.Weapon.Range + (_attack.Progression != null ? _attack.Progression.BonusRange : 0f);
            bool flash = _attack.IsRecentlyAttacking && _attack.CurrentCooldown > 0f;
            Color color = flash ? new Color(1f,.86f,.4f,.85f) : new Color(1f,.94f,.72f,.4f);
            _line.startColor = _line.endColor = color;
            _line.startWidth = _line.endWidth = flash ? .055f : .03f;
            for (int i = 0; i < _line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / _line.positionCount;
                _line.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0f) * DisplayedRange);
            }
        }

        private void OnDisable() { if (_line != null) _line.enabled = false; }
        private void OnDestroy() { if (_material != null) Destroy(_material); }
    }
}
