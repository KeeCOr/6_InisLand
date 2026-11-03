using System.Collections.Generic;
using UnityEngine;
namespace IL6
{
    public static class DialoguePortraits
    {
        private static readonly Dictionary<string,Texture2D> Cache=new Dictionary<string,Texture2D>();
        public static Texture2D ForRole(string role)
        {
            string key=(role??"").ToLowerInvariant() switch
            {
                "father" or "아버지" => "father",
                "player" or "hero" or "주인공" => "hero",
                "hunter" or "사냥꾼" => "hunter",
                "warrior" or "전사" => "warrior",
                "farmer" or "농부" => "farmer",
                "cook" or "요리사" => "cook",
                "healer" or "치유사" => "healer",
                _ => SpriteBank.IsChildRole(role)?"child":SpriteBank.IsFemaleRole(role)?"aunt":"uncle"
            };
            if(Cache.TryGetValue(key,out var portrait) && portrait!=null) return portrait;
            portrait=Resources.Load<Texture2D>("UI/DialoguePortraits/"+key);
            if(portrait!=null) Cache[key]=portrait;
            return portrait;
        }
    }
}
