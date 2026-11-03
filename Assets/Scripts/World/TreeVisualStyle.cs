using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    public static class TreeVisualStyle
    {
        private static readonly Dictionary<(Sprite,float),Sprite> SizedSprites = new();

        // Change sprite density, never the physics root or its colliders.
        public static void Apply(SpriteRenderer renderer, int variant)
        {
            if(renderer==null) return;
            var source=SpriteBank.TreeVariant(variant);
            if(source==null) return;
            float rootScale=Mathf.Abs(renderer.transform.lossyScale.y);
            if(rootScale<.001f) {renderer.sprite=source;return;}
            float sourceHeight=source.name=="pine"?1.35f:source.name=="birch"?1.6f:1.5f;
            float worldHeight=source.name switch
            {"pine"=>3.10f,"birch"=>3.35f,"fir"=>3.45f,"sparse_spruce"=>3.10f,"birch_cluster"=>3.35f,
                "oak"=>3.35f,"cedar"=>3.50f,"larch"=>3.40f,_=>3.225f};
            float ppu=source.pixelsPerUnit*sourceHeight*rootScale/worldHeight;
            var key=(source,ppu);
            if(!SizedSprites.TryGetValue(key,out var sprite) || sprite==null)
            {
                float minY=source.pivot.y-.24f*source.pixelsPerUnit;
                var pivot=new Vector2(source.pivot.x/source.rect.width,(minY+.24f*ppu)/source.rect.height);
                sprite=Sprite.Create(source.texture,source.rect,pivot,ppu,0,SpriteMeshType.FullRect);
                sprite.name=source.name;
                sprite.hideFlags=HideFlags.DontSave;
                SizedSprites[key]=sprite;
            }
            renderer.sprite=sprite;
        }
    }
}
