using UnityEngine;

namespace IL6
{
    // Opposing actors can overlap without transferring locomotion impulses.
    // Combat uses distance/overlap queries, so damage and explicit knockback remain active.
    public static class ActorBodyCollision
    {
        public const int FriendlyLayer=8;
        public const int EnemyLayer=9;

        public static void Configure(GameObject actor,bool enemy)
        {
            Physics2D.IgnoreLayerCollision(FriendlyLayer,EnemyLayer,true);
            actor.layer=enemy?EnemyLayer:FriendlyLayer;
        }
    }
}
