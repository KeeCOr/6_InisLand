using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class ConstructionGridTests
    {
        [TestCase(1.24f, -2.34f, 1f, -2f)]
        [TestCase(-1.76f, 2.78f, -2f, 3f)]
        public void PlacementSnapsToWorldGrid(float x,float y,float sx,float sy)
        {
            var snapped=ConstructionPlacement.Snap(new Vector3(x,y,7));
            Assert.That(snapped,Is.EqualTo(new Vector3(sx,sy,0)));
            Assert.That(ConstructionPlacement.Snap(snapped),Is.EqualTo(snapped));
        }
        [Test] public void ExistingBuildingsAndUnfinishedSitesReserveTheirFootprints()
        {
            var point=new Vector3(1000,-1000,0);
            var house=BuildingFactory.SpawnHouse(point);
            try {
                Assert.That(ConstructionPlacement.IsAvailable(BuildingKind.House,point),Is.False);
                Assert.That(ConstructionPlacement.IsAvailable(BuildingKind.House,point+Vector3.right),Is.False);
                Assert.That(ConstructionPlacement.IsAvailable(BuildingKind.House,point+Vector3.right*2),Is.False);
                Assert.That(ConstructionPlacement.IsAvailable(BuildingKind.House,point+Vector3.right*3),Is.False);
                Assert.That(ConstructionPlacement.IsAvailable(BuildingKind.House,point+Vector3.right*4),Is.True);
            } finally { Object.DestroyImmediate(house); }
            var go=new GameObject("Reserved construction");go.transform.position=point;
            go.AddComponent<ConstructionSite>().Kind=BuildingKind.House;
            try { Assert.That(ConstructionPlacement.IsAvailable(BuildingKind.House,point),Is.False); }
            finally { Object.DestroyImmediate(go); }
        }
        [Test] public void FenceKeepsGateSegmentSpacing()
        {
            Assert.That(ConstructionPlacement.Scale(BuildingKind.Fence),Is.EqualTo(Vector3.one));
        }
        [Test] public void BuildingKindsHaveIndividualUniformProportions()
        {
            Assert.That(ConstructionPlacement.Scale(BuildingKind.Watchtower),Is.EqualTo(new Vector3(1.8f,1.8f,1f)));
            Assert.That(ConstructionPlacement.Scale(BuildingKind.LookoutPost),Is.EqualTo(new Vector3(1.5f,1.5f,1f)));
            Assert.That(ConstructionPlacement.Scale(BuildingKind.Storage),Is.EqualTo(new Vector3(2.4f,2.4f,1f)));
            foreach(BuildingKind kind in System.Enum.GetValues(typeof(BuildingKind)))
                Assert.That(ConstructionPlacement.Scale(kind).x,Is.EqualTo(ConstructionPlacement.Scale(kind).y),kind.ToString());
        }
        [Test] public void HouseAndColliderUseTheEnlargedPlacementDimensions()
        {
            var house=BuildingFactory.SpawnHouse(Vector3.zero);
            try {
                Assert.That(house.transform.localScale.x,Is.EqualTo(3.2f).Within(.001f));
                Assert.That(house.transform.localScale.y,Is.EqualTo(3.2f).Within(.001f));
                Assert.That(house.GetComponent<BoxCollider2D>().size.x*house.transform.localScale.x,Is.EqualTo(3.2f).Within(.001f));
            } finally {Object.DestroyImmediate(house);}
        }
    }
}
