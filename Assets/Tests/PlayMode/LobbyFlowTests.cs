using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class LobbyFlowTests
    {
        [UnityTest] public IEnumerator BootWaitsForChoiceAndContinuesOnlySelectedSlot()
        {
            Assert.That(System.Environment.GetEnvironmentVariable("IL6_TEST_SAVE_DIRECTORY"),Is.Not.Null.And.Not.Empty);
            SaveLoad.SelectSlot(3);SaveLoad.Clear();
            SaveLoad.SelectSlot(1);SaveLoad.Save(new SaveFileV1{currentDay=4,storyTutorialComplete=true});
            SaveLoad.SelectSlot(2);SaveLoad.Save(new SaveFileV1{currentDay=8,storyTutorialComplete=true});
            SaveLoad.SelectSlot(1);BootController.SkipLobbyOnce=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return new WaitForSecondsRealtime(.3f);
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("BootScene"));
            Assert.That(GameSession.Instance.enabled,Is.False);
            var lobby=Object.FindFirstObjectByType<LobbyController>();Assert.That(lobby,Is.Not.Null);
            Assert.That(lobby.TryContinue(3),Is.False);Assert.That(SaveLoad.ActiveSlot,Is.EqualTo(1));
            Assert.That(lobby.TryContinue(2),Is.True);yield return new WaitForSecondsRealtime(1);
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("SnowfieldScene"));
            Assert.That(GameSession.Instance.Cycle.Day,Is.EqualTo(8));Assert.That(SaveLoad.ActiveSlot,Is.EqualTo(2));
            Assert.That(SaveLoad.LoadSlot(1).currentDay,Is.EqualTo(4));
            GameSession.Instance.ReturnToLobby();yield return new WaitForSecondsRealtime(1);
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("BootScene"));
            Assert.That(SaveLoad.LoadSlot(2).currentDay,Is.EqualTo(8));
            SaveLoad.SelectSlot(1);
        }
        [UnityTest] public IEnumerator OverwriteRequiresConfirmationAndTouchesOnlyChosenSlot()
        {
            SaveLoad.SelectSlot(1);SaveLoad.Save(new SaveFileV1{currentDay=9,storyTutorialComplete=true});
            SaveLoad.SelectSlot(2);SaveLoad.Save(new SaveFileV1{currentDay=6,storyTutorialComplete=true});SaveLoad.SelectSlot(1);
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            var lobby=Object.FindFirstObjectByType<LobbyController>();Assert.That(lobby,Is.Not.Null);
            byte[] before=File.ReadAllBytes(SaveLoad.SlotPath(1));
            lobby.RequestNew(1);Assert.That(lobby.PendingOverwriteSlot,Is.EqualTo(1));
            CollectionAssert.AreEqual(before,File.ReadAllBytes(SaveLoad.SlotPath(1)));
            lobby.CancelOverwrite();Assert.That(lobby.PendingOverwriteSlot,Is.Zero);
            CollectionAssert.AreEqual(before,File.ReadAllBytes(SaveLoad.SlotPath(1)));
            lobby.RequestNew(1);lobby.ConfirmOverwrite();yield return new WaitForSecondsRealtime(1);
            var loading=Object.FindFirstObjectByType<NewAdventureLoading>();Assert.That(loading,Is.Not.Null);
            for(int i=0;i<4;i++)loading.Advance();yield return new WaitForSecondsRealtime(1);
            Assert.That(GameSession.Instance.Cycle.Day,Is.EqualTo(1));Assert.That(GameSession.Instance.StoryTutorialEnabled,Is.True);
            Assert.That(SaveLoad.LoadSlot(2).currentDay,Is.EqualTo(6));
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("SnowfieldScene"));
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
