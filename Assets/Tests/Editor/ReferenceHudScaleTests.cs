using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class ReferenceHudScaleTests
    {
        [Test]
        public void SecondaryHudLeavesTheWorldVisibleWhilePrimaryAttackStaysLarge()
        {
            Assert.That(MobileHudLayout.Status.width,Is.LessThanOrEqualTo(260));
            Assert.That(MobileHudLayout.Status.height,Is.LessThanOrEqualTo(78));
            Assert.That(MobileHudLayout.Resources.width,Is.LessThanOrEqualTo(480));
            Assert.That(MobileHudLayout.Goals.width,Is.LessThanOrEqualTo(280));
            Assert.That(MobileHudLayout.Clock.height,Is.LessThanOrEqualTo(48));
            Assert.That(MobileHudLayout.Movement.width,Is.InRange(140,150));
            Assert.That(MobileHudLayout.Menu.width,Is.InRange(44,52));
            Assert.That(MobileHudLayout.ActionButton(3).width,Is.GreaterThanOrEqualTo(108));
            Assert.That(MobileHudLayout.ActionButton(0).width,Is.InRange(44,56));
            Assert.That(MobileHudLayout.ActionButton(4).width,Is.InRange(64,80));
        }
        [TestCase(960,540)]
        [TestCase(1280,720)]
        [TestCase(1920,1080)]
        [TestCase(2400,1080)]
        public void PersistentRegionsKeepTheCentralPlayfieldClear(int width,int height)
        {
            var center=new Rect(width*.375f,height*.375f,width*.25f,height*.25f);
            foreach(var r in MobileHudLayout.Regions(width,height)) Assert.That(r.Overlaps(center),Is.False);
        }
    }
}
