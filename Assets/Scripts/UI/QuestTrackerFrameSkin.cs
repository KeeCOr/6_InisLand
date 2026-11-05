using UnityEngine;
namespace IL6
{
    public static class QuestTrackerFrameSkin
    {
        public const string FramePath="UI/Quest20261104/frame";
        public const string RibbonPath="UI/Quest20261104/title_ribbon";
        private static Texture2D _ribbon,_frame;
        public const float BodyOpacity=.92f;
        public static Rect Body(Rect panel)=>new Rect(panel.x,panel.y+38,panel.width,panel.height-38);
        public static Rect Slice(Rect panel,int x,int y)
        {
            float edge=Mathf.Min(16,Mathf.Min(panel.width,panel.height)/2);
            float left=x==0?panel.x:x==1?panel.x+edge:panel.xMax-edge;
            float top=y==0?panel.yMax-edge:y==1?panel.y+edge:panel.y;
            return new Rect(left,top,x==1?panel.width-2*edge:edge,y==1?panel.height-2*edge:edge);
        }
        public static Rect Source(int width,int height,int x,int y)
        {
            float u=160f/width,v=160f/height;
            return new Rect(x==0?0:x==1?u:1-u,y==0?0:y==1?v:1-v,x==1?1-2*u:u,y==1?1-2*v:v);
        }
        public static void Draw(Rect panel)
        {
            if(_ribbon==null)_ribbon=Resources.Load<Texture2D>(RibbonPath);
            if(_frame==null)_frame=Resources.Load<Texture2D>(FramePath);
            var old=GUI.color;
            // A single continuous slate plate keeps white copy readable over snow without strip seams.
            GUI.color=new Color(1,1,1,BodyOpacity);
            if(_frame!=null)GUI.DrawTextureWithTexCoords(Body(panel),_frame,new Rect(.35f,.35f,.3f,.3f),true);
            GUI.color=Color.white;
            if(_ribbon!=null)
            {
                float cap=40,u=160f/_ribbon.width;
                GUI.DrawTextureWithTexCoords(new Rect(panel.x,panel.y,cap,40),_ribbon,new Rect(0,0,u,1),true);
                GUI.DrawTextureWithTexCoords(new Rect(panel.x+cap,panel.y,panel.width-2*cap,40),_ribbon,new Rect(u,0,1-2*u,1),true);
                GUI.DrawTextureWithTexCoords(new Rect(panel.xMax-cap,panel.y,cap,40),_ribbon,new Rect(1-u,0,u,1),true);
            }
            GUI.color=old;
        }
    }
}
