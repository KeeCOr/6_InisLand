using UnityEngine;
namespace IL6
{
    public sealed partial class SimpleHud
    {
        private Texture2D _homeDirectionIcon;
        private void DrawHomeCompass()
        {
            if(Player==null || Player.IsDead || ControlsBlocked)return;
            var frame=HomeNavigation.Resolve(Player.transform.position,Object.FindObjectsByType<Building>(FindObjectsSortMode.None));
            if(!frame.Visible)return;
            var rect=HomeNavigation.Bounds(MobileHudLayout.Width);
            UiTheme.Rect(rect,new Color(.035f,.06f,.08f,.76f));
            UiTheme.Rect(new Rect(rect.x,rect.y+6,1,rect.height-12),new Color(.58f,.71f,.76f,.4f));
            if(_homeDirectionIcon==null)_homeDirectionIcon=Resources.Load<Texture2D>(HomeNavigation.IconPath);
            var center=new Vector2(rect.x+18,rect.center.y);var icon=new Rect(center.x-10,center.y-10,20,20);
            var matrix=GUI.matrix;var color=GUI.color;
            try
            {
                GUIUtility.RotateAroundPivot(Mathf.Atan2(-frame.Direction.y,frame.Direction.x)*Mathf.Rad2Deg,center);
                GUI.color=new Color(.87f,.94f,.97f,.95f);
                if(_homeDirectionIcon!=null)GUI.DrawTexture(icon,_homeDirectionIcon,ScaleMode.ScaleToFit,true);
                else HudText.Label(icon,">",new GUIStyle(_label){fontSize=18,alignment=TextAnchor.MiddleCenter});
            }
            finally {GUI.matrix=matrix;GUI.color=color;}
            if(_compassDist==null)_compassDist=new GUIStyle(_label){fontSize=12,fontStyle=FontStyle.Normal,alignment=TextAnchor.MiddleLeft,normal={textColor=new Color(.87f,.92f,.94f)}};
            HudText.Label(new Rect(rect.x+36,rect.y+3,rect.width-44,rect.height-6),frame.Label,_compassDist);
        }
    }
}
