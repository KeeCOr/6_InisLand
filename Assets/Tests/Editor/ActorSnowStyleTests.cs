using NUnit.Framework;
using UnityEngine;
using UnityEditor;
namespace IL6.Tests
{
    public class ActorSnowStyleTests
    {
        [Test] public void WorldActorsHaveOneSupportedSnowPaletteMaterial()
        {
            var material=Resources.Load<Material>("Materials/SnowActor");Assert.That(material,Is.Not.Null);
            Assert.That(material.shader.name,Is.EqualTo("InisLand/SnowActor"));Assert.That(ShaderUtil.ShaderHasError(material.shader),Is.False);
            Assert.That(material.GetFloat("_Saturation"),Is.InRange(.75f,.95f));
            Assert.That(material.GetFloat("_Contrast"),Is.InRange(.88f,.99f));
            Assert.That(Resources.Load<Sprite>("Sprites/Environment/Snowfield/actor_shadow"),Is.Not.Null);
            foreach(string key in new[]{"deer","rabbit","boar","wolf","bear","mammoth","fox","snow_hare","dog","hawk"})
            {
                var sprite=AnimalMotionVisual.SpriteFor(key,CharacterFacing.Right,false);
                var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite.texture));
                Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Bilinear),key+" must not retain an unrelated point-filtered edge style");
            }
        }
    }
}
