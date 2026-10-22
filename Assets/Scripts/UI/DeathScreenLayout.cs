using UnityEngine;
namespace IL6
{
    public readonly struct DeathScreenLayout
    {
        public readonly Rect Panel;
        public DeathScreenLayout(float width, float height)
        {
            float w=Mathf.Min(640f,width-32f), h=Mathf.Min(528f,height-32f);
            Panel=new Rect((width-w)/2f,(height-h)/2f,w,h);
        }
        public Rect Kicker => new Rect(Panel.x+28,Panel.y+18,Panel.width-56,24);
        public Rect Title => new Rect(Panel.x+28,Panel.y+48,Panel.width-56,46);
        public Rect Subtitle => new Rect(Panel.x+28,Panel.y+100,Panel.width-56,26);
        public Rect Metric(int index)
        {
            float w=(Panel.width-72f)/2f;
            return new Rect(Panel.x+28+(index%2)*(w+16),Panel.y+148+(index/2)*90,w,78);
        }
        public Rect Score => new Rect(Panel.x+28,Panel.y+332,Panel.width-56,52);
        public Rect Restart => new Rect(Panel.center.x-140,Panel.y+408,280,58);
        public Rect Hint => new Rect(Panel.x+28,Panel.y+478,Panel.width-56,26);
    }
}
