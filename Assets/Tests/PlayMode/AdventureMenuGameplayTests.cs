using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class AdventureMenuGameplayTests
    {
        private readonly string[] _keys={"IL6.UiScale","IL6.WorldZoom","il6_bgm_vol","il6_sfx_vol"};
        private readonly bool[] _had=new bool[4];private readonly float[] _values=new float[4];
        [SetUp] public void PreservePreferences(){for(int i=0;i<_keys.Length;i++){_had[i]=PlayerPrefs.HasKey(_keys[i]);_values[i]=PlayerPrefs.GetFloat(_keys[i]);}}
        [TearDown] public void RestorePreferences(){for(int i=0;i<_keys.Length;i++){if(_had[i])PlayerPrefs.SetFloat(_keys[i],_values[i]);else PlayerPrefs.DeleteKey(_keys[i]);}PlayerPrefs.Save();}
        [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;InputReader.HudBlocked=false;yield return SceneManager.LoadSceneAsync("BootScene");}
        [UnityTest] public IEnumerator MenuResearchReturnAndResumePreservePauseOwnership()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            typeof(SimpleHud).GetMethod("PrepareHudPreview",flags).Invoke(hud,null);
            var toggle=typeof(SimpleHud).GetMethod("ToggleMenu",flags);
            Time.timeScale=.75f;toggle.Invoke(hud,null);yield return null;
            Assert.That(Time.timeScale,Is.Zero);Assert.That(InputReader.HudBlocked,Is.True);
            Assert.That(hud.OpenResearchCatalog(),Is.True);Assert.That(Time.timeScale,Is.Zero);
            hud.CloseResearchCatalog();Assert.That(Time.timeScale,Is.Zero,"Closing research must return to the paused menu");
            toggle.Invoke(hud,null);yield return null;
            Assert.That(Time.timeScale,Is.EqualTo(.75f).Within(.001f));Assert.That(InputReader.HudBlocked,Is.False);
        }
    }
}
