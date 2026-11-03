using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public sealed class MeleeKnockbackAdvantageTests
    {
        private static Zombie Target(Vector2 position)
        {
            var go=new GameObject("Strong melee knockback test");go.transform.position=position;
            go.AddComponent<CircleCollider2D>().radius=.2f;
            var zombie=go.AddComponent<Zombie>();zombie.InitHp(1000);zombie.enabled=false;return zombie;
        }

        [UnityTest]
        public IEnumerator PushCreatesSpaceThenBrieflyKeepsTargetStaggeredAndRespectsPause()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            var target=Target(new Vector2(3400,3400));var body=target.GetComponent<Rigidbody2D>();
            Vector2 start=body.position;
            try
            {
                MeleeKnockback.Apply(target,start-Vector2.right);
                yield return new WaitForSeconds(.08f);
                Assert.That(MeleeKnockback.IsActive(target.gameObject),Is.True);
                Vector2 paused=body.position;Time.timeScale=0;
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(Vector2.Distance(body.position,paused),Is.LessThan(.001f));
                Time.timeScale=1;
                yield return new WaitForSeconds(.18f);
                Assert.That(Vector2.Distance(body.position,start),Is.InRange(1.35f,1.55f));
                Assert.That(body.velocity.magnitude,Is.LessThan(.01f));
                Assert.That(MeleeKnockback.IsActive(target.gameObject),Is.True,"Short recovery must still block AI pursuit/attacks after displacement");
                yield return new WaitForSeconds(.16f);
                Assert.That(MeleeKnockback.IsActive(target.gameObject),Is.False);
                Assert.That(Vector2.Distance(body.position,start),Is.InRange(1.35f,1.55f));
            }
            finally {Time.timeScale=1;InputReader.HudBlocked=false;Object.Destroy(target.gameObject);}
            yield return null;
        }

        [UnityTest]
        public IEnumerator EveryExtraMeleeTargetIsPushedAndRepeatedHitsRefreshDirection()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            var actor=new GameObject("Multi melee test");actor.transform.position=new Vector3(3500,3500);
            var p=actor.AddComponent<PlayerProgression>();p.ApplyRune(RuneKind.MeleeCleave);
            var attack=actor.AddComponent<PlayerAttackController>();attack.enabled=false;
            var first=Target((Vector2)actor.transform.position+Vector2.right);
            var second=Target((Vector2)actor.transform.position+new Vector2(1.2f,.4f));
            try
            {
                Physics2D.SyncTransforms();Assert.That(attack.TryAttack(),Is.True);
                Assert.That(MeleeKnockback.IsActive(first.gameObject),Is.True);
                Assert.That(MeleeKnockback.IsActive(second.gameObject),Is.True);
                yield return new WaitForSeconds(.08f);
                Vector2 position=first.GetComponent<Rigidbody2D>().position;
                MeleeKnockback.Apply(first,position+Vector2.right);
                yield return new WaitForSeconds(.4f);
                Assert.That(first.transform.position.x-position.x,Is.InRange(-1.55f,-1.35f));
                Assert.That(MeleeKnockback.IsActive(first.gameObject),Is.False);
                Assert.That(MeleeKnockback.IsActive(second.gameObject),Is.False);
            }
            finally {Object.Destroy(first.gameObject);Object.Destroy(second.gameObject);Object.Destroy(actor);Time.timeScale=1;InputReader.HudBlocked=false;}
            yield return null;
        }
    }
}
