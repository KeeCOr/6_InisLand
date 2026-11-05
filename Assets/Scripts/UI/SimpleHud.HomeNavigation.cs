using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private void DrawHomeCompass()
        {
            if(Player==null || Player.IsDead || ControlsBlocked)return;
            var frame=HomeNavigation.Resolve(Player.transform.position,Object.FindObjectsByType<Building>(FindObjectsSortMode.None));
            if(!frame.Visible)return;
            var rect=HomeNavigation.Bounds(MobileHudLayout.Width);
            NavigationIconSkin.DrawBacking(rect,new Color(.3f,.4f,.44f,.9f));
            if(NavigationIconSkin.Home!=null)GUI.DrawTexture(new Rect(rect.x+8,rect.y+5,22,22),NavigationIconSkin.Home,ScaleMode.ScaleToFit,true);
            NavigationIconSkin.DrawDirection(new Rect(rect.xMax-26,rect.y+7,18,18),frame.Direction,UiTheme.TextCream);
            if(_compassDist==null)_compassDist=new GUIStyle(_label){fontSize=12,fontStyle=FontStyle.Normal,alignment=TextAnchor.MiddleLeft,normal={textColor=new Color(.87f,.92f,.94f)}};
            HudText.Label(new Rect(rect.x+36,rect.y+3,rect.width-66,rect.height-6),frame.Label,_compassDist);
        }
    }
}
