using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class ExperienceHudGameplayTests
    {
        [UnityTest] public IEnumerator ExperienceDisplayTracksGainsAndLevelRolloverAndConsumesHudInput()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            typeof(SimpleHud).GetMethod("PrepareHudPreview",flags).Invoke(hud,null);
            var ratio=typeof(SimpleHud).GetProperty("ExperienceProgress",flags);var label=typeof(SimpleHud).GetProperty("ExperienceLabel",flags);
            Assert.That(ratio,Is.Not.Null);Assert.That(label,Is.Not.Null);
            var progression=hud.Progression;Assert.That((float)ratio.GetValue(hud),Is.Zero);
            progression.GrantXp(6);Assert.That((float)ratio.GetValue(hud),Is.EqualTo(.5f).Within(.001f));Assert.That((string)label.GetValue(hud),Is.EqualTo("EXP 6 / 12"));
            var property=typeof(MobileHudLayout).GetProperty("Experience");var xp=(Rect)property.GetValue(null);
            Assert.That((bool)typeof(SimpleHud).GetMethod("IsHudPoint",flags).Invoke(hud,new object[]{xp.center}),Is.True);
            var details=(Rect)typeof(SimpleHud).GetProperty("ResourceDetailsRect",flags).GetValue(hud);Assert.That(details.yMin,Is.GreaterThan(xp.yMax));
            Assert.That(details.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(details.xMax,Is.LessThanOrEqualTo(MobileHudLayout.Width-16));
            progression.GrantXp(8);Assert.That(progression.Level,Is.EqualTo(2));Assert.That((float)ratio.GetValue(hud),Is.EqualTo(2f/16).Within(.001f));Assert.That((string)label.GetValue(hud),Is.EqualTo("EXP 2 / 16"));
            Time.timeScale=1;InputReader.HudBlocked=false;yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
