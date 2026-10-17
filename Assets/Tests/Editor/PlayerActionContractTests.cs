using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace IL6.Tests
{
    public sealed class PlayerActionContractTests
    {
        [Test]
        public void ExclusiveActionStateAndTreeHealthComponentsExist()
        {
            Assert.That(typeof(PlayerController).Assembly.GetType("IL6.PlayerActionState"), Is.Not.Null);
            Assert.That(typeof(PlayerController).Assembly.GetType("IL6.TreeHealth"), Is.Not.Null);
        }

        [TestCase("axe")]
        [TestCase("bow")]
        public void EveryPlayerActionHasStartLoopEndClips(string weapon)
        {
            foreach (string state in new[]{"Idle","Move","Attack","Logging","Gather","Dead"})
                foreach (string phase in new[]{"Start","Loop","End"})
                {
                    var clip=Resources.Load<AnimationClip>($"Sprites/Player/RedHairGirl/Actions/{weapon}_{state}_{phase}");
                    Assert.That(clip, Is.Not.Null, $"{weapon}/{state}/{phase}");
                    Assert.That(AnimationUtility.GetAnimationClipSettings(clip).loopTime, Is.EqualTo(phase=="Loop"));
                    Assert.That(AnimationUtility.GetObjectReferenceCurveBindings(clip).Length, Is.GreaterThan(0));
                }
        }
    }
}
