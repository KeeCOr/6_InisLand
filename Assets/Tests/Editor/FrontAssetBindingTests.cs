using NUnit.Framework;
using UnityEngine;

namespace IL6.Tests
{
    public class FrontAssetBindingTests
    {
        private static Sprite Expected(string relative) =>
            Resources.Load<Sprite>("Sprites/GeneratedStatic20261014/" + relative);

        [TestCase(BuildingKind.Barricade, "wood_barricade")]
        [TestCase(BuildingKind.House, "cabin")]
        [TestCase(BuildingKind.Storage, "storage")]
        [TestCase(BuildingKind.Watchtower, "watchtower")]
        [TestCase(BuildingKind.Farm, "farm")]
        [TestCase(BuildingKind.Infirmary, "infirmary")]
        [TestCase(BuildingKind.HuntersHut, "hunters_hut")]
        public void BuildingCatalog_UsesDedicatedFrontSprite(BuildingKind kind, string name)
        {
            Assert.That(SpriteBank.BuildingByKind(kind), Is.SameAs(Expected("Buildings/" + name)));
        }

        [TestCase("Hunter", "hunter")]
        [TestCase("Warrior", "warrior")]
        [TestCase("Farmer", "farmer")]
        [TestCase("Cook", "cook")]
        [TestCase("Healer", "healer")]
        [TestCase("사냥꾼", "hunter")]
        [TestCase("전사", "warrior")]
        [TestCase("농부", "farmer")]
        [TestCase("요리사", "cook")]
        [TestCase("치유사", "healer")]
        public void CompanionRole_UsesDedicatedFrontSprite(string role, string name)
        {
            Assert.That(SpriteBank.CompanionSpriteForRole(role), Is.SameAs(Expected("Companions/" + name)));
        }

        [TestCase("Fox_proc", "fox")]
        [TestCase("SnowHare_proc", "snow_hare")]
        public void AnimalVariant_DoesNotReuseBearOrRabbit(string archetype, string name)
        {
            Assert.That(SpriteBank.AnimalByName(archetype), Is.SameAs(Expected("Animals/" + name)));
        }

        [TestCase(BuildingKind.Barricade, "wood_barricade")]
        [TestCase(BuildingKind.House, "cabin")]
        [TestCase(BuildingKind.Storage, "storage")]
        [TestCase(BuildingKind.Watchtower, "watchtower")]
        [TestCase(BuildingKind.Farm, "farm")]
        [TestCase(BuildingKind.Infirmary, "infirmary")]
        [TestCase(BuildingKind.HuntersHut, "hunters_hut")]
        public void RuntimeBuildingFactory_BindsTheFrontSprite(BuildingKind kind, string name)
        {
            GameObject go = null;
            try
            {
                go = kind switch {
                    BuildingKind.Barricade => BuildingFactory.SpawnBarricade(Vector3.zero),
                    BuildingKind.House => BuildingFactory.SpawnHouse(Vector3.zero),
                    BuildingKind.Storage => BuildingFactory.SpawnStorage(Vector3.zero),
                    BuildingKind.Watchtower => BuildingFactory.SpawnWatchtower(Vector3.zero),
                    BuildingKind.Farm => BuildingFactory.SpawnFarm(Vector3.zero),
                    BuildingKind.Infirmary => BuildingFactory.SpawnInfirmary(Vector3.zero),
                    _ => BuildingFactory.SpawnHuntersHut(Vector3.zero)
                };
                Assert.That(go.GetComponent<SpriteRenderer>().sprite, Is.SameAs(Expected("Buildings/" + name)));
            }
            finally { if (go != null) Object.DestroyImmediate(go); }
        }
    }
}
