using UnityEngine;
namespace IL6
{
    // IMGUI's default padding can cut Korean glyphs inside compact HUD labels.
    public static class HudText
    {
        public static GUIStyle Fit(Rect rect, GUIContent content, GUIStyle source)
        {
            var style = new GUIStyle(source) { padding = new RectOffset(0,0,0,0), fixedWidth=0, fixedHeight=0, clipping=TextClipping.Clip };
            int size = source.fontSize > 0 ? source.fontSize : 14;
            for (; size > 1; size--)
            {
                style.fontSize=size;
                bool widthFits = style.wordWrap || style.CalcSize(content).x <= rect.width-2;
                if (widthFits && style.CalcHeight(content, Mathf.Max(1,rect.width-2)) <= rect.height-2) break;
            }
            style.fontSize=size;
            return style;
        }
        public static void Label(Rect rect, string text, GUIStyle style)
        { var content=new GUIContent(text); GUI.Label(rect,content,Fit(rect,content,style)); }
        public static void Label(Rect rect, string text) => Label(rect,text,GUI.skin.label);
        public static void Label(Rect rect, GUIContent content, GUIStyle style)
        { GUI.Label(rect,content,Fit(rect,content,style)); }
    }
}
