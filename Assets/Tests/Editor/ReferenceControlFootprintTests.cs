using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class ReferenceControlFootprintTests
    {
        [Test]
        public void ActionClusterIsSmallerWithSeparateWeaponCaptionsAndHitTargets()
        {
            var region=MobileHudLayout.Actions;
            Assert.That(region.width*region.height,Is.LessThan(320*224*.7f));
            for(int i=0;i<4;i++)
            {
                var button=MobileHudLayout.ActionButton(i);
                Assert.That(region.Contains(button.min),Is.True);
                Assert.That(button.xMax,Is.LessThanOrEqualTo(region.xMax));
                Assert.That(button.yMax,Is.LessThanOrEqualTo(region.yMax));
                Assert.That(button.width,Is.GreaterThanOrEqualTo(60));
                for(int j=i+1;j<4;j++) Assert.That(button.Overlaps(MobileHudLayout.ActionButton(j)),Is.False);
            }
        }
    }
}
