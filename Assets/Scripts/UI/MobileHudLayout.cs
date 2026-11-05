using UnityEngine;

namespace IL6
{
    // One landscape layout for desktop and touch, including ultrawide screens.
    public static class MobileHudLayout
    {
        public static float Scale => Mathf.Min(Screen.width / 1280f, Screen.height / 720f) * HudViewSettings.UiScale;
        public static int Width => Mathf.RoundToInt(Screen.width / Mathf.Max(0.01f, Scale));
        public static int Height => Mathf.RoundToInt(Screen.height / Mathf.Max(0.01f, Scale));
        public static Rect Status => new Rect(16,16,240,64);
        public static Rect Resources => ResourceStrip(Width);
        public static Rect ResourceStrip(float width) => new Rect(width-282,16,186,90);
        public static Rect Experience => new Rect(Resources.x,Resources.yMax+8,Resources.width,20);
        public static Rect ResourceTouchBounds => new Rect(Resources.x,Resources.y-2,Resources.width,94);
        public static Rect ResourceChip(int index) => new Rect(Resources.x+94*(index%2),Resources.y+30*(index/2),92,30);
        public static Rect ResourceIcon(int index) { var chip=ResourceChip(index);return new Rect(chip.x,chip.y+4,24,24); }
        public static Rect Goals => QuestTrackerLayout.Panel(Width,false);
        public static Rect Clock => new Rect(Width/2-94,16,188,44);
        public static Rect Actions => new Rect(Width-256,Height-214,240,198);
        public static Rect Movement => new Rect(16,Height-144,128,128);
        public static Rect Menu => new Rect(Width-70,16,54,74);
        public static Rect CompanionButton => new Rect(Movement.xMax+12,Movement.yMax-80,56,80);
        public static Rect CompanionSummary(bool expanded) => new Rect(16,96,240,expanded?220:96);
        public static Rect ActionButton(int slot)
        {
            var r=Actions;
            switch(slot)
            {
                case 0:return new Rect(r.x+40,r.y+118,60,80);
                case 1:return new Rect(r.x+106,r.y+118,60,80);
                case 2:return new Rect(r.x+172,r.y+118,60,80);
                case 3:return new Rect(r.x+132,r.y,108,112);
                case 4:return new Rect(r.x+52,r.y+20,64,76);
                default:return new Rect(r.x+46,r.y,54,54);
            }
        }
        public static Rect[] Regions(int width, int height)
        {
            float s = Mathf.Min(width / 1280f, height / 720f);
            float w = width / s, h = height / s;
            var r = new[] {
                new Rect(16,16,240,64),ResourceStrip(w),
                QuestTrackerLayout.Panel(w,false),new Rect(w-256,h-214,240,198),
                new Rect(16,h-144,128,128),new Rect(w-70,16,54,74),
                new Rect(w/2-94,16,188,44),new Rect(16,96,240,96),new Rect(156,h-96,56,80),
                new Rect(w-282,114,186,20)
            };
            for (int i = 0; i < r.Length; i++)
                r[i] = new Rect(r[i].x*s, r[i].y*s, r[i].width*s, r[i].height*s);
            return r;
        }
        public static Vector2 GuiPoint(Vector2 screenPoint)
            => new Vector2(screenPoint.x, Screen.height - screenPoint.y) / Mathf.Max(0.01f, Scale);
        public static Vector2 MoveAt(Vector2 point)
        {
            if (!Movement.Contains(point)) return Vector2.zero;
            var delta = point - Movement.center;
            if (delta.magnitude < Movement.width*.11f) return Vector2.zero;
            return Vector2.ClampMagnitude(new Vector2(delta.x,-delta.y)/(Movement.width*.35f),1f);
        }
    }
}
