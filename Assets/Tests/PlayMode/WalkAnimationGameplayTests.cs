using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace IL6.Tests
{
    public class WalkAnimationGameplayTests
    {
        [UnityTest] public IEnumerator CompanionSwapsImagesAndReturnsToIdleWithoutChangingPhysics()
        {
            Time.timeScale=1;InputReader.HudBlocked=false;
            var go=new GameObject("Walk animation probe");var renderer=go.AddComponent<SpriteRenderer>();
            renderer.sprite=SpriteBank.CompanionSpriteForRole("Hunter");var body=go.AddComponent<Rigidbody2D>();body.gravityScale=0;
            var visual=go.AddComponent<DirectionalCharacterVisual>();visual.Configure("Hunter");visual.enabled=false;
            body.velocity=Vector2.up*5;
            visual.AnimateMovement(body.velocity,.02f);var first=renderer.sprite;
            Assert.That(first,Is.SameAs(DirectionalCharacterVisual.WalkSprite("hunter",CharacterFacing.Back,0)));
            visual.AnimateMovement(body.velocity,.16f);Assert.That(renderer.sprite,Is.SameAs(DirectionalCharacterVisual.WalkSprite("hunter",CharacterFacing.Back,1)));
            Assert.That(body.velocity,Is.EqualTo(Vector2.up*5));Assert.That(visual.MovementPhase,Is.EqualTo(ActionPhase.Loop));
            visual.AnimateMovement(Vector2.left*5,.02f);Assert.That(renderer.flipX,Is.True);
            Assert.That(renderer.sprite,Is.SameAs(DirectionalCharacterVisual.WalkSprite("hunter",CharacterFacing.Left,visual.MovementFrame)));
            visual.AnimateMovement(Vector2.zero,.02f);Assert.That(renderer.sprite,Is.SameAs(DirectionalCharacterVisual.DirectionSprite("hunter",CharacterFacing.Front)));
            Assert.That(renderer.flipX,Is.False);
            Assert.That(visual.MovementPhase,Is.EqualTo(ActionPhase.End));
            visual.AnimateMovement(Vector2.down*5,.02f);visual.AnimateMovement(Vector2.zero,.02f);
            Assert.That(renderer.sprite,Is.SameAs(DirectionalCharacterVisual.DirectionSprite("hunter",CharacterFacing.Front)));
            Object.Destroy(go);yield return null;
        }
    }
}
