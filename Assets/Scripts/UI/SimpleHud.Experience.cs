using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private float ExperienceProgress=>Progression!=null && Progression.XpToNext>0
            ? Mathf.Clamp01((float)Progression.Xp/Progression.XpToNext):0;
        private string ExperienceLabel=>Progression!=null?$"EXP {Progression.Xp:N0} / {Progression.XpToNext:N0}":"";
        private void DrawExperienceBar()
        {
            if(Progression==null)return;
            var r=MobileHudLayout.Experience;
            if(!GeneratedUiSkin.Pill(r,new Color(.18f,.28f,.32f,.96f)))
                MobileButtonTheme.RoundRect(r,6,UiTheme.BarBg);
            var fill=new Rect(r.x+3,r.y+3,(r.width-6)*ExperienceProgress,r.height-6);
            if(fill.width>0 && !GeneratedUiSkin.Pill(fill,new Color(.42f,.83f,1f,1f)))
                MobileButtonTheme.RoundRect(fill,4,UiTheme.BarXpFill);
            var text=new GUIStyle(_label){fontSize=11,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,normal={textColor=Color.white}};
            var shadow=new GUIStyle(text){normal={textColor=new Color(.01f,.025f,.04f,1)}};
            HudText.Label(new Rect(r.x+1,r.y+1,r.width,r.height),ExperienceLabel,shadow);
            HudText.Label(r,ExperienceLabel,text);
        }
    }
}
