using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class GroundPropResourceTests
    {
        [TestCase(ResourceKind.Wood,"wood")][TestCase(ResourceKind.Stone,"stone")]
        [TestCase(ResourceKind.Ore,"ore")][TestCase(ResourceKind.Meat,"meat")]
        [TestCase(ResourceKind.Food,"food")][TestCase(ResourceKind.Frostbloom,"frostbloom")]
        public void GroundPropsUseDedicatedWorldArtInsteadOfHudPictures(ResourceKind kind,string key)
        {
            var drop=ResourceIconSkin.WorldSprite(kind);var site=DiscoverySite.SpriteFor(kind);
            var worldDrop=Resources.Load<Sprite>("Sprites/Environment/GroundProps20261103/drops/"+key);
            var worldSite=Resources.Load<Sprite>("Sprites/Environment/GroundProps20261103/sites/"+key);
            Assert.That(worldDrop,Is.Not.Null);Assert.That(worldSite,Is.Not.Null);
            Assert.That(drop,Is.SameAs(worldDrop));Assert.That(site,Is.SameAs(worldSite));
            Assert.That(drop.texture,Is.Not.SameAs(site.texture));
            Assert.That(drop.texture.filterMode,Is.EqualTo(FilterMode.Bilinear));Assert.That(site.texture.filterMode,Is.EqualTo(FilterMode.Bilinear));
            Assert.That(Mathf.Max(drop.bounds.size.x,drop.bounds.size.y),Is.InRange(.64f,.66f));
            Assert.That(Mathf.Max(site.bounds.size.x,site.bounds.size.y),Is.InRange(.69f,1.41f));
        }
    }
}
