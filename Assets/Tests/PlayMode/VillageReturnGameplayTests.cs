using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public sealed class VillageReturnGameplayTests
    {
        [UnityTest]
        public IEnumerator ReturnShowsOnceTeleportsUnderCoverAndReleasesControlsAtNight()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();var flags=BindingFlags.NonPublic|BindingFlags.Instance;
            typeof(SimpleHud).GetMethod("PrepareHudPreview",flags).Invoke(hud,null);
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None))spawner.enabled=false;
            var player=hud.Player;player.enabled=false;player.transform.position=new Vector3(4500,4500,0);
            var cycle=GameSession.Instance.Cycle;var saved=cycle.Snapshot();
            var update=typeof(SimpleHud).GetMethod("UpdateVillageReturn",flags);
            Vector3 village=new Vector3(GameConstants.VillageCenterX,GameConstants.VillageCenterY,0);
            try
            {
                cycle.Restore(new DayNightSnapshot{day=70,phase=Phase.Evening,elapsedInPhase=.2f});
                update.Invoke(hud,null);Assert.That((bool)typeof(SimpleHud).GetProperty("VillageReturnVisible",flags).GetValue(hud),Is.True);
                Assert.That((bool)typeof(SimpleHud).GetProperty("ControlsBlocked",flags).GetValue(hud),Is.True);
                Assert.That(Vector3.Distance(player.transform.position,village),Is.GreaterThan(100));
                cycle.Restore(new DayNightSnapshot{day=70,phase=Phase.Evening,elapsedInPhase=.8f});
                update.Invoke(hud,null);Assert.That(player.transform.position,Is.EqualTo(village));
                player.transform.position+=Vector3.right;update.Invoke(hud,null);
                Assert.That(player.transform.position,Is.EqualTo(village+Vector3.right),"The same transition must not teleport again on every frame");
                float elapsed=cycle.ElapsedInPhase;Time.timeScale=0;yield return new WaitForSecondsRealtime(.2f);
                Assert.That(cycle.ElapsedInPhase,Is.EqualTo(elapsed));Time.timeScale=1;
                cycle.Restore(new DayNightSnapshot{day=70,phase=Phase.Night});
                EventBus.Instance.Emit(new NightStartedPayload(70));update.Invoke(hud,null);
                Assert.That(player.transform.position,Is.EqualTo(village+Vector3.right),"Night fallback must not teleport a second time");
                Assert.That((bool)typeof(SimpleHud).GetProperty("VillageReturnVisible",flags).GetValue(hud),Is.False);
                Assert.That((bool)typeof(SimpleHud).GetProperty("ControlsBlocked",flags).GetValue(hud),Is.False);
                player.transform.position=new Vector3(4500,4500,0);
                EventBus.Instance.Emit(new NightStartedPayload(71));
                Assert.That(player.transform.position,Is.EqualTo(village),"Skipped Evening must still gather the player safely");
            }
            finally {cycle.Restore(saved);Time.timeScale=1;InputReader.HudBlocked=false;player.enabled=true;}
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
