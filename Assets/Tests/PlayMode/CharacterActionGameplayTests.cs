using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public class CharacterActionGameplayTests
    {
        [UnityTest] public IEnumerator AllCompanionRolesShowThreeAttackPhasesWhileKeepingMovementAndPhysics()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            var actors=new List<GameObject>();var phases=new HashSet<ActionPhase>();
            var target=new GameObject("Action visual target");target.transform.position=new Vector3(9500,9504,0);
            target.AddComponent<SpriteRenderer>().sprite=SpriteBank.QuadrupedRobot();var enemy=target.AddComponent<Zombie>();enemy.enabled=false;enemy.InitHp(10000);
            var previous=new HashSet<Projectile>(Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None));
            var roles=new[]{"Hunter","Warrior","Farmer","Cook","Healer","Aunt","Uncle","Child"};
            try
            {
                foreach(string role in roles)
                {
                    var go=new GameObject("Action "+role);go.transform.position=new Vector3(9500+actors.Count*3,9500,0);
                    go.AddComponent<SpriteRenderer>().sprite=SpriteBank.CompanionSpriteForRole(role);
                    var companion=go.AddComponent<Companion>();companion.enabled=false;
                    go.AddComponent<DirectionalCharacterVisual>().Configure(role);
                    go.GetComponent<Rigidbody2D>().velocity=Vector2.right*2;actors.Add(go);
                    target.transform.position=go.transform.position+Vector3.up*4;
                    typeof(Companion).GetMethod("SpawnProjectile",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(companion,new object[]{enemy});
                }
                float until=Time.time+1;
                while(Time.time<until && actors[0].GetComponent<CharacterActionVisual>().IsPlaying)
                {
                    yield return null;
                    foreach(var go in actors)
                    {
                        var motion=go.GetComponent<CharacterActionVisual>();if(!motion.IsPlaying)continue;
                        phases.Add(motion.Phase);var key=go.GetComponent<DirectionalCharacterVisual>().CharacterKey;
                        Assert.That(go.GetComponent<SpriteRenderer>().sprite,Is.SameAs(CharacterActionVisual.Frame(key,CharacterFacing.Back,motion.Phase)));
                        Assert.That(go.GetComponent<Rigidbody2D>().velocity.x,Is.EqualTo(2).Within(.001f));
                        Assert.That(go.transform.localScale,Is.EqualTo(Vector3.one));
                        Assert.That(go.GetComponent<CircleCollider2D>().radius,Is.EqualTo(.35f));
                    }
                }
                Assert.That(phases.Count,Is.EqualTo(3));
                yield return null;
                foreach(var go in actors)Assert.That(go.GetComponent<SpriteRenderer>().sprite.name,Does.Not.StartWith("attack_"));
            }
            finally
            {
                foreach(var go in actors)Object.Destroy(go);Object.Destroy(target);
                foreach(var p in Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None))if(!previous.Contains(p))Object.Destroy(p.gameObject);
            }
            yield return null;
        }

        [UnityTest] public IEnumerator HeroActualAttacksUseWeaponAndTargetDirectionWithoutStoppingMovement()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(hud,null);
            hud.enabled=false;var player=hud.Player;player.enabled=false;hud.Attacker.enabled=false;
            player.transform.position=new Vector3(9600,9600,0);
            var target=new GameObject("Hero action target");target.transform.position=player.transform.position+Vector3.up*.6f;
            target.AddComponent<CircleCollider2D>().radius=.1f;var enemy=target.AddComponent<Zombie>();enemy.enabled=false;enemy.InitHp(10000);
            foreach(int weapon in new[]{0,1})
            {
                player.Actions.ResetToIdle();hud.Attacker.SwitchToWeapon(weapon);
                typeof(PlayerAttackController).GetField("_cooldown",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(hud.Attacker,0f);
                target.transform.position=player.transform.position+Vector3.up*.6f;Physics2D.SyncTransforms();
                player.GetComponent<Rigidbody2D>().velocity=Vector2.right;
                Assert.That(hud.Attacker.TryAttack(),Is.True);yield return null;
                var motion=player.GetComponent<CharacterActionVisual>();Assert.That(motion.IsPlaying,Is.True);
                Assert.That(motion.Facing,Is.EqualTo(CharacterFacing.Back));
                Assert.That(player.GetComponent<SpriteRenderer>().sprite,Is.SameAs(CharacterActionVisual.Frame(weapon==0?"hero_axe":"hero_bow",CharacterFacing.Back,motion.Phase)));
                Assert.That(player.GetComponent<Rigidbody2D>().velocity.x,Is.EqualTo(1).Within(.001f));
                yield return new WaitForSeconds(.8f);
            }
            Object.Destroy(target);yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
