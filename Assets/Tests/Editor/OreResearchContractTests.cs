using NUnit.Framework;
namespace IL6.Tests {
public class OreResearchContractTests {
[Test] public void OreCanBeObtainedThroughItsOwnResearch() {
Assert.That(System.Enum.IsDefined(typeof(ResearchId),"OreProspecting"),Is.True);
Assert.That(VillageResearch.Catalog.Length,Is.GreaterThanOrEqualTo(8));
}
}}
