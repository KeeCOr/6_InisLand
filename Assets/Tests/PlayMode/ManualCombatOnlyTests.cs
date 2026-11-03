using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Reflection;
namespace IL6.Tests
{
    public class AutomaticCombatTests
    {
        [UnityTest] public IEnumerator CombatRunsWithoutInputAndRespectsUiPauseHammerAndWork()
        {
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSecondsRealtime(2);
            foreach(var hud in Object.FindObjectsByType<SimpleHud>(FindObjectsSortMode.None))
            {
                typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(hud,null);
                hud.enabled=false;
            }
            foreach(var attackProbe in Object.FindObjectsByType<PlayerAttackController>(FindObjectsSortMode.None)) attackProbe.enabled=false;
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None)) spawner.enabled=false;
            foreach(var night in Object.FindObjectsByType<NightController>(FindObjectsSortMode.None)) Object.Destroy(night);
            yield return null;
            Time.timeScale=1;InputReader.HudBlocked=false;
            var actor=new GameObject("Automatic combat player");actor.transform.position=Vector3.one*5000;
            var player=actor.AddComponent<PlayerController>();player.enabled=false;
            actor.GetComponent<InputReader>().enabled=false;
            var attack=actor.AddComponent<PlayerAttackController>();
            var go=new GameObject("Automatic combat enemy");go.transform.position=actor.transform.position+Vector3.right*.5f;
            go.AddComponent<CircleCollider2D>().radius=.1f;
            var enemy=go.AddComponent<Zombie>();enemy.InitHp(1000);enemy.enabled=false;
            Physics2D.SyncTransforms();
            InputReader.HudBlocked=true;yield return new WaitForSecondsRealtime(.3f);
            Assert.That(enemy.CurrentHp,Is.EqualTo(1000));
            InputReader.HudBlocked=false;attack.SwitchToWeapon(2);
            yield return new WaitForSecondsRealtime(.3f);Assert.That(enemy.CurrentHp,Is.EqualTo(1000),"Hammer must not attack");
            attack.SwitchToWeapon(0);Time.timeScale=0;
            yield return new WaitForSecondsRealtime(.3f);Assert.That(enemy.CurrentHp,Is.EqualTo(1000),"Pause must stop combat");
            Time.timeScale=1;Assert.That(player.Actions.TryBegin(PlayerAction.Logging,Vector2.right),Is.True);
            yield return new WaitForSecondsRealtime(.8f);Assert.That(enemy.CurrentHp,Is.EqualTo(1000),"Auto attack must not interrupt logging");
            player.Actions.ResetToIdle();
            yield return new WaitForSecondsRealtime(.8f);
            Assert.That(enemy.CurrentHp,Is.LessThan(1000),"Melee must attack without Space or button input");
            InputReader.HudBlocked=true;player.Actions.ResetToIdle();attack.SwitchToWeapon(1);
            go.transform.position=actor.transform.position+Vector3.right*3f;Physics2D.SyncTransforms();
            yield return new WaitForSecondsRealtime(1.2f);int hp=enemy.CurrentHp;
            InputReader.HudBlocked=false;
            yield return new WaitForSecondsRealtime(1.2f);
            Assert.That(enemy.CurrentHp,Is.LessThan(hp),"Bow must also attack without input");
            Object.Destroy(go);Object.Destroy(actor);Time.timeScale=1;InputReader.HudBlocked=false;yield return null;
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
