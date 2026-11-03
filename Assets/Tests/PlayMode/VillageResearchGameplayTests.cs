using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class VillageResearchGameplayTests
    {
        [UnityTest] public IEnumerator BuildingResearchAppliesEffectsAndRestoresPauseAndSave()
        {
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("BootScene");yield return null;yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            hud.Player.transform.position=new Vector3(1000,1000,0);
            var session=GameSession.Instance;var previous=session.Research.Snapshot();var resources=session.Resources.Snapshot();
            string path=SaveLoad.StoragePath;byte[] saved=File.Exists(path)?File.ReadAllBytes(path):null;
            var objects=new List<GameObject>();
            session.Research.Restore(null);session.Resources.IncreaseCap(1000);session.Resources.Add(ResourceKind.Wood,700);session.Resources.Add(ResourceKind.Stone,700);session.Resources.Spend(ResourceKind.Ore,session.Resources.Get(ResourceKind.Ore));session.Resources.Add(ResourceKind.Ore,90);
            try {
                var fenceGo=new GameObject("Research fence");objects.Add(fenceGo);var fence=fenceGo.AddComponent<Building>();fence.Initialize(BuildingKind.Fence);
                Assert.That(fence.MaxHp,Is.EqualTo(100));
                Time.timeScale=.5f;Assert.That(hud.OpenResearchCatalog(),Is.True);Assert.That(Time.timeScale,Is.Zero);
                Assert.That(hud.TryResearch(ResearchId.Fence),Is.False);
                var spawnProbeGo=new GameObject("Research spawn integration probe");objects.Add(spawnProbeGo);
                var spawnProbe=spawnProbeGo.AddComponent<ProceduralSpawner>();spawnProbe.enabled=false;
                var intervalMethod=typeof(ProceduralSpawner).GetMethod("EffectiveDailyNpcIntervalSec",BindingFlags.Instance|BindingFlags.NonPublic);
                foreach(var item in VillageResearch.Catalog) {
                    var ratesBefore=spawnProbe.EffectiveSpawnChances();float intervalBefore=(float)intervalMethod.Invoke(spawnProbe,null);
                    var go=new GameObject("Research station "+item.Id);objects.Add(go);go.transform.position=hud.Player.transform.position;
                    var station=go.AddComponent<Building>();station.Initialize(item.Station);
                    float before=item.Id==ResearchId.Combat?BuildingUpgradeRules.TrainingDamageMultiplier():item.Id==ResearchId.Lumber?BuildingUpgradeRules.SawmillWoodYieldMultiplier():item.Id==ResearchId.Crops?BuildingUpgradeRules.CropYieldMultiplier():item.Id==ResearchId.Vision?BuildingUpgradeRules.LookoutVisionRadius():item.Id==ResearchId.Morale?BuildingUpgradeRules.ChurchMoraleLossReductionChance():session.Resources.GetCap(ResourceKind.Food);
                    Assert.That(hud.TryResearch(item.Id),Is.True,item.Name);
                    Assert.That(session.Research.Level(item.Id),Is.EqualTo(1));
                    switch(item.Id) {
                        case ResearchId.Fence:Assert.That(fence.MaxHp,Is.EqualTo(120));break;
                        case ResearchId.Combat:Assert.That(BuildingUpgradeRules.TrainingDamageMultiplier(),Is.EqualTo(before+.1f).Within(.001f));break;
                        case ResearchId.Lumber:Assert.That(BuildingUpgradeRules.SawmillWoodYieldMultiplier(),Is.EqualTo(before+.15f).Within(.001f));break;
                        case ResearchId.Crops:Assert.That(BuildingUpgradeRules.CropYieldMultiplier(),Is.EqualTo(before+.15f).Within(.001f));break;
                        case ResearchId.Vision:Assert.That(BuildingUpgradeRules.LookoutVisionRadius(),Is.EqualTo(before+.5f).Within(.001f));break;
                        case ResearchId.Morale:Assert.That(BuildingUpgradeRules.ChurchMoraleLossReductionChance(),Is.EqualTo(before+.05f).Within(.001f));break;
                        case ResearchId.OreProspecting:Assert.That(LootRules.OreDropChance,Is.EqualTo(BalanceConfig.Instance.RockOreDropChance+.02f).Within(.001f));Assert.That(session.Resources.Get(ResourceKind.Ore),Is.EqualTo(95));break;
                        case ResearchId.Recruitment:Assert.That(spawnProbe.EffectiveSpawnChances().w,Is.EqualTo(ratesBefore.w*1.1f).Within(.0001f));Assert.That((float)intervalMethod.Invoke(spawnProbe,null),Is.EqualTo(intervalBefore/1.1f).Within(.001f));break;
                        case ResearchId.ResourceSurvey:Assert.That(spawnProbe.EffectiveSpawnChances().x,Is.EqualTo(ratesBefore.x*1.1f).Within(.0001f));Assert.That(spawnProbe.EffectiveSpawnChances().y,Is.EqualTo(ratesBefore.y*1.1f).Within(.0001f));break;
                        case ResearchId.FoodCapacity:Assert.That(session.Resources.GetCap(ResourceKind.Food),Is.EqualTo(before+40));break;
                    }
                    station.transform.position+=Vector3.right*10;
                    Assert.That(hud.TryResearch(item.Id),Is.False,"Research must require the correct nearby building");
                    station.transform.position=hud.Player.transform.position;
                    station.TakeDamage(station.MaxHp);
                    Assert.That(hud.TryResearch(item.Id),Is.False,"Destroyed building must not allow another purchase");
                }
                var data=JsonUtility.FromJson<SaveFileV1>(File.ReadAllText(path));
                Assert.That(data.researchLevels,Is.EqualTo(new[]{1,1,1,1,1,1,1,1,1,1}));
                var finalStationGo=new GameObject("Ten tier carpenter");objects.Add(finalStationGo);
                finalStationGo.transform.position=hud.Player.transform.position;
                var finalStation=finalStationGo.AddComponent<Building>();finalStation.Initialize(BuildingKind.Carpenter);
                session.Resources.Add(ResourceKind.Wood,400);session.Resources.Add(ResourceKind.Stone,400);
                for(int stage=2;stage<=10;stage++) {
                    Assert.That(hud.TryResearch(ResearchId.Fence),Is.True,"Fence stage "+stage);
                    Assert.That(fence.MaxHp,Is.EqualTo(100+20*stage));
                }
                int finalWood=session.Resources.Get(ResourceKind.Wood);
                Assert.That(hud.TryResearch(ResearchId.Fence),Is.False);
                Assert.That(session.Resources.Get(ResourceKind.Wood),Is.EqualTo(finalWood));
                var finalSave=JsonUtility.FromJson<SaveFileV1>(File.ReadAllText(path));
                Assert.That(finalSave.researchLevels[0],Is.EqualTo(10));
                hud.CloseResearchCatalog();Assert.That(Time.timeScale,Is.EqualTo(.5f));Assert.That(InputReader.HudBlocked,Is.False);
            } finally {
                hud.CloseResearchCatalog();session.Research.Restore(previous);session.Resources.Restore(resources);session.Resources.IncreaseCap(-1000);
                foreach(var go in objects) if(go!=null) Object.Destroy(go);
                if(saved==null) {if(File.Exists(path)) File.Delete(path);} else File.WriteAllBytes(path,saved);
                Time.timeScale=1;InputReader.HudBlocked=false;
            }
            yield return null;
        }
    }
}
