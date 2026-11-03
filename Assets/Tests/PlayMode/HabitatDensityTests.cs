using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class HabitatDensityTests
    {
        const BindingFlags Hidden=BindingFlags.NonPublic|BindingFlags.Instance;
        [UnityTest] public IEnumerator StarterGroveAddsHarvestableTreesOnceOutsideVillageAndGate()
        {
            var root=new GameObject("Grove density test");var spawner=root.AddComponent<ProceduralSpawner>();spawner.enabled=false;
            var before=new HashSet<Gatherable>(Object.FindObjectsByType<Gatherable>(FindObjectsSortMode.None));
            var seed=typeof(ProceduralSpawner).GetMethod("SeedStarterGrove",Hidden);Assert.That(seed,Is.Not.Null);
            seed.Invoke(spawner,null);var added=new List<Gatherable>();
            foreach(var g in Object.FindObjectsByType<Gatherable>(FindObjectsSortMode.None)) if(!before.Contains(g)) added.Add(g);
            Assert.That(added.Count,Is.EqualTo(20));
            foreach(var tree in added)
            {
                Assert.That(tree.IsTree,Is.True);Assert.That(tree.EnsureTreeHealth(),Is.Not.Null);
                var d=(Vector2)tree.transform.position-new Vector2(GameConstants.VillageCenterX,GameConstants.VillageCenterY);
                Assert.That(Mathf.Max(Mathf.Abs(d.x),Mathf.Abs(d.y)),Is.GreaterThanOrEqualTo(VillageStarter.CurrentHalfSize+3f));
                Assert.That(d.y>=0 || Mathf.Abs(d.x)>=2.5f,Is.True,"South gate approach must stay open");
            }
            seed.Invoke(spawner,null);
            int after=0;foreach(var g in Object.FindObjectsByType<Gatherable>(FindObjectsSortMode.None))if(!before.Contains(g))after++;
            Assert.That(after,Is.EqualTo(20),"Entering night or retrying initialization must not duplicate starter trees");
            foreach(var g in added)Object.Destroy(g.gameObject);Object.Destroy(root);yield return null;
        }
        [UnityTest] public IEnumerator ExploringCannotAccumulateUnlimitedProceduralAnimals()
        {
            var root=new GameObject("Wildlife density test");root.transform.position=Vector3.one*5000;
            var spawner=root.AddComponent<ProceduralSpawner>();spawner.enabled=false;spawner.Player=root.transform;
            var before=new HashSet<AnimalAi>(Object.FindObjectsByType<AnimalAi>(FindObjectsSortMode.None));
            var spawn=typeof(ProceduralSpawner).GetMethod("SpawnAnimalAt",Hidden|BindingFlags.Static);
            for(int i=0;i<30;i++)spawn.Invoke(spawner,new object[]{5000f+i,5000f,new SeededRng((uint)(500+i)),new List<GameObject>()});
            var added=new List<AnimalAi>();foreach(var a in Object.FindObjectsByType<AnimalAi>(FindObjectsSortMode.None))if(!before.Contains(a))added.Add(a);
            Assert.That(added.Count,Is.InRange(1,12),"Animal group size must also respect the population cap");
            Assert.That(added.Count,Is.GreaterThan(1));
            var near=added[0];var far=added[added.Count-1];
            far.transform.position=root.transform.position+Vector3.right*100;
            typeof(ProceduralSpawner).GetMethod("CleanupDistantWildlife",Hidden).Invoke(spawner,null);
            yield return null;
            Assert.That(far==null,Is.True,"Far procedural animals must not accumulate forever");
            Assert.That(near!=null,Is.True,"Nearby wildlife must not disappear");
            foreach(var a in added)if(a!=null)Object.Destroy(a.gameObject);Object.Destroy(root);yield return null;
        }
    }
}
