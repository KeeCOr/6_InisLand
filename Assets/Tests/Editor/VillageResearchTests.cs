using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests
{
    public class VillageResearchTests
    {
        private GameObject _go;
        private Building _station;
        private VillageResearch _research;
        private ResourceStore _store;
        [SetUp] public void Setup() {
            _research=new VillageResearch();_store=new ResourceStore();
            _store.IncreaseCap(1000);_store.Add(ResourceKind.Wood,1000);_store.Add(ResourceKind.Stone,1000);_store.Add(ResourceKind.Ore,100);
            _go=new GameObject("Research station test");_station=_go.AddComponent<Building>();_station.Initialize(BuildingKind.Carpenter);
        }
        [TearDown] public void Cleanup() { Object.DestroyImmediate(_go); }
        [Test] public void CannotBuyFromWrongBuildingFarAwayOrWithoutResources() {
            Assert.That(_research.TryUpgrade(ResearchId.Combat,_station,Vector3.zero,_store),Is.False);
            Assert.That(_research.TryUpgrade(ResearchId.Fence,_station,Vector3.right*4,_store),Is.False);
            Assert.That(_research.TryUpgrade(ResearchId.Fence,_station,Vector3.zero,new ResourceStore()),Is.False);
            Assert.That(_research.Level(ResearchId.Fence),Is.Zero);
            Assert.That(_store.Get(ResourceKind.Wood),Is.EqualTo(1000));
        }
        [Test] public void ResearchUsesExactCostAndCapsAtTenIndependentLevels() {
            for(int i=0;i<10;i++) Assert.That(_research.TryUpgrade(ResearchId.Fence,_station,Vector3.zero,_store),Is.True);
            Assert.That(_research.Level(ResearchId.Fence),Is.EqualTo(10));
            Assert.That(_research.Level(ResearchId.Combat),Is.Zero);
            Assert.That(_store.Get(ResourceKind.Wood),Is.EqualTo(340));
            Assert.That(_store.Get(ResourceKind.Stone),Is.EqualTo(670));
            Assert.That(_research.TryUpgrade(ResearchId.Fence,_station,Vector3.zero,_store),Is.False);
            Assert.That(_store.Get(ResourceKind.Wood),Is.EqualTo(340));
        }
        [Test] public void DisabledAndUnfinishedStationsCannotResearch() {
            _go.AddComponent<ConstructionSite>();
            Assert.That(_research.TryUpgrade(ResearchId.Fence,_station,Vector3.zero,_store),Is.False);
            Object.DestroyImmediate(_go.GetComponent<ConstructionSite>());
            _station.enabled=false;
            Assert.That(_research.TryUpgrade(ResearchId.Fence,_station,Vector3.zero,_store),Is.False);
        }
        [Test] public void ResearchSaveRoundTripAndLegacySaveAreSafe() {
            _research.Restore(new[]{2,1,0,4,3,2,1});
            var save=new SaveFileV1 {researchLevels=_research.Snapshot()};
            var copy=JsonUtility.FromJson<SaveFileV1>(JsonUtility.ToJson(save));var restored=new VillageResearch();restored.Restore(copy.researchLevels);
            Assert.That(restored.Snapshot(),Is.EqualTo(_research.Snapshot()));
            copy.researchLevels[0]=0;Assert.That(restored.Level(ResearchId.Fence),Is.EqualTo(2));
            restored.Restore(null);Assert.That(restored.Level(ResearchId.Fence),Is.Zero);
            restored.Restore(new[]{-3,99});Assert.That(restored.Level(ResearchId.Fence),Is.Zero);Assert.That(restored.Level(ResearchId.Combat),Is.EqualTo(10));
        }
        [Test] public void PreviousMaximumSaveCanContinueToFifthStage() {
            _research.Restore(new[]{4,4,4,4,4,4,4});
            Assert.That(_research.TryUpgrade(ResearchId.Fence,_station,Vector3.zero,_store),Is.True);
            Assert.That(_research.Level(ResearchId.Fence),Is.EqualTo(5));
            var restored=new VillageResearch();restored.Restore(_research.Snapshot());
            Assert.That(restored.Level(ResearchId.Fence),Is.EqualTo(5));
        }
        [Test] public void EachResearchHasADistinctBuildingAndIncreasingCosts() {
            var kinds=new System.Collections.Generic.HashSet<BuildingKind>();
            foreach(var item in VillageResearch.Catalog) { Assert.That(kinds.Add(item.Station),Is.True);Assert.That(item.Cost(3).Wood,Is.EqualTo(4*item.Cost(0).Wood)); }
            Assert.That(kinds.Count,Is.EqualTo(10));
        }
        [Test] public void UnrelatedBuildingsDoNotInflateFenceBaseHealth() {
            var fence=new GameObject("Fence test");try {
                var building=fence.AddComponent<Building>();building.Initialize(BuildingKind.Fence);
                Assert.That(building.MaxHp,Is.EqualTo(40));
                Assert.That(building.TryUpgrade(_store),Is.False,"Fence upgrades belong to carpenter research");
            } finally { Object.DestroyImmediate(fence); }
        }
    }
}
