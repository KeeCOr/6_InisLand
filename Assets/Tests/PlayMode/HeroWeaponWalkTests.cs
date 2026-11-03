using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class HeroWeaponWalkTests
    {
        [UnityTest] public IEnumerator HorizontalWalkAlternatesLegFramesForEveryWeaponAndRestoresIdle()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            yield return SceneManager.LoadSceneAsync("BootScene");yield return null;
            yield return SceneManager.LoadSceneAsync("SnowfieldScene");yield return new WaitForSecondsRealtime(1);
            var hud=Object.FindFirstObjectByType<SimpleHud>();
            typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var player=hud.Player;player.enabled=false;
            var visual=player.GetComponent<DirectionalCharacterVisual>();visual.enabled=false;
            var renderer=player.GetComponent<SpriteRenderer>();var animator=player.GetComponent<Animator>();
            var attack=player.GetComponent<PlayerAttackController>();
            foreach(int weapon in new[]{0,1,2})
            {
                player.Actions.ResetToIdle();attack.SwitchToWeapon(weapon);
                typeof(SimpleHud).GetMethod("PrepareHudPreview",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
                string key=weapon==0?"hero_axe":weapon==1?"hero_bow":"hero_hammer";
                visual.AnimateMovement(Vector2.zero,.02f);animator.Update(.02f);
                visual.AnimateMovement(Vector2.right*5,.02f);
                Assert.That(renderer.sprite,Is.SameAs(DirectionalCharacterVisual.WalkSprite(key,CharacterFacing.Right,0)),key+" must use its actual side step, not legacy animation");
                animator.Update(.16f);visual.AnimateMovement(Vector2.right*5,.16f);
                Assert.That(renderer.sprite,Is.SameAs(DirectionalCharacterVisual.WalkSprite(key,CharacterFacing.Right,1)));
                visual.AnimateMovement(Vector2.left*5,.02f);Assert.That(renderer.flipX,Is.True);
                visual.AnimateMovement(Vector2.zero,.02f);Assert.That(renderer.sprite.name,Is.EqualTo("idle_side"));
            }
            yield return SceneManager.LoadSceneAsync("BootScene");
        }
    }
}
