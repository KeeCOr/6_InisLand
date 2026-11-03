using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Reflection;
namespace IL6.Tests
{
    public class DirectionalCharacterPlayTests
    {
        [UnityTest] public IEnumerator HeroUsesWeaponSpecificVerticalArtAndKeepsActionClips()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSeconds(2);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var player=hud.Player;player.enabled=false;
            var visual=player.GetComponent<DirectionalCharacterVisual>();
            var renderer=player.GetComponent<SpriteRenderer>();var attack=player.GetComponent<PlayerAttackController>();
            foreach(int weapon in new[]{0,1,2})
            {
                player.Actions.ResetToIdle();attack.SwitchToWeapon(weapon);
                string key=weapon==1?"hero_bow":weapon==2?"hero_hammer":"hero_axe";
                visual.ShowDirection(Vector2.up);
                Assert.That(renderer.sprite,Is.SameAs(DirectionalCharacterVisual.DirectionSprite(key,CharacterFacing.Back)));
                visual.ShowDirection(Vector2.down);
                Assert.That(renderer.sprite,Is.SameAs(DirectionalCharacterVisual.DirectionSprite(key,CharacterFacing.Front)));
            }
            player.Actions.ResetToIdle();attack.SwitchToWeapon(0);
            Assert.That(player.Actions.TryBegin(PlayerAction.Attack,Vector2.right),Is.True);
            player.GetComponent<Animator>().Update(.05f);visual.ShowDirection(Vector2.up);
            Assert.That(renderer.sprite.name,Does.StartWith("redhair_"),"Action clips retain ownership of the renderer");
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
        [UnityTest] public IEnumerator CompanionDirectionsSurviveRecruitmentAndReturnToFrontIdle()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            var go=new GameObject("direction-test");var renderer=go.AddComponent<SpriteRenderer>();
            renderer.sprite=SpriteBank.CompanionSpriteForRole("Hunter");
            var npc=go.AddComponent<RecruitableNpc>();npc.Role="Hunter";
            yield return null;
            var visual=go.GetComponent<DirectionalCharacterVisual>();Assert.That(visual,Is.Not.Null);
            Assert.That(renderer.sprite.name,Is.EqualTo("front"));
            visual.ShowDirection(Vector2.up);Assert.That(renderer.sprite.name,Is.EqualTo("back"));Assert.That(renderer.flipX,Is.False);
            visual.ShowDirection(Vector2.zero);Assert.That(renderer.sprite.name,Is.EqualTo("front"));
            visual.ShowDirection(Vector2.down);Assert.That(renderer.sprite.name,Is.EqualTo("front"));
            visual.ShowDirection(Vector2.left);Assert.That(renderer.sprite,Is.SameAs(SpriteBank.CompanionSpriteForRole("Hunter")));Assert.That(renderer.flipX,Is.True);
            npc.Recruit();yield return null;
            Assert.That(go.GetComponent<DirectionalCharacterVisual>(),Is.SameAs(visual));
            visual.ShowDirection(Vector2.up);Assert.That(renderer.sprite.name,Is.EqualTo("back"));
            visual.ShowDirection(Vector2.zero);Assert.That(renderer.sprite.name,Is.EqualTo("front"));Assert.That(renderer.flipX,Is.False);
            Object.Destroy(go);yield return null;
        }
    }
}
