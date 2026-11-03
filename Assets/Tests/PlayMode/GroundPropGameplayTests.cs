using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class GroundPropGameplayTests
    {
        [UnityTearDown] public IEnumerator Cleanup()
        {Time.timeScale=1;InputReader.HudBlocked=false;yield return SceneManager.LoadSceneAsync("BootScene");}
        private IEnumerator Ready()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(hud,null);
            GameSession.Instance.Cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=0});
        }
        [UnityTest] public IEnumerator AllDropsSettleOnGroundAndHideTheirShadowDuringPickupFlight()
        {
            yield return Ready();var player=Object.FindFirstObjectByType<PlayerController>();player.enabled=false;
            player.GetComponent<PlayerAttackController>().enabled=false;player.transform.position=new Vector3(1000,1000,0);
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None))spawner.enabled=false;
            var drops=new List<WoodPickup>();var store=GameSession.Instance.Resources;int i=0;
            foreach(ResourceKind kind in System.Enum.GetValues(typeof(ResourceKind)))
            {
                store.Spend(kind,store.Get(kind));
                var drop=WoodPickup.Spawn(player.transform.position+Vector3.right*(4+i++),2,store,kind);drops.Add(drop);
            }
            yield return new WaitForSeconds(.5f);
            foreach(var drop in drops)
            {
                var art=drop.transform.Find("WoodArt");var shadow=drop.transform.Find("PickupShadow").GetComponent<SpriteRenderer>();
                Assert.That(art.GetComponent<SpriteRenderer>().sprite,Is.SameAs(ResourceIconSkin.WorldSprite(drop.Kind)));
                Assert.That(art.GetComponent<YSort>(),Is.Not.Null);Assert.That(art.localPosition.y,Is.EqualTo(.04f).Within(.001f));
                Assert.That(shadow.enabled,Is.True);Assert.That(drop.GetComponentsInChildren<Collider2D>(),Is.Empty);
            }
            yield return new WaitForSeconds(.2f);
            foreach(var drop in drops)Assert.That(drop.transform.Find("WoodArt").localPosition.y,Is.EqualTo(.04f).Within(.001f),"Landed items do not keep floating");
            var target=drops[0];player.transform.position=target.transform.position;
            Assert.That(target.TryCollect(player),Is.True);Assert.That(target.transform.Find("PickupShadow").GetComponent<SpriteRenderer>().enabled,Is.False);
            yield return new WaitForSeconds(.7f);Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator ProceduralDiscoveriesKeepTheirRewardsAndCollisionWhileUsingGroundProps()
        {
            yield return Ready();var create=typeof(ProceduralSpawner).GetMethod("CreateDiscovery",BindingFlags.NonPublic|BindingFlags.Static);
            var seen=new HashSet<ResourceKind>();var store=new ResourceStore();
            for(int i=1;i<40 && seen.Count<5;i++)
            {
                var go=(GameObject)create.Invoke(null,new object[]{2000f+i*2,2000f,new SeededRng(unchecked((uint)i*2654435761u))});
                var site=go.GetComponent<DiscoverySite>();
                if(!seen.Add(site.RewardKind)){Object.Destroy(go);continue;}
                yield return null;
                Assert.That(go.GetComponent<SpriteRenderer>().sprite,Is.SameAs(DiscoverySite.SpriteFor(site.RewardKind)));
                Assert.That(go.GetComponent<YSort>(),Is.Not.Null);Assert.That(go.transform.localScale,Is.EqualTo(Vector3.one*1.15f));
                Assert.That(go.GetComponent<CircleCollider2D>().radius,Is.EqualTo(.45f));
                var gather=go.GetComponent<Gatherable>();Assert.That(gather.DurationSec,Is.EqualTo(3.5f));Assert.That(gather.YieldAmount,Is.Zero);
                int before=store.Get(site.RewardKind),amount=site.RewardAmount;gather.OnGathered(store);
                Assert.That(store.Get(site.RewardKind),Is.EqualTo(before+amount));
            }
            Assert.That(seen.Count,Is.EqualTo(5),"Existing procedural loot kinds remain unchanged");
        }
    }
}
