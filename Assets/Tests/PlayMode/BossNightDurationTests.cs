using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class BossNightDurationTests
    {
        [UnityTest] public IEnumerator BothScheduledAndKillTriggeredBossesHoldNightThroughWarningAndCombat()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            Time.timeScale=1;
            var night=Object.FindFirstObjectByType<NightController>();night.enabled=false;night.BlizzardChance=0;
            var cycle=GameSession.Instance.Cycle;
            foreach(int day in new[]{5,1})
            {
                cycle.Restore(new DayNightSnapshot{day=day,phase=Phase.Night,elapsedInPhase=0});
                night.StartNight(day);
                if(day==1){night.BossKillThreshold=1;night.OnNightKill();}
                Assert.That(night.BossWarningRemaining,Is.GreaterThan(0));
                cycle.Update(cycle.PhaseDurationSec+10);
                Assert.That(cycle.Phase,Is.EqualTo(Phase.Night),"A boss warning near the time limit must not be skipped by dawn");
                yield return new WaitForSeconds(3.2f);
                Zombie boss=null;
                foreach(var enemy in Object.FindObjectsByType<Zombie>(FindObjectsSortMode.None))
                    if(enemy.name=="Boss_d"+day)boss=enemy;
                Assert.That(boss,Is.Not.Null);boss.enabled=false;
                cycle.Update(10000);
                Assert.That(cycle.Phase,Is.EqualTo(Phase.Night),"A living boss must survive the normal night deadline");
                Assert.That(cycle.ElapsedInPhase,Is.LessThanOrEqualTo(cycle.PhaseDurationSec),"Overtime must not spill into later phases");
                // Prevent the manually triggered test boss death from queuing another encounter.
                night.BossKillThreshold=int.MaxValue;
                boss.TakeDamage(boss.MaxHp);
                cycle.Update(.01f);
                Assert.That(cycle.Phase,Is.EqualTo(Phase.Dawn),"An expired night ends as soon as the boss is defeated");
                Assert.That(cycle.Day,Is.EqualTo(day));
                yield return null;
            }
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
