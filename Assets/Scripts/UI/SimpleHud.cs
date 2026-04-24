using UnityEngine;

namespace IL6
{
    /// <summary>
    /// IMGUI 기반 HUD: 자원/HP/위치/채집/무기/쿨다운 + 모닥불 빌드 버튼.
    /// Canvas/TMP 셋업 없이 즉시 작동.
    /// </summary>
    public sealed class SimpleHud : MonoBehaviour
    {
        public PlayerController Player;
        public GatherController Gather;
        public PlayerAttackController Attacker;

        private GUIStyle _labelStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _weaponStyle;

        private void OnGUI()
        {
            EnsureStyles();

            GUI.Box(new Rect(10, 10, 280, 320), "");
            int y = 18;
            GUI.Label(new Rect(20, y, 260, 24), "=== IL6 Snowfield ===", _titleStyle); y += 26;

            var session = GameSession.Instance;
            if (session != null)
            {
                GUI.Label(new Rect(20, y, 260, 22), $"Wood:  {session.Resources.Get(ResourceKind.Wood)}", _labelStyle); y += 20;
                GUI.Label(new Rect(20, y, 260, 22), $"Meat:  {session.Resources.Get(ResourceKind.Meat)}", _labelStyle); y += 20;
                GUI.Label(new Rect(20, y, 260, 22), $"Food:  {session.Resources.Get(ResourceKind.Food)}", _labelStyle); y += 20;
                GUI.Label(new Rect(20, y, 260, 22), $"Day {session.Cycle.Day}  Phase: {session.Cycle.Phase}", _labelStyle); y += 22;
            }
            else
            {
                GUI.Label(new Rect(20, y, 260, 22), "GameSession: NOT FOUND", _labelStyle); y += 22;
            }

            if (Player != null)
            {
                GUI.Label(new Rect(20, y, 260, 22), $"HP: {Player.CurrentHp} / {Player.MaxHp}", _labelStyle); y += 20;
                var p = Player.transform.position;
                GUI.Label(new Rect(20, y, 260, 22), $"Pos: ({p.x:F1}, {p.y:F1})", _labelStyle); y += 22;
            }
            else
            {
                GUI.Label(new Rect(20, y, 260, 22), "Player: NULL", _labelStyle); y += 22;
            }

            if (Gather != null && Gather.IsActive)
            {
                GUI.Label(new Rect(20, y, 260, 22), $"Gathering: {(Gather.Progress * 100):F0}%", _labelStyle); y += 22;
            }

            // 장착 무기
            if (Attacker != null && Attacker.Weapon != null)
            {
                var w = Attacker.Weapon;
                GUI.Label(new Rect(20, y, 260, 22), $"[Weapon] {w.DisplayName}", _weaponStyle); y += 22;
                GUI.Label(new Rect(20, y, 260, 22), $"DMG {w.BaseDamage}  RNG {w.Range:F1}u  CD {w.CooldownSec:F2}s", _labelStyle); y += 20;

                float cd = Attacker.CurrentCooldown;
                float ready = 1f - Mathf.Clamp01(cd / Mathf.Max(0.01f, w.CooldownSec));
                // Bar 시각화
                var barBg = new Rect(20, y, 200, 14);
                GUI.Box(barBg, "");
                var fill = new Rect(22, y + 2, 196 * ready, 10);
                GUI.DrawTexture(fill, Texture2D.whiteTexture);
                GUI.Label(new Rect(230, y - 4, 50, 22), ready >= 1f ? "READY" : $"{(cd):F1}s", _labelStyle);
                y += 18;
            }

            y += 6;
            GUI.enabled = session != null && Player != null && session.Resources.Get(ResourceKind.Wood) >= 5;
            if (GUI.Button(new Rect(20, y, 220, 30), "Build Campfire (5 Wood)"))
            {
                if (session.Resources.Spend(ResourceKind.Wood, 5))
                {
                    SpawnCampfire(Player.transform.position);
                }
            }
            GUI.enabled = true;
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null) return;
            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, normal = { textColor = Color.white } };
            _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.yellow } };
            _weaponStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.7f, 0.95f, 1f) } };
        }

        private void SpawnCampfire(Vector3 playerPos)
        {
            var go = new GameObject("Campfire");
            go.transform.position = playerPos + new Vector3(1.2f, 0f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;

            var cf = go.AddComponent<ColorFallback>();
            cf.Tint = new Color(1f, 0.5f, 0.1f);
            cf.Shape = FallbackShape.Rounded;
            cf.Circle = false;
            cf.PixelSize = 64;
            cf.OutlineWidth = 2;
            cf.OutlineColor = new Color(0.3f, 0.1f, 0f, 1f);
        }
    }
}
