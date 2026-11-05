using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public sealed class HomeNavigationGameplayTests
    {
        [UnityTest] public IEnumerator DestroyedHouseAndNearbyManualFencesCannotRedirectReturnGuidance()
        {
            Vector2 village=new Vector2(GameConstants.VillageCenterX,GameConstants.VillageCenterY);
            var house=BuildingFactory.SpawnHouse(village+new Vector2(-2,-2));
            var fence=VillageStarter.SpawnFence(village+Vector2.right*31,0,FencePieceType.Center);
            try {
                var selected=HomeNavigation.Resolve(village+Vector2.right*32,new[]{fence.GetComponent<Building>(),house.GetComponent<Building>()});
                Assert.That(selected.HasHouse,Is.True);Assert.That(selected.Label,Does.StartWith("집까지 "));
                house.GetComponent<Building>().TakeDamage(100000);
                var fallback=HomeNavigation.Resolve(village+Vector2.right*32,new[]{fence.GetComponent<Building>(),house.GetComponent<Building>()});
                Assert.That(fallback.HasHouse,Is.False);Assert.That(fallback.Destination,Is.EqualTo(village));
                Assert.That(fallback.Label,Is.EqualTo("마을까지 32칸"));
                Assert.That(Resources.Load<Texture2D>(HomeNavigation.IconPath),Is.Not.Null);
            } finally {if(house!=null)Object.Destroy(house);Object.Destroy(fence);}
            yield return new WaitForSeconds(.4f);
        }
    }
}
