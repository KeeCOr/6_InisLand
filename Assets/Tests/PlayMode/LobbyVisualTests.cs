using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class LobbyVisualTests
    {
        [UnityTest] public IEnumerator CaptureLobbySlotsAndOverwriteConfirmation()
        {
            if(Application.isBatchMode)Assert.Ignore("Requires interactive Game View");
#if UNITY_EDITOR
            var type=System.Type.GetType("UnityEditor.GameView,UnityEditor");
            var view=(UnityEditor.EditorWindow)ScriptableObject.CreateInstance(type);
            view.position=new Rect(40,40,1280,742);view.ShowUtility();
            type.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,0);
            float original=HudViewSettings.UiScale;bool hadUi=PlayerPrefs.HasKey("IL6.UiScale");
            try
            {
                SaveLoad.SelectSlot(1);SaveLoad.Save(new SaveFileV1{currentDay=9,storyTutorialComplete=true});
                SaveLoad.SelectSlot(2);SaveLoad.Clear();
                SaveLoad.SelectSlot(3);SaveLoad.Save(new SaveFileV1{currentDay=3,storyTutorialComplete=true});SaveLoad.SelectSlot(1);
                BootController.SkipLobbyOnce=false;yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
                var lobby=Object.FindFirstObjectByType<LobbyController>();Assert.That(lobby,Is.Not.Null);
                foreach(float scale in new[]{1f,.8f})foreach(int width in new[]{960,1280,1920})
                {
                    HudViewSettings.UiScale=scale;view.position=new Rect(40,40,width,width*9/16+22);view.Focus();Screen.SetResolution(width,width*9/16,false);
                    yield return new WaitForSecondsRealtime(.3f);yield return new WaitForEndOfFrame();
                    var image=ScreenCapture.CaptureScreenshotAsTexture();Assert.That(image,Is.Not.Null);
                    File.WriteAllBytes(Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"),$"inisland-lobby-{width}-{Mathf.RoundToInt(scale*100)}.png"),image.EncodeToPNG());Object.Destroy(image);
                }
                lobby.RequestNew(1);yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();
                Assert.That(lobby.PendingOverwriteSlot,Is.EqualTo(1));
                var modal=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"),"inisland-lobby-confirm.png"),modal.EncodeToPNG());Object.Destroy(modal);lobby.CancelOverwrite();
            }
            finally
            {
                HudViewSettings.UiScale=original;if(!hadUi)PlayerPrefs.DeleteKey("IL6.UiScale");PlayerPrefs.Save();view.Close();
            }
#endif
        }
    }
}
