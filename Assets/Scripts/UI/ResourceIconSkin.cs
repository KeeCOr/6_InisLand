using UnityEngine;
namespace IL6
{
    public static class ResourceIconSkin
    {
        [System.Serializable] private sealed class Region { public float x,y,width,height; }
        [System.Serializable] private sealed class Metrics { public Region[] icons; }
        private static Texture2D _atlas;
        private static Metrics _metrics;
        private static Texture2D _ore;
        public static Sprite WorldSprite(ResourceKind kind)=>GroundResourceArt.Drop(kind);
        public static Sprite WorldWoodSprite=>WorldSprite(ResourceKind.Wood);
        public static int IconCount
        {
            get
            {
                if(_metrics==null)
                {
                    var json=Resources.Load<TextAsset>("UI/Generated20261014/resource-metrics");
                    if(json!=null) _metrics=JsonUtility.FromJson<Metrics>(json.text);
                }
                return _metrics?.icons?.Length??0;
            }
        }
        public static bool Draw(Rect rect,string key)
        {
            if(key=="ore")
            {
                if(_ore==null) _ore=Resources.Load<Texture2D>("UI/Generated20261017/ore");
                if(_ore==null) return false;
                GUI.DrawTexture(rect,_ore,ScaleMode.ScaleToFit,true);return true;
            }
            int index=key switch {"wood"=>0,"stone"=>1,"meat"=>2,"food"=>3,"frostbloom"=>4,_=>-1};
            if(index<0 || IconCount!=5) return false;
            if(_atlas==null) _atlas=Resources.Load<Texture2D>("UI/Generated20261014/resources");
            if(_atlas==null) return false;
            var region=_metrics.icons[index];
            float aspect=region.width*_atlas.width/(region.height*_atlas.height);
            float width=Mathf.Min(rect.width,rect.height*aspect),height=width/aspect;
            var target=new Rect(rect.center.x-width/2,rect.center.y-height/2,width,height);
            GUI.DrawTextureWithTexCoords(target,_atlas,new Rect(region.x,region.y,region.width,region.height),true);
            return true;
        }
    }
}
