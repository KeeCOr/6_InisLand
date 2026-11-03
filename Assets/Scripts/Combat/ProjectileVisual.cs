using UnityEngine;
namespace IL6
{
    // Physical missiles share neutral art; the enemy shaft has a muted rust tint.
    public static class ProjectileVisual
    {
        public static bool Apply(GameObject body,bool arrow,bool hostile=false)
        {
            var sr=body.GetComponent<SpriteRenderer>();
            var sprite=Resources.Load<Sprite>(arrow?"Sprites/FX/nordic_arrow":"Sprites/FX/throwing_stone");
            if(sr==null || sprite==null) return false;
            sr.sprite=sprite;sr.color=hostile?new Color(1f,.70f,.58f):Color.white;
            float size=arrow?.75f:.4f;
            body.transform.localScale=Vector3.one*(size/Mathf.Max(.001f,sprite.bounds.size.x));
            return true;
        }
        public static void Orient(Transform body,Vector2 direction)
        {
            body.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
        }
    }
}
