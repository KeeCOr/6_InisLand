using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class VisibleEnemyNightTests
    {
        [UnityTest] public IEnumerator VisibleEnemyHoldsExpiredNightAndLeavingViewAllowsDawn()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            Time.timeScale=1;
            var night=Object.FindFirstObjectByType<NightController>();night.enabled=false;night.BlizzardChance=0;
            var cycle=GameSession.Instance.Cycle;
            cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Night,elapsedInPhase=0});night.StartNight(1);
            var camera=Camera.main;Assert.That(camera,Is.Not.Null);
            var go=new GameObject("Visible night blocker");
            go.transform.position=new Vector3(camera.transform.position.x,camera.transform.position.y,0);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=SpriteBank.QuadrupedRobot();
            var enemy=go.AddComponent<Zombie>();enemy.enabled=false;
            try
            {
                cycle.Update(cycle.PhaseDurationSec+10);
                Assert.That(cycle.Phase,Is.EqualTo(Phase.Night),"On-screen enemy must survive the deadline");
                Assert.That(cycle.ElapsedInPhase,Is.EqualTo(cycle.PhaseDurationSec));
                var flags=BindingFlags.Instance|BindingFlags.NonPublic;
                typeof(NightController).GetField("_spawnTimer",flags).SetValue(night,100f);
                int before=Object.FindObjectsByType<Zombie>(FindObjectsSortMode.None).Length;
                typeof(NightController).GetMethod("Update",flags).Invoke(night,null);
                Assert.That(Object.FindObjectsByType<Zombie>(FindObjectsSortMode.None).Length,Is.EqualTo(before),"No extra normal spawns during overtime");
                go.transform.position+=Vector3.right*1000;
                cycle.Update(.01f);Assert.That(cycle.Phase,Is.EqualTo(Phase.Dawn),"Off-screen ordinary enemies do not hold night");
            }
            finally {Object.Destroy(go);}
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
