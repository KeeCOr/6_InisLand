using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class UiOverlayFlowTests
    {
        [UnityTest]
        public IEnumerator RunePanelDoesNotUnpauseMenuAndBuildingSelectionDefersSpending()
        {
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            typeof(SimpleHud).GetMethod("PrepareHudPreview",flags).Invoke(hud,null);
            var details=typeof(SimpleHud).GetField("_resourceDetailsOpen",flags);
            var detailsRect=(Rect)typeof(SimpleHud).GetProperty("ResourceDetailsRect",flags).GetValue(hud);
            var hudPoint=typeof(SimpleHud).GetMethod("IsHudPoint",flags);
            Assert.That((bool)hudPoint.Invoke(hud,new object[]{detailsRect.center}),Is.False);
            details.SetValue(hud,true);
            Assert.That((bool)hudPoint.Invoke(hud,new object[]{detailsRect.center}),Is.True,"Expanded resource details must intercept world placement clicks");
            Assert.That(Time.timeScale,Is.EqualTo(1),"Resource inspection does not pause the world");
            details.SetValue(hud,false);
            typeof(SimpleHud).GetField("_paused",flags).SetValue(hud,true);
            Time.timeScale=0;
            typeof(SimpleHud).GetMethod("DrawRuneModal",flags).Invoke(hud,null);
            Assert.That(Time.timeScale,Is.EqualTo(0),"An inactive rune panel must not resume a paused menu");
            typeof(SimpleHud).GetField("_paused",flags).SetValue(hud,false);Time.timeScale=1;
            typeof(SimpleHud).GetMethod("SetHudMode",flags).Invoke(hud,new object[]{SimpleHud.HudMode.Build});
            var catalog=typeof(SimpleHud).GetField("_buildCatalogOpen",flags);
            Assert.That(catalog,Is.Not.Null);
            Assert.That((bool)catalog.GetValue(hud),Is.True);
            Assert.That(Time.timeScale,Is.EqualTo(0),"Entering build mode must immediately freeze gameplay");
            typeof(SimpleHud).GetMethod("DrawRuneModal",flags).Invoke(hud,null);
            Assert.That(Time.timeScale,Is.EqualTo(0),"Inactive overlays must preserve the construction pause");
            var resources=GameSession.Instance.Resources;
            resources.Add(ResourceKind.Wood,100);resources.Add(ResourceKind.Stone,100);
            int wood=resources.Get(ResourceKind.Wood),stone=resources.Get(ResourceKind.Stone);
            var choose=typeof(SimpleHud).GetMethod("TrySelectBuilding");Assert.That(choose,Is.Not.Null);
            Assert.That((bool)choose.Invoke(hud,new object[]{BuildingKind.House}),Is.True);
            Assert.That((bool)catalog.GetValue(hud),Is.False);
            Assert.That(resources.Get(ResourceKind.Wood),Is.EqualTo(wood));
            Assert.That(resources.Get(ResourceKind.Stone),Is.EqualTo(stone));
            Assert.That((BuildingKind?)typeof(SimpleHud).GetField("_pendingBuildKind",flags).GetValue(hud),Is.EqualTo(BuildingKind.House));
            Assert.That(Time.timeScale,Is.EqualTo(0),"Selecting a building must keep placement paused");
            Assert.That((bool)typeof(SimpleHud).GetMethod("IsHudPoint",flags).Invoke(hud,new object[]{new Vector2(500,360)}),Is.False,"Paused building mode must accept world placement clicks");
            float multiplier=(float)typeof(SimpleHud).GetMethod("CostMul",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{BuildingKind.House});
            var cost=BuildingUpgradeRules.BuildCost(BuildingKind.House,multiplier);
            var place=typeof(SimpleHud).GetMethod("TryPlaceBuildingAt");
            Assert.That(place,Is.Not.Null);
            var position=new Vector3(1000.34f,-1000.27f,0);
            Assert.That((bool)place.Invoke(hud,new object[]{position}),Is.True);
            Assert.That(resources.Get(ResourceKind.Wood),Is.EqualTo(wood-cost.Wood));
            Assert.That(resources.Get(ResourceKind.Stone),Is.EqualTo(stone-cost.Stone));
            Assert.That(Time.timeScale,Is.EqualTo(0),"Successful placement keeps construction paused for the next building");
            Assert.That((BuildingKind?)typeof(SimpleHud).GetField("_pendingBuildKind",flags).GetValue(hud),Is.EqualTo(BuildingKind.House));
            var site=Object.FindFirstObjectByType<ConstructionSite>();
            Assert.That(site,Is.Not.Null);
            Assert.That(site.transform.position,Is.EqualTo(new Vector3(1000,-1000,0)));
            Assert.That(site.transform.localScale,Is.EqualTo(ConstructionPlacement.Scale(BuildingKind.House)));
            Assert.That(site.GetComponent<SpriteRenderer>().sprite,Is.EqualTo(ConstructionPlacement.Sprite(BuildingKind.House)));
            typeof(SimpleHud).GetMethod("CloseBuildCatalog",flags).Invoke(hud,null);
            Assert.That(Time.timeScale,Is.EqualTo(1),"Only leaving construction restores gameplay");
            Time.timeScale=.5f;
            typeof(SimpleHud).GetMethod("SetHudMode",flags).Invoke(hud,new object[]{SimpleHud.HudMode.Build});
            Assert.That(Time.timeScale,Is.EqualTo(0));
            Assert.That(hud.TrySelectBuilding(BuildingKind.House),Is.True);
            int beforeCancel=resources.Get(ResourceKind.Wood);
            Assert.That(hud.TryPlaceBuildingAt(position),Is.False,"An unfinished site must prevent duplicate placement");
            Assert.That(resources.Get(ResourceKind.Wood),Is.EqualTo(beforeCancel),"Rejected overlap must not charge resources");
            Assert.That(Time.timeScale,Is.EqualTo(0),"Rejected placement stays in building mode");
            typeof(SimpleHud).GetMethod("CloseBuildCatalog",flags).Invoke(hud,null);
            Assert.That(Time.timeScale,Is.EqualTo(.5f),"Cancel restores the previous game speed");
            Assert.That(resources.Get(ResourceKind.Wood),Is.EqualTo(beforeCancel));
            Assert.That(typeof(SimpleHud).GetField("_pendingBuildKind",flags).GetValue(hud),Is.Null);
            Time.timeScale=1;
            int beforeFence=resources.Get(ResourceKind.Wood),stoneBeforeFence=resources.Get(ResourceKind.Stone);
            typeof(SimpleHud).GetMethod("SetHudMode",flags).Invoke(hud,new object[]{SimpleHud.HudMode.Build});
            Assert.That(hud.TrySelectBuilding(BuildingKind.Fence),Is.True);
            Assert.That(hud.TryPlaceBuildingAt(position+Vector3.right*5),Is.True);
            Assert.That(resources.Get(ResourceKind.Wood),Is.EqualTo(beforeFence-1),"Fence placement must cost one wood despite existing village fences");
            Assert.That(resources.Get(ResourceKind.Stone),Is.EqualTo(stoneBeforeFence));
            Assert.That(Time.timeScale,Is.EqualTo(0));
            Assert.That(hud.TryPlaceBuildingAt(position+Vector3.right*6),Is.True,"Adjacent fence segments can be placed without reopening the catalog");
            Assert.That(resources.Get(ResourceKind.Wood),Is.EqualTo(beforeFence-2));
            Assert.That(resources.Get(ResourceKind.Stone),Is.EqualTo(stoneBeforeFence));
            resources.Restore(new ResourceSnapshot());
            Assert.That(hud.TryPlaceBuildingAt(position+Vector3.right*7),Is.False,"Repeated placement rechecks affordability");
            Assert.That(resources.Get(ResourceKind.Wood),Is.Zero);
            Assert.That(Time.timeScale,Is.EqualTo(0),"Resource shortage does not silently leave construction");
            Assert.That((BuildingKind?)typeof(SimpleHud).GetField("_pendingBuildKind",flags).GetValue(hud),Is.EqualTo(BuildingKind.Fence));
            typeof(SimpleHud).GetMethod("CloseBuildCatalog",flags).Invoke(hud,null);
            Assert.That(Time.timeScale,Is.EqualTo(1));
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
