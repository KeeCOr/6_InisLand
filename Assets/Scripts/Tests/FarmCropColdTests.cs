#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using IL6;

namespace IL6.Tests
{
    public class FarmCropColdTests
    {
        [Test]
        public void SurvivalChance_AtVeryLowTemperature_FavorsTurnipOverPotatoOverWheat()
        {
            float temp = -32f;

            float turnip = FarmBuilding.CropColdSurvivalChance(FarmBuilding.CropKind.Turnip, temp);
            float potato = FarmBuilding.CropColdSurvivalChance(FarmBuilding.CropKind.Potato, temp);
            float wheat = FarmBuilding.CropColdSurvivalChance(FarmBuilding.CropKind.Wheat, temp);

            Assert.Greater(turnip, potato);
            Assert.Greater(potato, wheat);
        }

        [Test]
        public void YieldMultiplier_AtLowTemperature_FavorsColdHardyCrops()
        {
            float temp = -24f;

            float turnip = FarmBuilding.CropColdYieldMultiplier(FarmBuilding.CropKind.Turnip, temp);
            float potato = FarmBuilding.CropColdYieldMultiplier(FarmBuilding.CropKind.Potato, temp);
            float wheat = FarmBuilding.CropColdYieldMultiplier(FarmBuilding.CropKind.Wheat, temp);

            Assert.Greater(turnip, potato);
            Assert.Greater(potato, wheat);
        }

        [Test]
        public void YieldMultiplier_AtMildTemperature_DoesNotPenalizeAnyCrop()
        {
            float temp = -6f;

            Assert.AreEqual(1f, FarmBuilding.CropColdYieldMultiplier(FarmBuilding.CropKind.Turnip, temp), 0.001f);
            Assert.AreEqual(1f, FarmBuilding.CropColdYieldMultiplier(FarmBuilding.CropKind.Potato, temp), 0.001f);
            Assert.AreEqual(1f, FarmBuilding.CropColdYieldMultiplier(FarmBuilding.CropKind.Wheat, temp), 0.001f);
        }

        [Test]
        public void CropColorForVisual_Withered_IsDarkerThanGrowing()
        {
            var growing = FarmBuilding.CropColorForVisual(FarmBuilding.CropKind.Potato, harvestReady: false, withered: false, growth01: 0.5f);
            var withered = FarmBuilding.CropColorForVisual(FarmBuilding.CropKind.Potato, harvestReady: false, withered: true, growth01: 0.5f);

            Assert.Less(withered.grayscale, growing.grayscale);
        }

        [Test]
        public void CropColorForVisual_HarvestReady_IsBrighterThanGrowing()
        {
            var growing = FarmBuilding.CropColorForVisual(FarmBuilding.CropKind.Wheat, harvestReady: false, withered: false, growth01: 0.5f);
            var ready = FarmBuilding.CropColorForVisual(FarmBuilding.CropKind.Wheat, harvestReady: true, withered: false, growth01: 1f);

            Assert.Greater(ready.grayscale, growing.grayscale);
        }
    }
}
#endif
