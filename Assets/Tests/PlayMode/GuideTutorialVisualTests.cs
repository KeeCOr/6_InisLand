using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class GuideTutorialVisualTests
    {
        [UnityTest] public IEnumerator CaptureLoadingStoryAndHeroMonologueInGameView()
        {
            if(Application.isBatchMode)Assert.Ignore("Requires interactive Game View");
#if UNITY_EDITOR
            var type=System.Type.GetType("UnityEditor.GameView,UnityEditor");var view=(UnityEditor.EditorWindow)ScriptableObject.CreateInstance(type);
            view.position=new Rect(40,40,1280,742);view.ShowUtility();
            type.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,0);
            float original=HudViewSettings.UiScale;bool hadUi=PlayerPrefs.HasKey("IL6.UiScale");
            try
            {
                HudViewSettings.UiScale=1;SaveLoad.SelectSlot(3);SaveLoad.Clear();BootController.SkipLobbyOnce=false;
                yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
                Object.FindFirstObjectByType<LobbyController>().RequestNew(3);yield return new WaitForSecondsRealtime(1);
                var intro=Object.FindFirstObjectByType<NewAdventureLoading>();Assert.That(intro,Is.Not.Null);
                for(int i=0;i<4;i++)
                {
                    yield return new WaitForSecondsRealtime(.15f);yield return new WaitForEndOfFrame();Capture("inisland-loading-story-"+i);
                    if(i<3)intro.Advance();
                }
                Assert.That(intro.Ready,Is.True);intro.Advance();yield return new WaitForSecondsRealtime(1);
                var hud=Object.FindFirstObjectByType<SimpleHud>();hud.Player.enabled=false;hud.Attacker.enabled=false;
                Assert.That(GameObject.Find("Story guide - traveller"),Is.Null);
                Assert.That(hud.GuideConversationOpen,Is.True);
                foreach(int width in new[]{960,1920})
                {
                    view.position=new Rect(40,40,width,width*9/16+22);view.Focus();Screen.SetResolution(width,width*9/16,false);
                    yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();Capture("inisland-monologue-"+width);
                }
                hud.AdvanceStoryDialogue();yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();Capture("inisland-guide-hero");
                for(int i=0;i<10 && hud.CurrentStoryTutorial.IsDialogue;i++)hud.AdvanceStoryDialogue();
                Assert.That(Time.timeScale,Is.EqualTo(1));yield return new WaitForEndOfFrame();Capture("inisland-guide-waiting");
                yield return SceneManager.LoadSceneAsync("BootScene");
            }
            finally
            {
                HudViewSettings.UiScale=original;if(!hadUi)PlayerPrefs.DeleteKey("IL6.UiScale");PlayerPrefs.Save();view.Close();SaveLoad.SelectSlot(1);
            }
#endif
        }
        private static void Capture(string name)
        {
            var image=ScreenCapture.CaptureScreenshotAsTexture();Assert.That(image,Is.Not.Null);
            File.WriteAllBytes(Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"),name+".png"),image.EncodeToPNG());Object.Destroy(image);
        }
    }
}
