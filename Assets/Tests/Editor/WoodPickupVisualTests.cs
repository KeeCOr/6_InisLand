using System.Reflection;
using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class WoodPickupVisualTests
    {
        [Test]
        public void WorldDropUsesSnowfieldArtWhileKeepingThePickupSize()
        {
            var sprite=ResourceIconSkin.WorldWoodSprite;
            Assert.That(sprite,Is.Not.Null);
            Assert.That(sprite.texture,Is.SameAs(Resources.Load<Texture2D>("Sprites/Environment/GroundProps20261103/drops/wood")));
            Assert.That(Mathf.Max(sprite.bounds.size.x,sprite.bounds.size.y),Is.InRange(.64f,.66f));
        }
        [Test]
        public void PickupFlightEndsAtTheCurrentWoodIconAfterLayoutChanges()
        {
            var go=new GameObject("Pickup arc test");
            try
            {
                var pickup=go.AddComponent<WoodPickup>();
                var flags=BindingFlags.Instance|BindingFlags.NonPublic;
                typeof(WoodPickup).GetField("_screenStart",flags).SetValue(pickup,new Vector2(.5f,.5f));
                var sample=typeof(WoodPickup).GetMethod("FlightPoint",flags);
                var end=(Vector2)sample.Invoke(pickup,new object[]{1f});
                Assert.That(Vector2.Distance(end,MobileHudLayout.ResourceIcon(0).center),Is.LessThan(.01f));
                var start=(Vector2)sample.Invoke(pickup,new object[]{0f});
                Assert.That(Vector2.Distance(start,new Vector2(MobileHudLayout.Width*.5f,MobileHudLayout.Height*.5f)),Is.LessThan(.01f));
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
