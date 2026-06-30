#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using IL6;

namespace IL6.Tests
{
    public class HudGuidanceTextTests
    {
        [Test]
        public void Build_NightThreat_ReturnsStatusRiskAndNextAction()
        {
            var guidance = HudGuidanceText.Build(Phase.Night, activeZombies: 4, wavePending: 2, isBlizzard: false, foodShortage: 0);

            Assert.AreEqual("상태: 밤 방어 중", guidance.Status);
            StringAssert.Contains("좀비 4", guidance.Risk);
            StringAssert.Contains("대기 2", guidance.Risk);
            StringAssert.Contains("동료를 사수로", guidance.NextAction);
        }

        [Test]
        public void Build_BlizzardNight_PrioritizesWarmthAndVillageReturn()
        {
            var guidance = HudGuidanceText.Build(Phase.Night, activeZombies: 1, wavePending: 8, isBlizzard: true, foodShortage: 0);

            Assert.AreEqual("상태: 눈보라 밤", guidance.Status);
            StringAssert.Contains("시야", guidance.Risk);
            StringAssert.Contains("모닥불", guidance.NextAction);
        }

        [Test]
        public void Build_DayFoodShortage_PointsToGatheringBeforeExploration()
        {
            var guidance = HudGuidanceText.Build(Phase.Day, activeZombies: 0, wavePending: 0, isBlizzard: false, foodShortage: 3);

            Assert.AreEqual("상태: 보급 부족", guidance.Status);
            StringAssert.Contains("식량 -3", guidance.Risk);
            StringAssert.Contains("채집", guidance.NextAction);
        }
    }
}
#endif