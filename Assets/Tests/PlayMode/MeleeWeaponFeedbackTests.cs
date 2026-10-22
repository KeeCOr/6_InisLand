using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class MeleeWeaponFeedbackTests
    {
        private static Zombie Target(Vector2 position)
        {
            var go = new GameObject("Weapon feedback target");
            go.transform.position = position;
            go.AddComponent<CircleCollider2D>().radius = .2f;
            var zombie = go.AddComponent<Zombie>();
            zombie.InitHp(1000);
            zombie.enabled = false; // Isolate weapon physics from daytime wave cleanup and pursuit.
            return zombie;
        }

        private static IEnumerator Ready()
        {
            yield return SceneManager.LoadSceneAsync("BootScene"); yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene"); yield return new WaitForSeconds(2f);
            Time.timeScale=1f; InputReader.HudBlocked=false;
        }

        [UnityTest] public IEnumerator MeleeShowsExactRangeAndPushesWhileBowOnlyDamages()
        {
            yield return Ready();
            var player = new GameObject("Weapon feedback player");
            player.transform.position = new Vector3(2000,2000,0);
            var attack = player.AddComponent<PlayerAttackController>();
            var target = Target((Vector2)player.transform.position + Vector2.right * 1.3f);
            Physics2D.SyncTransforms();
            yield return null; yield return null;
            var indicator = player.GetComponent<MeleeRangeIndicator>();
            Assert.That(indicator.Visible,Is.True);
            Assert.That(indicator.DisplayedRange,Is.EqualTo(attack.Weapon.Range));
            float before = target.transform.position.x;
            Assert.That(attack.TryAttack(),Is.True);
            Assert.That(target.CurrentHp,Is.LessThan(1000));
            Assert.That(MeleeKnockback.IsActive(target.gameObject),Is.True);
            yield return new WaitForSeconds(.25f);
            Assert.That(target.transform.position.x-before,Is.InRange(.6f,.85f));
            Assert.That(MeleeKnockback.IsActive(target.gameObject),Is.False);

            attack.SwitchToWeapon(1);
            yield return new WaitForSeconds(1f);
            Assert.That(indicator.Visible,Is.False);
            before = target.transform.position.x;
            int hp = target.CurrentHp;
            Assert.That(attack.TryAttack(),Is.True);
            yield return new WaitForSeconds(.35f);
            Assert.That(target.CurrentHp,Is.LessThan(hp));
            Assert.That(MeleeKnockback.IsActive(target.gameObject),Is.False);
            Assert.That(target.transform.position.x,Is.EqualTo(before).Within(.02f));

            attack.SwitchToWeapon(0);
            target.transform.position = player.transform.position + Vector3.right * (attack.Weapon.Range+.05f);
            Physics2D.SyncTransforms();
            yield return new WaitForSeconds(1f);
            Assert.That(attack.TryAttack(),Is.False,"A collider crossing the ring must not extend the displayed center-based attack range");
            Object.Destroy(target.gameObject); Object.Destroy(player);
            yield return null;
        }

        [UnityTest] public IEnumerator MeleePushStopsAtSolidWalls()
        {
            yield return Ready();
            var target=Target(new Vector2(2000,2000));
            var wall=new GameObject("Knockback wall");
            wall.transform.position=new Vector3(2000.6f,2000,0);
            wall.AddComponent<BoxCollider2D>().size=new Vector2(.2f,3f);
            Physics2D.SyncTransforms();
            MeleeKnockback.Apply(target,new Vector2(1999,2000));
            yield return new WaitForSeconds(.3f);
            Assert.That(target.transform.position.x,Is.LessThanOrEqualTo(2000.32f));
            Assert.That(MeleeKnockback.IsActive(target.gameObject),Is.False);
            Object.Destroy(target.gameObject);Object.Destroy(wall);yield return null;
        }
    }
}
