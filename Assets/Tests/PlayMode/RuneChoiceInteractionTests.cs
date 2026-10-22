using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class RuneChoiceInteractionTests
    {
        [UnityTest] public IEnumerator DetailsDoNotApplyRuneAndCardAppliesExactlyOnce()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            typeof(SimpleHud).GetMethod("PrepareHudPreview",flags).Invoke(hud,null);
            var progression=hud.Progression;progression.GrantXp(progression.XpToNext);
            typeof(SimpleHud).GetField("_runeOffer",flags).SetValue(hud,new List<RuneKind>{RuneKind.PoisonBlade,RuneKind.IceArrow,RuneKind.Thorns});
            Time.timeScale=0;int before=progression.GetStacks(RuneKind.PoisonBlade);
            Assert.That((bool)typeof(SimpleHud).GetMethod("ShowRuneDetails",flags).Invoke(hud,new object[]{0}),Is.True);
            Assert.That(progression.GetStacks(RuneKind.PoisonBlade),Is.EqualTo(before));Assert.That(progression.LevelUpPending,Is.True);
            Assert.That(Time.timeScale,Is.Zero);
            var choose=typeof(SimpleHud).GetMethod("TryChooseRune",flags);
            Assert.That((bool)choose.Invoke(hud,new object[]{99}),Is.False);
            Assert.That((bool)choose.Invoke(hud,new object[]{0}),Is.True);
            Assert.That(progression.GetStacks(RuneKind.PoisonBlade),Is.EqualTo(before+1));Assert.That(progression.LevelUpPending,Is.False);
            Assert.That((bool)choose.Invoke(hud,new object[]{0}),Is.False);
            typeof(SimpleHud).GetMethod("DrawRuneModal",flags).Invoke(hud,null);Assert.That(Time.timeScale,Is.EqualTo(1));
            InputReader.HudBlocked=false;yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
