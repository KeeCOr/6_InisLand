using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class HitAndFenceWreckageTests
    {
        [UnityTest] public IEnumerator FuelUseHasNoImpactButCampfireCombatDamageDoes()
        {
            Time.timeScale=1;
            var fire=VillageStarter.SpawnCampfire(new Vector3(9200,9200,0));yield return null;
            var building=fire.GetComponent<Building>();int before=building.CurrentHp;
            building.ConsumeFuelHp(2);
            Assert.That(building.CurrentHp,Is.EqualTo(before-2));
            Assert.That(GameObject.Find("__HitImpact"),Is.Null);
            building.TakeDamage(1);
            Assert.That(GameObject.Find("__HitImpact"),Is.Not.Null,"Campfire child artwork must also receive combat feedback");
            Object.Destroy(fire);yield return new WaitForSeconds(.5f);
        }
        [UnityTest] public IEnumerator LethalHitLeavesBriefIndependentImpactWithoutBlockingMovement()
        {
            Time.timeScale=1;
            var go=new GameObject("Impact enemy probe");go.transform.position=new Vector3(9000,9000,0);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=SpriteBank.QuadrupedRobot();
            var enemy=go.AddComponent<Zombie>();enemy.enabled=false;enemy.InitHp(20);
            enemy.TakeDamage(20);yield return null;
            var impact=GameObject.Find("__HitImpact");
            Assert.That(impact,Is.Not.Null,"A lethal hit must still show its impact after the target disappears");
            Assert.That(impact.GetComponent<SpriteRenderer>().sprite,Is.Not.Null);
            Assert.That(impact.GetComponentInChildren<Collider2D>(),Is.Null);
            yield return new WaitForSeconds(.5f);
            Assert.That(impact==null,Is.True,"Hit effects must clean themselves up");
        }

        [UnityTest] public IEnumerator BrokenFenceLeavesNonblockingRubbleUntilRebuilt()
        {
            Time.timeScale=1;
            var position=new Vector3(9100,9100,0);
            var go=VillageStarter.SpawnFence(position,0,FencePieceType.Center);
            yield return null;
            var fence=go.GetComponent<Building>();fence.TakeDamage(fence.MaxHp);yield return null;
            var rubble=GameObject.Find("__FenceWreckage");
            Assert.That(rubble,Is.Not.Null,"A destroyed fence must leave a visible broken section");
            Assert.That(rubble.transform.position,Is.EqualTo(position));
            Assert.That(rubble.GetComponent<SpriteRenderer>().sprite,Is.Not.Null);
            Assert.That(rubble.GetComponentInChildren<Collider2D>(),Is.Null);
            Assert.That(rubble.GetComponent<Building>(),Is.Null);
            Assert.That(rubble.GetComponent<Gatherable>(),Is.Null);
            yield return new WaitForSeconds(.5f);Assert.That(rubble!=null,Is.True);
            var replacement=VillageStarter.SpawnFence(position,0,FencePieceType.Center);
            yield return null;yield return null;
            Assert.That(rubble==null,Is.True,"A rebuilt fence clears its old rubble");
            Object.Destroy(replacement);yield return null;
        }
    }
}
