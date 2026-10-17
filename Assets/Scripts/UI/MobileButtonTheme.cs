using UnityEngine;

namespace IL6
{
    public enum HudGlyph { Sword, Bow, Hammer, Hand, Door, Auto, Menu, People }
    public static class MobileButtonTheme
    {
        private static Texture2D _disc;
        public static readonly Color Jade=new Color(0.13f,0.67f,0.48f);
        public static readonly Color Gold=new Color(0.78f,0.57f,0.24f);
        public static bool Contains(Rect r,Vector2 p) => (p-r.center).sqrMagnitude<=r.width*r.width*0.25f;
        private static void Disc(Rect r,Color c)
        {
            if(_disc==null)
            {
                _disc=new Texture2D(128,128,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave,filterMode=FilterMode.Bilinear};
                var pixels=new Color[128*128];
                for(int y=0;y<128;y++) for(int x=0;x<128;x++)
                {
                    float d=Vector2.Distance(new Vector2(x+0.5f,y+0.5f),new Vector2(64,64));
                    pixels[y*128+x]=new Color(1,1,1,Mathf.Clamp01(64-d));
                }
                _disc.SetPixels(pixels);_disc.Apply(false,true);
            }
            var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,_disc);GUI.color=old;
        }
        private static Rect Inset(Rect r,float n)=>new Rect(r.x+n,r.y+n,r.width-2*n,r.height-2*n);
        public static void RoundRect(Rect r,float radius,Color color)
        {
            UiTheme.Rect(new Rect(r.x+radius,r.y,r.width-2*radius,r.height),color);
            UiTheme.Rect(new Rect(r.x,r.y+radius,r.width,r.height-2*radius),color);
            Disc(new Rect(r.x,r.y,radius*2,radius*2),color);
            Disc(new Rect(r.xMax-radius*2,r.y,radius*2,radius*2),color);
            Disc(new Rect(r.x,r.yMax-radius*2,radius*2,radius*2),color);
            Disc(new Rect(r.xMax-radius*2,r.yMax-radius*2,radius*2,radius*2),color);
        }
        public static bool Pill(Rect r,string label,GUIStyle style,bool enabled)
        {
            bool usable=GUI.enabled && enabled;
            bool hover=r.Contains(MobileHudLayout.GuiPoint(Input.mousePosition));
            if (!GeneratedUiSkin.Pill(r,usable ? (hover ? new Color(1f,1f,.9f) : Color.white) : new Color(.6f,.65f,.65f,.65f)))
            {
                RoundRect(r,Mathf.Min(8,r.height*.3f),new Color(.64f,.75f,.64f,usable?.8f:.3f));
                RoundRect(Inset(r,1),Mathf.Min(7,r.height*.3f),usable ? (hover ? new Color(.18f,.42f,.34f,.98f) : new Color(.12f,.26f,.23f,.96f)) : new Color(.13f,.18f,.18f,.8f));
            }
            var text=new GUIStyle(style){alignment=TextAnchor.MiddleCenter};
            text.normal.background=null;text.hover.background=null;text.active.background=null;text.focused.background=null;
            text.normal.textColor=usable ? new Color(1,.99f,.9f) : new Color(.57f,.63f,.59f);
            HudText.Label(r,label,text);
            bool pointer=Event.current.type==EventType.MouseUp;
            bool previous=GUI.enabled;GUI.enabled=usable;
            bool clicked=GUI.Button(r,"",GUIStyle.none);GUI.enabled=previous;
            return clicked && pointer;
        }
        public static void Pad(Rect r,Vector2 axis)
        {
            if (GeneratedUiSkin.Atlas(r,8,new Color(1,1,1,.65f)))
            {
                var thumb=r.center+new Vector2(axis.x,-axis.y)*Mathf.Min(40,r.width*.24f);
                GeneratedUiSkin.Atlas(new Rect(thumb.x-27,thumb.y-27,54,54),9,Color.white);
                return;
            }
            Disc(r,new Color(0.03f,0.09f,0.09f,0.22f));
            Disc(Inset(r,20),new Color(0.75f,0.94f,0.84f,0.15f));
            Disc(Inset(r,23),new Color(0.04f,0.1f,0.1f,0.25f));
            var center=r.center+new Vector2(axis.x,-axis.y)*Mathf.Min(40,r.width*.24f);
            Disc(new Rect(center.x-27,center.y-27,54,54),new Color(0.8f,0.96f,0.88f,0.75f));
            Disc(new Rect(center.x-22,center.y-22,44,44),new Color(0.26f,0.66f,0.5f,0.8f));
        }
        public static bool Button(Rect r,string label,string key,HudGlyph glyph,Color tint,bool enabled=true,bool selected=false)
        {
            bool usable=enabled && GUI.enabled;
            float alpha=usable ? 1f : 0.38f;
            Disc(new Rect(r.x-5,r.y-5,r.width+10,r.height+10),new Color(tint.r,tint.g,tint.b,selected?0.35f:0.12f));
            float artSize=r.width*(r.width>=100?.9f:.78f);
            bool generated=GeneratedUiSkin.Atlas(new Rect(r.center.x-artSize/2,r.y,artSize,artSize),(int)glyph,new Color(1,1,1,alpha));
            if (!generated)
            {
                Disc(new Rect(r.x,r.y+3,r.width,r.height),new Color(0.02f,0.05f,0.04f,0.45f));
                Disc(r,new Color(0.84f,0.93f,0.82f,0.8f*alpha));
                Disc(Inset(r,3),new Color(tint.r,tint.g,tint.b,0.94f*alpha));
                Disc(Inset(r,7),new Color(tint.r*0.78f,tint.g*0.72f,tint.b*0.78f,0.92f*alpha));
                float size=r.width>=90?40:24;
                var icon=new Rect(r.center.x-size/2,r.y+r.height*0.19f,size,size);
                DrawGlyph(icon,glyph,new Color(1,0.98f,0.88f,alpha));
            }
            var text=new GUIStyle(GUI.skin.label){fontSize=r.width>=90?15:12,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(1,1,0.94f,alpha)}};
            var nameRect=new Rect(r.x-6,r.yMax-25,r.width+12,22);
            if(generated) RoundRect(new Rect(r.x+2,r.yMax-24,r.width-4,22),6,new Color(.02f,.09f,.08f,.88f*alpha));
            HudText.Label(nameRect,label,text);
            if(!string.IsNullOrEmpty(key))
            {
                float w=Mathf.Max(22,key.Length*7+10);
                var badge=new Rect(r.xMax-w+5,generated?r.y-2:r.yMax-9,w,20);
                UiTheme.Rect(badge,new Color(0.04f,0.09f,0.09f,0.9f));
                HudText.Label(badge,key,new GUIStyle(text){fontSize=11});
            }
            bool pointerRelease=Event.current.type==EventType.MouseUp;
            return usable && GUI.Button(r,"",GUIStyle.none) && pointerRelease && Contains(r,MobileHudLayout.GuiPoint(Input.mousePosition));
        }
        private static void Line(Vector2 a,Vector2 b,float width,Color c)
        {
            var matrix=GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg,a);
            UiTheme.Rect(new Rect(a.x,a.y-width/2,Vector2.Distance(a,b),width),c);
            GUI.matrix=matrix;
        }
        private static void DrawGlyph(Rect r,HudGlyph glyph,Color c)
        {
            Vector2 P(float x,float y)=>new Vector2(r.x+x*r.width,r.y+y*r.height);
            void L(float x,float y,float a,float b)=>Line(P(x,y),P(a,b),Mathf.Max(2,r.width*0.08f),c);
            switch(glyph)
            {
                case HudGlyph.Sword: L(.2f,.85f,.78f,.15f);L(.16f,.6f,.44f,.86f);L(.62f,.19f,.78f,.15f);L(.78f,.15f,.8f,.32f);break;
                case HudGlyph.Bow: L(.3f,.1f,.6f,.3f);L(.6f,.3f,.7f,.5f);L(.7f,.5f,.6f,.7f);L(.6f,.7f,.3f,.9f);L(.3f,.1f,.3f,.9f);L(.1f,.5f,.9f,.5f);L(.74f,.37f,.9f,.5f);L(.74f,.63f,.9f,.5f);break;
                case HudGlyph.Hammer: L(.3f,.9f,.65f,.3f);L(.28f,.2f,.85f,.48f);L(.33f,.1f,.9f,.38f);L(.28f,.2f,.33f,.1f);L(.85f,.48f,.9f,.38f);break;
                case HudGlyph.Hand: L(.3f,.8f,.12f,.5f);L(.12f,.5f,.22f,.4f);L(.22f,.4f,.35f,.57f);L(.35f,.57f,.35f,.15f);L(.5f,.6f,.5f,.1f);L(.65f,.6f,.65f,.17f);L(.8f,.6f,.8f,.3f);L(.8f,.6f,.7f,.9f);L(.7f,.9f,.3f,.8f);break;
                case HudGlyph.Door: L(.2f,.9f,.2f,.1f);L(.2f,.1f,.75f,.1f);L(.75f,.1f,.75f,.9f);L(.75f,.9f,.2f,.9f);L(.2f,.1f,.57f,.25f);L(.57f,.25f,.57f,.8f);L(.57f,.8f,.2f,.9f);break;
                case HudGlyph.Auto: L(.2f,.25f,.78f,.25f);L(.78f,.25f,.64f,.1f);L(.78f,.25f,.64f,.4f);L(.8f,.75f,.22f,.75f);L(.22f,.75f,.36f,.6f);L(.22f,.75f,.36f,.9f);break;
                case HudGlyph.Menu: L(.15f,.2f,.85f,.2f);L(.15f,.5f,.85f,.5f);L(.15f,.8f,.85f,.8f);break;
                case HudGlyph.People: Disc(new Rect(r.x+r.width*.35f,r.y,r.width*.3f,r.height*.3f),c);L(.5f,.4f,.5f,.85f);L(.2f,.8f,.5f,.4f);L(.5f,.4f,.8f,.8f);break;
            }
        }
    }
}
