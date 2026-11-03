using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class AnimalMotionGameplayTests
    {
        [UnityTest] public IEnumerator WildAnimalsAttachSwapDirectionsAndStopWithoutChangingMovementOrColliderScale()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            foreach(string key in new[]{"deer","rabbit","boar","wolf","bear","mammoth","fox","snow_hare"})
            {
                var go=new GameObject("Animal animation "+key);go.transform.position=new Vector3(12000,12000,0);
                go.transform.localScale=Vector3.one*.7f;
                var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=AnimalMotionVisual.ReferenceSprite(key);
                AnimalAi ai=key=="wolf"||key=="bear"||key=="mammoth"?(AnimalAi)go.AddComponent<WolfAi>():go.AddComponent<DeerAi>();
                ai.InitHp(10);yield return null;ai.enabled=false;
                var visual=go.GetComponent<AnimalMotionVisual>();Assert.That(visual,Is.Not.Null,key);visual.enabled=false;
                var body=go.GetComponent<Rigidbody2D>();body.velocity=Vector2.up*5;
                visual.AnimateMovement(body.velocity,.02f);var first=renderer.sprite;
                Assert.That(first,Is.SameAs(AnimalMotionVisual.SpriteFor(key,CharacterFacing.Back,true,0)));
                for(int i=0;i<40 && renderer.sprite==first;i++)visual.AnimateMovement(body.velocity,.02f);
                Assert.That(renderer.sprite,Is.Not.SameAs(first),key+" must alternate steps");
                Assert.That(body.velocity,Is.EqualTo(Vector2.up*5));Assert.That(go.transform.localScale,Is.EqualTo(Vector3.one*.7f));
                var paused=renderer.sprite;visual.AnimateMovement(Vector2.left,0);Assert.That(renderer.sprite,Is.SameAs(paused));
                visual.AnimateMovement(Vector2.zero,.02f);Assert.That(renderer.sprite,Is.SameAs(AnimalMotionVisual.SpriteFor(key,CharacterFacing.Back,false)));
                Assert.That(visual.MovementPhase,Is.EqualTo(ActionPhase.End));
                visual.AnimateMovement(Vector2.left*3,.02f);Assert.That(renderer.flipX,Is.True);
                Assert.That(renderer.sprite,Is.SameAs(AnimalMotionVisual.SpriteFor(key,CharacterFacing.Left,true,0)));
                Object.Destroy(go);yield return null;
            }
        }
        [UnityTest] public IEnumerator SummonedDogAndHawkReceiveTheirOwnMotionWithoutChangingCombatSettings()
        {
            Time.timeScale=1;
            var owner=new GameObject("Animal motion pet owner");owner.transform.position=new Vector3(15000,15000,0);
            foreach(var kind in new[]{Pet.Kind.Dog,Pet.Kind.Hawk})
            {
                var go=Pet.Spawn(kind,owner.transform);var pet=go.GetComponent<Pet>();
                float speed=pet.MoveSpeed,range=pet.AttackRange;int damage=pet.Damage;var scale=go.transform.localScale;
                yield return null;pet.enabled=false;
                var visual=go.GetComponent<AnimalMotionVisual>();Assert.That(visual,Is.Not.Null);visual.enabled=false;
                string key=kind==Pet.Kind.Hawk?"hawk":"dog";Assert.That(visual.SpeciesKey,Is.EqualTo(key));
                visual.AnimateMovement(Vector2.right*5,.02f);
                Assert.That(go.GetComponent<SpriteRenderer>().sprite,Is.SameAs(AnimalMotionVisual.SpriteFor(key,CharacterFacing.Right,true,0)));
                Assert.That(pet.MoveSpeed,Is.EqualTo(speed));Assert.That(pet.AttackRange,Is.EqualTo(range));Assert.That(pet.Damage,Is.EqualTo(damage));
                Assert.That(go.transform.localScale,Is.EqualTo(scale));Assert.That(go.GetComponent<CircleCollider2D>(),Is.Not.Null);
                Object.Destroy(go);yield return null;
            }
            Object.Destroy(owner);yield return null;
        }
    }
}
