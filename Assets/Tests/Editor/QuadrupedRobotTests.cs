using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public sealed class QuadrupedRobotTests
    {
        [Test]
        public void QuadrupedSpriteIsTransparentAndSizedForBasicEnemy()
        {
            var sprite = Resources.Load<Sprite>("Sprites/Enemies/quadruped_robot");
            Assert.That(sprite, Is.Not.Null);
            Assert.That(sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(sprite.bounds.size.x, Is.LessThan(2.5f));
        }

        [Test]
        public void FirstNightSpawnUsesUntintedQuadrupedWithSameCombatComponent()
        {
            var host = new GameObject("NightTest");
            try
            {
                var night = host.AddComponent<NightController>();
                night.Player = host.transform;
                typeof(NightController).GetMethod("SpawnOne", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(night, null);
                var enemy = GameObject.Find("Zombie_wave");
                Assert.That(enemy, Is.Not.Null);
                try
                {
                    Assert.That(enemy.GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo("quadruped_robot"));
                    Assert.That(enemy.GetComponent<SpriteRenderer>().color, Is.EqualTo(Color.white));
                    Assert.That(enemy.GetComponent<Zombie>(), Is.Not.Null);
                    Assert.That(enemy.GetComponent<HpBarUi>().Offset.y, Is.GreaterThan(0.8f));
                }
                finally { Object.DestroyImmediate(enemy); }
            }
            finally { Object.DestroyImmediate(host); }
        }
    }
}
