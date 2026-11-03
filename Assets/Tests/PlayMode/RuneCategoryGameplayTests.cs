using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public sealed class RuneCategoryGameplayTests
    {
        [UnityTest] public IEnumerator MeleePoisonAndRangedIceOnlyAffectTheirOwnWeapon()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            var actor=new GameObject("Category effects actor");actor.transform.position=new Vector3(8200,8200,0);
            var progression=actor.AddComponent<PlayerProgression>();progression.ApplyRune(RuneKind.PoisonBlade);progression.ApplyRune(RuneKind.IceArrow);
            var attack=actor.AddComponent<PlayerAttackController>();attack.enabled=false;attack.Progression=progression;
            var first=new GameObject("Melee category target");first.transform.position=actor.transform.position+Vector3.right;
            var a=first.AddComponent<Zombie>();a.enabled=false;a.InitHp(1000);
            var second=new GameObject("Ranged category target");second.transform.position=actor.transform.position+Vector3.right*3;
            var b=second.AddComponent<Zombie>();b.enabled=false;b.InitHp(1000);
            var arrowGo=new GameObject("Category arrow");var arrow=arrowGo.AddComponent<Projectile>();arrow.OwnerProgression=progression;
            try {
                typeof(PlayerAttackController).GetMethod("DealImmediate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(attack,new object[]{a,10});
                Assert.That(a.PoisonRemainingSec,Is.GreaterThan(0));Assert.That(a.SlowRemainingSec,Is.Zero);
                arrow.ApplyEffectsTo(b);Assert.That(b.PoisonRemainingSec,Is.Zero);Assert.That(b.SlowRemainingSec,Is.GreaterThan(0));
                progression.ApplyRune(RuneKind.MultiShot);Assert.That(progression.MultiShotExtra,Is.EqualTo(1));Assert.That(progression.MeleeExtraTargets,Is.Zero);
                progression.ApplyRune(RuneKind.MeleeCleave);Assert.That(progression.MeleeExtraTargets,Is.EqualTo(1));Assert.That(progression.MultiShotExtra,Is.EqualTo(1));
            } finally {Object.Destroy(actor);Object.Destroy(first);Object.Destroy(second);Object.Destroy(arrowGo);Time.timeScale=1;InputReader.HudBlocked=false;}
            yield return new WaitForSeconds(.4f);
        }
    }
}
