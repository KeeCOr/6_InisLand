using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class ExperienceHudLayoutTests
    {
        [Test] public void ExperienceFitsBetweenResourcesAndCompanionPanelWithoutStealingResourceTouches()
        {
            var property=typeof(MobileHudLayout).GetProperty("Experience");Assert.That(property,Is.Not.Null);
            var xp=(Rect)property.GetValue(null);
            Assert.That(xp.yMin,Is.GreaterThan(MobileHudLayout.Resources.yMax));
            Assert.That(xp.x,Is.EqualTo(MobileHudLayout.Resources.x));Assert.That(xp.width,Is.EqualTo(MobileHudLayout.Resources.width));
            Assert.That(xp.height,Is.InRange(18,24));
            Assert.That(xp.Overlaps(MobileHudLayout.ResourceTouchBounds),Is.False);
            Assert.That(xp.yMax,Is.LessThan(MobileHudLayout.CompanionSummary(false).yMin));
        }
    }
}
