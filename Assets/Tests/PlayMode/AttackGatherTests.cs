using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class AttackGatherTests
    {
        [UnityTest]
        public IEnumerator ManualAttackStartsNearbyTreeWithoutResettingAndKeepsCombatPriority()
        {
            Time.timeScale = 1;
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return null;
            yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");
            yield return new WaitForSeconds(2);
            var hud = Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hud, null);
            var attack = hud.Attacker;

            attack.SwitchToWeapon(0);
            hud.Player.transform.position = new Vector3(1000, 1000, 0);
            var tree = new GameObject("Attack tree test");
            tree.transform.position = hud.Player.transform.position + Vector3.right;
            tree.AddComponent<CircleCollider2D>().radius = 0.16f;
            var wood = tree.AddComponent<Gatherable>();
            wood.DurationSec = 10;
            var method = typeof(PlayerAttackController).GetMethod("TryManualAction");
            Assert.That(method, Is.Not.Null, "Manual attack must support nearby tree interaction");
            Physics2D.SyncTransforms();
            Assert.That(attack.TryAttack(), Is.False);
            Assert.That(hud.Gather.IsActive, Is.False, "Auto combat cannot start logging");
            Time.timeScale = 0;
            Assert.That(method.Invoke(attack, null), Is.EqualTo(false));
            Time.timeScale = 1;
            Assert.That(method.Invoke(attack, null), Is.EqualTo(true));
            Assert.That(hud.Gather.ActiveTarget, Is.SameAs(wood));
            yield return new WaitForSeconds(.2f);
            float progress = hud.Gather.Progress;
            method.Invoke(attack, null);
            Assert.That(hud.Gather.Progress, Is.GreaterThanOrEqualTo(progress));
            hud.Gather.Cancel();
            tree.transform.position += Vector3.right * 5;
            Physics2D.SyncTransforms();
            Assert.That(method.Invoke(attack, null), Is.EqualTo(false));
            tree.transform.position = hud.Player.transform.position + Vector3.right;
            wood.YieldKind = ResourceKind.Stone;
            Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.6f);
            Assert.That(attack.ManualActionLabel,Is.EqualTo("채굴"));
            Assert.That(method.Invoke(attack, null), Is.EqualTo(true),"Space must also start nearby mining");
            Assert.That(hud.Gather.ActiveTarget,Is.SameAs(wood));
            yield return new WaitForSeconds(.2f);
            progress=hud.Gather.Progress;
            hud.UseUnifiedAction();
            Assert.That(hud.Gather.Progress,Is.GreaterThanOrEqualTo(progress),"Repeated Space must not reset mining");
            Assert.That(hud.Gather.ActiveTarget,Is.SameAs(wood),"HUD Space must keep the same mining target");
            hud.Gather.Cancel();
            wood.YieldKind = ResourceKind.Wood;
            yield return new WaitForSeconds(.6f); // Finish the logging end phase before combat.
            var foe = new GameObject("Logging combat test");
            foe.transform.position = hud.Player.transform.position + Vector3.left * .5f;
            foe.AddComponent<SpriteRenderer>();
            foe.AddComponent<CircleCollider2D>();
            var zombie = foe.AddComponent<Zombie>();
            zombie.InitHp(1000);
            Physics2D.SyncTransforms();
            Assert.That(method.Invoke(attack, null), Is.EqualTo(true));
            Assert.That(zombie.CurrentHp, Is.LessThan(1000));
            Assert.That(hud.Gather.IsActive, Is.False);
            Object.Destroy(tree);
            Object.Destroy(foe);
            InputReader.HudBlocked = false;
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
