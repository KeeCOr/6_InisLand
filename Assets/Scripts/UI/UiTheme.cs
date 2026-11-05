using UnityEngine;

namespace IL6
{
    /// <summary>
    /// IMGUI HUD 공용 색상 + 헬퍼. 모든 패널/바/아이콘 그릴 때 이 함수만 호출하면
    /// 일관된 중세 양피지/네이비 톤이 유지됨.
    /// </summary>
    public enum UiButtonRole { Primary, Secondary, Destructive }

    public static class UiTheme
    {
        private static Font _hudFont;
        public static Font HudFont => _hudFont != null ? _hudFont :
            (_hudFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Noto Sans CJK KR", "Apple SD Gothic Neo", "Arial" }, 18));
        public static readonly Color PanelBg = new Color(0.055f, 0.12f, 0.135f, 0.92f);
        public static readonly Color PanelInnerBg = new Color(0.09f, 0.19f, 0.2f, 1f);
        public static readonly Color PanelBorder = new Color(0.64f, 0.79f, 0.74f, 0.42f);
        public static readonly Color PanelBorderDim = new Color(0.45f, 0.36f, 0.18f, 1f);
        public static readonly Color SeparatorGold = new Color(0.78f, 0.62f, 0.30f, 0.4f);

        public static readonly Color TextCream = new Color(0.96f, 0.94f, 0.88f);
        public static readonly Color TextGold = new Color(1f, 0.86f, 0.45f);
        public static readonly Color TextDanger = new Color(0.95f, 0.35f, 0.32f);
        public static readonly Color TextSubtle = new Color(0.76f, 0.84f, 0.82f);
        public static readonly Color TextHealth = new Color(0.86f, 0.95f, 0.85f);

        public static readonly Color BarBg = new Color(0.04f, 0.05f, 0.08f, 1f);
        public static readonly Color BarHpFill = new Color(0.28f, 0.78f, 0.5f);
        public static readonly Color BarXpFill = new Color(0.4f, 0.85f, 1f);
        public static readonly Color BarCdFill = new Color(0.95f, 0.78f, 0.35f);

        // 자원별 색
        public static Color ResColor(ResourceKind k) => k switch
        {
            ResourceKind.Wood => new Color(0.55f, 0.36f, 0.18f),
            ResourceKind.Ore => new Color(.65f,.76f,.84f),
            ResourceKind.Stone => new Color(0.55f, 0.55f, 0.6f),
            ResourceKind.Meat => new Color(0.78f, 0.30f, 0.36f),
            ResourceKind.Food => new Color(0.7f, 0.78f, 0.32f),
            ResourceKind.Frostbloom => new Color(0.45f, 0.85f, 1f),
            _ => Color.white,
        };

        public static void Rect(Rect r, Color c)
        {
            var oldC = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = oldC;
        }

        public static void Panel(Rect r)
        {
            if (GeneratedUiSkin.Panel(r,Color.white)) return;
            MobileButtonTheme.RoundRect(new Rect(r.x,r.y+4,r.width,r.height),12,new Color(0.015f,0.04f,0.045f,0.28f));
            MobileButtonTheme.RoundRect(r,12,PanelBorder);
            MobileButtonTheme.RoundRect(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),11,PanelBg);
        }

        public static void TitleBar(Rect r, string title, GUIStyle titleStyle)
        {
            Rect(new Rect(r.x+12, r.y+6, r.width-24, 28), new Color(0.15f,0.35f,0.29f,0.28f));
            // 좌우 끝 작은 다이아몬드 표시
            Rect(new Rect(r.x + 6, r.y + 10, 4, 4), TextGold);
            Rect(new Rect(r.x + r.width - 10, r.y + 10, 4, 4), TextGold);
            HudText.Label(new Rect(r.x+14, r.y+7, r.width-28, 28), title, titleStyle);
            // 하단 골드 라인
            Rect(new Rect(r.x + 4, r.y + 24, r.width - 8, 1), SeparatorGold);
        }

        public static void Separator(Rect r)
        {
            Rect(r, SeparatorGold);
        }

        public static void Icon(Rect r, Color fill)
        {
            // 검은 외곽
            Rect(r, new Color(0f, 0f, 0f, 1f));
            // 안쪽
            Rect(new Rect(r.x + 1, r.y + 1, r.width - 2, r.height - 2), fill);
            // 상단 하이라이트
            Rect(new Rect(r.x + 2, r.y + 2, r.width - 4, 1), new Color(1f, 1f, 1f, 0.25f));
        }

        public static void Bar(Rect r, float pct, Color fill)
        {
            // 깊은 어두운 배경
            Rect(r, BarBg);
            // 바깥 1px 어두운 골드
            Rect(new Rect(r.x, r.y, r.width, 1), PanelBorderDim);
            Rect(new Rect(r.x, r.yMax - 1, r.width, 1), PanelBorderDim);
            Rect(new Rect(r.x, r.y, 1, r.height), PanelBorderDim);
            Rect(new Rect(r.xMax - 1, r.y, 1, r.height), PanelBorderDim);
            // 채움
            float w = (r.width - 2) * Mathf.Clamp01(pct);
            Rect(new Rect(r.x + 1, r.y + 1, w, r.height - 2), fill);
            // 상단 하이라이트 라인 (살짝 밝은 톤)
            if (w > 2)
            {
                Rect(new Rect(r.x + 1, r.y + 1, w, 1), new Color(1f, 1f, 1f, 0.3f));
            }
        }

        public static bool Button(Rect r, string label, GUIStyle style, bool enabled = true, UiButtonRole role = UiButtonRole.Primary)
        {
            if (role == UiButtonRole.Primary) return MobileButtonTheme.Pill(r,label,style,enabled);
            bool usable = GUI.enabled && enabled;
            bool hover = r.Contains(MobileHudLayout.GuiPoint(Input.mousePosition));
            Color edge = role == UiButtonRole.Destructive ? TextDanger : PanelBorder;
            MobileButtonTheme.RoundRect(r, 7, usable ? edge : PanelBorderDim);
            MobileButtonTheme.RoundRect(new Rect(r.x+1,r.y+1,r.width-2,r.height-2), 6,
                hover && usable ? PanelInnerBg : PanelBg);
            var text = new GUIStyle(style) { alignment = TextAnchor.MiddleCenter };
            text.normal.textColor = usable ? (role == UiButtonRole.Destructive ? TextDanger : TextCream) : TextSubtle;
            HudText.Label(r,label,text);
            bool previous = GUI.enabled; GUI.enabled = usable;
            bool clicked = GUI.Button(r, "", GUIStyle.none); GUI.enabled = previous;
            return clicked && MobileButtonTheme.Contains(r,Event.current.mousePosition);
        }
    }
}
