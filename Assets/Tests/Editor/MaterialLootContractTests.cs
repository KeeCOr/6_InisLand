using NUnit.Framework;
namespace IL6.Tests {
public class MaterialLootContractTests {
[Test] public void OreIsARealSavedResourceAndResearchMaterial() {
Assert.That(System.Enum.IsDefined(typeof(ResourceKind),"Ore"),Is.True);
Assert.That(typeof(ResourceSnapshot).GetField("ore"),Is.Not.Null);
Assert.That(typeof(ResourceCost).GetField("Ore"),Is.Not.Null);
}
[Test] public void RockOreDropRateIsConfigurable() {
Assert.That(typeof(BalanceConfig).GetField("RockOreDropChance"),Is.Not.Null);
}
}}
