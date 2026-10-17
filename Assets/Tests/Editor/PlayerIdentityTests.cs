using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace IL6.Tests
{
    public sealed class PlayerIdentityTests
    {
        [Test]
        public void DefaultPlayerSpriteUsesRedHairedGirl()
        {
            var sprite=SpriteBank.PlayerIdle();
            Assert.That(sprite,Is.Not.Null);
            Assert.That(sprite.name,Is.EqualTo("redhair_axe_0"));
            Assert.That(sprite.rect.width,Is.LessThan(sprite.texture.width));
        }
        [TestCase("Axe", "axe")]
        [TestCase("Bow", "bow")]
        public void IdleAndWalkControllersCannotRestoreOldMaleSprite(string weapon,string key)
        {
            var controller=Resources.Load<AnimatorOverrideController>("Sprites/Player/RedHairGirl/RedHairGirl"+weapon);
            Assert.That(controller,Is.Not.Null);
            foreach(var clipName in new[]{"playerIdle","playerWalk"})
            {
                var clip=controller[clipName];
                Assert.That(clip,Is.Not.Null);
                var bindings=AnimationUtility.GetObjectReferenceCurveBindings(clip);
                Assert.That(bindings.Length,Is.EqualTo(1));
                var keys=AnimationUtility.GetObjectReferenceCurve(clip,bindings[0]);
                Assert.That(keys.Length,Is.GreaterThanOrEqualTo(4));
                foreach(var frame in keys)
                    Assert.That(((Sprite)frame.value).name,Does.StartWith("redhair_"+key+"_"));
            }
        }
    }
}
