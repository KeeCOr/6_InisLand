using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class AnimalFleePacingTests
    {
        [UnityTest] public IEnumerator CompanionShotsMakeRemotePreyFleeFromEitherAttackDirection()
        {
            Time.timeScale=1;
            foreach(float side in new[]{-1f,1f})
            {
                var player=new GameObject("Distant player");player.transform.position=new Vector3(8100,8000,0);
                var prey=new GameObject("Deer hit reaction");prey.transform.position=new Vector3(8000,8000,0);
                var ai=prey.AddComponent<DeerAi>();ai.InitHp(100);
                var ally=new GameObject("Hunting companion");ally.transform.position=prey.transform.position+Vector3.right*side*4;
                var companion=ally.AddComponent<Companion>();companion.enabled=false;
                try
                {
                    yield return null;
                    typeof(AnimalAi).GetField("_player",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(ai,player.transform);
                    typeof(Companion).GetMethod("SpawnProjectile",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(companion,new object[]{ai});
                    float until=Time.time+3;
                    while(ai.CurrentHp==100 && Time.time<until)yield return null;
                    Assert.That(ai.CurrentHp,Is.LessThan(100),"The actual companion projectile must hit");
                    for(int i=0;i<4;i++)yield return new WaitForFixedUpdate();
                    var body=prey.GetComponent<Rigidbody2D>();
                    Assert.That(body.velocity.x*(-side),Is.GreaterThan(.1f),"Prey must flee away from the companion even with a distant player");
                    Assert.That(body.velocity.magnitude,Is.LessThanOrEqualTo(ai.FleeSpeed+.01f));
                    yield return new WaitForSeconds(ai.AttackAlertDuration+1);
                    Assert.That(body.velocity.magnitude,Is.LessThan(.01f),"A distant shot must not cause permanent fleeing");
                }
                finally {Object.Destroy(prey);Object.Destroy(ally);Object.Destroy(player);}
                yield return null;
            }
        }
        [UnityTest] public IEnumerator FleeingAcceleratesGraduallyAndStopsWhenThreatLeaves()
        {
            Time.timeScale=1;
            var player=new GameObject("Flee pacing threat");player.transform.position=new Vector3(8000,8000,0);
            var animal=new GameObject("Pacing prey");animal.transform.position=player.transform.position+Vector3.right*2;
            var ai=animal.AddComponent<DeerAi>();ai.FleeSpeed=3.6f;ai.FleeRadius=3.6f;ai.InitHp(10);
            yield return null;
            typeof(AnimalAi).GetField("_player",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(ai,player.transform);
            var body=animal.GetComponent<Rigidbody2D>();
            yield return new WaitForFixedUpdate();
            Assert.That(body.velocity.magnitude,Is.GreaterThan(0));
            Assert.That(body.velocity.magnitude,Is.LessThan(1f),"No instant leap to top speed");
            for(int i=0;i<30;i++) yield return new WaitForFixedUpdate();
            Assert.That(body.velocity.magnitude,Is.LessThanOrEqualTo(3.61f));
            Assert.That(body.velocity.x,Is.GreaterThan(0));
            player.transform.position+=Vector3.left*30;
            for(int i=0;i<30;i++) yield return new WaitForFixedUpdate();
            Assert.That(body.velocity.magnitude,Is.LessThan(.01f));
            Object.Destroy(animal);Object.Destroy(player);
        }
    }
}
