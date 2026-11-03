using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class AnimalAttackAlertTests
    {
        [UnityTest]
        public IEnumerator PredatorSeesRemoteTargetsOnlyDuringRefreshedDamageAlert()
        {
            Time.timeScale=1;
            var target=new GameObject("Remote living companion");
            target.transform.position=new Vector3(9000,9000,0);
            var companion=target.AddComponent<Companion>();companion.enabled=false;
            var animal=new GameObject("Alert wolf");
            animal.transform.position=target.transform.position+Vector3.right*100;
            var wolf=animal.AddComponent<WolfAi>();wolf.InitHp(100);wolf.enabled=false;
            var find=typeof(WolfAi).GetMethod("FindTarget",BindingFlags.NonPublic|BindingFlags.Instance);
            try
            {
                yield return null;
                Assert.That(find.Invoke(wolf,null),Is.Not.EqualTo(target.transform));
                Assert.That(wolf.AttackAlertDuration,Is.EqualTo(5));
                wolf.AttackAlertDuration=.6f;
                wolf.TakeDamage(1,(Vector2)target.transform.position);
                Assert.That(find.Invoke(wolf,null),Is.EqualTo(target.transform));
                Time.timeScale=0;
                yield return new WaitForSecondsRealtime(.7f);
                Assert.That(wolf.IsAttackAlert,Is.True,"Pause must preserve the alert");
                Time.timeScale=1;
                yield return new WaitForSeconds(.4f);
                wolf.TakeDamage(1);
                yield return new WaitForSeconds(.3f);
                Assert.That(find.Invoke(wolf,null),Is.EqualTo(target.transform),"Every hit refreshes the timer");
                yield return new WaitForSeconds(.4f);
                Assert.That(wolf.IsAttackAlert,Is.False);
                Assert.That(find.Invoke(wolf,null),Is.Not.EqualTo(target.transform));
                wolf.TakeDamage(0);Assert.That(wolf.IsAttackAlert,Is.False);
            }
            finally {Time.timeScale=1;Object.Destroy(animal);Object.Destroy(target);}
        }
    }
}
