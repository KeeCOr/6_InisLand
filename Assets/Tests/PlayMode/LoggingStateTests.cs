using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class LoggingStateTests
    {
        [UnityTest]
        public IEnumerator LoggingDamagesTreeExclusivelyAndRewardsOnlyAtZero()
        {
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var player=hud.Player;
            player.transform.position=new Vector3(1000,1000,0);
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None)) spawner.enabled=false;
            var tree=new GameObject("State logging test");tree.transform.position=player.transform.position+Vector3.left;
            tree.AddComponent<CircleCollider2D>();
            var g=tree.AddComponent<Gatherable>();g.DurationSec=1;g.YieldAmount=3;g.DestroyOnGather=false;
            var store=GameSession.Instance.Resources;store.Spend(ResourceKind.Wood,store.Get(ResourceKind.Wood));
            Assert.That(hud.Gather.StartGathering(g),Is.True);
            var health=g.EnsureTreeHealth();
            Assert.That(player.Actions.State,Is.EqualTo(PlayerAction.Logging));
            Assert.That(player.Actions.Phase,Is.EqualTo(ActionPhase.Start));
            Assert.That(player.GetComponent<SpriteRenderer>().flipX,Is.True);
            Assert.That(health.CurrentHp,Is.EqualTo(100));
            yield return new WaitForSeconds(.65f);
            Assert.That(health.CurrentHp,Is.GreaterThan(0).And.LessThan(100));
            Assert.That(store.Get(ResourceKind.Wood),Is.Zero);
            var enemy=new GameObject("Logging blocked attack");enemy.transform.position=player.transform.position+Vector3.right*.5f;
            enemy.AddComponent<CircleCollider2D>();enemy.AddComponent<SpriteRenderer>();
            var z=enemy.AddComponent<Zombie>();z.InitHp(1000);z.enabled=false;
            Physics2D.SyncTransforms();
            Assert.That(hud.Attacker.TryAttack(),Is.False);
            Assert.That(z.CurrentHp,Is.EqualTo(1000));
            var rb=player.GetComponent<Rigidbody2D>();rb.velocity=Vector2.right*3;
            yield return new WaitForFixedUpdate();
            Assert.That(rb.velocity,Is.EqualTo(Vector2.zero),"Logging blocks movement as well as attacking");
            hud.Gather.Cancel();
            int remaining=health.CurrentHp;
            Assert.That(player.Actions.Phase,Is.EqualTo(ActionPhase.End));
            Assert.That(hud.Gather.StartGathering(g),Is.False,"End phase must finish before another action");
            yield return new WaitForSeconds(.5f);
            Assert.That(health.CurrentHp,Is.EqualTo(remaining));
            Assert.That(hud.Gather.StartGathering(g),Is.True);
            Time.timeScale=0;yield return new WaitForSecondsRealtime(.15f);
            Assert.That(health.CurrentHp,Is.EqualTo(remaining));Time.timeScale=1;
            float deadline=Time.time+5;
            while(health.CurrentHp>0 && Time.time<deadline) yield return null;
            Assert.That(health.CurrentHp,Is.Zero);
            Assert.That(store.Get(ResourceKind.Wood),Is.Zero);
            Assert.That(Object.FindFirstObjectByType<WoodPickup>(),Is.Not.Null);
            g.OnGathered(store);
            Assert.That(Object.FindObjectsByType<WoodPickup>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            yield return new WaitForSeconds(1.2f);
            Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(3));
            Assert.That(hud.Gather.IsActive,Is.False);
            Assert.That(player.Actions.State,Is.EqualTo(PlayerAction.Idle));
            Object.Destroy(tree);Object.Destroy(enemy);InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
