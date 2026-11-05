using UnityEngine;

namespace IL6
{
    public static class AdventureMenuLayout
    {
        public static Rect Panel(float width,float height)=>new Rect((width-600)/2f,(height-640)/2f,600,640);
        public static Rect Panel()=>Panel(MobileHudLayout.Width,MobileHudLayout.Height);
        public static Rect Title(Rect p)=>new Rect(p.x+24,p.y+14,p.width-48,32);
        public static Rect Continue(Rect p)=>new Rect(p.x+24,p.y+64,(p.width-60)/2,48);
        public static Rect Save(Rect p)=>new Rect(p.center.x+6,p.y+64,(p.width-60)/2,48);
        public static Rect Research(Rect p)=>new Rect(p.x+24,p.y+124,p.width-48,48);
        public static Rect SettingsTitle(Rect p)=>new Rect(p.x+24,p.y+184,p.width-48,24);
        public static Rect Setting(Rect p,int index)=>new Rect(p.x+24,p.y+216+52*index,p.width-48,44);
        public static Rect SettingLabel(Rect row)=>new Rect(row.x,row.y+10,136,24);
        public static Rect SettingTrack(Rect row)=>new Rect(row.x+152,row.y+18,row.width-232,24);
        public static Rect SettingValue(Rect row)=>new Rect(row.xMax-64,row.y+10,64,24);
        public static Rect Credits(Rect p)=>new Rect(p.x+24,p.y+436,p.width-48,100);
        public static Rect Restart(Rect p)=>new Rect(p.x+24,p.y+560,p.width-48,48);
    }
}
