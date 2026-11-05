using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class HudReferenceVisualTests
    {
        [UnityTest]
        public IEnumerator CaptureActualHudWithResourceStripAndCollapsedQuest()
        {
            if(Application.isBatchMode)Assert.Ignore("HUD screenshots require the interactive Game View render loop.");
#if UNITY_EDITOR
            var viewType=System.Type.GetType("UnityEditor.GameView,UnityEditor");
            var view=(UnityEditor.EditorWindow)ScriptableObject.CreateInstance(viewType);
            view.position=new Rect(40,40,1280,742);view.ShowUtility();
            viewType.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,0);
            view.Focus();
#endif
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            hud.Player.enabled=false;hud.Attacker.enabled=false;
            foreach(int width in new[]{1280,1920})
            {
#if UNITY_EDITOR
                view.position=new Rect(40,40,width,width*9/16+22);view.Focus();
#endif
                Screen.SetResolution(width,width*9/16,false);
                yield return new WaitForSeconds(1);yield return new WaitForEndOfFrame();
                var capture=ScreenCapture.CaptureScreenshotAsTexture();
                Assert.That(capture,Is.Not.Null);
                var path=Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"),"inisland-hud-reference-"+width+".png");
                File.WriteAllBytes(path,capture.EncodeToPNG());
                Debug.Log("HUD visual capture "+capture.width+"x"+capture.height+": "+path);
                Object.Destroy(capture);
            }
            if(System.Environment.GetEnvironmentVariable("IL6_RESOURCE_VISUAL")=="1")
            {
                var field=typeof(SimpleHud).GetField("_resourceDetailsOpen",BindingFlags.Instance|BindingFlags.NonPublic);
                field.SetValue(hud,true);
                yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();
                Assert.That((bool)field.GetValue(hud),Is.True);
                var capture=ScreenCapture.CaptureScreenshotAsTexture();Assert.That(capture,Is.Not.Null);
                File.WriteAllBytes(Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"),"inisland-resource-details.png"),capture.EncodeToPNG());
                Object.Destroy(capture);field.SetValue(hud,false);
            }
            if(System.Environment.GetEnvironmentVariable("IL6_QUEST_VISUAL")=="1")
            {
                var flags=BindingFlags.Instance|BindingFlags.NonPublic;
                foreach(bool story in new[]{false,true})
                {
                    typeof(SimpleHud).GetField("_goalsCollapsed",flags).SetValue(hud,false);
                    typeof(SimpleHud).GetField("_tutorialDismissed",flags).SetValue(hud,!story);
                    if(story)
                    {
                        typeof(SimpleHud).GetField("_storyTutorial",flags).SetValue(hud,new StoryTutorial((int)StoryTutorialStage.Explore));
                        typeof(SimpleHud).GetMethod("CaptureStoryBaseline",flags).Invoke(hud,new object[]{true});
                    }
                    yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();
                    if(story)Assert.That(hud.CurrentStoryTutorial.Stage,Is.EqualTo(StoryTutorialStage.Explore));
                    var capture=ScreenCapture.CaptureScreenshotAsTexture();Assert.That(capture,Is.Not.Null);
                    File.WriteAllBytes(Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"),story?"inisland-quest-story.png":"inisland-quest-expanded.png"),capture.EncodeToPNG());
                    Object.Destroy(capture);
                }
            }
#if UNITY_EDITOR
            if(System.Environment.GetEnvironmentVariable("IL6_MENU_VISUAL")=="1")
            {
                bool hadUi=PlayerPrefs.HasKey("IL6.UiScale"),hadZoom=PlayerPrefs.HasKey("IL6.WorldZoom");
                float originalUi=HudViewSettings.UiScale,originalZoom=HudViewSettings.WorldZoom;
                bool hadMusic=PlayerPrefs.HasKey("il6_bgm_vol"),hadEffects=PlayerPrefs.HasKey("il6_sfx_vol");
                float originalMusic=Music.Volume,originalEffects=Sfx.Volume;
                var toggle=typeof(SimpleHud).GetMethod("ToggleMenu",BindingFlags.Instance|BindingFlags.NonPublic);
                toggle.Invoke(hud,null);
                try
                {
                    foreach(float density in new[]{1f,.8f})foreach(int width in new[]{1280,1920})
                    {
                        HudViewSettings.UiScale=density;
                        view.position=new Rect(40,40,width,width*9/16+22);view.Focus();Screen.SetResolution(width,width*9/16,false);
                        yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();
                        Assert.That(Time.timeScale,Is.Zero);
                        var capture=ScreenCapture.CaptureScreenshotAsTexture();Assert.That(capture,Is.Not.Null);
                        File.WriteAllBytes(Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"),"inisland-menu-"+width+"-"+Mathf.RoundToInt(density*100)+".png"),capture.EncodeToPNG());
                        Object.Destroy(capture);
                    }
                }
                finally
                {
                    toggle.Invoke(hud,null);
                    HudViewSettings.UiScale=originalUi;HudViewSettings.WorldZoom=originalZoom;
                    if(!hadUi)PlayerPrefs.DeleteKey("IL6.UiScale");if(!hadZoom)PlayerPrefs.DeleteKey("IL6.WorldZoom");
                    Music.Volume=originalMusic;Sfx.Volume=originalEffects;
                    if(!hadMusic)PlayerPrefs.DeleteKey("il6_bgm_vol");if(!hadEffects)PlayerPrefs.DeleteKey("il6_sfx_vol");PlayerPrefs.Save();
                }
            }
            view.Close();
#endif
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
