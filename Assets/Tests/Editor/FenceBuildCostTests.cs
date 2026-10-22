using NUnit.Framework;

namespace IL6.Tests
{
    public class FenceBuildCostTests
    {
        [TestCase(1f)]
        [TestCase(1.35f)]
        [TestCase(100f)]
        public void FenceRemainsTheCheapestEvenAfterRepeatedConstruction(float duplicateMultiplier)
        {
            var fence=BuildingUpgradeRules.BuildCost(BuildingKind.Fence,duplicateMultiplier);
            Assert.That(fence.Wood,Is.EqualTo(1));Assert.That(fence.Stone,Is.Zero);
            foreach(BuildingKind kind in System.Enum.GetValues(typeof(BuildingKind)))
            {
                if(kind==BuildingKind.Fence) continue;
                var other=BuildingUpgradeRules.BuildCost(kind,1f);
                Assert.That(fence.Wood+fence.Stone,Is.LessThan(other.Wood+other.Stone),kind.ToString());
            }
        }
    }
}
