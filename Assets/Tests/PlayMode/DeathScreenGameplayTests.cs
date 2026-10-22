using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class DeathScreenGameplayTests
    {
        private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        [UnityTest] public IEnumerator DeathOverridesOtherModalsAndBlocksWorldInput()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",Flags).Invoke(hud,null);
            foreach(string field in new[]{"_researchOpen","_buildCatalogOpen","_buildPaused","_paused","_tutorialPaused","_recruitCutPaused","_resourceDetailsOpen","_showCompanions"})
                typeof(SimpleHud).GetField(field,Flags).SetValue(hud,true);
            typeof(SimpleHud).GetField("_pendingBuildKind",Flags).SetValue(hud,(BuildingKind?)BuildingKind.House);
            hud.Player.TakeDamage(hud.Player.MaxHp+100);
            var sync=typeof(SimpleHud).GetMethod("UpdateDeathScreenState",Flags);
            try {
                Assert.That(sync,Is.Not.Null,"Death must take priority before all other modal rendering");
                Assert.That((bool)sync.Invoke(hud,null),Is.True);
                Assert.That(Time.timeScale,Is.Zero);Assert.That(InputReader.HudBlocked,Is.True);
                foreach(string field in new[]{"_researchOpen","_buildCatalogOpen","_buildPaused","_paused","_tutorialPaused","_recruitCutPaused","_resourceDetailsOpen","_showCompanions"})
                    Assert.That((bool)typeof(SimpleHud).GetField(field,Flags).GetValue(hud),Is.False,field);
                Assert.That(typeof(SimpleHud).GetField("_pendingBuildKind",Flags).GetValue(hud),Is.Null);
                Assert.That(hud.Attacker.TryManualAction(),Is.False);
                Assert.That((bool)typeof(SimpleHud).GetMethod("IsHudPoint",Flags).Invoke(hud,new object[]{Vector2.zero}),Is.True);
                var summary=typeof(SimpleHud).GetField("_deathSummary",Flags).GetValue(hud);
                sync.Invoke(hud,null);
                Assert.That(typeof(SimpleHud).GetField("_deathSummary",Flags).GetValue(hud),Is.SameAs(summary),"Results must be captured once");
            } finally { Time.timeScale=1;InputReader.HudBlocked=false; }
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
