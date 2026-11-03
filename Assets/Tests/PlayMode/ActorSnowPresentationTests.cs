using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class ActorSnowPresentationTests
    {
        [UnityTest] public IEnumerator AllAnimalFramesKeepSharedPaletteAndBodyGeometry()
        {
            Time.timeScale=1;
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None))spawner.enabled=false;
            var shared=Resources.Load<Material>("Materials/SnowActor");
            foreach(string key in new[]{"deer","rabbit","boar","wolf","bear","mammoth","fox","snow_hare","dog","hawk"})
            {
                var go=new GameObject("Snow actor "+key);go.transform.position=Vector3.one*5000;
                var size=AnimalSizeProfile.For(key);go.transform.localScale=Vector3.one*size.ActorScale;
                var body=go.AddComponent<SpriteRenderer>();body.sprite=AnimalMotionVisual.ReferenceSprite(key);
                var collider=go.AddComponent<CircleCollider2D>();var motion=AnimalMotionVisual.Attach(go,key);
                var original=go.transform.localScale;float radius=collider.radius;
                body.color=new Color(.8f,.7f,.6f,.5f);
                foreach(var facing in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right})
                {
                    motion.AnimateMovement(facing,.2f);
                    Assert.That(body.sharedMaterial,Is.SameAs(shared));Assert.That(body.sprite,Is.Not.Null);
                    Assert.That(body.color,Is.EqualTo(new Color(.8f,.7f,.6f,.5f)),"Palette must not overwrite hit/fade tints");
                    Assert.That(go.transform.localScale,Is.EqualTo(original));Assert.That(collider.radius,Is.EqualTo(radius));
                }
                yield return null;yield return null;
                var shadow=go.GetComponent<ActorSnowPresentation>().Shadow;Assert.That(shadow,Is.Not.Null);
                Assert.That(shadow.transform.parent,Is.Null);Assert.That(shadow.sortingOrder,Is.InRange(-9799,-9501));
                Assert.That(shadow.GetComponent<Collider2D>(),Is.Null);
                if(key=="hawk")Assert.That(shadow.transform.position.y,Is.LessThan(go.transform.position.y-.5f));
                body.enabled=false;yield return null;Assert.That(shadow.enabled,Is.False);
                Object.Destroy(go);yield return null;yield return null;Assert.That(shadow==null,Is.True);
            }
        }
        [UnityTest] public IEnumerator HeroAndEveryCompanionKeepPaletteAcrossDirectionChanges()
        {
            var shared=Resources.Load<Material>("Materials/SnowActor");
            foreach(string role in new[]{"Hero","Hunter","Warrior","Farmer","Cook","Healer","Aunt","Uncle","Child"})
            {
                var go=new GameObject("Snow character "+role);go.transform.position=Vector3.one*6000;
                var body=go.AddComponent<SpriteRenderer>();body.sprite=role=="Hero"?SpriteBank.PlayerIdle():SpriteBank.CompanionSpriteForRole(role);
                var directions=go.AddComponent<DirectionalCharacterVisual>();directions.Configure(role);
                var scale=go.transform.localScale;
                foreach(var direction in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right})
                {directions.AnimateMovement(direction,.2f);Assert.That(body.sharedMaterial,Is.SameAs(shared));Assert.That(body.sprite,Is.Not.Null);Assert.That(go.transform.localScale,Is.EqualTo(scale));}
                yield return null;yield return null;
                var contact=go.GetComponent<ActorSnowPresentation>();Assert.That(contact.Shadow,Is.Not.Null);
                Object.Destroy(go);yield return null;yield return null;
            }
        }
    }
}
