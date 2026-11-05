using UnityEngine;
namespace IL6
{
    public static class NavigationIconSkin
    {
        public const string Folder="UI/Navigation20261106/";
        private static Texture2D _arrow,_home,_damage,_backing;
        public static void DrawBacking(Rect rect,Color tint)
        {
            if(_backing==null)_backing=Resources.Load<Texture2D>("UI/Generated20261014/pill");
            var color=GUI.color;GUI.color=tint;
            if(_backing!=null)GUI.DrawTextureWithTexCoords(rect,_backing,GeneratedUiSkin.SourceBounds(true),true);
            GUI.color=color;
        }
        public static Texture2D Arrow=>_arrow!=null?_arrow:(_arrow=Resources.Load<Texture2D>(Folder+"arrow"));
        public static Texture2D Home=>_home!=null?_home:(_home=Resources.Load<Texture2D>(Folder+"home"));
        public static Texture2D Damage=>_damage!=null?_damage:(_damage=Resources.Load<Texture2D>(Folder+"damage"));
        public static void DrawDirection(Rect rect,Vector2 direction,Color tint)
        {
            var matrix=GUI.matrix;var color=GUI.color;
            try
            {
                float angle=Mathf.Atan2(-direction.y,direction.x)*Mathf.Rad2Deg;
                GUI.matrix=matrix*Matrix4x4.TRS(rect.center,Quaternion.Euler(0,0,angle),Vector3.one);
                GUI.color=tint;if(Arrow!=null)GUI.DrawTexture(new Rect(-rect.width/2,-rect.height/2,rect.width,rect.height),Arrow,ScaleMode.ScaleToFit,true);
            }
            finally{GUI.matrix=matrix;GUI.color=color;}
        }
    }
}
