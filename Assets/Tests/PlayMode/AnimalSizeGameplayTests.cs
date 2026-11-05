using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class AnimalSizeGameplayTests
    {
        [UnityTest] public IEnumerator SpawnedAnimalsAndPetsHaveBodySizedFootprintsAndStableRootScale()
        {
            Time.timeScale=1f;
            foreach(var arch in ProceduralSpawner._animals)
            {
                var animal=ProceduralSpawner.CreateOneAnimal(arch,20000,20000,1,1000);
                var visual=animal.GetComponent<AnimalMotionVisual>();
                Assert.That(visual,Is.Not.Null);
                var profile=AnimalSizeProfile.For(visual.SpeciesKey);
                Assert.That(animal.transform.localScale.x,Is.EqualTo(profile.ActorScale));
                Assert.That(animal.GetComponent<CircleCollider2D>().radius*arch.Scale,Is.EqualTo(profile.FootprintRadius).Within(.001f));
                animal.GetComponent<AnimalAi>().enabled=false;
                yield return null;
                var bar=animal.transform.Find("HpBar");
                if(bar!=null)
                {
                    var sprite=animal.GetComponent<SpriteRenderer>();
                    Assert.That(bar.position.y,Is.EqualTo(sprite.bounds.max.y+.18f).Within(.001f));
                    // Non-player character gauges retain the approved 70% width.
                    Assert.That(bar.lossyScale.x,Is.EqualTo(.63f).Within(.001f));
                }
                Object.Destroy(animal);yield return null;
            }
            var owner=new GameObject("Pet size owner");owner.transform.position=new Vector3(20000,20000);
            foreach(var kind in new[]{Pet.Kind.Dog,Pet.Kind.Hawk})
            {
                var pet=Pet.Spawn(kind,owner.transform);string key=kind==Pet.Kind.Dog?"dog":"hawk";
                var profile=AnimalSizeProfile.For(key);
                Assert.That(pet.GetComponent<CircleCollider2D>().radius*pet.transform.localScale.x,Is.EqualTo(profile.FootprintRadius).Within(.001f));
                Assert.That(pet.transform.localScale.x,Is.EqualTo(profile.ActorScale));
                Object.Destroy(pet);yield return null;
            }
            Object.Destroy(owner);
        }
    }
}
