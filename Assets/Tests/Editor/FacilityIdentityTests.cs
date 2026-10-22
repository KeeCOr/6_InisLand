using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public class FacilityIdentityTests
    {
        public static readonly BuildingKind[] Kinds={BuildingKind.SeedStorage,BuildingKind.Carpenter,
            BuildingKind.Blacksmith,BuildingKind.TrainingCamp,BuildingKind.FoodStorage,
            BuildingKind.LookoutPost,BuildingKind.Sawmill,BuildingKind.Church,BuildingKind.Brazier};
        [Test] public void ReusedFacilitiesHaveNineDedicatedSpritesAndMatchingCatalogPreviews()
        {
            var textures=new HashSet<Texture2D>();
            foreach(var kind in Kinds)
            {
                var sprite=ConstructionPlacement.Sprite(kind);
                Assert.That(sprite,Is.Not.Null,kind.ToString());
                Assert.That(sprite,Is.SameAs(SpriteBank.Facility(kind)));
                Assert.That(SpriteBank.BuildingByKind(kind),Is.SameAs(sprite));
                Assert.That(textures.Add(sprite.texture),Is.True,"Each facility needs a distinct illustration: "+kind);
            }
            Assert.That(ConstructionPlacement.Scale(BuildingKind.Fence),Is.EqualTo(Vector3.one));
        }
    }
}
