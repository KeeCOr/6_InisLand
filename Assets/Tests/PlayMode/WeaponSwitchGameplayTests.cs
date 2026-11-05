using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public sealed class WeaponSwitchGameplayTests
    {
        private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;InputReader.HudBlocked=false;yield return SceneManager.LoadSceneAsync("BootScene");}
        [UnityTest] public IEnumerator SlotsSwitchDuringAttacksAndGatheringWithoutExtraHitsAndHammerPauseReleases()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",Flags).Invoke(hud,null);
            var player=hud.Player;var attack=hud.Attacker;player.enabled=false;attack.enabled=false;
            player.transform.position=new Vector3(9100,9100,0);player.GetComponent<Rigidbody2D>().velocity=Vector2.zero;
            var target=new GameObject("Weapon switch combat target");target.transform.position=player.transform.position+Vector3.left;
            target.AddComponent<CircleCollider2D>().radius=.2f;var deer=target.AddComponent<DeerAi>();deer.InitHp(1000);deer.enabled=false;
            Physics2D.SyncTransforms();attack.SwitchToWeapon(0);Assert.That(attack.TryAttack(),Is.True);
            Assert.That(player.Actions.IsBusy,Is.True);float cooldown=attack.CurrentCooldown;int hp=deer.CurrentHp;
            var invoke=typeof(SimpleHud).GetMethod("InvokeHudAction",Flags);invoke.Invoke(hud,new object[]{1});
            Assert.That(attack.WeaponIndex,Is.EqualTo(1),"An auto attack must not swallow the bow slot selection");
            Assert.That(player.Actions.IsBusy,Is.False);Assert.That(player.GetComponent<CharacterActionVisual>().IsPlaying,Is.False);
            Assert.That(attack.CurrentCooldown,Is.EqualTo(cooldown).Within(.001f));Assert.That(deer.CurrentHp,Is.EqualTo(hp));
            Assert.That(attack.TryAttack(),Is.False,"Switching cannot bypass the previous attack cooldown");
            invoke.Invoke(hud,new object[]{2});Assert.That(attack.IsConstructionHammer,Is.True);Assert.That(Time.timeScale,Is.Zero);
            Assert.That((bool)typeof(SimpleHud).GetField("_buildCatalogOpen",Flags).GetValue(hud),Is.True);
            invoke.Invoke(hud,new object[]{0});Assert.That(attack.WeaponIndex,Is.Zero);Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(InputReader.HudBlocked,Is.False);
            var tree=new GameObject("Weapon switch logging target");tree.transform.position=player.transform.position+Vector3.right;
            tree.AddComponent<CircleCollider2D>().radius=.2f;var resource=tree.AddComponent<Gatherable>();resource.DurationSec=10;
            player.Actions.ResetToIdle();Assert.That(hud.Gather.StartGathering(resource),Is.True);int treeHp=tree.GetComponent<TreeHealth>().CurrentHp;
            invoke.Invoke(hud,new object[]{1});Assert.That(attack.WeaponIndex,Is.EqualTo(1));Assert.That(hud.Gather.IsActive,Is.False);Assert.That(player.Actions.IsBusy,Is.False);
            yield return new WaitForSeconds(.3f);Assert.That(tree.GetComponent<TreeHealth>().CurrentHp,Is.EqualTo(treeHp),"Cancelled logging must not apply a delayed chop");
            player.TakeDamage(player.MaxHp+100);attack.SwitchToWeapon(0);Assert.That(attack.WeaponIndex,Is.EqualTo(1),"Dead actors cannot switch weapons");
            Object.Destroy(tree);Object.Destroy(target);
        }
    }
}
