using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace IL6.Tests
{
    public sealed class PlayerIdentityGameplayTests
    {
        [UnityTest]
        public IEnumerator GirlIdentitySurvivesAnimationAndWeaponChanges()
        {
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return null; yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");
            yield return new WaitForSeconds(2);
            var player=Object.FindFirstObjectByType<PlayerController>();
            Assert.That(player,Is.Not.Null);
            var renderer=player.GetComponent<SpriteRenderer>();
            var animator=player.GetComponent<Animator>();
            var attacker=player.GetComponent<PlayerAttackController>();
            player.enabled=false;
            attacker.enabled=false;
            Assert.That(renderer.sprite,Is.SameAs(DirectionalCharacterVisual.HeroSideIdle("hero_axe")));
            // Test the retained action Animator separately from LateUpdate locomotion ownership.
            player.GetComponent<DirectionalCharacterVisual>().enabled=false;
            foreach(var weapon in new[]{0,1,0})
            {
                attacker.SwitchToWeapon(weapon);
                string key=weapon==1?"bow":"axe";
                foreach(bool moving in new[]{false,true,false})
                {
                    animator.SetBool("isMove",moving);
                    for(int i=0;i<12;i++)
                    {
                        animator.Update(0.1f);
                        Assert.That(renderer.sprite.name,Does.StartWith("redhair_"+key+"_"));
                    }
                }
            }
            var move=typeof(PlayerController).GetMethod("UpdateMoveAnimation",BindingFlags.Instance|BindingFlags.NonPublic);
            move.Invoke(player,new object[]{Vector2.left});
            Assert.That(renderer.flipX,Is.True);
            move.Invoke(player,new object[]{Vector2.right});
            Assert.That(renderer.flipX,Is.False);
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
