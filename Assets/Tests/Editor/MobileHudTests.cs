using NUnit.Framework;
using UnityEngine;
using System;

namespace IL6.Tests
{
    public sealed class MobileHudTests
    {
        [TestCase(960, 540)]
        [TestCase(1280, 720)]
        [TestCase(1920, 1080)]
        [TestCase(2400, 1080)]
        public void HudRegionsFitAndDoNotOverlap(int width, int height)
        {
            var type = typeof(SimpleHud).Assembly.GetType("IL6.MobileHudLayout");
            Assert.That(type, Is.Not.Null, "Shared PC/touch layout is required");
            var regions = (Rect[])type.GetMethod("Regions").Invoke(null, new object[] { width, height });
            for (int i = 0; i < regions.Length; i++)
            {
                Assert.That(regions[i].xMin, Is.GreaterThanOrEqualTo(0));
                Assert.That(regions[i].yMin, Is.GreaterThanOrEqualTo(0));
                Assert.That(regions[i].xMax, Is.LessThanOrEqualTo(width));
                Assert.That(regions[i].yMax, Is.LessThanOrEqualTo(height));
                for (int j = i + 1; j < regions.Length; j++)
                    Assert.That(regions[i].Overlaps(regions[j]), Is.False, $"Regions {i}/{j}");
            }
        }

        [Test]
        public void TouchAndKeyboardHaveCallableGameplayActions()
        {
            Assert.That(typeof(PlayerAttackController).GetMethod("TryAttack"), Is.Not.Null);
            Assert.That(typeof(PlayerController).GetMethod("TryInteractDoor"), Is.Not.Null);
            Assert.That(typeof(GatherController).GetMethod("TryStart"), Is.Not.Null);
            Assert.That(typeof(SimpleHud).GetMethod("UsePrimaryAction"), Is.Not.Null);
        }

        [Test]
        public void DirectionPadDoesNotMoveFromActionButtons()
        {
            Assert.That(MobileHudLayout.MoveAt(MobileHudLayout.Actions.center), Is.EqualTo(Vector2.zero));
            Assert.That(MobileHudLayout.MoveAt(MobileHudLayout.Movement.center), Is.EqualTo(Vector2.zero));
            Assert.That(MobileHudLayout.MoveAt(MobileHudLayout.Movement.center + Vector2.up*55).y, Is.LessThan(0));
            Assert.That(MobileHudLayout.MoveAt(MobileHudLayout.Movement.center + Vector2.right*55).x, Is.GreaterThan(0));
        }

        [Test]
        public void CircularButtonsHaveDistinctHitTargetsAndIgnoreCorners()
        {
            for(int i=0;i<7;i++)
            {
                var r=MobileHudLayout.ActionButton(i);
                Assert.That(MobileButtonTheme.Contains(r,r.center),Is.True);
                Assert.That(MobileButtonTheme.Contains(r,r.position),Is.False);
                Assert.That(r.width,Is.GreaterThanOrEqualTo(48));
                Assert.That(r.xMin,Is.GreaterThanOrEqualTo(MobileHudLayout.Actions.xMin));
                Assert.That(r.yMax,Is.LessThanOrEqualTo(MobileHudLayout.Actions.yMax));
                for(int j=i+1;j<7;j++) Assert.That(r.Overlaps(MobileHudLayout.ActionButton(j)),Is.False);
            }
        }
    }
}
