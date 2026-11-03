using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public sealed class SunsetWarningGameplayTests
    {
        [UnityTest]
        public IEnumerator ActualHudWarnsBeforeTransitionWithoutChangingTimeOrBlockingPlay()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();var flags=BindingFlags.NonPublic|BindingFlags.Instance;
            typeof(SimpleHud).GetMethod("PrepareHudPreview",flags).Invoke(hud,null);
            var cycle=GameSession.Instance.Cycle;var saved=cycle.Snapshot();
            var update=typeof(SimpleHud).GetMethod("UpdateSunsetWarning",flags);
            var state=(SunsetWarningState)typeof(SimpleHud).GetField("_sunsetWarning",flags).GetValue(hud);
            try
            {
                cycle.Restore(new DayNightSnapshot{day=50,phase=Phase.Day,elapsedInPhase=cycle.PhaseDurationSec-15});
                Time.timeScale=0;update.Invoke(hud,null);Assert.That(state.AlertCount,Is.Zero);
                Time.timeScale=1;update.Invoke(hud,null);Assert.That(state.AlertCount,Is.EqualTo(1));
                Assert.That((bool)typeof(SimpleHud).GetProperty("SunsetWarningVisible",flags).GetValue(hud),Is.True);
                float before=cycle.ElapsedInPhase;update.Invoke(hud,null);Assert.That(cycle.ElapsedInPhase,Is.EqualTo(before));
                Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(InputReader.HudBlocked,Is.False);
                var rect=(Rect)typeof(SimpleHud).GetProperty("SunsetWarningRect",flags).GetValue(hud);
                Assert.That(rect.Overlaps(MobileHudLayout.Clock),Is.False);Assert.That(rect.Overlaps(MobileHudLayout.Status),Is.False);Assert.That(rect.Overlaps(MobileHudLayout.Goals),Is.False);
                Assert.That((bool)typeof(SimpleHud).GetMethod("IsHudPoint",flags).Invoke(hud,new object[]{rect.center}),Is.True);
                cycle.Restore(new DayNightSnapshot{day=50,phase=Phase.Day,elapsedInPhase=cycle.PhaseDurationSec-5});
                update.Invoke(hud,null);Assert.That(state.Stage,Is.EqualTo(SunsetWarningStage.Imminent));Assert.That(state.AlertCount,Is.EqualTo(2));
                cycle.Restore(new DayNightSnapshot{day=50,phase=Phase.Night});update.Invoke(hud,null);Assert.That(state.Visible,Is.False);
            }
            finally {cycle.Restore(saved);Time.timeScale=1;InputReader.HudBlocked=false;}
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
