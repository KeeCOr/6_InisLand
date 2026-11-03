using UnityEngine;
namespace IL6
{
    // World props are independent of the HUD icon atlas and its layout metrics.
    public static class GroundResourceArt
    {
        public static string Key(ResourceKind kind)=>kind switch
        {
            ResourceKind.Wood=>"wood",ResourceKind.Stone=>"stone",ResourceKind.Ore=>"ore",
            ResourceKind.Meat=>"meat",ResourceKind.Food=>"food",_=>"frostbloom"
        };
        public static Sprite Drop(ResourceKind kind)=>SpriteBank.Load("Environment/GroundProps20261103/drops/"+Key(kind));
        public static Sprite Site(ResourceKind kind)=>SpriteBank.Load("Environment/GroundProps20261103/sites/"+Key(kind));
    }
}
