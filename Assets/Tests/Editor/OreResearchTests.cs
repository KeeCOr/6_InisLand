using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests {
public class OreResearchTests {
private GameObject _go;private Building _station;private ResourceStore _store;private VillageResearch _research;
[SetUp] public void Setup() {
_go=new GameObject("Ore research test");_station=_go.AddComponent<Building>();_station.Initialize(BuildingKind.Blacksmith);
_store=new ResourceStore();_store.IncreaseCap(1000);_store.Add(ResourceKind.Wood,1000);_store.Add(ResourceKind.Stone,1000);_research=new VillageResearch();
}
[TearDown] public void Cleanup() {Object.DestroyImmediate(_go);}
[Test] public void CanResearchWithZeroOreAndObtainFiftyAcrossTenStages() {
for(int i=0;i<10;i++) {
Assert.That(VillageResearch.Catalog[(int)ResearchId.OreProspecting].Cost(i).Ore,Is.Zero);
Assert.That(_research.TryUpgrade(ResearchId.OreProspecting,_station,Vector3.zero,_store),Is.True);
Assert.That(_store.Get(ResourceKind.Ore),Is.EqualTo((i+1)*5));
}
Assert.That(_research.TryUpgrade(ResearchId.OreProspecting,_station,Vector3.zero,_store),Is.False);
Assert.That(_store.Get(ResourceKind.Ore),Is.EqualTo(50));Assert.That(_store.Get(ResourceKind.Wood),Is.EqualTo(340));Assert.That(_store.Get(ResourceKind.Stone),Is.EqualTo(670));
}
[Test] public void InsufficientRewardSpaceDoesNotConsumeResourcesOrResearchStage() {
_store.Add(ResourceKind.Ore,96);
Assert.That(_research.TryUpgrade(ResearchId.OreProspecting,_station,Vector3.zero,_store),Is.False);
Assert.That(_research.Level(ResearchId.OreProspecting),Is.Zero);Assert.That(_store.Get(ResourceKind.Wood),Is.EqualTo(1000));
_store.Spend(ResourceKind.Ore,1);
Assert.That(_research.TryUpgrade(ResearchId.OreProspecting,_station,Vector3.zero,_store),Is.True);Assert.That(_store.Get(ResourceKind.Ore),Is.EqualTo(100));
}
[Test] public void OldSevenResearchSaveStartsProspectingAtZeroAndNewProgressPersists() {
_research.Restore(new[]{2,3,4,5,6,7,8});Assert.That(_research.Level(ResearchId.OreProspecting),Is.Zero);
Assert.That(_research.TryUpgrade(ResearchId.OreProspecting,_station,Vector3.zero,_store),Is.True);
var data=new SaveFileV1 {researchLevels=_research.Snapshot(),resources=_store.Snapshot()};
var restored=JsonUtility.FromJson<SaveFileV1>(JsonUtility.ToJson(data));var research=new VillageResearch();research.Restore(restored.researchLevels);
Assert.That(research.Level(ResearchId.OreProspecting),Is.EqualTo(1));Assert.That(restored.resources.ore,Is.EqualTo(5));Assert.That(research.Level(ResearchId.Morale),Is.EqualTo(8));
}
}}
