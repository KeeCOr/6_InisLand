using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public sealed class WoodPickupTests
    {
        [UnityTest]
        public IEnumerator BrokenTreeDropsWoodAndOnlyCreditsAfterPickupFlight()
        {
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var player=hud.Player;hud.Attacker.AutoAttackEnabled=false;
            player.transform.position=new Vector3(1000,1000,0);
            foreach(var s in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None)) s.enabled=false;
            var tree=new GameObject("Wood drop test");tree.transform.position=player.transform.position+Vector3.right*3;
            var node=tree.AddComponent<Gatherable>();node.YieldAmount=3;node.DestroyOnGather=false;
            var store=GameSession.Instance.Resources;store.Spend(ResourceKind.Wood,store.Get(ResourceKind.Wood));
            node.EnsureTreeHealth().Chop(100,store);
            Assert.That(store.Get(ResourceKind.Wood),Is.Zero,"Breaking a tree must drop wood instead of paying immediately");
            var type=typeof(PlayerController).Assembly.GetType("IL6.WoodPickup");Assert.That(type,Is.Not.Null);
            var pickup=(MonoBehaviour)Object.FindObjectOfType(type);Assert.That(pickup,Is.Not.Null);
            var collect=type.GetMethod("TryCollect");var amount=type.GetProperty("Amount");var flying=type.GetProperty("IsFlying");
            node.OnGathered(store);
            Assert.That(Object.FindObjectsOfType(type).Length,Is.EqualTo(1),"A broken tree can only drop once");
            yield return new WaitForSeconds(.5f);
            Assert.That((int)amount.GetValue(pickup),Is.EqualTo(3));
            player.transform.position=pickup.transform.position;Physics2D.SyncTransforms();
            Time.timeScale=0;
            Assert.That(collect.Invoke(pickup,new object[]{player}),Is.EqualTo(false));
            Time.timeScale=1;
            Assert.That(collect.Invoke(pickup,new object[]{player}),Is.EqualTo(true));
            Assert.That((bool)flying.GetValue(pickup),Is.True);
            Assert.That(store.Get(ResourceKind.Wood),Is.Zero);
            Assert.That(collect.Invoke(pickup,new object[]{player}),Is.EqualTo(false));
            Time.timeScale=0;yield return new WaitForSecondsRealtime(.2f);
            Assert.That(store.Get(ResourceKind.Wood),Is.Zero,"Paused pickup flights must not complete");Time.timeScale=1;
            yield return new WaitForSeconds(.8f);
            Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(3));
            Assert.That(pickup==null,Is.True);
            Object.Destroy(tree);

            var fullTree=new GameObject("Full inventory tree");fullTree.transform.position=player.transform.position+Vector3.right*3;
            var fullNode=fullTree.AddComponent<Gatherable>();fullNode.YieldAmount=3;fullNode.DestroyOnGather=false;
            store.Add(ResourceKind.Wood,store.GetCap(ResourceKind.Wood));
            fullNode.EnsureTreeHealth().Chop(100,store);
            var remainder=(MonoBehaviour)Object.FindObjectOfType(type);
            yield return new WaitForSeconds(.5f);
            player.transform.position=remainder.transform.position;Physics2D.SyncTransforms();
            Assert.That(collect.Invoke(remainder,new object[]{player}),Is.EqualTo(false),"Full storage leaves wood on the ground");
            store.Spend(ResourceKind.Wood,2);
            Assert.That(collect.Invoke(remainder,new object[]{player}),Is.EqualTo(true));
            store.Add(ResourceKind.Wood,1); // Capacity can change during the visual flight.
            yield return new WaitForSeconds(.8f);
            Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(store.GetCap(ResourceKind.Wood)));
            Assert.That((int)amount.GetValue(remainder),Is.EqualTo(2));
            Assert.That((bool)flying.GetValue(remainder),Is.False);
            Object.Destroy(fullTree);Object.Destroy(remainder.gameObject);InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
