using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class DeathScreenLayoutTests
    {
        [TestCase(1280,720)] [TestCase(1920,720)] [TestCase(1280,960)]
        public void ResultsAndRestartFitWithoutOverlap(int width,int height)
        {
            var layout=new DeathScreenLayout(width,height);
            Rect[] regions={layout.Kicker,layout.Title,layout.Subtitle,layout.Metric(0),layout.Metric(1),layout.Metric(2),layout.Metric(3),layout.Score,layout.Restart,layout.Hint};
            foreach(var r in regions) {
                Assert.That(layout.Panel.Contains(r.min),Is.True);
                Assert.That(layout.Panel.Contains(r.max-Vector2.one*.01f),Is.True);
                Assert.That(r.width,Is.GreaterThan(0));Assert.That(r.height,Is.GreaterThan(0));
            }
            for(int i=0;i<regions.Length;i++) for(int j=i+1;j<regions.Length;j++)
                Assert.That(regions[i].Overlaps(regions[j]),Is.False,$"Regions {i} and {j}");
            Assert.That(layout.Panel.xMin,Is.GreaterThanOrEqualTo(16));
            Assert.That(layout.Panel.yMin,Is.GreaterThanOrEqualTo(16));
            Assert.That(layout.Restart.height,Is.GreaterThanOrEqualTo(48));
        }
    }
}
