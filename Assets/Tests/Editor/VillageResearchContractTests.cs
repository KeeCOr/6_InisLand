using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests {
public class VillageResearchContractTests {
[Test] public void FenceStartsAtForty() { Assert.That(BalanceConfig.Instance.FenceHp,Is.EqualTo(40)); }
[Test] public void BuildingResearchHasItsOwnSavedProgress() {
Assert.That(typeof(Building).Assembly.GetType("IL6.VillageResearch"),Is.Not.Null);
Assert.That(typeof(SaveFileV1).GetField("researchLevels"),Is.Not.Null);
}
}}
