using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class GatherCompletionFxTests
    {
        [UnityTest]
        public IEnumerator CompletionRunsThreePhasesOnceAndSurvivesNodeRemoval()
        {
            Time.timeScale=1;
            var node=new GameObject("Gather effect test");node.transform.position=new Vector3(7000,7000,0);
            var gather=node.AddComponent<Gatherable>();gather.YieldKind=ResourceKind.Frostbloom;gather.YieldAmount=3;
            var store=new ResourceStore();
            int poofs=Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Count(x=>x.name=="__poof");
            gather.OnGathered(store);gather.OnGathered(store);
            var effects=Object.FindObjectsByType<GatherCompletionFx>(FindObjectsSortMode.None).Where(x=>Vector3.Distance(x.transform.position,node.transform.position)<1).ToArray();
            Assert.That(effects.Length,Is.EqualTo(1));
            var effect=effects[0];var renderer=effect.GetComponent<SpriteRenderer>();
            try
            {
                Assert.That(store.Get(ResourceKind.Frostbloom),Is.EqualTo(3));
                Assert.That(renderer.sprite.name,Is.EqualTo("plant_start"));
                Assert.That(effect.GetComponent<Collider2D>(),Is.Null);
                Assert.That(renderer.bounds.size.x,Is.EqualTo(1.35f).Within(.001f));
                yield return new WaitForSeconds(.09f);
                Assert.That(node==null,Is.True);
                Assert.That(renderer.sprite.name,Is.EqualTo("plant_loop"));
                Time.timeScale=0;yield return new WaitForSecondsRealtime(.2f);
                Assert.That(renderer.sprite.name,Is.EqualTo("plant_loop"));
                Time.timeScale=1;yield return new WaitForSeconds(.12f);
                Assert.That(renderer.sprite.name,Is.EqualTo("plant_end"));
                Assert.That(Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Count(x=>x.name=="__poof"),Is.EqualTo(poofs));
                yield return new WaitForSeconds(.2f);
                Assert.That(effect==null,Is.True);
            }
            finally {Time.timeScale=1;if(node!=null)Object.Destroy(node);if(effect!=null)Object.Destroy(effect.gameObject);}
        }

        [UnityTest]
        public IEnumerator FinalMiningHitUsesOneCompletionWithoutDuplicateFragments()
        {
            Time.timeScale=1;
            var node=new GameObject("Mining finish effect test");node.transform.position=new Vector3(7100,7100,0);
            var gather=node.AddComponent<Gatherable>();gather.YieldKind=ResourceKind.Stone;gather.YieldAmount=2;
            var rock=gather.EnsureRockHealth();var store=new ResourceStore();
            int fragments=Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Count(x=>x.name=="__materialImpact");
            rock.Hit(rock.MaxHp,store);
            var effect=Object.FindObjectsByType<GatherCompletionFx>(FindObjectsSortMode.None).Single(x=>Vector3.Distance(x.transform.position,node.transform.position)<1);
            Assert.That(effect.GetComponent<SpriteRenderer>().sprite.name,Is.EqualTo("stone_start"));
            Assert.That(Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Count(x=>x.name=="__materialImpact"),Is.EqualTo(fragments));
            Assert.That(store.Get(ResourceKind.Stone),Is.EqualTo(2));
            yield return new WaitForSeconds(.5f);
            Assert.That(effect==null,Is.True);
        }
    }
}
