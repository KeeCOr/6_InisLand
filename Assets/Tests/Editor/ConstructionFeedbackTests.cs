using NUnit.Framework;

namespace IL6.Tests
{
    public sealed class ConstructionFeedbackTests
    {
        [Test]
        public void UnaffordablePlacementExplainsResourceShortage()
        {
            Assert.That(ConstructionPlacement.BlockedReason(true,true,false),Is.EqualTo("자원 부족"));
        }

        [Test]
        public void OccupiedPlacementExplainsOverlapBeforeOtherFailures()
        {
            StringAssert.Contains("겹침",ConstructionPlacement.BlockedReason(false,false,false));
        }

        [Test]
        public void FarmLimitExplainsExpansionRequirement()
        {
            StringAssert.Contains("씨앗 창고",ConstructionPlacement.BlockedReason(true,false,true));
        }

        [Test]
        public void AffordableEmptyPlacementHasNoBlockingReason()
        {
            Assert.That(ConstructionPlacement.BlockedReason(true,true,true),Is.Empty);
        }
    }
}
