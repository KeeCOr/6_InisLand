using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class GateAndPlayerVisualTests
    {
        [UnityTest]
        public IEnumerator ApproachingAutomaticallyOpensExitAndLeavingClosesIt()
        {
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return null; yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");
            yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var player=hud.Player;

            var gate=Door.FindNearest(player.transform.position);
            Assert.That(gate,Is.Not.Null);
            gate.SetOpen(false);
            player.transform.position=gate.transform.position+Vector3.up*1.8f;
            Physics2D.SyncTransforms();
            yield return null;
            yield return null;
            Assert.That((bool)typeof(Door).GetField("isOpen",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(gate),Is.True);
            var collider=player.GetComponent<BoxCollider2D>();
            Assert.That(Physics2D.GetIgnoreCollision(gate.GetComponent<Collider2D>(),collider),Is.True);
            player.enabled=false;
            var debug=player.GetComponent<DebugMove>(); if(debug!=null) debug.enabled=false;
            var rb=player.GetComponent<Rigidbody2D>();
            for(int i=0;i<65;i++) { rb.velocity=Vector2.down*3; yield return new WaitForFixedUpdate(); }
            Assert.That(player.transform.position.y,Is.LessThan(gate.transform.position.y-0.8f));
            rb.velocity=Vector2.zero;
            // Stay open while passing through the doorway, then close after walking away.
            Assert.That(gate.IsOpen, Is.True);
            player.transform.position=gate.transform.position+Vector3.down*5;
            Physics2D.SyncTransforms();
            yield return new WaitForSeconds(1.2f);
            Assert.That(gate.IsOpen, Is.False);
            Assert.That(Physics2D.GetIgnoreCollision(gate.GetComponent<Collider2D>(),collider),Is.False);
            var enemy=new GameObject("Enemy cannot open gate");
            enemy.transform.position=gate.transform.position;
            var enemyCol=enemy.AddComponent<CircleCollider2D>();
            enemy.AddComponent<SpriteRenderer>();
            // Keep the collision probe alive: normal zombies self-remove during daytime.
            enemy.AddComponent<Zombie>().enabled=false;
            yield return null;
            Assert.That(gate.IsOpen, Is.False);
            Assert.That(Physics2D.GetIgnoreCollision(gate.GetComponent<Collider2D>(),enemyCol),Is.False);
            Time.timeScale=0;
            player.transform.position=gate.transform.position;
            Physics2D.SyncTransforms();
            yield return null;
            Assert.That(gate.IsOpen, Is.False, "Paused gates must not change state");
            Time.timeScale=1;
            yield return null;
            Assert.That(gate.IsOpen, Is.True);
            Assert.That(Physics2D.GetIgnoreCollision(gate.GetComponent<Collider2D>(),enemyCol),Is.False);
            Object.Destroy(enemy);
            yield return SceneManager.LoadSceneAsync("BootScene");
        }

        [UnityTest]
        public IEnumerator PlayerRetainsUntintedArtAndHpBarDoesNotCoverBody()
        {
            Time.timeScale=1;
            var go=new GameObject("Tinted legacy player"); go.SetActive(false);
            var sr=go.AddComponent<SpriteRenderer>(); sr.color=new Color(1,0.4f,0.2f);
            var player=go.AddComponent<PlayerController>();
            var hp=go.AddComponent<HpBarUi>(); hp.Player=player;
            go.SetActive(true);
            Assert.That(sr.color,Is.EqualTo(Color.white));
            player.TakeDamage(5);
            yield return null; yield return null;
            var bar=go.transform.Find("HpBar");
            Assert.That(bar,Is.Not.Null);
            Assert.That(bar.GetComponent<SpriteRenderer>().bounds.Intersects(sr.bounds),Is.False);
            Object.Destroy(go);
        }
    }
}
