using UnityEngine;
namespace IL6
{
    public static class RockVisualStyle
    {
        private static readonly System.Collections.Generic.Dictionary<Sprite,float> Tops=new();
        public static Vector3 TopPoint(SpriteRenderer renderer)
        {
            var sprite=renderer.sprite;
            if(!Tops.TryGetValue(sprite,out float top))
            {
                top=float.MinValue;foreach(var vertex in sprite.vertices)top=Mathf.Max(top,vertex.y);
                Tops[sprite]=top;
            }
            return renderer.transform.TransformPoint(new Vector3(0,top,0));
        }
        public const int VariantCount=6;
        public static int VariantFor(Vector2 position)
        {
            int x=Mathf.FloorToInt(position.x*37.17f),y=Mathf.FloorToInt(position.y*53.31f);
            return (int)((uint)(x*73856093^y*19349663)%VariantCount);
        }
        public static Sprite SpriteFor(int variant)
        {
            int index=(int)((uint)variant%VariantCount);
            if(index==0)return SpriteBank.SnowRock();
            string name=index switch {1=>"split",2=>"rounded",3=>"angular",4=>"cluster",_=>"weathered"};
            return Resources.Load<Sprite>("Sprites/Props/NordicRocks20261103/"+name);
        }
        public static void Apply(SpriteRenderer renderer)
        {
            if(renderer==null)return;
            var sprite=SpriteFor(VariantFor(renderer.transform.position));
            if(sprite!=null)renderer.sprite=sprite;
        }
    }
}
