using NUnit.Framework;
using UnityEngine;
using UnityEditor;
namespace IL6.Tests
{
    public class ChibiCharacterAssetTests
    {
        [TestCase("Hunter")][TestCase("Warrior")][TestCase("Farmer")][TestCase("Cook")]
        [TestCase("Healer")][TestCase("Aunt")][TestCase("Uncle")][TestCase("Child")]
        public void EveryCompanionUsesTheCompactHighResolutionCharacterSet(string role)
        {
            var sprite=SpriteBank.CompanionSpriteForRole(role);
            Assert.That(sprite,Is.Not.Null);
            Assert.That(sprite.texture.width,Is.GreaterThanOrEqualTo(256));
            Assert.That(sprite.rect.width/sprite.rect.height,Is.InRange(.9f,1.1f),"All roles must use the new compact atlas cells");
            float worldHeight=sprite.bounds.size.y*SpriteBank.CompanionScaleForRole(role).y;
            Assert.That(worldHeight,Is.InRange(.9f,1.4f),"Rendered world scale must stay consistent across roles");
            Assert.That(sprite.pivot.y/sprite.rect.height,Is.InRange(0f,.3f),"Feet must anchor to the ground");
        }
        [TestCase("axe")][TestCase("bow")]
        public void HeroSheetsKeepSixteenFrameIdentitiesAndNewArtwork(string weapon)
        {
            var sprites=Resources.LoadAll<Sprite>("Sprites/Player/RedHairGirl/"+weapon);
            Assert.That(sprites.Length,Is.EqualTo(16));
            foreach(var sprite in sprites) {
                Assert.That(sprite.texture.width,Is.GreaterThanOrEqualTo(1000));
                Assert.That(sprite.name,Does.StartWith("redhair_"+weapon+"_"));
                Assert.That(sprite.rect.width,Is.EqualTo(sprite.texture.width/4f).Within(1));
            }
        }
    }
}
