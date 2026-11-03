using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class DogResourceTests
    {
        [UnityTest]
        public IEnumerator SummonedDogsUseDedicatedSpriteWithoutChangingProgression()
        {
            Time.timeScale = 1;
            InputReader.HudBlocked = false;
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return null;
            yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");
            yield return null;
            foreach (var pet in Object.FindObjectsByType<Pet>(FindObjectsSortMode.None))
                Object.Destroy(pet.gameObject);
            yield return null;
            var player = Object.FindFirstObjectByType<PlayerController>().gameObject;
            var owner = new GameObject("Dog resource test owner");
            owner.transform.position = new Vector3(3000, 3000, 0);
            player.transform.position = owner.transform.position;
            var progression = owner.AddComponent<PlayerProgression>();
            for (int level = 1; level <= 3; level++) progression.ApplyRune(RuneKind.SummonDog);
            yield return null;
            yield return null;
            var expected = SpriteBank.DogPet();
            Assert.That(expected, Is.Not.Null);
            int count = 0;
            var damageValues = new System.Collections.Generic.List<int>();
            foreach (var pet in Object.FindObjectsByType<Pet>(FindObjectsSortMode.None))
            {
                if (pet.Type != Pet.Kind.Dog || pet.Player != player.transform) continue;
                count++;
                var renderer = pet.GetComponent<SpriteRenderer>();
                var motion=pet.GetComponent<AnimalMotionVisual>();Assert.That(motion,Is.Not.Null);
                Assert.That(motion.SpeciesKey,Is.EqualTo("dog"));
                Assert.That(renderer.sprite,Is.SameAs(AnimalMotionVisual.SpriteFor("dog",motion.Facing,motion.MovementPhase!=ActionPhase.End,motion.MovementFrame)));
                Assert.That(renderer.color, Is.EqualTo(Color.white));
                // The dog is now a medium-sized companion (.70 opaque height), not the old tiny sprite.
                Assert.That(renderer.bounds.size.x, Is.InRange(1f, 1.4f));
                Assert.That(pet.GetComponent<CircleCollider2D>().radius*pet.transform.lossyScale.x,
                    Is.EqualTo(AnimalSizeProfile.For("dog").FootprintRadius).Within(.001f));
                damageValues.Add(pet.Damage);
                Assert.That(pet.AttackRange, Is.EqualTo(1.4f));
                Assert.That(pet.GetComponent<CircleCollider2D>(), Is.Not.Null);
                Object.Destroy(pet.gameObject);
            }
            Assert.That(count, Is.EqualTo(3));
            CollectionAssert.AreEquivalent(new[] {5, 8, 11}, damageValues);
            Object.Destroy(owner);
            Object.Destroy(player);
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
