using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public class GeneratedStaticAssetTests
    {
        [TestCase("Buildings/wood_barricade")]
        [TestCase("Buildings/cabin")]
        [TestCase("Buildings/storage")]
        [TestCase("Buildings/watchtower")]
        [TestCase("Buildings/farm")]
        [TestCase("Buildings/infirmary")]
        [TestCase("Buildings/hunters_hut")]
        [TestCase("Companions/hunter")]
        [TestCase("Companions/warrior")]
        [TestCase("Companions/farmer")]
        [TestCase("Companions/cook")]
        [TestCase("Companions/healer")]
        [TestCase("Animals/fox")]
        [TestCase("Animals/snow_hare")]
        public void GeneratedPng_ImportsAsUsableSingleSprite(string path)
        {
            var sprite = Resources.Load<Sprite>("Sprites/GeneratedStatic20261014/" + path);
            Assert.That(sprite, Is.Not.Null, path);
            Assert.That(sprite.texture.filterMode, Is.EqualTo(FilterMode.Point), path);
            Assert.That(sprite.pixelsPerUnit, Is.GreaterThan(50f), path);
            Assert.That(sprite.bounds.size.y, Is.LessThan(3.5f), path);
        }
    }
}
