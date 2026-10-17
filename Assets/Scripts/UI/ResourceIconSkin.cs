using UnityEngine;
namespace IL6
{
    public static class ResourceIconSkin
    {
        [System.Serializable] private sealed class Region { public float x,y,width,height; }
        [System.Serializable] private sealed class Metrics { public Region[] icons; }
        private static Texture2D _atlas;
        private static Metrics _metrics;
        private static Sprite _woodSprite;
        private static Texture2D _ore;
        private static readonly System.Collections.Generic.Dictionary<ResourceKind,Sprite> WorldSprites=new();
        public static Sprite WorldSprite(ResourceKind kind)
        {
            if(kind==ResourceKind.Wood) return WorldWoodSprite;
            if(WorldSprites.TryGetValue(kind,out var cached) && cached!=null) return cached;
            Texture2D texture;Rect pixels;
            if(kind==ResourceKind.Ore)
            {
                if(_ore==null) _ore=Resources.Load<Texture2D>("UI/Generated20261017/ore");
                texture=_ore;if(texture==null) return null;
                pixels=new Rect(0,0,texture.width,texture.height);
            }
            else
            {
                if(IconCount!=5) return null;
                if(_atlas==null) _atlas=Resources.Load<Texture2D>("UI/Generated20261014/resources");
                texture=_atlas;if(texture==null) return null;
                int index=kind switch {ResourceKind.Stone=>1,ResourceKind.Meat=>2,ResourceKind.Food=>3,_=>4};
                var r=_metrics.icons[index];
                pixels=new Rect(Mathf.Round(r.x*texture.width),Mathf.Round(r.y*texture.height),Mathf.Round(r.width*texture.width),Mathf.Round(r.height*texture.height));
            }
            var sprite=Sprite.Create(texture,pixels,new Vector2(.5f,.5f),Mathf.Max(pixels.width,pixels.height)/.65f,0,SpriteMeshType.FullRect);
            sprite.name="Dropped"+kind;WorldSprites[kind]=sprite;return sprite;
        }
        public static Sprite WorldWoodSprite
        {
            get
            {
                if(_woodSprite!=null) return _woodSprite;
                if(IconCount!=5) return null;
                if(_atlas==null) _atlas=Resources.Load<Texture2D>("UI/Generated20261014/resources");
                if(_atlas==null) return null;
                var r=_metrics.icons[0];
                var pixels=new Rect(Mathf.Round(r.x*_atlas.width),Mathf.Round(r.y*_atlas.height),Mathf.Round(r.width*_atlas.width),Mathf.Round(r.height*_atlas.height));
                _woodSprite=Sprite.Create(_atlas,pixels,new Vector2(.5f,.5f),Mathf.Max(pixels.width,pixels.height)/.65f,0,SpriteMeshType.FullRect);
                _woodSprite.name="DroppedWood";return _woodSprite;
            }
        }
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
