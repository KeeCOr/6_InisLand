using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using IL6.Events;
namespace IL6.Tests
{
    public sealed class NewAdventureParityTests
    {
        private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        private static int Count(BuildingKind kind){int count=0;foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))if(b.Kind==kind && b.CurrentHp>0)count++;return count;}
        private static IEnumerator WaitForPlayableStory(SimpleHud previous=null)
        {
            float deadline=Time.realtimeSinceStartup+8;
            while(Time.realtimeSinceStartup<deadline)
            {
                var hud=Object.FindFirstObjectByType<SimpleHud>();
                if(hud!=null && hud!=previous && hud.CurrentStoryTutorial!=null && GameSession.Instance!=null
                    && GameSession.Instance.enabled && SceneManager.GetActiveScene().name=="SnowfieldScene")yield break;
                yield return null;
            }
            Assert.Fail("Story did not become ready in scene "+SceneManager.GetActiveScene().name);
        }
        [UnityEngine.TestTools.UnityTest] public IEnumerator DeathNewAdventureMatchesFreshStartAndRetainsStarterKitAfterStory()
        {
            if(string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("IL6_TEST_SAVE_DIRECTORY")))Assert.Ignore("Requires isolated save");
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return new WaitForSecondsRealtime(.5f);
            GameSession.Instance.NpcTutorialEnabled=false; // Legacy save's restart path remains compatible.
            GameSession.Instance.HardReset();yield return WaitForPlayableStory();
            int initialFences=0;int[] startingResources=null;
            for(int attempt=0;attempt<2;attempt++)
            {
                var session=GameSession.Instance;var hud=Object.FindFirstObjectByType<SimpleHud>();
                Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.Prologue));
                Assert.That(session.Cycle.Day,Is.EqualTo(1));Assert.That(session.Cycle.Phase,Is.EqualTo(Phase.Day));Assert.That(session.TotalKills,Is.Zero);
                Assert.That(SettlementGoalManager.Instance,Is.Not.Null);Assert.That(SettlementGoalManager.Instance.gameObject,Is.SameAs(session.gameObject));
                Assert.That(AchievementManager.Instance,Is.Not.Null);Assert.That(AchievementManager.Instance.gameObject,Is.SameAs(session.gameObject));
                Assert.That(Count(BuildingKind.Campfire),Is.EqualTo(1));Assert.That(Count(BuildingKind.House),Is.EqualTo(1));
                foreach(var building in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                    if(building.Kind==BuildingKind.House)Assert.That(building.transform.position,
                        Is.EqualTo(ConstructionPlacement.Snap(new Vector3(GameConstants.VillageCenterX,GameConstants.VillageCenterY,0))),"Fresh and death-restart houses must both use the north-facing layout");
                Assert.That(Count(BuildingKind.Fence),Is.GreaterThan(0));Assert.That(Object.FindObjectsByType<Door>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
                var kinds=(ResourceKind[])System.Enum.GetValues(typeof(ResourceKind));var resources=new int[kinds.Length];for(int i=0;i<kinds.Length;i++)resources[i]=session.Resources.Get(kinds[i]);
                if(attempt==0){initialFences=Count(BuildingKind.Fence);startingResources=resources;}else{Assert.That(Count(BuildingKind.Fence),Is.EqualTo(initialFences));CollectionAssert.AreEqual(startingResources,resources);}
                for(int i=0;i<20 && hud.CurrentStoryTutorial.IsDialogue;i++)hud.AdvanceStoryDialogue();
                yield return null;
                Assert.That(Count(BuildingKind.Campfire),Is.EqualTo(1));Assert.That(Count(BuildingKind.House),Is.EqualTo(1),"Story must preserve guaranteed starting home");
                Assert.That(Count(BuildingKind.Fence),Is.EqualTo(initialFences));
                var ruins=GameObject.Find("StoryVillageRuins");
                foreach(var building in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))if(building.Kind==BuildingKind.House)
                    foreach(Transform ruin in ruins.transform)Assert.That(Vector2.Distance(building.transform.position,ruin.position),Is.GreaterThanOrEqualTo(1.2f),"Ruins must not cover the guaranteed home");
                if(attempt==0)
                {
                    session.Resources.Add(ResourceKind.Wood,100);session.OnZombieKilled();
                    hud.Player.TakeDamage(hud.Player.MaxHp+100);
                    typeof(SimpleHud).GetMethod("UpdateDeathScreenState",Flags).Invoke(hud,null);
                    typeof(SimpleHud).GetField("_deathButtonReadyAt",Flags).SetValue(hud,0f);
                    Assert.That((bool)typeof(SimpleHud).GetMethod("RestartAfterDeath",Flags).Invoke(hud,null),Is.True);
                    yield return WaitForPlayableStory(hud);
                    Assert.That(GameSession.Instance,Is.Not.SameAs(session));
                }
            }
            Time.timeScale=1;InputReader.HudBlocked=false;yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
