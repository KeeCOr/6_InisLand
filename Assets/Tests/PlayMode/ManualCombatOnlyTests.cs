using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class ManualCombatOnlyTests
    {
        [UnityTest] public IEnumerator NearbyEnemiesTakeDamageOnlyAfterManualInput()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            hud.Player.transform.position=new Vector3(1000,1000,0);hud.Attacker.SwitchToWeapon(0);
            var go=new GameObject("Manual combat target");go.transform.position=hud.Player.transform.position+Vector3.right*.5f;
            go.AddComponent<SpriteRenderer>();go.AddComponent<CircleCollider2D>();
            var enemy=go.AddComponent<Zombie>();enemy.InitHp(1000);enemy.enabled=false;Physics2D.SyncTransforms();
            yield return new WaitForSeconds(1.2f);
            Assert.That(enemy.CurrentHp,Is.EqualTo(1000),"Standing beside a target must not trigger player attacks");
            hud.UseUnifiedAction();Assert.That(enemy.CurrentHp,Is.LessThan(1000),"Space must still attack");
            Object.Destroy(go);Time.timeScale=1;yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
