using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class NavigationNoticeVisualTests
    {
        [UnityTest] public IEnumerator CaptureGroupedThreatsAndHomeDirectionsWithoutClipping()
        {
            if(Application.isBatchMode)Assert.Ignore("Requires interactive Game View");
#if UNITY_EDITOR
            var type=System.Type.GetType("UnityEditor.GameView,UnityEditor");var view=(UnityEditor.EditorWindow)ScriptableObject.CreateInstance(type);
            view.position=new Rect(40,40,1280,742);view.ShowUtility();type.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,0);
            float original=HudViewSettings.UiScale;bool hadUi=PlayerPrefs.HasKey("IL6.UiScale");
            try
            {
                Time.timeScale=1;yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
                yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSecondsRealtime(1);
                var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
                hud.Player.enabled=false;hud.Attacker.enabled=false;GameSession.Instance.enabled=false;
                foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None))spawner.enabled=false;
                hud.Player.transform.position=new Vector3(30,0);yield return new WaitForSecondsRealtime(1);
                var buildings=new Building[3];for(int i=0;i<3;i++)buildings[i]=BuildingFactory.SpawnHouse(new Vector3(110+i*4,0)).GetComponent<Building>();
                var actor=new GameObject("Threat companion");actor.transform.position=new Vector3(30,-80);var ally=actor.AddComponent<Companion>();ally.Player=hud.Player.transform;ally.IsCombat=false;ally.SetMaxHp(5000,true);actor.GetComponent<Rigidbody2D>().simulated=false;
                foreach(float scale in new[]{1f,.8f})foreach(int width in new[]{960,1280,1920})
                {
                    HudViewSettings.UiScale=scale;view.position=new Rect(40,40,width,width*9/16+22);view.Focus();Screen.SetResolution(width,width*9/16,false);
                    foreach(var building in buildings)building.TakeDamage(1);ally.TakeDamage(1);
                    yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();
                    Assert.That(ThreatNavigation.Collect(Camera.main,hud.Player.transform.position,buildings,new[]{ally},Time.time,false).Count,Is.EqualTo(3));
                    Assert.That(ThreatNavigation.Collect(Camera.main,hud.Player.transform.position,buildings,new[]{ally},Time.time,true).Count,Is.EqualTo(1));
                    var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"),$"inisland-navigation-{width}-{Mathf.RoundToInt(scale*100)}.png"),image.EncodeToPNG());Object.Destroy(image);
                }
                yield return new WaitForSecondsRealtime(4.2f);
                Assert.That(ThreatNavigation.Collect(Camera.main,hud.Player.transform.position,buildings,new[]{ally},Time.time,false).Visible,Is.False);
                yield return SceneManager.LoadSceneAsync("BootScene");
            }
            finally{HudViewSettings.UiScale=original;if(!hadUi)PlayerPrefs.DeleteKey("IL6.UiScale");PlayerPrefs.Save();view.Close();}
#endif
        }
    }
}
