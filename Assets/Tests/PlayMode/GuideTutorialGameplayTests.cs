using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class GuideTutorialGameplayTests
    {
        private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        private static void Finish(SimpleHud hud)
        {for(int i=0;i<12 && hud.CurrentStoryTutorial.IsDialogue;i++){Assert.That(hud.CurrentStoryTutorial.CurrentLine.Speaker,Is.EqualTo("Hero"));hud.AdvanceStoryDialogue();}}
        [UnityTest] public IEnumerator FreshStoryUsesHeroOnlyAndRealTasksResumeAfterEachMonologue()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;SaveLoad.SelectSlot(3);SaveLoad.Clear();
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            Object.FindFirstObjectByType<LobbyController>().RequestNew(3);yield return new WaitForSecondsRealtime(1);
            var intro=Object.FindFirstObjectByType<NewAdventureLoading>();Assert.That(intro,Is.Not.Null);Assert.That(intro.Ready,Is.True);
            for(int i=0;i<4;i++)intro.Advance();yield return new WaitForSecondsRealtime(1);
            var session=GameSession.Instance;var hud=Object.FindFirstObjectByType<SimpleHud>();hud.Player.enabled=false;hud.Attacker.enabled=false;
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None))spawner.enabled=false;
            Assert.That(session.StoryIntroSeen,Is.True);Assert.That(hud.GuideConversationOpen,Is.True);Assert.That(Time.timeScale,Is.Zero);
            Assert.That(GameObject.Find("Story guide - traveller"),Is.Null);Assert.That(hud.NpcStoryWaiting,Is.False);
            typeof(SimpleHud).GetField("_guideOpenedFrame",Flags).SetValue(hud,Time.frameCount);
            hud.HandleGuideConversationKey(new Event{type=EventType.KeyDown,keyCode=KeyCode.Space});Assert.That(hud.CurrentStoryTutorial.LineIndex,Is.Zero);
            hud.HandleGuideConversationKey(new Event{type=EventType.KeyUp,keyCode=KeyCode.Space});yield return null;
            hud.HandleGuideConversationKey(new Event{type=EventType.KeyDown,keyCode=KeyCode.Space});Assert.That(hud.CurrentStoryTutorial.LineIndex,Is.EqualTo(1));
            Finish(hud);Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.Explore));Assert.That(Time.timeScale,Is.EqualTo(1));
            hud.Player.Actions.SetMoving(true);Assert.That(DayActionTime.Delta(Phase.Day,hud.Player,1),Is.Zero);hud.Player.Actions.SetMoving(false);
            hud.Player.transform.position+=Vector3.right*3;yield return null;
            Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.WoodMemory));Assert.That(hud.GuideConversationOpen,Is.True);Assert.That(Time.timeScale,Is.Zero);
            Finish(hud);Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.GatherWood));
            var wood=WoodPickup.Spawn(hud.Player.transform.position,1,session.Resources);typeof(WoodPickup).GetField("_age",Flags).SetValue(wood,1f);
            Assert.That(wood.TryCollect(hud.Player),Is.True);yield return new WaitForSecondsRealtime(.8f);
            Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.BuildMemory));Assert.That(hud.GuideConversationOpen,Is.True);
            session.ReturnToLobby();yield return new WaitForSecondsRealtime(.5f);
            Assert.That(Object.FindFirstObjectByType<LobbyController>().TryContinue(3),Is.True);yield return new WaitForSecondsRealtime(1);
            Assert.That(Object.FindFirstObjectByType<NewAdventureLoading>(),Is.Null);
            hud=Object.FindFirstObjectByType<SimpleHud>();session=GameSession.Instance;hud.Player.enabled=false;hud.Attacker.enabled=false;
            Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.BuildMemory));Assert.That(hud.GuideConversationOpen,Is.True);
            Finish(hud);var fence=VillageStarter.SpawnFence(hud.Player.transform.position+Vector3.right*2,0,FencePieceType.Center);
            yield return null;Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.Resolve));Finish(hud);yield return null;
            Assert.That(session.StoryTutorialComplete,Is.True);Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(InputReader.HudBlocked,Is.False);
            hud.Player.Actions.SetMoving(true);Assert.That(DayActionTime.Delta(Phase.Day,hud.Player,1),Is.EqualTo(1));hud.Player.Actions.SetMoving(false);
            Assert.That(GameObject.Find("Story guide - traveller"),Is.Null);Object.Destroy(fence);
            session.HardReset();yield return new WaitForSecondsRealtime(1);intro=Object.FindFirstObjectByType<NewAdventureLoading>();Assert.That(intro,Is.Not.Null);
            for(int i=0;i<4;i++)intro.Advance();yield return new WaitForSecondsRealtime(1);
            Assert.That(Object.FindFirstObjectByType<SimpleHud>().CurrentStoryTutorial.CurrentLine.Speaker,Is.EqualTo("Hero"));
            SaveLoad.SelectSlot(3);SaveLoad.Clear();SaveLoad.SelectSlot(1);yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
