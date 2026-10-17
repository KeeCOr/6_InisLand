using NUnit.Framework;
using UnityEngine;
using System.Reflection;
namespace IL6.Tests
{
    public sealed class CompactResourceHudTests
    {
        [Test]
        public void FiveResourceIconsAreLoadedFromGeneratedRasterAtlas()
        {
            Assert.That(Resources.Load<Texture2D>("UI/Generated20261014/resources"),Is.Not.Null);
            Assert.That(ResourceIconSkin.IconCount,Is.EqualTo(5));
        }
        [Test]
        public void ResourcesUseTwoCompactRowsWithoutLargeInventoryCard()
        {
            Assert.That(MobileHudLayout.Resources.height,Is.LessThanOrEqualTo(64));
            var method=typeof(MobileHudLayout).GetMethod("ResourceChip");
            Assert.That(method,Is.Not.Null);
            for(int i=0;i<5;i++)
            {
                var r=(Rect)method.Invoke(null,new object[]{i});
                Assert.That(r.yMin,Is.GreaterThanOrEqualTo(MobileHudLayout.Resources.y));
                Assert.That(r.yMax,Is.LessThanOrEqualTo(MobileHudLayout.Resources.yMax));
                Assert.That(r.xMax,Is.LessThanOrEqualTo(MobileHudLayout.Resources.xMax));
                for(int j=i+1;j<5;j++) Assert.That(r.Overlaps((Rect)method.Invoke(null,new object[]{j})),Is.False);
            }
        }
        [TestCase(999,"999")]
        [TestCase(1200,"1.2K")]
        [TestCase(1999,"1.9K")]
        [TestCase(2000000,"2M")]
        public void CountsFitTypicalGameCurrencyLabelsWithoutRoundingUp(int value,string expected)
        {
            var type=typeof(SimpleHud).Assembly.GetType("IL6.ResourceHudFormat");
            Assert.That(type,Is.Not.Null);
            Assert.That(type.GetMethod("Compact").Invoke(null,new object[]{value}),Is.EqualTo(expected));
        }
    }
}
