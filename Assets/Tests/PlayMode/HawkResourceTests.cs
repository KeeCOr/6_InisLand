using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class HawkResourceTests
    {
        [UnityTest] public IEnumerator EveryHawkSummonedByProgressionKeepsItsDedicatedSpriteAndNaturalColor()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            foreach(var pet in Object.FindObjectsByType<Pet>(FindObjectsSortMode.None)) Object.Destroy(pet.gameObject);
            yield return null;
            var owner=new GameObject("Hawk resource test owner");owner.transform.position=new Vector3(3000,3000,0);
            var player=GameObject.FindWithTag("Player");player.transform.position=owner.transform.position;
            var progression=owner.AddComponent<PlayerProgression>();
            for(int level=1;level<=3;level++) progression.ApplyRune(RuneKind.SummonHawk);
            yield return null;yield return null;
            var expected=SpriteBank.HawkPet();Assert.That(expected,Is.Not.Null);
            int count=0;
            foreach(var pet in Object.FindObjectsByType<Pet>(FindObjectsSortMode.None))
            {
                if(pet.Type!=Pet.Kind.Hawk || pet.Player!=player.transform) continue;
                count++;
                var sprite=pet.GetComponent<SpriteRenderer>();
                Assert.That(sprite.sprite,Is.SameAs(expected),"ColorFallback must not replace the hawk with a triangle");
                Assert.That(sprite.color,Is.EqualTo(Color.white),"Preserve the painted feather colors");
                Assert.That(sprite.bounds.size.x,Is.InRange(1f,1.6f));
                typeof(Pet).GetMethod("Move",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(pet,new object[]{Vector2.left});
                Assert.That(sprite.flipX,Is.True);
                Object.Destroy(pet.gameObject);
            }
            Assert.That(count,Is.EqualTo(3));Object.Destroy(owner);
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
