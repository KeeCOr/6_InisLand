using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests {
public class SpawnResearchRulesTests {
[Test] public void NoResearchPreservesDefaultSpawnProbabilities() {
var p=SpawnResearchRules.Chances(.22f,.08f,.1f,.04f,0,0);
Assert.That(p.x,Is.EqualTo(.22f));Assert.That(p.y,Is.EqualTo(.08f));Assert.That(p.z,Is.EqualTo(.1f));Assert.That(p.w,Is.EqualTo(.04f));
}
[Test] public void TenLevelsDoubleResourceAndNpcProbabilitiesAndVisitFrequency() {
var p=SpawnResearchRules.Chances(.22f,.08f,.1f,.04f,10,10);
Assert.That(p.x,Is.EqualTo(.44f).Within(.0001f));Assert.That(p.y,Is.EqualTo(.16f).Within(.0001f));
Assert.That(p.w,Is.EqualTo(.08f).Within(.0001f));Assert.That(p.z,Is.EqualTo(.1f));
Assert.That(SpawnResearchRules.NpcInterval(30,10),Is.EqualTo(15));Assert.That(SpawnResearchRules.NpcInterval(12,10),Is.EqualTo(6));
}
[Test] public void CrowdedResourceSlotsCannotEraseNpcChanceOrExceedOne() {
var p=SpawnResearchRules.Chances(.8f,.8f,.5f,.2f,10,10);
Assert.That(p.w,Is.EqualTo(.4f).Within(.0001f));Assert.That(p.z,Is.EqualTo(.5f).Within(.0001f));
Assert.That(p.x+p.y+p.z+p.w,Is.EqualTo(1).Within(.0001f));Assert.That(p.x,Is.GreaterThanOrEqualTo(0));Assert.That(p.y,Is.GreaterThanOrEqualTo(0));
}
[Test] public void LegacyEightResearchSavePreservesProgressAndStartsNewTracksAtZero() {
var research=new VillageResearch();research.Restore(new[]{1,2,3,4,5,6,7,8});
Assert.That(research.Level(ResearchId.OreProspecting),Is.EqualTo(8));
Assert.That(research.Level(ResearchId.Recruitment),Is.Zero);Assert.That(research.Level(ResearchId.ResourceSurvey),Is.Zero);
research.Restore(new[]{1,2,3,4,5,6,7,8,9,10});var save=JsonUtility.FromJson<SaveFileV1>(JsonUtility.ToJson(new SaveFileV1{researchLevels=research.Snapshot()}));
var restored=new VillageResearch();restored.Restore(save.researchLevels);
Assert.That(restored.Level(ResearchId.Recruitment),Is.EqualTo(9));Assert.That(restored.Level(ResearchId.ResourceSurvey),Is.EqualTo(10));
}
[Test] public void OutOfRangeResearchLevelsCannotAmplifyBeyondTen() {
Assert.That(SpawnResearchRules.Multiplier(999),Is.EqualTo(2));Assert.That(SpawnResearchRules.Multiplier(-1),Is.EqualTo(1));
}
}}
