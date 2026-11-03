using UnityEngine;
namespace IL6
{
    /// <summary>Existing painted timber assembled at world-grid joints, independent of atlas padding.</summary>
    public static class FenceVisual
    {
        private static Sprite _post,_rail;
        private static void LoadParts()
        {
            if(_post!=null)return;
            var texture=Resources.Load<Texture2D>("Sprites/Props/Fence/fenceArchive");
            if(texture==null)return;
            float sx=texture.width/2816f,sy=texture.height/512f;
            _post=Part(texture,new Rect(612*sx,260*sy,56*sx,236*sy),new Vector2(.5f,0));
            _rail=Part(texture,new Rect(512*sx,392*sy,80*sx,50*sy),new Vector2(.5f,.5f));
        }
        private static Sprite Part(Texture2D texture,Rect rect,Vector2 pivot)
            => Sprite.Create(texture,rect,pivot,256,0,SpriteMeshType.FullRect);
        public static void Apply(GameObject owner,bool vertical=false,FenceCornerPieceType? corner=null)
        {
            LoadParts();if(_post==null)return;
            owner.GetComponent<SpriteRenderer>().forceRenderingOff=true;
            var root=new GameObject("__FenceArt").transform;root.SetParent(owner.transform,false);
            int rail=0;
            if(corner.HasValue)
            {
                bool left=corner==FenceCornerPieceType.TopLeft || corner==FenceCornerPieceType.BottomLeft;
                bool bottom=corner==FenceCornerPieceType.BottomLeft || corner==FenceCornerPieceType.BottomRight;
                Horizontal(root,left?.255f:-.255f,.53f,ref rail);
                Vertical(root,bottom?.255f:-.255f,.53f,ref rail);
            }
            else if(vertical)Vertical(root,0,1.04f,ref rail);
            else Horizontal(root,0,1.04f,ref rail);
            Draw(root,"Post",_post,Vector2.zero,new Vector2(.185f,.78f),0,10);
            YSort.Ensure(owner);
        }
        private static void Horizontal(Transform root,float center,float length,ref int index)
        {
            for(int i=0;i<3;i++)Draw(root,"Rail"+index++,_rail,new Vector2(center,.15f+i*.215f),new Vector2(length,.14f),0,0);
        }
        private static void Vertical(Transform root,float center,float length,ref int index)
        {
            // Rails recede along the ground axis; their common elevation matches horizontal rails.
            for(int i=0;i<3;i++)Draw(root,"Rail"+index++,_rail,new Vector2((i-1)*.025f,center+.15f+i*.215f),new Vector2(length,.055f),90,0);
        }
        private static void Draw(Transform root,string name,Sprite sprite,Vector2 position,Vector2 size,float angle,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(root,false);go.transform.localPosition=position;
            go.transform.localRotation=Quaternion.Euler(0,0,angle);
            go.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=order;
            renderer.color=root.parent.GetComponent<SpriteRenderer>().color;
        }
        public static void DrawHorizontalPreview(Vector2 ground,float unit,Color tint)
        {
            LoadParts();if(_post==null)return;
            var old=GUI.color;GUI.color=tint;
            for(int i=0;i<3;i++)DrawPart(new Rect(ground.x-.52f*unit,ground.y-(.22f+i*.215f)*unit,1.04f*unit,.14f*unit),_rail);
            DrawPart(new Rect(ground.x-.0925f*unit,ground.y-.78f*unit,.185f*unit,.78f*unit),_post);
            GUI.color=old;
        }
        private static void DrawPart(Rect destination,Sprite sprite)
        {
            var rect=sprite.rect;var texture=sprite.texture;
            GUI.DrawTextureWithTexCoords(destination,texture,new Rect(rect.x/texture.width,rect.y/texture.height,rect.width/texture.width,rect.height/texture.height),true);
        }
        public static void ApplyGate(SpriteRenderer body)
        {
            if(body==null || body.sprite==null)return;
            body.forceRenderingOff=true;
            var root=body.transform.Find("__FenceGateArt");
            if(root==null){root=new GameObject("__FenceGateArt").transform;root.SetParent(body.transform,false);root.gameObject.AddComponent<SpriteRenderer>();}
            var sprite=body.sprite;root.GetComponent<SpriteRenderer>().sprite=sprite;
            var scale=new Vector3(1.04f/sprite.bounds.size.x,.84f/sprite.bounds.size.y,1);
            root.localScale=scale;root.localPosition=new Vector3(-sprite.bounds.center.x*scale.x,-sprite.bounds.min.y*scale.y,0);
        }
    }
}
