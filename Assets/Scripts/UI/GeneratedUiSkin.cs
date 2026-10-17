using UnityEngine;

namespace IL6
{
    public static class GeneratedUiSkin
    {
        [System.Serializable] private sealed class Region
        {
            public float x,y,width,height;
            public Rect Rect => new Rect(x,y,width,height);
        }
        [System.Serializable] private sealed class Metrics { public Region panel; public Region pill; }
        private static Texture2D _buttons, _panel, _pill;
        private static Metrics _metrics;
        private static Texture2D Load(ref Texture2D texture,string name)
            => texture != null ? texture : texture=Resources.Load<Texture2D>("UI/Generated20261014/"+name);
        public static bool Atlas(Rect target,int index,Color color)
        {
            var texture=Load(ref _buttons,"buttons");
            if(texture==null) return false;
            const float margin=.01f;
            var uv=new Rect((index%4+margin)/4f,(2-index/4+margin)/3f,(1-2*margin)/4f,(1-2*margin)/3f);
            var old=GUI.color;GUI.color=color;
            GUI.DrawTextureWithTexCoords(target,texture,uv,true);GUI.color=old;
            return true;
        }
        public static bool Panel(Rect target,Color color) => NineSlice(target,Load(ref _panel,"panel"),false,color);
        public static bool Pill(Rect target,Color color) => NineSlice(target,Load(ref _pill,"pill"),true,color);
        public static Rect SourceBounds(bool pill)
        {
            if(_metrics==null)
            {
                var data=Resources.Load<TextAsset>("UI/Generated20261014/metrics");
                _metrics=data!=null ? JsonUtility.FromJson<Metrics>(data.text) : new Metrics {panel=new Region {width=1,height=1},pill=new Region {width=1,height=1}};
            }
            return (pill?_metrics.pill:_metrics.panel).Rect;
        }
        private static bool NineSlice(Rect target,Texture2D texture,bool pill,Color color)
        {
            if(texture==null || target.width<=0 || target.height<=0) return false;
            var uv=SourceBounds(pill);
            float borderPixels=Mathf.Min(uv.width*texture.width,uv.height*texture.height)*(pill?.45f:.16f);
            float bx=borderPixels/texture.width,by=borderPixels/texture.height;
            float corner=Mathf.Min(pill?12:18,Mathf.Min(target.width,target.height)*.4f);
            float[] xs={target.x,target.x+corner,target.xMax-corner},ys={target.y,target.y+corner,target.yMax-corner};
            float[] widths={corner,target.width-2*corner,corner},heights={corner,target.height-2*corner,corner};
            float[] us={uv.x,uv.x+bx,uv.xMax-bx},vs={uv.yMax-by,uv.y+by,uv.y};
            float[] uw={bx,uv.width-2*bx,bx},vh={by,uv.height-2*by,by};
            var old=GUI.color;GUI.color=color;
            for(int y=0;y<3;y++) for(int x=0;x<3;x++)
                GUI.DrawTextureWithTexCoords(new Rect(xs[x],ys[y],widths[x],heights[y]),texture,new Rect(us[x],vs[y],uw[x],vh[y]),true);
            GUI.color=old;return true;
        }
    }
}
