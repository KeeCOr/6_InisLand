using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public class MusicAssetTests
    {
        [TestCase(Phase.Day, "day")]
        [TestCase(Phase.Night, "night")]
        [TestCase(Phase.Evening, "evening")]
        [TestCase(Phase.Dawn, "dawn")]
        public void PhaseUsesLongStereoRecording(Phase phase, string name)
        {
            var clip = Resources.Load<AudioClip>("Music/" + name);
            Assert.That(clip, Is.Not.Null, "Time-of-day music must ship in the Unity player");
            Assert.That(clip.length, Is.GreaterThan(90f), "Avoid a short repeated motif");
            Assert.That(clip.channels, Is.EqualTo(2));
        }

        [Test] public void AttributionShipsInsideThePlayer()
        {
            var credits = Resources.Load<TextAsset>("Music/CREDITS");
            Assert.That(credits, Is.Not.Null);
            StringAssert.Contains("Original InisLand", credits.text);
            StringAssert.Contains("GeneralUser GS", credits.text);
            StringAssert.Contains("edited", credits.text);
        }
    }
}
