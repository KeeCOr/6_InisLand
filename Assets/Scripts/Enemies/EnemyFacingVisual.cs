using System.Collections.Generic;
using UnityEngine;
namespace IL6
{
    public static class EnemyFacingVisual
    {
        public static readonly string[] Keys={"quadruped_robot","zombie","boss_frost_zombie","boss_winter_knight","boss_iron_giant","boss_frost_lich"};
        private static readonly Dictionary<string,Sprite> Cache=new();
        public static Sprite Frame(string key,CharacterFacing facing)
        {
            string name=key+"_"+(facing==CharacterFacing.Front?"front":facing==CharacterFacing.Back?"back":"side");
            if(!Cache.TryGetValue(name,out var sprite))
            {
                sprite=Resources.Load<Sprite>("Sprites/Enemies/Directions20261103/"+name);
                if(sprite!=null)Cache[name]=sprite;
            }
            return sprite;
        }
        public static CharacterFacing Apply(SpriteRenderer renderer,Vector2 direction,CharacterFacing previous)
        {
            if(renderer==null || direction.sqrMagnitude<.0025f)return previous;
            var facing=DirectionalCharacterVisual.ResolveFacing(direction,previous);
            if(renderer.sprite!=null)
            {
                string name=renderer.sprite.name;
                foreach(string key in Keys)
                {
                    if(name!=key && name!=key+"_front" && name!=key+"_back" && name!=key+"_side")continue;
                    var frame=Frame(key,facing);if(frame!=null)renderer.sprite=frame;
                    break;
                }
            }
            renderer.flipX=facing==CharacterFacing.Left;
            return facing;
        }
    }
}
