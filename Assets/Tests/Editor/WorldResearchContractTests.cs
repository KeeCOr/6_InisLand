using NUnit.Framework;
namespace IL6.Tests {
public class WorldResearchContractTests {
[Test] public void ResearchIncludesRecruitmentAndWorldResourceGeneration() {
Assert.That(System.Enum.IsDefined(typeof(ResearchId),"Recruitment"),Is.True);
Assert.That(System.Enum.IsDefined(typeof(ResearchId),"ResourceSurvey"),Is.True);
Assert.That(VillageResearch.Catalog.Length,Is.EqualTo(10));
}
}}
