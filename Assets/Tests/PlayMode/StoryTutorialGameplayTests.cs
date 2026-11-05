using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class StoryTutorialGameplayTests
    {
        [UnityTest] public IEnumerator NewGameRecreatesSessionAndStartsTheDaughtersStory()
        {
            if(string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("IL6_TEST_SAVE_DIRECTORY")))
                Assert.Ignore("Hard reset validation requires an isolated test save directory");
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return new WaitForSecondsRealtime(1);
            var previous=GameSession.Instance;Assert.That(previous,Is.Not.Null);
            previous.HardReset();yield return new WaitForSecondsRealtime(1);
            Assert.That(GameSession.Instance,Is.Not.Null);Assert.That(GameSession.Instance,Is.Not.SameAs(previous));
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("SnowfieldScene"));
            Assert.That(GameSession.Instance.StoryTutorialEnabled,Is.True);Assert.That(GameSession.Instance.StoryVillageRuined,Is.False);
            var hud=Object.FindFirstObjectByType<SimpleHud>();Assert.That(hud.CurrentStoryTutorial,Is.Not.Null);
            Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.Prologue));
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            Time.timeScale=1;InputReader.HudBlocked=false;
        }
        private static void FinishDialogue(SimpleHud hud) {for(int i=0;i<20 && hud.CurrentStoryTutorial.IsDialogue;i++) hud.AdvanceStoryDialogue();}
        [UnityTest] public IEnumerator DialoguePausesButMovementPickupAndConstructionArePlayable()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSecondsRealtime(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None)) spawner.enabled=false;
            Time.timeScale=1;var player=hud.Player;player.enabled=false;hud.Attacker.enabled=false;
            hud.BeginStoryTutorial(true);Assert.That(Time.timeScale,Is.Zero);Assert.That(InputReader.HudBlocked,Is.True);
            FinishDialogue(hud);Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(InputReader.HudBlocked,Is.False);
            player.transform.position+=Vector3.right*3;
            yield return null;Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.WoodMemory));Assert.That(Time.timeScale,Is.Zero);
            FinishDialogue(hud);Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.GatherWood));
            var store=GameSession.Instance.Resources;store.IncreaseCap(ResourceKind.Wood,10);
            var wood=WoodPickup.Spawn(player.transform.position,1,store);
            typeof(WoodPickup).GetField("_age",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(wood,1f);
            Assert.That(wood.TryCollect(player),Is.True);yield return new WaitForSecondsRealtime(.8f);
            Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.BuildMemory));
            FinishDialogue(hud);Assert.That(Time.timeScale,Is.EqualTo(1));
            var fence=VillageStarter.SpawnFence(player.transform.position+Vector3.right*2,0,FencePieceType.Center);
            yield return null;Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.Resolve));
            FinishDialogue(hud);Assert.That(hud.CurrentStoryTutorial.IsComplete,Is.True);Assert.That(Time.timeScale,Is.EqualTo(1));
            Assert.That(InputReader.HudBlocked,Is.False);Object.Destroy(fence);
            hud.BeginStoryTutorial(true);hud.enabled=false;Assert.That(Time.timeScale,Is.EqualTo(1),"Disabling dialogue releases its pause");
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
        [UnityTest] public IEnumerator StormKeepsStartingFenceAndGateWhileRuiningInteriorOnlyOnce()
        {
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSecondsRealtime(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            Time.timeScale=1;
            var fences=new System.Collections.Generic.List<Building>();
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))if(b.Kind==BuildingKind.Fence)fences.Add(b);
            Assert.That(fences.Count,Is.GreaterThan(0),"A new village must have a starting perimeter");
            var gate=Object.FindFirstObjectByType<Door>();Assert.That(gate,Is.Not.Null);
            StoryVillageRuins.Apply();yield return null;
            foreach(var fence in fences)Assert.That(fence!=null && fence.gameObject.activeInHierarchy && fence.CurrentHp==fence.MaxHp,Is.True,"Storm must preserve the initial fence");
            Assert.That(gate!=null && gate.gameObject.activeInHierarchy,Is.True,"Initial automatic entrance must remain");
            var center=new Vector2(GameConstants.VillageCenterX,GameConstants.VillageCenterY);
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                if(Vector2.Distance(b.transform.position,center)<=8) Assert.That(b.Kind==BuildingKind.Campfire || b.Kind==BuildingKind.Fence || b.Kind==BuildingKind.House,Is.True);
            var fresh=VillageStarter.SpawnFence((Vector3)center+Vector3.right,0,FencePieceType.Center);
            StoryVillageRuins.Apply();yield return null;Assert.That(fresh!=null && fresh.activeSelf,Is.True,"Re-entry cannot erase a newly restored fence");
            Object.Destroy(fresh);yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
