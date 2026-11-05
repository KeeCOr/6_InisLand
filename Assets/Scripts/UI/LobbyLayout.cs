using UnityEngine;
namespace IL6
{
    public static class LobbyLayout
    {
        public static Rect Panel(float width,float height)=>new Rect((width-1000)/2,(height-460)/2+44,1000,460);
        public static Rect Slot(Rect panel,int index)=>new Rect(panel.x+24+index*320,panel.y+92,312,328);
        public static Rect Continue(Rect slot)=>new Rect(slot.x+20,slot.y+190,slot.width-40,48);
        public static Rect New(Rect slot)=>new Rect(slot.x+20,slot.y+250,slot.width-40,48);
    }
}
