using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public sealed class ActionDrivenDayTests
    {
        [UnityTest] public IEnumerator IdleDayWaitsWhileMovementCombatAndWorkConsumeTimeAndNightStillFlows()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var player=hud.Player;player.enabled=false;hud.Attacker.enabled=false;player.GetComponent<GatherController>().enabled=false;hud.enabled=false;
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None))spawner.enabled=false;
            var night=Object.FindFirstObjectByType<NightController>();night.enabled=false;
            var cycle=GameSession.Instance.Cycle;
            player.transform.position=new Vector3(8800,8800,0);player.GetComponent<Rigidbody2D>().velocity=Vector2.zero;player.Actions.ResetToIdle();
            Time.timeScale=1;InputReader.HudBlocked=false;
            cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=5});
            yield return new WaitForSeconds(.25f);
            Assert.That(cycle.ElapsedInPhase,Is.EqualTo(5).Within(.001f),"Standing still must preserve remaining daylight");
            player.Actions.SetMoving(true);float before=cycle.ElapsedInPhase;
            yield return new WaitForSeconds(.25f);Assert.That(cycle.ElapsedInPhase-before,Is.InRange(.20f,.35f));
            player.Actions.SetMoving(false);yield return null;before=cycle.ElapsedInPhase;
            yield return new WaitForSeconds(.2f);Assert.That(cycle.ElapsedInPhase,Is.EqualTo(before).Within(.001f));
            var target=new GameObject("Daytime attack target");target.transform.position=player.transform.position+Vector3.right;
            target.AddComponent<CircleCollider2D>().radius=.2f;var animal=target.AddComponent<DeerAi>();animal.InitHp(1000);animal.enabled=false;
            Physics2D.SyncTransforms();before=cycle.ElapsedInPhase;Assert.That(hud.Attacker.TryAttack(),Is.True);
            yield return new WaitForSeconds(.2f);Assert.That(cycle.ElapsedInPhase,Is.GreaterThan(before));
            Object.Destroy(target);yield return new WaitForSeconds(1);
            player.Actions.ResetToIdle();yield return null;
            Assert.That(player.Actions.TryBegin(PlayerAction.Logging,Vector2.right,.25f),Is.True);before=cycle.ElapsedInPhase;
            yield return new WaitForSeconds(.2f);Assert.That(cycle.ElapsedInPhase,Is.GreaterThan(before));
            InputReader.HudBlocked=true;before=cycle.ElapsedInPhase;
            yield return new WaitForSeconds(.2f);Assert.That(cycle.ElapsedInPhase,Is.EqualTo(before).Within(.001f));
            InputReader.HudBlocked=false;Time.timeScale=0;
            yield return new WaitForSecondsRealtime(.15f);Assert.That(cycle.ElapsedInPhase,Is.EqualTo(before).Within(.001f));
            Time.timeScale=1;player.Actions.ResetToIdle();yield return null;
            cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Night,elapsedInPhase=5});before=cycle.ElapsedInPhase;
            yield return new WaitForSeconds(.25f);Assert.That(cycle.ElapsedInPhase-before,Is.InRange(.20f,.35f),"Night keeps realtime flow even when idle");
            cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=0});
            cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=cycle.PhaseDurationSec-.05f});player.Actions.SetMoving(true);
            yield return new WaitForSeconds(.2f);Assert.That(cycle.Phase,Is.EqualTo(Phase.Evening));
            cycle.Restore(new DayNightSnapshot{day=1,phase=Phase.Day,elapsedInPhase=0});
            Time.timeScale=1;InputReader.HudBlocked=false;yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
