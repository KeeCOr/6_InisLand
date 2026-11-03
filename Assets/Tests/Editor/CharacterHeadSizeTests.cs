using NUnit.Framework;
namespace IL6.Tests
{
    public class CharacterHeadSizeTests
    {
        [TestCase("axe",90f)][TestCase("bow",106f)][TestCase("logging",84f)]
        public void HeroWeaponSheetsKeepTheSameHeadHeight(string sheet,float headPixels)
        {
            var sprites=UnityEngine.Resources.LoadAll<UnityEngine.Sprite>("Sprites/Player/RedHairGirl/"+sheet);
            Assert.That(sprites,Is.Not.Empty);
            Assert.That(headPixels/sprites[0].pixelsPerUnit,Is.EqualTo(.4f).Within(.03f));
        }
        // Reviewed side-idle silhouette spans in the head_proportion_manifest source set.
        // Include the cap/hood/fur down to the covered neck, but exclude hanging braids.
        // Pixel spans must be remeasured when source resolution changes; the world-size
        // target/tolerance stays fixed so higher-density art cannot silently enlarge heads.
        [TestCase("Hunter",166f)][TestCase("Warrior",120f)]
        [TestCase("Farmer",146f)][TestCase("Cook",130f)]
        [TestCase("Healer",144f)][TestCase("Aunt",146f)]
        [TestCase("Uncle",136f)][TestCase("Child",161f)]
        public void CompanionHeadMatchesHeroReference(string role,float headPixels)
        {
            var sprite=SpriteBank.CompanionSpriteForRole(role);
            float height=headPixels/sprite.pixelsPerUnit*SpriteBank.CompanionScaleForRole(role).y;
            Assert.That(height,Is.EqualTo(.4f).Within(.09f),role+" head world height");
        }
    }
}
