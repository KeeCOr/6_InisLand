using UnityEngine;
namespace IL6
{
    public readonly struct RuneChoiceLayout
    {
        public readonly Rect Panel;
        public RuneChoiceLayout(float width,float height) {Panel=new Rect((width-940)/2,(height-560)/2,940,560);}
        public Rect Card(int index)=>new Rect(Panel.x+30+index*300,Panel.y+64,280,366);
        public Rect Details=>new Rect(Panel.x+24,Panel.y+448,Panel.width-48,90);
        public static Rect Info(Rect card)=>new Rect(card.xMax-54,card.y+8,44,44);
        public static Rect Title(Rect card)=>new Rect(card.x+54,card.y+8,card.width-108,44);
        public static Rect Icon(Rect card)=>new Rect(card.center.x-52,card.y+54,104,104);
        public static Rect Trigger(Rect card)=>new Rect(card.x+16,card.y+162,card.width-32,22);
        public static Rect Progress(Rect card)=>new Rect(card.center.x-36,card.y+190,72,12);
        public static Rect Stat(Rect card,int index)=>Stat(card,index,4);
        public static Rect Stat(Rect card,int index,int count)
        {
            int rows=Mathf.Clamp(count,1,4);
            return new Rect(card.center.x-80,card.y+212+(4-rows)*17+index*34,160,32);
        }
    }
}
