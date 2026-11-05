using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public sealed class HomeNavigationTests
    {
        private static Building House(Vector2 point,BuildingKind kind=BuildingKind.House)
        {var go=new GameObject("Navigation destination");go.transform.position=point;var building=go.AddComponent<Building>();building.Initialize(kind);return building;}
        [Test] public void DestinationStaysAtVillageHouseInsteadOfNearestFenceOrRemoteBuilding()
        {
            Vector2 village=new Vector2(GameConstants.VillageCenterX,GameConstants.VillageCenterY);
            var home=House(village+new Vector2(-2,-2));var distant=House(village+Vector2.right*29);var fence=House(village+Vector2.right*30,BuildingKind.Fence);
            try {
                var buildings=new[]{fence,distant,home};var frame=HomeNavigation.Resolve(village+Vector2.right*31,buildings);
                Assert.That(frame.Destination,Is.EqualTo((Vector2)home.transform.position));Assert.That(frame.HasHouse,Is.True);
                Assert.That(HomeNavigation.Resolve(village+Vector2.left*31,buildings).Destination,Is.EqualTo(frame.Destination));
                home.enabled=false;Assert.That(HomeNavigation.Resolve(village,buildings).Destination,Is.EqualTo((Vector2)distant.transform.position));
            } finally {Object.DestroyImmediate(home.gameObject);Object.DestroyImmediate(distant.gameObject);Object.DestroyImmediate(fence.gameObject);}
        }
        [Test] public void MissingHouseFallsBackToVillageAndUsesClearGridDistance()
        {
            Vector2 village=new Vector2(GameConstants.VillageCenterX,GameConstants.VillageCenterY);
            var frame=HomeNavigation.Resolve(village+new Vector2(12,5),new Building[0]);
            Assert.That(frame.Destination,Is.EqualTo(village));Assert.That(frame.HasHouse,Is.False);Assert.That(frame.Label,Is.EqualTo("마을까지 13칸"));
            Assert.That(frame.Direction.x,Is.LessThan(0));Assert.That(frame.Direction.y,Is.LessThan(0));
            Assert.That(frame.Visible,Is.True);Assert.That(HomeNavigation.Resolve(village,new Building[0]).Visible,Is.False);
        }
        [TestCase(1280)][TestCase(1920)] public void SmallNavigationDoesNotOverlapClockMenuOrResourcePanels(float width)
        {
            var rect=HomeNavigation.Bounds(width);
            Assert.That(rect.Overlaps(new Rect(width/2-94,16,188,44)),Is.False);
            Assert.That(rect.Overlaps(new Rect(width-70,16,54,74)),Is.False);
            Assert.That(rect.Overlaps(new Rect(16,16,280,68)),Is.False);
            Assert.That(rect.xMax,Is.LessThan(width-280));Assert.That(rect.height,Is.EqualTo(32));
        }
    }
}
