using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class MobileHudGameplayTests
    {
        [UnityTest]
        public IEnumerator PrimaryActionRepairsAndManualAttackRespectsPauseAndCooldown()
        {
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return null;
            yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");
            yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            Assert.That(hud,Is.Not.Null);
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var player=hud.Player;
            player.transform.position=new Vector3(80,80,0);
            var house=BuildingFactory.SpawnHouse(new Vector3(81,80,0)).GetComponent<Building>();
            yield return null;
            house.TakeDamage(20);
            var store=GameSession.Instance.Resources;
            store.Add(ResourceKind.Wood,10);
            int wood=store.Get(ResourceKind.Wood), hp=house.CurrentHp;
            hud.Attacker.SwitchToWeapon(2);
            Time.timeScale=0;
            hud.UsePrimaryAction();
            Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(wood));
            Assert.That(house.CurrentHp,Is.EqualTo(hp));
            Time.timeScale=1;
            hud.UsePrimaryAction();
            Assert.That(store.Get(ResourceKind.Wood),Is.EqualTo(wood-1));
            Assert.That(house.CurrentHp,Is.GreaterThan(hp));
            var attack=hud.Attacker;

            attack.SwitchToWeapon(0);
            yield return new WaitForSeconds(2);
            var go=new GameObject("Manual attack target");
            go.transform.position=player.transform.position+Vector3.right*0.5f;
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<CircleCollider2D>();
            var zombie=go.AddComponent<Zombie>();
            zombie.InitHp(1000);
            Physics2D.SyncTransforms();
            Time.timeScale=0;
            Assert.That(attack.TryAttack(),Is.False);
            Assert.That(zombie.CurrentHp,Is.EqualTo(1000));
            Time.timeScale=1;
            Assert.That(attack.TryAttack(),Is.True);
            Assert.That(zombie.CurrentHp,Is.LessThan(1000));
            int after=zombie.CurrentHp;
            Assert.That(attack.TryAttack(),Is.False);
            Assert.That(zombie.CurrentHp,Is.EqualTo(after));
            Object.Destroy(go); Object.Destroy(house.gameObject);
            InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
