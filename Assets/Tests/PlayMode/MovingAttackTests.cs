using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class MovingAttackTests
    {
        [UnityTest] public IEnumerator BothWeaponsAllowFullSpeedMovementAndDirectionChangesDuringAttack()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            foreach(var spawner in Object.FindObjectsByType<ProceduralSpawner>(FindObjectsSortMode.None)) spawner.enabled=false;
            // Day transitions clean up all zombies, including this stationary combat probe.
            foreach(var night in Object.FindObjectsByType<NightController>(FindObjectsSortMode.None)) Object.Destroy(night);
            yield return null; // OnDestroy unsubscribes cycle events; disabling alone leaves handlers active.
            var player=hud.Player;var actions=player.Actions;var rb=player.GetComponent<Rigidbody2D>();
            var input=player.GetComponent<InputReader>();input.enabled=false;
            var axis=typeof(InputReader).GetField("<MoveAxis>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic);
            var fixedUpdate=typeof(PlayerController).GetMethod("FixedUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
            var target=new GameObject("Moving attack target");target.AddComponent<CircleCollider2D>().radius=.15f;
            var zombie=target.AddComponent<Zombie>();zombie.InitHp(1000);zombie.enabled=false;
            for(int weapon=0;weapon<2;weapon++)
            {
                actions.ResetToIdle();hud.Attacker.SwitchToWeapon(weapon);
                player.transform.position=new Vector3(3000+weapon*10,3000,0);
                target.transform.position=player.transform.position+Vector3.right;
                Physics2D.SyncTransforms();axis.SetValue(input,Vector2.up);fixedUpdate.Invoke(player,null);
                Vector2 speed=rb.velocity;
                Assert.That(hud.Attacker.TryAttack(),Is.True);
                Assert.That(rb.velocity,Is.EqualTo(speed),"Starting an attack must not cancel movement");
                Assert.That(actions.IsBusy,Is.True,"Attacks still exclude logging and weapon changes");
                Assert.That(actions.BlocksMovement,Is.False);
                Assert.That(actions.TryBegin(PlayerAction.Logging,Vector2.right),Is.False);
                fixedUpdate.Invoke(player,null);
                Assert.That(rb.velocity,Is.EqualTo(speed));
                Vector2 before=rb.position;yield return new WaitForFixedUpdate();yield return null;
                Assert.That(rb.position.y,Is.GreaterThan(before.y));
                axis.SetValue(input,Vector2.left);fixedUpdate.Invoke(player,null);
                Assert.That(rb.velocity.x,Is.EqualTo(-speed.magnitude).Within(.001f));
                Assert.That(rb.velocity.y,Is.Zero);
                axis.SetValue(input,Vector2.zero);fixedUpdate.Invoke(player,null);
                Assert.That(rb.velocity,Is.EqualTo(Vector2.zero));
                yield return new WaitForSeconds(1.1f);
            }
            Object.Destroy(target);input.enabled=true;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
