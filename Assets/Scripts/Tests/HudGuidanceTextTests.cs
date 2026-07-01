#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using IL6;

namespace IL6.Tests
{
    public class HudGuidanceTextTests
    {
        [Test]
        public void Build_NightThreat_ReturnsTopPriorityObjectiveRiskAndAction()
        {
            var guidance = HudGuidanceText.Build(Phase.Night, activeZombies: 4, wavePending: 2, isBlizzard: false, foodShortage: 0);

            Assert.AreEqual("OBJECTIVE", guidance.Objective.Label);
            Assert.AreEqual("Hold the village line", guidance.Objective.Text);
            Assert.AreEqual(0, guidance.Objective.Priority);
            Assert.AreEqual("RISK", guidance.ImmediateRisk.Label);
            StringAssert.Contains("4 active", guidance.ImmediateRisk.Text);
            StringAssert.Contains("2 pending", guidance.ImmediateRisk.Text);
            Assert.AreEqual(1, guidance.ImmediateRisk.Priority);
            Assert.AreEqual("ACTION", guidance.RecommendedAction.Label);
            StringAssert.Contains("Keep companions near the gate", guidance.RecommendedAction.Text);
            Assert.AreEqual(2, guidance.RecommendedAction.Priority);
        }

        [Test]
        public void Build_BlizzardNight_PrioritizesWarmthAndReturnAction()
        {
            var guidance = HudGuidanceText.Build(Phase.Night, activeZombies: 1, wavePending: 8, isBlizzard: true, foodShortage: 0);

            Assert.AreEqual("Survive the blizzard night", guidance.Objective.Text);
            StringAssert.Contains("visibility low", guidance.ImmediateRisk.Text);
            StringAssert.Contains("8 pending", guidance.ImmediateRisk.Text);
            StringAssert.Contains("campfire", guidance.RecommendedAction.Text);
            Assert.AreEqual("danger", guidance.ImmediateRisk.Tone);
        }

        [Test]
        public void Build_DayFoodShortage_PointsToGatheringBeforeExploration()
        {
            var guidance = HudGuidanceText.Build(Phase.Day, activeZombies: 0, wavePending: 0, isBlizzard: false, foodShortage: 3);

            Assert.AreEqual("Restore food supplies", guidance.Objective.Text);
            StringAssert.Contains("food -3", guidance.ImmediateRisk.Text);
            StringAssert.Contains("Gather berries or hunt", guidance.RecommendedAction.Text);
            Assert.AreEqual("warning", guidance.ImmediateRisk.Tone);
        }

        [Test]
        public void Build_DaySafeState_KeepsThreeTopSlotsInFixedOrder()
        {
            var guidance = HudGuidanceText.Build(Phase.Day, activeZombies: 0, wavePending: 0, isBlizzard: false, foodShortage: 0);

            Assert.AreEqual(3, guidance.TopPrioritySlots.Length);
            Assert.AreSame(guidance.Objective, guidance.TopPrioritySlots[0]);
            Assert.AreSame(guidance.ImmediateRisk, guidance.TopPrioritySlots[1]);
            Assert.AreSame(guidance.RecommendedAction, guidance.TopPrioritySlots[2]);
            Assert.AreEqual("Explore and stockpile", guidance.Objective.Text);
            Assert.AreEqual("No immediate threat", guidance.ImmediateRisk.Text);
            Assert.AreEqual("Gather wood, stone, or scout the next shelter", guidance.RecommendedAction.Text);
        }
    }
}
#endif
