using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class ForestExpansionGameplayTests
    {
        [UnityTest]
        public IEnumerator AddedSpeciesActuallySpawnAndDropWoodOnlyAfterFelling()
        {
            Time.timeScale=1;
            var create=typeof(ProceduralSpawner).GetMethod("CreateTree",BindingFlags.NonPublic|BindingFlags.Static);
            var required=new HashSet<string>{"oak","cedar","larch"};
            var cleanup=new List<GameObject>();var store=new ResourceStore();
            try
            {
                for(int i=0;i<120 && required.Count>0;i++)
                {
                    var tree=(GameObject)create.Invoke(null,new object[]{6000f+i*1.173f,6000f-i*.839f});cleanup.Add(tree);
                    string key=tree.GetComponent<SpriteRenderer>().sprite.name;
                    if(!required.Remove(key)){Object.Destroy(tree);continue;}
                    var node=tree.GetComponent<Gatherable>();var health=node.EnsureTreeHealth();
                    var collider=tree.GetComponent<CircleCollider2D>();
                    Assert.That(collider.radius,Is.EqualTo(.16f));
                    Assert.That(collider.offset,Is.EqualTo(new Vector2(0,-.24f)));
                    var before=new HashSet<WoodPickup>(Object.FindObjectsByType<WoodPickup>(FindObjectsSortMode.None));
                    health.Chop(25,store);Assert.That(health.CurrentHp,Is.EqualTo(75));
                    Assert.That(Object.FindObjectsByType<WoodPickup>(FindObjectsSortMode.None).Length,Is.EqualTo(before.Count));
                    health.Chop(100,store);node.OnGathered(store);
                    int drops=0;
                    foreach(var pickup in Object.FindObjectsByType<WoodPickup>(FindObjectsSortMode.None))
                        if(!before.Contains(pickup)){drops++;cleanup.Add(pickup.gameObject);Assert.That(pickup.Amount,Is.GreaterThan(0));}
                    Assert.That(drops,Is.EqualTo(1),key+" must produce exactly one real wood pickup");
                    Assert.That(store.Get(ResourceKind.Wood),Is.Zero,"Felling must not bypass pickup/UI flight credit");
                }
                Assert.That(required,Is.Empty,"All3 new species must be reachable through the actual procedural factory");
                yield return null;
            }
            finally {foreach(var go in cleanup)if(go!=null)Object.Destroy(go);Time.timeScale=1;}
            yield return new WaitForSeconds(.5f);
        }
    }
}
