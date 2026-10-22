using UnityEngine;

namespace IL6
{
    // One landscape layout for desktop and touch, including ultrawide screens.
    public static class MobileHudLayout
    {
        public static float Scale => Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        public static int Width => Mathf.RoundToInt(Screen.width / Mathf.Max(0.01f, Scale));
        public static int Height => Mathf.RoundToInt(Screen.height / Mathf.Max(0.01f, Scale));
        public static Rect Status => new Rect(16,16,248,76);
        public static Rect Resources => new Rect(16,98,300,64);
        public static Rect ResourceTouchBounds => new Rect(Resources.x,Resources.y-2,Resources.width,68);
        public static Rect ResourceChip(int index) => new Rect(Resources.x+100*(index<3?index:index-3),Resources.y+(index<3?0:32),100,32);
        public static Rect ResourceIcon(int index) { var chip=ResourceChip(index);return new Rect(chip.x,chip.y+4,24,24); }
        public static Rect Goals => new Rect(Width-280,92,264,210);
        public static Rect Clock => new Rect(Width/2-94,16,188,44);
        public static Rect Actions => new Rect(Width-336,Height-198,320,182);
        public static Rect Movement => new Rect(16,Height-160,144,144);
        public static Rect Menu => new Rect(Width-64,16,48,48);
        public static Rect CompanionButton => new Rect(Movement.xMax+12,Movement.yMax-54,54,54);
        public static Rect CompanionSummary(bool expanded) => new Rect(16,180,248,expanded?220:96);
        public static Rect ActionButton(int slot)
        {
            var r=Actions;
            switch(slot)
            {
                case 0:return new Rect(r.x+106,r.y,54,54);
                case 1:return new Rect(r.x+166,r.y,54,54);
                case 2:return new Rect(r.x+226,r.y,54,54);
                case 3:return new Rect(r.x+200,r.y+62,120,120);
                case 4:return new Rect(r.x+116,r.y+106,76,76);
                default:return new Rect(r.x+46,r.y,54,54);
            }
        }
        public static Rect[] Regions(int width, int height)
        {
            float s = Mathf.Min(width / 1280f, height / 720f);
            float w = width / s, h = height / s;
            var r = new[] {
                new Rect(16,16,248,76),new Rect(16,98,300,64),
                new Rect(w-280,92,264,210),new Rect(w-336,h-198,320,182),
                new Rect(16,h-160,144,144),new Rect(w-64,16,48,48),
                new Rect(w/2-94,16,188,44),new Rect(16,180,248,96),new Rect(172,h-70,54,54)
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
