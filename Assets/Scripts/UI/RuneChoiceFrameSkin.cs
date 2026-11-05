using UnityEngine;
namespace IL6
{
    public static class RuneChoiceFrameSkin
    {
        public const string FramePath="UI/LevelUp20261104/frame";
        public const string CardPath="UI/LevelUp20261104/card";
        private static Texture2D _frame,_card;
        public static void DrawRoot(Rect rect,string title,GUIStyle style)
        {
            if(_frame==null)_frame=Resources.Load<Texture2D>(FramePath);
            if(_frame!=null)
            {
                var old=GUI.color;GUI.color=Color.white;
                // Keep the painted title rail within its 56px allocation, above the cards.
                const float headerUv=.13f,headerHeight=56;
                GUI.DrawTextureWithTexCoords(new Rect(rect.x,rect.y,rect.width,headerHeight),_frame,new Rect(0,1-headerUv,1,headerUv),true);
                GUI.DrawTextureWithTexCoords(new Rect(rect.x,rect.y+headerHeight,rect.width,rect.height-headerHeight),_frame,new Rect(0,0,1,1-headerUv),true);
                GUI.color=old;
            }
            else UiTheme.Panel(rect);
            HudText.Label(new Rect(rect.x+150,rect.y+16,rect.width-300,30),title,
                new GUIStyle(style){fontSize=23,alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(.96f,.88f,.7f)}});
        }
        public static void DrawCard(Rect rect)
        {
            if(_card==null)_card=Resources.Load<Texture2D>(CardPath);
            if(_card==null){UiTheme.Panel(rect);return;}
            var old=GUI.color;GUI.color=Color.white;GUI.DrawTexture(rect,_card,ScaleMode.StretchToFill,true);GUI.color=old;
        }
        public static void DrawDetails(Rect rect)
        {
            UiTheme.Rect(rect,new Color(.025f,.045f,.06f,.65f));
            UiTheme.Rect(new Rect(rect.x+10,rect.y,rect.width-20,1),new Color(.72f,.6f,.37f,.28f));
        }
    }
}
