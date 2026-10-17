using NUnit.Framework;
using UnityEngine;
namespace IL6.Tests {
public class MaterialLootTests {
[Test] public void OreSnapshotRoundTripsWithoutChangingExistingEnumValues() {
Assert.That((int)ResourceKind.Stone,Is.EqualTo(4));Assert.That((int)ResourceKind.Ore,Is.EqualTo(5));
var store=new ResourceStore();store.Add(ResourceKind.Ore,7);
var snapshot=JsonUtility.FromJson<ResourceSnapshot>(JsonUtility.ToJson(store.Snapshot()));
var restored=new ResourceStore();restored.Restore(snapshot);Assert.That(restored.Get(ResourceKind.Ore),Is.EqualTo(7));
restored.Restore(JsonUtility.FromJson<ResourceSnapshot>("{\"wood\":3}"));Assert.That(restored.Get(ResourceKind.Ore),Is.Zero);
}
[Test] public void MissingOreCannotConsumeOtherResearchMaterials() {
var store=new ResourceStore();store.Add(ResourceKind.Wood,50);store.Add(ResourceKind.Stone,50);
var cost=new ResourceCost(12,6,ore:2);Assert.That(cost.Pay(store),Is.False);
Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(50));Assert.That(store.Get(ResourceKind.Stone),Is.EqualTo(50));
store.Add(ResourceKind.Ore,3);Assert.That(cost.Pay(store),Is.True);Assert.That(store.Get(ResourceKind.Ore),Is.EqualTo(1));
}
[Test] public void OreBeginsAtStageThreeAndIncreasesToEight() {
foreach(var item in VillageResearch.Catalog) {
Assert.That(item.Cost(0).Ore,Is.Zero);Assert.That(item.Cost(1).Ore,Is.Zero);
Assert.That(item.Cost(2).Ore,Is.EqualTo(item.Id==ResearchId.OreProspecting?0:1));Assert.That(item.Cost(9).Ore,Is.EqualTo(item.Id==ResearchId.OreProspecting?0:8)); }
}
[TestCase(0f,0f,false)] [TestCase(.099f,.1f,true)] [TestCase(.1f,.1f,false)] [TestCase(1f,1f,true)]
public void RockRareDropHasExactChanceBoundary(float sample,float chance,bool result) { Assert.That(LootRules.RockDropsOre(sample,chance),Is.EqualTo(result)); }
[Test] public void MeatAndOreDropsUseRealRasterSpritesAndDistinctHudTargets() {
Assert.That(ResourceIconSkin.WorldSprite(ResourceKind.Meat),Is.Not.Null);
Assert.That(ResourceIconSkin.WorldSprite(ResourceKind.Ore),Is.Not.Null);
Assert.That(ResourceIconSkin.WorldSprite(ResourceKind.Ore).texture,Is.SameAs(Resources.Load<Texture2D>("UI/Generated20261017/ore")));
Assert.That(WoodPickup.HudIndex(ResourceKind.Ore),Is.EqualTo(2));Assert.That(WoodPickup.HudIndex(ResourceKind.Meat),Is.EqualTo(3));
for(int i=0;i<6;i++) {var chip=MobileHudLayout.ResourceChip(i);Assert.That(chip.xMax,Is.LessThanOrEqualTo(MobileHudLayout.Resources.xMax));Assert.That(chip.yMax,Is.LessThanOrEqualTo(MobileHudLayout.Resources.yMax));}
}
}}
